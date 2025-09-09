using System.Collections;
using UnityEngine;

public class AvgPlayer : MonoBehaviour
{

    private static AvgPlayer _instance;
    public static AvgPlayer Instance => _instance ??= new AvgPlayer();

    int nowEventId;
    int lastEventId;
    Coroutine corNextEvent;




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
    void PlayAvgEvent(cfg.avg.AvgEvent _avgEvent)
    {
        if (_avgEvent == null)
        {
            Debug.LogError($"{_avgEvent}为空，无法播放");
            return;
        }

        if (_avgEvent?.TimeDelay != 0)
        {
            //如果有延迟时间，在AVGMANAGER调用一个协程来管理下一个对话（注意后续可能做成List<Coroutine>的形式）
            AvgManager.Instance.StartWaitingNextAvgDialogue(_avgEvent);
        }
        else
        {
            UIManager.Instance.ShowAvgDialogue(_avgEvent);
        }
        // await Task.Delay((int)(_avgEvent.TimeDelay * 1000));
    }



    public void NextAvgEvent()
    {
        if (nowEventId == lastEventId)
        {
            Debug.Log("最后一个event已播放，剧情播放完成");
            AvgManager.Instance.isPlayingAvg = null;
            return;
        }


        //首先令正在播放的指针+1
        nowEventId += 1;

        //如果找不到这个ID，则+1以后找下一个ID，直到找到最后一个ID为止
        var nowEvent = cfg.Tables.tb.AvgEvent.GetOrDefault(nowEventId);
        var recheckCount = 10;
        while (nowEvent == null)
        {
            --recheckCount;
            nowEventId += 1;
            //如果已经超过了最后一个，就返回
            if (nowEventId > lastEventId)
            {
                Debug.LogWarning("找不到下一个可播放的event，剧情播放完成");
                AvgManager.Instance.isPlayingAvg = null;
                return;
            }
            else if (recheckCount < 0)
            {
                Debug.LogWarning("超出连续查找的数值上限，请配置连续的avgeventid");
                AvgManager.Instance.isPlayingAvg = null;
                return;
            }
            ;
            nowEvent = cfg.Tables.tb.AvgEvent.GetOrDefault(nowEventId);
        }

        //如果找得到，则播放
        PlayAvgEvent(nowEvent);



    }



}