using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// namespace cfg
// {
//     public partial class AvgEvent
//     {
//         public bool canInterruptAvg = false;
//     }
// }



// AVG 触发器管理器
public class AvgManager : MonoBehaviour
{

    private static AvgManager _instance;
    public static AvgManager Instance => _instance;


    public static AvgDialogueUI dialogueAvgInstance;


    public int? isPlayingAvg;//是否正在播放AVG

    public Coroutine nowAvgCor;



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
            case cfg.Enums.Com.TriggerType.INCODE://代码写死的AVGSTORY规则
                return null;
            case cfg.Enums.Com.TriggerType.DUNGEON_START://关卡开始
                return new DungeonStartTrigger(config);
            case cfg.Enums.Com.TriggerType.DUNGEON_OVER://关卡结束
                return new DungeonOverTrigger(config);
            case cfg.Enums.Com.TriggerType.UI_STATE://切UI
                return new UIShowTrigger(config);
            case cfg.Enums.Com.TriggerType.PLAYAD://看广告
                return new AdPlayTrigger(config);

            default:
                Debug.LogWarning($"未知的触发器类型，avgId：{config.Id}");
                return null;
        }
    }










    // 检查并触发符合条件的AVG
    public void CheckAndTriggerAvgs(cfg.Enums.Com.TriggerType triggerType)
    {
        if (isPlayingAvg != null)
        {
            Debug.LogWarning("有正在播放的avg，终止AVG检测");
            return;
        }

        //遍历寻找在当前triggerType下可以触发的AVG（只触发1个）
        //多个允许触发的则随机触发其中1个
        List<IAvgTrigger> lists = new();
        foreach (var trigger in _triggers.Values)
        {
            if (trigger.TriggerType == triggerType && trigger.ShouldTrigger())
            {
                lists.Add(trigger);
            }

        }
        var random = Utility.GetRandomByList(lists);
        if (random == null) return;

        if (TriggerAvg(random.config.Id))
        {
            //标记触发器为触发的
            random.MarkAsTriggered();
        }

    }












    // 触发特定AVG
    public bool TriggerAvg(int avgId)
    {

        //直接使用这个会跳过正在播放AVG的检测
        //注意：可能会导致trigger不触发，并且目前很难解决它会打断其他AVG的情况

        if (isPlayingAvg != null)
        {
            Debug.LogWarning("有正在播放的avg，终止AVG检测");
            return false;
        }
        var config = cfg.Tables.tb.AvgStory.GetOrDefault(avgId);

        if (config == null)
        {
            Debug.LogError($"AVG {avgId} 不存在");
            return false;
        }

        if (_triggeredAvgs.Contains(avgId) && config.CanRecur == false)
        {
            Debug.Log($"AVG {avgId} 为一次性事件，并且已经触发过，跳过");
            return false;
        }

        // 播放AVG
        PlayAvg(avgId);

        // 触发事件
        OnAvgTriggered?.Invoke(avgId);

        // 如果没标记为已触发，则标记一下
        // 此处主要以防万一有反复触发的被反复标记
        if (!_triggeredAvgs.Contains(avgId)) _triggeredAvgs.Add(avgId);


        Debug.Log($"触发AVG: {avgId}");
        return true;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 测试AVG（仅编辑器可用）
    /// </summary>
    /// <param name="avgId"></param>
    public void TestAvg(int avgId)
    {
        if (cfg.Tables.tb.AvgStory.GetOrDefault(avgId) == null)
        {
            Debug.LogError($"AVG {avgId} 不存在");
            return;
        }

        // 播放AVG
        PlayAvg(avgId);
    }
#endif

    // 播放AVG的具体实现
    private void PlayAvg(int _avgStoryId)
    {
        isPlayingAvg = _avgStoryId;


        // 这里实现AVG播放逻辑
        var avgConfig = cfg.Tables.tb.AvgStory.Get(_avgStoryId);
        //在AVG播放脚本中初始化本次AVG
        AvgPlayer.Instance.Initialize(avgConfig);
        //保存已触发的avg们
        SaveTriggeredAvgs();
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
        string triggeredIds = PlayerPrefs.GetString("triggered_avgs", "");
        if (triggeredIds == "") return;
        var ids = triggeredIds.Split(',').Select(int.Parse);
        _triggeredAvgs = new HashSet<int>(ids);
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


    public void StartWaitingNextAvgDialogue(cfg.avg.AvgEvent _avgEvent)
    {
        nowAvgCor = StartCoroutine(CWaitingForShowAvgDialogue(_avgEvent));
    }

    /// <summary>
    /// 对于延后执行的事件
    /// </summary>
    /// <param name="_avgEvent"></param>
    /// <returns></returns>
    IEnumerator CWaitingForShowAvgDialogue(cfg.avg.AvgEvent _avgEvent)
    {
        var elapsedTime = 0f;
        var waitSeconds = 0.02f;
        var wait = new WaitForSecondsRealtime(waitSeconds);
        while (elapsedTime <= _avgEvent.TimeDelay)
        {
            elapsedTime += waitSeconds;
            yield return wait;
        }
        UIManager.Instance.ShowAvgDialogue(_avgEvent);
    }

}