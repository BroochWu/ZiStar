using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// AVG 触发器管理器
public class AvgManager : MonoBehaviour
{
    private static AvgManager _instance;
    public static AvgManager Instance => _instance;


    public AvgDialogueUI dialogueAvgInstance;


    private bool isPlayingAvg;//是否正在播放AVG


    private Dictionary<int, IAvgTrigger> _triggers = new Dictionary<int, IAvgTrigger>();
    private HashSet<int> _triggeredAvgs = new HashSet<int>();

    // 事件委托
    public delegate void AvgTriggeredHandler(int avgId);
    public event AvgTriggeredHandler OnAvgTriggered;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        // DontDestroyOnLoad(gameObject);

        LoadTriggeredAvgs();
        InitializeTriggers();
    }

    void OnDestroy()
    {
        SaveTriggeredAvgs();
    }

    // 初始化所有触发器
    private void InitializeTriggers()
    {
        // 从配置表加载所有AVG触发器配置
        var avgConfigs = cfg.Tables.tb.AvgStory.DataList;

        foreach (var config in avgConfigs)
        {
            IAvgTrigger trigger = CreateTriggerFromConfig(config);
            if (trigger != null)
            {
                //增加记录AVG在哪个trigger触发
                _triggers.Add(config.Id, trigger);
            }
        }
    }

    // 根据配置创建触发器
    private IAvgTrigger CreateTriggerFromConfig(cfg.avg.AvgStory config)
    {
        switch (config.Trigger.TriggerType)
        {
            case cfg.Enums.Com.TriggerType.DUNGEON_START:
                return new DungeonStartTrigger(config.Id, config.Trigger.IntParams[0]);

            default:
                Debug.LogWarning($"未知的触发器类型: {config.Id}");
                return null;
        }
    }




    // 检查并触发符合条件的AVG
    public void CheckAndTriggerAvgs(cfg.Enums.Com.TriggerType triggerType)
    {
        if (isPlayingAvg == true)
        {
            Debug.LogError("已经在播放avg");
            return;
        }

        //寻找可以触发的AVG（只触发1个）
        foreach (var trigger in _triggers.Values)
        {
            if (trigger.TriggerType == triggerType && trigger.ShouldTrigger())
            {
                TriggerAvg(trigger.AvgId);
                trigger.MarkAsTriggered();
                break;
            }
        }

        isPlayingAvg = false;


    }

    // 触发特定AVG
    public void TriggerAvg(int avgId)
    {
        if (_triggeredAvgs.Contains(avgId))
        {
            Debug.Log($"AVG {avgId} 已经触发过，跳过");
            return;
        }

        // 播放AVG
        PlayAvg(avgId);

        // 标记为已触发
        _triggeredAvgs.Add(avgId);

        // 触发事件
        OnAvgTriggered?.Invoke(avgId);

        Debug.Log($"触发AVG: {avgId}");
    }

    // 播放AVG的具体实现
    private void PlayAvg(int avgId)
    {
        isPlayingAvg = true;


        // 这里实现AVG播放逻辑
        var avgConfig = cfg.Tables.tb.AvgStory.Get(avgId);
        //在AVG播放脚本中初始化本次AVG
        AvgPlayer.Instance.Initialize(avgConfig);
    }

    // 保存已触发的AVG
    private void SaveTriggeredAvgs()
    {
        string triggeredIds = string.Join(",", _triggeredAvgs);
        PlayerPrefs.SetString("triggered_avgs", triggeredIds);
    }

    // 加载已触发的AVG
    private void LoadTriggeredAvgs()
    {
        if (PlayerPrefs.HasKey("triggered_avgs"))
        {
            string triggeredIds = PlayerPrefs.GetString("triggered_avgs");
            var ids = triggeredIds.Split(',').Select(int.Parse);
            _triggeredAvgs = new HashSet<int>(ids);
        }
    }

    // // 重置所有AVG触发状态（用于测试或重置游戏）
    // public void ResetAllTriggers()
    // {
    //     _triggeredAvgs.Clear();
    //     PlayerPrefs.DeleteKey("triggered_avgs");

    //     foreach (var trigger in _triggers.Values)
    //     {
    //         if (trigger is IResettableTrigger resettable)
    //         {
    //             resettable.Reset();
    //         }
    //     }
    // }
}