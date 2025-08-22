using System.Collections.Generic;
using cfg.Enums.Com;

// AVG 触发器接口
public interface IAvgTrigger
{
    //Avg配置
    cfg.avg.AvgStory config { get; }
    //触发类型
    cfg.Enums.Com.TriggerType TriggerType { get; }
    //已触发标记
    bool HasTriggered { get; }

    //判断是否触发
    bool ShouldTrigger();
    //标记为已触发
    void MarkAsTriggered();
}


//战斗开始时的触发器
public class DungeonStartTrigger : IAvgTrigger
{
    public cfg.avg.AvgStory config { get; }
    public bool HasTriggered { get; private set; }

    public TriggerType TriggerType => TriggerType.DUNGEON_START;


    /// <summary>
    /// 关卡开启
    /// </summary>
    /// <param name="_avgId">AVGID</param>
    /// <param name="_targetDungeonId">第几关</param>
    public DungeonStartTrigger(cfg.avg.AvgStory _config)
    {
        config = _config;
    }


    public bool ShouldTrigger()
    {
        //如果不可反复触发并且触发过了，则无法触发本剧情
        if (!config.CanRecur && HasTriggered) return false;

        //如果不是指定的关卡并且指定了关卡，则无法触发
        var targetDungeonId = config.Trigger.IntParams.Count == 0 ? -1 : config.Trigger.IntParams[0];
        if ((targetDungeonId != BattleManager.Instance.dungeonId) && (targetDungeonId > -1))
            return false;

        //接下来麻烦的来了：判断条件是否都满足
        if (!Utility.CondListCheck(config.UnlockConds))
            return false;


        return true;
    }
    public void MarkAsTriggered()
    {
        HasTriggered = true;
    }
}


//战斗结束时的触发器
public class DungeonOverTrigger : IAvgTrigger
{
    public cfg.avg.AvgStory config { get; }
    public bool HasTriggered { get; private set; }

    public TriggerType TriggerType => TriggerType.DUNGEON_OVER;


    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="_avgId">AVGID</param>
    /// <param name="_targetDungeonId">第几关</param>
    public DungeonOverTrigger(cfg.avg.AvgStory _config)
    {
        config = _config;
    }


    public bool ShouldTrigger()
    {
        //如果不可反复触发并且触发过了，则无法触发本剧情
        if (!config.CanRecur && HasTriggered)
            return false;

        //如果不是指定的关卡并且指定了关卡，则无法触发
        var targetDungeonId = config.Trigger.IntParams.Count == 0 ? -1 : config.Trigger.IntParams[0];
        if ((targetDungeonId != BattleManager.Instance.dungeonId) && (targetDungeonId > -1))
            return false;

        //接下来麻烦的来了：判断条件是否都满足
        if (!Utility.CondListCheck(config.UnlockConds))
            return false;

        return true;
    }
    public void MarkAsTriggered()
    {
        HasTriggered = true;
    }
}


