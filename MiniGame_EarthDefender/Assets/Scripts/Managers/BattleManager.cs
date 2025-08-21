using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public enum BattleState
{
    NULL,
    ISBATTLEING,
    BATTLEFAIL,
    BATTLESUCCESS
}
public enum BattleLoseReason
{
    NULL,
    ACTIVEQUIT,//主动放弃
    NORMAL,//空血死亡
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public static int dungeonLevel;//关卡等级
    public BattleState battleState;
    public BattleLoseReason battleLoseReason;

    [Header("资源索引")]
    public Transform BattleObjectsPath;
    public Transform PortalsPath;
    public Transform BulletsPath;
    public Transform EnemyPath;
    //传送门
    public GameObject PortalPrefab;


    public float GameTime;//游戏进行时间
    [Header("关卡数据")]

    public int dungeonId;//关卡Id
    bool canGameTimeCount;//可以计数了

    //地球初始血量
    public int dataInitEarthHp;

    public int currentEarthHp;//地球当前血量
    public int currentExp;//玩家当前经验值
    public int nextExp;//玩家当前经验值
    public int currentLv;//玩家当前等级
    public int globalDamageMultiInOneBattle;//当局所有武器的总伤害加成
    List<Portal> activePortals = new();//活跃的传送门
    public List<Enemy> activeEnemys = new();//活跃的敌人
    public int activeEnemysCount { get { return activeEnemys.Count; } }


    void Awake()
    {
        //实例
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }



    public void Initialize(int dungeonId)
    {

        // if (bulletPath == null) bulletPath = GameObject.Find("Bullets").transform;
        // if (PortalsPath == null) PortalsPath = GameObject.Find("Portals").transform;
        // if (EnemysPath == null) EnemysPath = GameObject.Find("Enemys").transform;

        if (PortalPrefab == null) PortalPrefab = Resources.Load<GameObject>("Prefabs/Portals/Portal");

        if (BattleObjectsPath == null)
        {
            BattleObjectsPath = new GameObject("BattleObjects").transform;

            PortalsPath = new GameObject("Portals").transform;
            PortalsPath.SetParent(BattleObjectsPath);

            BulletsPath = new GameObject("Bullets").transform;
            BulletsPath.SetParent(BattleObjectsPath);

            EnemyPath = new GameObject("Enemys").transform;
            EnemyPath.SetParent(BattleObjectsPath);
        }

        //令单局的全局攻击加成初始值为局外的全局加成总值
        globalDamageMultiInOneBattle = DataManager.Instance.TotalWeaponsGlobalAtkBonus;


        ObjectPoolManager.Instance.Initialize();

        StartCoroutine(CInitialize(dungeonId));

    }

    IEnumerator CInitialize(int dungeonId)
    {

        //加载可以被抽取的所有卡组
        TriCard.Instance.InitializeBeforeBattle();

        yield return new WaitForSeconds(1);

        //游戏开始
        BattleStart(dungeonId);
        yield return null;
    }



    /// <summary>
    /// 游戏开始
    /// </summary>
    /// <param name="dungeonId">关卡id</param>
    void BattleStart(int dungeonId)
    {
        //预热怪物
        //PreloadLevelEnemies(dungeonId);


        //玩家数据加载
        dataInitEarthHp = DataManager.Instance.GetPlayerBasicHp();
        currentEarthHp = dataInitEarthHp;

        //关卡配置
        var config = cfg.Tables.tb.Dungeon.Get(dungeonId);
        dungeonLevel = config.DungeonLevel;
        this.dungeonId = config.Id;

        //创建黑洞以生成敌人波次
        foreach (var i in config.Portals)
        {
            Debug.Log(i.WaveId + " 波次已加载");
            CreatePortals(i.WaveId_Ref, i.Position);
        }

        //关卡初始化
        currentExp = 0;
        currentLv = 0;
        nextExp = cfg.Tables.tb.DungeonExp.Get(currentLv).NextExp;
        canGameTimeCount = true;
        GameTime = 0;
        battleState = BattleState.ISBATTLEING;

        //加载UI
        UIManager.Instance.SwitchLayer(UILayer.BATTLELAYER);

        //执行角色事件（装载武器）
        Player.instance.BattleStart();

        //判断执行关卡开始时的AVG事件
        AvgManager.Instance.CheckAndTriggerAvgs(cfg.Enums.Com.TriggerType.DUNGEON_START);

    }

