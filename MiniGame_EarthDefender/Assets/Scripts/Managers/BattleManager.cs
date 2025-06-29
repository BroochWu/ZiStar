using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public Transform bulletPath;
    public Transform PortalsPath;
    public Transform EnemysPath;
    public GameObject PortalPrefab;


    public float GameTime;


    bool canGameTimeCount;//可以计数了
    Dictionary<int, bool> enemyList;//要生成的怪物的列表，false是没创建，true是创建了


    public PoolBullet poolBullet = new PoolBullet();



    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);


        if (bulletPath == null) bulletPath = GameObject.Find("Bullets").transform;
        if (PortalsPath == null) PortalsPath = GameObject.Find("Portals").transform;
        if (EnemysPath == null) EnemysPath = GameObject.Find("Enemys").transform;
        if (PortalPrefab == null) PortalPrefab = Resources.Load<GameObject>("Prefabs/Portals/Portal");




    }



    void Start()
    {
        BattleStart(1);
    }



    /// <summary>
    /// 游戏开始
    /// </summary>
    /// <param name="dungeonId">关卡id</param>
    public void BattleStart(int dungeonId)
    {
        var config = cfg.Tables.tb.Dungeon.Get(dungeonId);

        foreach (var i in config.Portals)
        {
            Debug.Log(i.WaveId + " 波次已加载");
            CreatePortals(i.WaveId_Ref, i.Position);
        }
        canGameTimeCount = true;
        GameTime = 0;

        Debug.Log(config.TextName + " 已加载！");
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
        Portal.AddComponent<Portal>().Initialize(waveId_Ref);
    }



}
