using System.Collections;
using UnityEditor;
using UnityEngine;


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



    void Awake()
    {
        if (Instance != null) return;
        Instance = this;

        BasicSetting();

        mainCam = Camera.main;
        mainCamBattlePos = new Vector3(0, -0.3f, -10);
        mainCamMainViewPos = new Vector3(0, -25, -10);
        mainCamBattleSize = 5.8f;
        mainCamMainViewSize = 30;


    }

    void Start()
    {
        CheckIfFirstLoad();
        SwitchGameStateToMainView();
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

        UIManager.Instance.SwitchLayer(UILayer.NULL);
        BattleManager.Instance.Initialize(dungeonId);

    }


    /// <summary>
    /// 检查是不是第一次启动游戏，如果是则给予数据
    /// </summary>
    void CheckIfFirstLoad()
    {
        //根据是否通关过任意关卡来判断是否首次加载游戏
        if (PlayerPrefs.HasKey("dungeon_passed_level")) return;
        //加载初始数据
        DataManager.Instance.FirstLoad();
        Debug.Log("首次加载游戏，加载成功");
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
        while (Mathf.Abs(newSize - mainCam.orthographicSize) >= 0.02f)
        {
            mainCam.transform.position = Vector3.Slerp(mainCam.transform.position, newPos, 0.04f);
            mainCam.orthographicSize = Mathf.Lerp(mainCam.orthographicSize, newSize, 0.04f);
            yield return null;
        }
        mainCam.transform.position = newPos;
        mainCam.orthographicSize = newSize;
    }







#if UNITY_EDITOR
    [MenuItem("微信小游戏 / 清除存档")]
    public static void DeleteAllPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
#endif
}
