using UnityEngine;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("资源索引")]
    //传送门
    public Transform PortalsPath;
    public GameObject PortalPrefab;


    public float GameTime;//游戏进行时间
    [Header("关卡数据")]
    int dungeonLevel;//关卡等级


    bool canGameTimeCount;//可以计数了

    //地球初始血量
    public int dataInitEarthHp;

    public int currentEarthHp;//地球当前血量


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
        if (PortalsPath == null) PortalsPath = GameObject.Find("Portals").transform;
        // if (EnemysPath == null) EnemysPath = GameObject.Find("Enemys").transform;
        if (PortalPrefab == null) PortalPrefab = Resources.Load<GameObject>("Prefabs/Portals/Portal");


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

        //加载UI
        UIManager.Instance.SwitchLayer(UILayer.BATTLELAYER);

    }

    void Update()
    {
        if (canGameTimeCount) GameTime += Time.deltaTime;
    }






    /// <summary>
    /// 游戏结束
    /// </summary>
    void BattleOver()
    {
        GameTime = 0;
        canGameTimeCount = false;
    }


    /// <summary>
    /// 生成传送门
    /// </summary>
    void CreatePortals(cfg.dungeon.DungeonWave waveId_Ref, Vector2 position)
    {
        var Portal = Instantiate(PortalPrefab, position, Quaternion.identity);
        var portalLevel = dungeonLevel;
        Portal.AddComponent<Portal>().Initialize(waveId_Ref, portalLevel);
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


    // /// <summary>
    // /// 回收子弹（简化版）
    // /// </summary>
    // public void ReturnBullet(Bullet bullet)
    // {
    //     bullet.isReleased = true;
    //     poolManager.ReleaseBullet(bullet.gameObject);
    // }

    // /// <summary>
    // /// 回收敌人（简化版）
    // /// </summary>
    // public void ReturnEnemy(Enemy enemy)
    // {
    //     poolManager.ReleaseEnemy(enemy.gameObject);
    // }





}
