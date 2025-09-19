using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;


public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        NULL,
        BATTLE,//战斗中
        MAINVIEW,//主场景中
    }


    public static GameManager Instance;
    public GameState gameState = GameState.NULL;
    public Camera mainCam;
    private Vector3 mainCamBattlePos;
    private Vector3 mainCamMainViewPos;
    private float mainCamBattleSize;
    private int mainCamMainViewSize;
    private Coroutine CorCameraMove;
    private EventSystem eventSystem => EventSystem.current;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("GM实例已存在");
            return;
        }
        Instance = this;
    }


    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {

        BasicSetting();

        mainCam = Camera.main;
        mainCamBattlePos = new Vector3(0, -0.3f, -10);
        mainCamMainViewPos = new Vector3(2, -16.5f, -10);
        mainCamBattleSize = 5.8f;
        mainCamMainViewSize = 18;

        //检查是否第一次登录
        if (CheckIfFirstLoad())
        {

            //加载初始数据
            DataManager.Instance.FirstLoad();

            //首次登陆直接打关卡1

            SwitchGameStateToBattle(1);
            UIManager.Instance.MainHallAnim.gameObject.SetActive(false);
        }
        else
        {
            //切换到主界面
            SwitchGameStateToMainView();
        }



        //统计并更新一次登录时间
        DataManager.Instance.SetLastLoadTime();

        //在线自动发箱子
        Instance.StartCoroutine(ChestsRewardSystem.OnlineSendChests());


        RedDotManager.Instance.Initialize();
        ShopShoppingManager.Instance.Initialize();




        // Debug.Log("GameManager is Initialize");

    }



    /// <summary>
    /// 切换游戏状态
    /// </summary>
    public void SwitchGameStateToMainView()
    {
        //关闭战斗场景
        //将游戏状态切换为主界面
        //打开主界面UI

        //设置状态
        gameState = GameState.MAINVIEW;
        BattleManager.Instance.battleState = BattleState.NULL;
        Time.timeScale = 1;

        UIManager.Instance.MainHallAnim.gameObject.SetActive(true);
        UIManager.Instance.MainHallAnim.Play("In");


        UIManager.Instance.SwitchLayer(UILayer.MAINLAYER);
        BattleManager.Instance.ResetDungeon();
        MoveCamera(mainCamMainViewPos, mainCamMainViewSize);

    }



    /// <summary>
    /// 基础设置
    /// </summary>
    void BasicSetting()
    {
        Application.targetFrameRate = 180;
    }



    /// <summary>
    /// 战斗开始
    /// </summary>
    /// <param name="dungeonId"></param>
    public void SwitchGameStateToBattle(int dungeonId)
    {
        gameState = GameState.BATTLE;
        MoveCamera(mainCamBattlePos, mainCamBattleSize);
        //初始化战斗管理器

        UIManager.Instance.MainHallAnim.Play("Out");

        UIManager.Instance.SwitchLayer(UILayer.NULL);
        BattleManager.Instance.Initialize(dungeonId);


    }


    /// <summary>
    /// 检查是不是第一次启动游戏，如果是则给予数据
    /// </summary>
    bool CheckIfFirstLoad()
    {
        bool result = DataManager.Instance.dungeonPassedLevel <= 0;
        if (result) Debug.LogWarning("首次加载游戏，加载成功");
        return result;
    }



    void MoveCamera(Vector3 newPos, float newSize)
    {
        if (CorCameraMove != null) StopCoroutine(CorCameraMove);
        CorCameraMove = StartCoroutine(CameraMove(newPos, newSize));
    }

    /// <summary>
    /// 相机移动
    /// </summary>
    /// <param name="newPos"></param>
    /// <returns></returns>
    IEnumerator CameraMove(Vector3 newPos, float newSize)
    {
        Debug.Log("move cam");
        while (Mathf.Abs(newSize - mainCam.orthographicSize) >= 0.02f)
        {
            mainCam.transform.position = Vector3.Slerp(mainCam.transform.position, newPos, 0.04f);
            mainCam.orthographicSize = Mathf.Lerp(mainCam.orthographicSize, newSize, 0.04f);
            yield return null;
        }
        mainCam.transform.position = newPos;
        mainCam.orthographicSize = newSize;
    }

    public void DisableEventSystem()
    {
        if (eventSystem != null)
        {
            eventSystem.enabled = false; // 禁用EventSystem
        }
    }

    public void EnableEventSystem()
    {
        if (eventSystem != null)
        {
            eventSystem.enabled = true; // 启用EventSystem
        }
    }





#if UNITY_EDITOR
    [MenuItem("微信小游戏 / 清除存档")]
    public static void DeleteAllPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
#endif
}
