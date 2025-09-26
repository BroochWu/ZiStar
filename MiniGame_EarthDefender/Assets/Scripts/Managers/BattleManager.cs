using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public enum BattleState
{
    NULL = -2,
    ISBATTLEING = -1,
    BATTLEFAIL = 0,
    BATTLESUCCESS = 1
}
public enum BattleLoseReason
{
    NULL = 0,
    ACTIVEQUIT = 1,//主动放弃
    NORMAL = 2,//空血死亡
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public static int dungeonLevel;//关卡等级
    public BattleState battleState;
    public BattleLoseReason battleLoseReason;

    [Header("资源索引")]
    public Transform BattleObjectsPath;
    public Transform PortalsPath { get; private set; }
    public Transform BulletsPath { get; private set; }
    public Transform EnemyPath { get; private set; }
    public Transform VfxsPath { get; private set; }//场景特效层
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
    public List<EnemyBase> activeEnemys = new();//活跃的敌人

    //战斗统计相关
    public int totalDamage;
    private List<KeyValuePair<Weapon, int>> _sortedWeaponDamageStatisticsList = new();
    public List<KeyValuePair<Weapon, int>> sortedWeaponDamageStatisticsList
    {
        get
        {
            return _sortedWeaponDamageStatisticsList;
        }
        set
        {
            _sortedWeaponDamageStatisticsList = value;
            //每次设置值的时候，都会更新总伤害
            totalDamage = 0;
            foreach (var a in value)
            {
                totalDamage += a.Value;
            }
        }
    }


    public int totalEnemyThisDungeon { get; private set; }//本局敌人总数，传送门会滞后生成就不能采用活跃的传送门+活跃的怪物来判断了
    public System.Random dungeonSeed { get; private set; }//关卡种子


    // 事件委托
    // public delegate void OnBattleSuccess();
    // public event OnBattleSuccess onBattleSuccess;


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

            VfxsPath = new GameObject("VFXs").transform;
            VfxsPath.SetParent(BattleObjectsPath);
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

        // 初始化随机数生成器(种子)
        dungeonSeed = new System.Random(dungeonId);


        Player.instance.canMove = true;

        //关卡配置
        var config = cfg.Tables.tb.Dungeon.Get(dungeonId);
        dungeonLevel = config.DungeonLevel;
        this.dungeonId = config.Id;

        //计算出本关敌人总数
        totalEnemyThisDungeon = 0;
        foreach (var portal in config.Portals)
        {
            foreach (var perWave in portal.WaveId_Ref.EnemyCreate)
            {
                totalEnemyThisDungeon += perWave.EnemyInit.Angles.Count;
            }
        }
        // Debug.Log("本关敌人总数：" + totalEnemyThisDungeon);


