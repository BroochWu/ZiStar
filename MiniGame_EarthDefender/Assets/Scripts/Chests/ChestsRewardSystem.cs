using System;
using System.Collections;
using UnityEngine;

public static class ChestsRewardSystem
{
    const int PENDING_TIME = 540;//投放间隔（秒、9分钟）
    public const int MAX_CHESTS = 20;//最多积累多少个箱子
    public const string PLAYERPREFS_KEY_CHEST_COUNT = "rewardchest_receive_chests_count";

    public static int currentRemainSeconds;//当前距离下一次领奖的剩余时间(秒)
    public static int nowRemainChests//当前未领取的箱子
    {
        get { return PlayerPrefs.GetInt(PLAYERPREFS_KEY_CHEST_COUNT); }
        set { SaveChestsCount(); }
    }


    static int initRemainSecond;//登录时的初始剩余时间


    /// <summary>
    /// 仅登陆时（或者每次切换到主界面时？）触发
    /// </summary>
    /// <param name="_last"></param>
    /// <param name="_now"></param>
    public static void SetChestRewardsOnLoad(DateTime _last, DateTime _now)
    {
        int timePend = (int)(_now - _last).TotalSeconds;
        if (timePend < 0)
        {
            CheaterInChestsReward();
            return;
        }

        var chestCount = timePend / PENDING_TIME + PlayerPrefs.GetInt(PLAYERPREFS_KEY_CHEST_COUNT);
        // var remainTime = TimeSpan.FromSeconds(initRemainSecond);
        Debug.Log("timePend:" + timePend);


        // initRemainSecond = PENDING_TIME - timePend % PENDING_TIME;

        SetChestCounts(chestCount);


        //每一次登录都按照9分钟开始倒计时
        initRemainSecond = PENDING_TIME;
        currentRemainSeconds = initRemainSecond;
        // SendChestsRewardAction(chestCount, remainTime);
    }

    static void CheaterInChestsReward()
    {
        UIManager.Instance.CommonToast("嗯？你是不是偷偷改数据啦（盯）");
    }

    static void SendChestsRewardAction(int count, TimeSpan initRemain)
    {

    }

    public static void SetChestCounts(int newChestCount)
    {

        if (newChestCount > MAX_CHESTS)
        {
            nowRemainChests = MAX_CHESTS;
        }
        nowRemainChests = newChestCount;
        SaveChestsCount();
        Debug.Log("当前剩余箱子数量：" + nowRemainChests);
    }


    /// <summary>
    /// 在线期间（尤其是停留在主界面）每隔一段时间就发箱子
    /// </summary>
    public static IEnumerator OnlineSendChests()
    {
        var newWait = new WaitForSecondsRealtime(1);
        while (true)
        {

            currentRemainSeconds -= 1;
            if (currentRemainSeconds <= 0)
            {
                SetChestCounts(nowRemainChests + 1);
                currentRemainSeconds = PENDING_TIME;
            }

            yield return newWait;
        }
    }

    static void SaveChestsCount()
    {
        PlayerPrefs.SetInt(PLAYERPREFS_KEY_CHEST_COUNT, nowRemainChests);
    }
}