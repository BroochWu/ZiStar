using System.Threading.Tasks;
using UnityEngine;

public class AvgPlayer
{

    private static AvgPlayer _instance;
    public static AvgPlayer Instance => _instance ??= new AvgPlayer();

    int nowEventId;
    int lastEventId;




    //构造函数
    public void Initialize(cfg.avg.AvgStory _avgStoryConfig)
    {
        //播放story
        //先播第一条，隔X秒后播第二条，以此类推，直到播完
        var initEvent = _avgStoryConfig.IdStart_Ref;


        nowEventId = _avgStoryConfig.IdStart;
        lastEventId = _avgStoryConfig.IdEnd;


        PlayAvgEvent(initEvent);

    }



    // /// <summary>
    // /// 循环播放avg事件
    // /// </summary>
    // async Task LoopPlayAvgEvent(cfg.avg.AvgEvent _avgInitEvent)
    // {
    //     if (_avgInitEvent.TimeDelay != 0) await Task.Delay((int)(_avgInitEvent.TimeDelay * 1000));
    //     PlayAvgEvent(_avgInitEvent);

    // }


    /// <summary>
    /// 执行AVG操作
    /// </summary>
    /// <param name="_eventId"></param>
    async Task PlayAvgEvent(cfg.avg.AvgEvent _avgEvent)
    {
        if (_avgEvent.TimeDelay != 0) await Task.Delay((int)(_avgEvent.TimeDelay * 1000));
        UIManager.Instance.ShowAvgDialogue(_avgEvent);
    }


    public void NextAvgEvent()
    {
        if (nowEventId == lastEventId)
        {
            Debug.Log("最后一个event已播放，剧情播放完成");
            AvgManager.Instance.isPlayingAvg = false;
            return;
        }


        //首先令正在播放的指针+1
        nowEventId += 1;

        //如果找不到这个ID，则+1以后找下一个ID，直到找到最后一个ID为止
        var nowEvent = cfg.Tables.tb.AvgEvent.GetOrDefault(nowEventId);
        while (nowEvent == null)
        {
            nowEventId += 1;
            //如果已经超过了最后一个，就返回
            if (nowEventId > lastEventId)
            {
                Debug.Log("找不到下一个可播放的event，剧情播放完成");
                AvgManager.Instance.isPlayingAvg = false;
                return;
            }
            nowEvent = cfg.Tables.tb.AvgEvent.GetOrDefault(nowEventId);
        }

        //如果找得到，则播放
        PlayAvgEvent(nowEvent);



    }



}