    void Update()
    {
        if (battleState != BattleState.ISBATTLEING) return;

        if (canGameTimeCount) GameTime += Time.deltaTime;

        if (Time.frameCount % 50 == 0)
        {
            //每50帧（大概1秒？）检测一次
            if (activeEnemys.Count <= 0 && activePortals.Count <= 0)
            {
                //当最后一波怪生成后
                //再判断当前场上剩余的怪物数量
                //如果最后一波怪物已经生成了&&怪物剩余数量==0，则战斗成功
                //可能需要等待1秒以后再判断，以防边际情况
                BattleSuccess();
            }
        }
    }


    /// <summary>
    /// 释放所有的传送门和怪物
    /// </summary>
    public void ResetDungeon()
    {

        //移除所有的玩家武器
        Player.instance.RemoveAllWeapons();

        //移除所有传送门
        var tempList = new List<Portal>();
        tempList.AddRange(activePortals);
        foreach (var portal in tempList)
        {
            UnregisterPortal(portal);
        }

        if (BulletsPath != null)
        {
            Destroy(BulletsPath.gameObject);
            BulletsPath = new GameObject("Bullets").transform;
            BulletsPath.SetParent(BattleObjectsPath);
        }


        //重置战斗对象池
        ObjectPoolManager.Instance.Reset();


        //if (BattleObjectsPath != null) Destroy(BattleObjectsPath.gameObject);
        //ObjectPoolManager.Instance.ClearAllPools();
        Player.instance.rotationTarget.transform.rotation = quaternion.identity;
    }


    Vector2 buttonInit = new();
    /// <summary>
    /// 主动放弃对战
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("buttonInit：" + buttonInit);
        if (DataManager.Instance.dungeonPassedLevel == 0)
        {
            var button = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            if (buttonInit == Vector2.zero)
            {
                Debug.Log("Reset buttonInit");
                buttonInit = Camera.main.WorldToViewportPoint(button.transform.position);
            }

            //随机移动放弃
            var randomX = UnityEngine.Random.Range(-0.2f, 0.2f);
            var randomY = UnityEngine.Random.Range(-0.3f, 0.3f);
            var newPos = Camera.main.ViewportToWorldPoint(new Vector2(buttonInit.x + randomX, buttonInit.y + randomY));
            newPos.z = 0;
            button.transform.position = newPos;

            //随机剧本
            var random = UnityEngine.Random.Range(2, 5);
            AvgManager.Instance.TriggerAvg(random);
            return;
        }

