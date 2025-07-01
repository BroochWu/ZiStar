using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Awake()
    {
        if (Instance != null) return;
        Instance = this;

        BasicSetting();
        CheckIfFirstLoad();

    }

    void Start()
    {
        BattleStart(1);
    }


    /// <summary>
    /// 基础设置
    /// </summary>
    void BasicSetting()
    {

        Application.targetFrameRate = 120;
    }

    /// <summary>
    /// 战斗开始
    /// </summary>
    /// <param name="dungeonId"></param>
    void BattleStart(int dungeonId)
    {
        BattleManager.Instance.Initialize(dungeonId);
    }


    /// <summary>
    /// 检查是不是第一次启动游戏，如果是则给予数据
    /// </summary>
    void CheckIfFirstLoad()
    {
        //根据是否通关过任意关卡来判断是否首次加载游戏
        if (PlayerPrefs.HasKey("dungeon_passed_level")) return;
        Debug.Log("首次加载游戏");
        FirstLoad();
    }
    /// <summary>
    /// 首次加载的内容
    /// </summary>
    void FirstLoad()
    {

        PlayerPrefs.SetInt("playerData_hp_level", 1);
        //暂时去掉，这样每次都可以判断
        PlayerPrefs.SetInt("dungeon_passed_level", 1);

    }


    [MenuItem("微信小游戏 / 清除存档")]
    public static void DeleteAllPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
