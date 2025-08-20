using cfg.Enums.Com;

// AVG 触发器接口
public interface IAvgTrigger
{
    //AvgId
    int AvgId { get; }
    //触发类型
    cfg.Enums.Com.TriggerType TriggerType { get; }
    //判断是否触发
    bool ShouldTrigger();
    //标记为已触发
    void MarkAsTriggered();
    //已触发标记
    bool HasTriggered { get; }
}


public class DungeonStartTrigger : IAvgTrigger
{
    //战斗开始时的触发器
    public int AvgId { get; }
    private int targetDungeonId;
    public bool HasTriggered { get; private set; }

    public TriggerType TriggerType => TriggerType.DUNGEON_START;


    /// <summary>
    /// 关卡开启
    /// </summary>
    /// <param name="_avgId">AVGID</param>
    /// <param name="_targetDungeonId">第几关</param>
    public DungeonStartTrigger(int _avgId, int _targetDungeonId)
    {
        AvgId = _avgId;
        targetDungeonId = _targetDungeonId;
    }


    public bool ShouldTrigger()
    {
        return (!HasTriggered) && (targetDungeonId == BattleManager.Instance.dungeonId);
    }
    public void MarkAsTriggered()
    {
        HasTriggered = true;
    }
}