        //创建黑洞以生成敌人波次
        foreach (var i in config.Portals)
        {
            // Debug.Log(i.WaveId + " 波次已加载");
            StartCoroutine(CorCreatePortal(i));
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
            if (totalEnemyThisDungeon <= 0)
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
    /// 重置关卡，释放所有的传送门和怪物
    /// </summary>
    public void ResetDungeon()
    {

        //移除所有的玩家武器
        sortedWeaponDamageStatisticsList?.Clear();
        Player.instance.RemoveAllWeapons();

        //移除所有传送门
        var tempList = new List<Portal>();
        tempList.AddRange(activePortals);
        foreach (var portal in tempList)
        {
            UnregisterPortal(portal);
        }
        activePortals.Clear();

        //移除所有敌人
        var tempList2 = new List<EnemyBase>();
        tempList2.AddRange(activeEnemys);
        foreach (var enemy in tempList2)
        {
            UnregisterEnemy(enemy);
        }
        activeEnemys.Clear();




        if (BulletsPath != null)
        {
            Destroy(BulletsPath.gameObject);
            BulletsPath = new GameObject("Bullets").transform;
            BulletsPath.SetParent(BattleObjectsPath);
        }


        //重置战斗对象池
        ObjectPoolManager.Instance.Reset();


        if (BattleObjectsPath != null) Destroy(BattleObjectsPath.gameObject);
        //ObjectPoolManager.Instance.ClearAllPools();
        Player.instance.rotationTarget.transform.rotation = quaternion.identity;
    }


    Vector2 buttonInit = new();
    /// <summary>
    /// 主动放弃对战
    /// </summary>
    public void QuitGame()
    {
        // Debug.Log("buttonInit：" + buttonInit);
        if (DataManager.Instance.dungeonPassedLevel == 0)
        {
            //注册button
            var button = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            if (buttonInit == Vector2.zero)
            {
                // Debug.Log("Reset buttonInit");
                buttonInit = Camera.main.WorldToViewportPoint(button.transform.position);
            }

            //随机移动放弃按钮的位置
            var randomX = UnityEngine.Random.Range(-0.2f, 0.2f);
            var randomY = UnityEngine.Random.Range(-0.3f, 0.3f);
            var newPos = Camera.main.ViewportToWorldPoint(new Vector2(buttonInit.x + randomX, buttonInit.y + randomY));
            newPos.z = 0;
            button.transform.position = newPos;

            //随机剧本
            var avgId = Utility.GetRandomByList(cfg.Tables.tb.GlobalParam.Get("avg_while_quit_in_dungeon_1").IntListValue);
            AvgManager.Instance.TriggerAvg(avgId);


            return;
        }

        battleLoseReason = BattleLoseReason.ACTIVEQUIT;
        BattleFail();
    }


    /// <summary>
    /// 更新本局武器伤害排序
    /// </summary>
    void RefreshSortedWeaponDamageStatisticsList()
    {
        sortedWeaponDamageStatisticsList = Player.instance.battleEquipedWeapon
                .OrderByDescending(n => n.Value)
                .ToList();

    }


    /// <summary>
    /// 战斗失败
    /// </summary>
    void BattleFail()
    {
        //地球血量清零则战斗失败
        battleState = BattleState.BATTLEFAIL;


        //战斗结束通用处理
        BattleOver();

    }

    /// <summary>
    /// 战斗成功
    /// </summary>
    void BattleSuccess()
    {
        battleState = BattleState.BATTLESUCCESS;
        //存档：已通关的最高等级（日后如果有活动和支线关卡的话另当别论）
        DataManager.Instance.dungeonPassedLevel = dungeonId;

        //通关第五关解锁并初始化商店
        // if (dungeonId == 5) ShopShoppingManager.Instance.UnlockShop();

        //战斗结束通用处理
        BattleOver();

    }


    /// <summary>
    /// 战斗结束通用处理
    /// </summary>
    void BattleOver()
    {
        //停止本脚本包括传送门生成在内的所有协程
        StopAllCoroutines();
        //刷新武器伤害总览
        RefreshSortedWeaponDamageStatisticsList();

        //奖励列表
        AwardDungeon();

        //数据重置
        canGameTimeCount = false;
        Time.timeScale = 0.05f;
        //UI播放通关
        UIManager.Instance.battleLayer.BattleOver();

        //移除所有的玩家武器
        Player.instance.RemoveAllWeapons();

        //判断执行关卡结束时的AVG事件
        AvgManager.Instance.CheckAndTriggerAvgs(cfg.Enums.Com.TriggerType.DUNGEON_OVER);

    }

    IEnumerator CorCreatePortal(cfg.Beans.Dungeon_Portals _portal)
    {
        yield return new WaitForSeconds(_portal.DelayTime);
        CreatePortal(_portal);
    }

    /// <summary>
    /// 生成单一传送门
    /// </summary>
    void CreatePortal(cfg.Beans.Dungeon_Portals _portal)
    {
        var portal = Instantiate(PortalPrefab, _portal.Position, Quaternion.identity).GetOrAddComponent<Portal>();
        portal.transform.SetParent(PortalsPath);
        portal.Initialize(_portal.WaveId_Ref);
        RegisterPortal(portal);
    }

    /// <summary>
    /// 计算伤害公式(返回真实扣除的伤害)
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="currentHp"></param>
    public int CalDamage(int damage, int currentHp, out int newHp)
    {
        newHp = Mathf.Max(0, currentHp - damage);
        return currentHp - newHp;
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

        CalDamage(_damage, currentEarthHp, out int newHp);
        currentEarthHp = newHp;

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
            portal.GetComponent<Animator>()?.Play("PortalDisappear");
        }
    }

    // 注册敌人
    public void RegisterEnemy(EnemyBase enemy)
    {
        if (!activeEnemys.Contains(enemy))
        {
            activeEnemys.Add(enemy);
        }
    }

    // 注销敌人
    public void UnregisterEnemy(EnemyBase enemy)
    {
        if (activeEnemys.Contains(enemy))
        {
            activeEnemys.Remove(enemy);
            if (GameManager.Instance.gameState == GameManager.GameState.BATTLE)
            {
                //每死一个B就检测一下
                // if (activePortals.Count <= 0 && activeEnemys.Count <= 0)
                // {
                //     BattleSuccess();
                // }
                // else
                // {
                totalEnemyThisDungeon -= 1;
                GainExp(enemy.enemyExp);
                UIManager.Instance.battleLayer.RefreshExpLevel();
                // }
            }
        }
        else
        {
            Debug.LogWarning("未能找到enemy  " + enemy);
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
        // Debug.Log("升级成功");
        //3选1
        StartTri(cfg.Enums.Card.Type.UPGRADE);

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


    public void StartTri(cfg.Enums.Card.Type _type)
    {
        // Debug.Log("开始抽卡：" + _type);

        Time.timeScale = 0;

        Player.instance.canMove = false;


        switch (_type)
        {
            case cfg.Enums.Card.Type.UPGRADE:
                TriCard.Instance.GetTriCards(cfg.Enums.Card.Type.UPGRADE);
                break;
            case cfg.Enums.Card.Type.WEAPONUNLOCK:
                TriCard.Instance.GetTriCards(cfg.Enums.Card.Type.WEAPONUNLOCK);
                break;
        }
    }

    public void EndTri()
    {


        //生效成功，关闭窗口们
        UIManager.Instance.battleLayer.triCardUI.gameObject.SetActive(false);
        Time.timeScale = 1;

        Player.instance.canMove = true;
        //再判断是否连续升级
        CheckLevelUp();

    }



    public void PlusGlobalDamageMultiInOneBattle(int number)
    {
        globalDamageMultiInOneBattle += number;
        Debug.Log("当前全局伤害加成：" + globalDamageMultiInOneBattle);
        foreach (var weapon in Player.instance.battleEquipedWeapon.Keys)
        {
            weapon.GetAndSetWeaponAttack();
        }
    }


}
