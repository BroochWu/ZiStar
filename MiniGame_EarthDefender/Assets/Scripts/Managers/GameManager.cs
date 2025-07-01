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

        Application.targetFrameRate = 180;
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
        FirstLoad();
        Debug.Log("首次加载游戏，加载成功");
    }


    /// <summary>
    /// 首次加载的内容
    /// </summary>
    void FirstLoad()
    {
        var playerData = cfg.Tables.tb.PlayerData;
        //初始化数据加载
        foreach (var data in playerData.DataList)
        {
            switch (data.ParamType)
            {
                case cfg.Enums.Com.ParamType.INT:
                    PlayerPrefs.SetInt(data.DataStr, int.Parse(data.ParamValueInit));
                    break;
                case cfg.Enums.Com.ParamType.STRING:
                    PlayerPrefs.SetString(data.DataStr, data.ParamValueInit);
                    break;
            }

        }

    }

    [MenuItem("微信小游戏 / 清除存档")]
    public static void DeleteAllPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