        battleLoseReason = BattleLoseReason.ACTIVEQUIT;
        BattleFail();
    }


    /// <summary>
    /// 战斗失败
    /// </summary>
    void BattleFail()
    {
        battleState = BattleState.BATTLEFAIL;
        //地球血量清零则战斗失败
        AwardDungeon();
        canGameTimeCount = false;
        Time.timeScale = 0.05f;

        UIManager.Instance.battleLayer.BattleFail();
        //移除所有的玩家武器
        Player.instance.RemoveAllWeapons();
    }

    /// <summary>
    /// 战斗成功
    /// </summary>
    void BattleSuccess()
    {
        battleState = BattleState.BATTLESUCCESS;
        //存档：已通关的最高等级（日后如果有活动和支线关卡的话另当别论）
        AwardDungeon();
        // PlayerPrefs.SetInt("dungeon_passed_level", dungeonId);
        DataManager.Instance.dungeonPassedLevel = dungeonId;

        canGameTimeCount = false;
        Time.timeScale = 0.05f;

        //UI播放通关
        UIManager.Instance.battleLayer.BattleSuccess();
        //移除所有的玩家武器
        Player.instance.RemoveAllWeapons();
    }


    /// <summary>
    /// 生成传送门
    /// </summary>
    void CreatePortals(cfg.dungeon.DungeonWave waveId_Ref, Vector2 position)
    {
        var portal = Instantiate(PortalPrefab, position, Quaternion.identity).GetOrAddComponent<Portal>();
        portal.transform.SetParent(PortalsPath);
        portal.Initialize(waveId_Ref);
        RegisterPortal(portal);
    }

    /// <summary>
    /// 计算伤害公式
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="currentHp"></param>
    public int CalDamage(int damage, int currentHp)
    {
        return Mathf.Max(0, currentHp - damage);
    }


    /// <summary>
    /// 自动战斗（暂时不管）
    /// </summary>
    void AutoBattle()
    {

    }

    /// <summary>
    /// 地球受到伤害时
    /// </summary>
    public void CalDamageEarthSuffer(int _damage)
    {

        //游戏结束就不计算了
        if (battleState != BattleState.ISBATTLEING) return;


        currentEarthHp = CalDamage(_damage, currentEarthHp);
        UIManager.Instance.battleLayer.RefreshEarthHp();
        UIManager.Instance.backbattleLayer.GrilleHit();
        if (currentEarthHp <= 0)
        {
            battleLoseReason = BattleLoseReason.NORMAL;
            BattleFail();
        }
    }


    #region 胜利条件相关信息注册(活跃的敌人、传送门)
    // 注册传送门
    public void RegisterPortal(Portal portal)
    {
        if (!activePortals.Contains(portal))
        {
            activePortals.Add(portal);
        }
    }

    // 注销传送门
    public void UnregisterPortal(Portal portal)
    {
        if (activePortals.Contains(portal))
        {
            activePortals.Remove(portal);
            portal.GetComponent<Animator>().Play("PortalDisappear");
        }
    }

    // 注册敌人
    public void RegisterEnemy(Enemy enemy)
    {
        if (!activeEnemys.Contains(enemy))
        {
            activeEnemys.Add(enemy);
        }
    }

    // 注销敌人
    public void UnregisterEnemy(Enemy enemy)
    {
        if (activeEnemys.Contains(enemy))
        {
            activeEnemys.Remove(enemy);
            GainExp(enemy.enemyExp);
            UIManager.Instance.battleLayer.RefreshExpLevel();
        }
    }
    #endregion

    #region 玩家局内成长

    /// <summary>
    /// 获得经验
    /// </summary>
    /// <param name="_number"></param>
    public void GainExp(int _number)
    {
        currentExp += _number;
        CheckLevelUp();
    }

    /// <summary>
    /// 检测是否升级
    /// </summary>
    void CheckLevelUp()
    {
        nextExp = cfg.Tables.tb.DungeonExp.Get(currentLv).NextExp;
        if (currentExp >= nextExp)
        {
            //升级
            currentExp -= nextExp;
            currentLv += 1;
            LevelUp();
        }

    }

    /// <summary>
    /// 升级操作
    /// </summary>
    void LevelUp()
    {
        Debug.Log("升级成功");
        //3选1
        StartTri();

        //判断连续升级（后面可能是选好卡牌以后判断？）
        // CheckLevelUp();
    }


    #endregion


    /// <summary>
    /// 关卡奖励
    /// </summary>
    void AwardDungeon()
    {
        //   - 根据存活时长，给予对应比例奖励
        //   - 思路：加大玩家直接采取拿体力（假设有）换道具的方式的成本，鼓励玩家正常游玩
        //   - 20秒内主动退出将没有奖励
        //   - 20秒后主动退出只能获得10%
        //   - 存活小于40秒失败，获得30%；否则获得50%

        var _awardPassed = cfg.Tables.tb.Dungeon.Get(dungeonId).PassAward;
        float _multi = 1;
        if (battleState == BattleState.BATTLESUCCESS)
        {
            _multi = 1;
        }
        else if (battleState == BattleState.BATTLEFAIL)
        {
            if (battleLoseReason == BattleLoseReason.ACTIVEQUIT)
            {
                if (GameTime <= 20)
                {
                    _multi = 0;
                }
                else
                {
                    _multi = 0.1f;
                }
            }
            else if (battleLoseReason == BattleLoseReason.NORMAL)
            {
                if (GameTime <= 40)
                {
                    _multi = 0.3f;
                }
                else if (GameTime > 40)
                {
                    _multi = 0.5f;
                }
            }
        }
        else
        {
            Debug.LogWarning($"干哪来了？怎么没有{battleState}啊");
        }

        UIManager.Instance.battleLayer.awardsList.Clear();
        foreach (var award in _awardPassed)
        {
            int num = (int)(award.Number * _multi);
            if (num != 0)
            {
                DataManager.Instance.GainResource(award.Id_Ref, num);
                UIManager.Instance.battleLayer.awardsList.Add(award.Id_Ref, num);
            }
        }

    }


    void StartTri()
    {
        Time.timeScale = 0;

        TriCard.Instance.GetTriCards();
    }

    public void EndTri()
    {
        //生效成功，关闭窗口们
        TriCard.Instance.canChooseCard = false;
        UIManager.Instance.battleLayer.triCardUI.gameObject.SetActive(false);
        Time.timeScale = 1;
        //再判断是否连续升级
        CheckLevelUp();

    }



    public void PlusGlobalDamageMultiInOneBattle(int number)
    {
        globalDamageMultiInOneBattle += number;
        Debug.Log("当前全局伤害加成：" + globalDamageMultiInOneBattle);
        foreach (var weapon in Player.instance.equipedWeapon)
        {
            weapon.GetAndSetWeaponAttack();
        }
    }


}
