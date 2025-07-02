using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public enum BattleState
{
    ISBATTLEING,
    BATTLEFAIL,
    BATTLESUCCESS
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public static int dungeonLevel;//关卡等级
    public BattleState battleState;

    [Header("资源索引")]
    public Transform BattleObjectsPath;
    public Transform PortalsPath;
    public Transform BulletsPath;
    public Transform EnemyPath;
    //传送门
    public GameObject PortalPrefab;


    public float GameTime;//游戏进行时间
    [Header("关卡数据")]

    bool canGameTimeCount;//可以计数了

    //地球初始血量
    public int dataInitEarthHp;

    public int currentEarthHp;//地球当前血量
    List<Portal> activePortals = new();//活跃的传送门
    List<Enemy> activeEnemys = new();//活跃的敌人
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

        BattleObjectsPath = new GameObject("BattleObjects").transform;

        PortalsPath = new GameObject("Portals").transform;
        PortalsPath.SetParent(BattleObjectsPath);

        BulletsPath = new GameObject("Bullets").transform;
        BulletsPath.SetParent(BattleObjectsPath);

        EnemyPath = new GameObject("Enemys").transform;
        EnemyPath.SetParent(BattleObjectsPath);



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
        var playerConfig = cfg.Tables.tb.PlayerAttrLevel;
        dataInitEarthHp = playerConfig.Get(PlayerPrefs.GetInt("playerData_hp_level")).EarthHp;
        currentEarthHp = dataInitEarthHp;

        //关卡配置
        var config = cfg.Tables.tb.Dungeon.Get(dungeonId);
        dungeonLevel = config.DungeonLevel;


        foreach (var i in config.Portals)
        {
            Debug.Log(i.WaveId + " 波次已加载");
            CreatePortals(i.WaveId_Ref, i.Position);
        }
        canGameTimeCount = true;
        GameTime = 0;

        Debug.Log(config.TextName + " 已加载！");

        battleState = BattleState.ISBATTLEING;
        
        //加载UI
        UIManager.Instance.SwitchLayer(UILayer.BATTLELAYER);

        //执行角色事件（装载武器）
        Player.instance.BattleStart();

    }

    void Update()
    {
        if (canGameTimeCount) GameTime += Time.deltaTime;

        if (Time.frameCount % 50 == 0)
        {
            //每50帧（大概1秒？）检测一次
            if (activeEnemys.Count <= 0 && activePortals.Count <= 0 && battleState == BattleState.ISBATTLEING)
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
        Destroy(BattleObjectsPath.gameObject);

        //移除所有的玩家武器
        Player.instance.RemoveAllWeapons();
        ObjectPoolManager.Instance.ClearAllPools();
        Player.instance.rotationTarget.transform.rotation = quaternion.identity;
    }




    /// <summary>
    /// 战斗失败
    /// </summary>
    void BattleOver()
    {
        //地球血量清零则战斗失败
        battleState = BattleState.BATTLEFAIL;
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
        canGameTimeCount = false;
        Time.timeScale = 0.05f;

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
        if (currentEarthHp <= 0) BattleOver();
    }


    #region "胜利条件相关信息注册"
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
        }
    }
    #endregion


}
