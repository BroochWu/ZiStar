using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChestsRewardSystem
{
    public const string PLAYERPREFS_KEY_CHEST_COUNT = "rewardchest_receive_chests_count";
    public const string PLAYERPREFS_KEY_CHEST_SCORE_COUNT = "rewardchest_current_score";
    public const string PLAYERPREFS_KEY_CHEST_SCORE_NEXT_SORT = "rewardchest_score_next_sort";

    public const int PENDING_TIME = 1;//投放间隔（秒、9分钟）
    public const int MAX_CHESTS = 20;//最多积累多少个箱子


    public static int currentRemainSeconds;//当前距离下一次领奖的剩余时间(秒)
    public static int nowRemainChests//当前未领取的箱子
    {
        get { return PlayerPrefs.GetInt(PLAYERPREFS_KEY_CHEST_COUNT); }
        private set
        {
            if (value > MAX_CHESTS)
                value = MAX_CHESTS;
            PlayerPrefs.SetInt(PLAYERPREFS_KEY_CHEST_COUNT, value);
        }
    }
    public static int currentChestScore//当前连续发箱子积分
    {
        get { return PlayerPrefs.GetInt(PLAYERPREFS_KEY_CHEST_SCORE_COUNT); }
        private set
        {
            PlayerPrefs.SetInt(PLAYERPREFS_KEY_CHEST_SCORE_COUNT, value);
        }
    }
    public static cfg.chest.ChestLoop nextChest
    {
        get
        {
            Debug.Log(PlayerPrefs.GetInt(PLAYERPREFS_KEY_CHEST_SCORE_NEXT_SORT));
            Debug.Log(cfg.Tables.tb.ChestLoop.DataList[PlayerPrefs.GetInt(PLAYERPREFS_KEY_CHEST_SCORE_NEXT_SORT)]);
            return cfg.Tables.tb.ChestLoop.DataList[PlayerPrefs.GetInt(PLAYERPREFS_KEY_CHEST_SCORE_NEXT_SORT)];
        }
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

        nowRemainChests = chestCount;


        //每一次登录都按照9分钟开始倒计时
        initRemainSecond = PENDING_TIME;
        currentRemainSeconds = initRemainSecond;
        // SendChestsRewardAction(chestCount, remainTime);
    }

    /// <summary>
    /// 幽默作弊检测
    /// </summary>
    static void CheaterInChestsReward()
    {
        UIManager.Instance.CommonToast("嗯？你是不是偷偷改数据啦（盯）");
    }

    /// <summary>
    /// 领取箱子
    /// </summary>
    public static void GainAndResetChestsRewardAction()
    {
        var greenChest = cfg.Tables.tb.Item.Get(3001);

        if (nowRemainChests <= 0) return;

        // Dictionary<cfg.item.Item, int> items = new()
        // {
        //     { greenChest, nowRemainChests }
        // };

        DataManager.Instance.GainResource(greenChest, nowRemainChests);
        UIManager.Instance.CommonCongra(new List<Rewards>() { { new Rewards() { rewardItem = greenChest, gainNumber = nowRemainChests } } });
        nowRemainChests = 0;
    }

    /// <summary>
    /// 在线期间（尤其是停留在主界面）每隔一段时间就发箱子
    /// </summary>
    public static IEnumerator OnlineSendChests()
    {
        var newWait = new WaitForSecondsRealtime(1);
        while (true)
        {
            //永远在循环判断是否可以
            while (nowRemainChests < MAX_CHESTS)
            {
                //如果没满就循环读时间

                currentRemainSeconds -= 1;
                if (currentRemainSeconds <= 0)
                {
                    nowRemainChests += 1;
                    currentRemainSeconds = PENDING_TIME;
                }

                yield return newWait;
            }
            yield return newWait;
        }
    }


    public static void PlusChestScore(int score)
    {
        currentChestScore += score;
    }

    public static void UseChestScore()
    {
        if (currentChestScore < nextChest.Score)
        {
            UIManager.Instance.CommonToast("积分不足，请继续开箱获取");
            return;
        }

        currentChestScore -= nextChest.Score;
        DataManager.Instance.rewardList.Clear();
        DataManager.Instance.GainResource(nextChest.Reward_Ref, 1);//暂时固定一次领1个，回头我看看指尖无双的
        UIManager.Instance.CommonCongra(DataManager.Instance.rewardList);

        PlayerPrefs.SetInt(PLAYERPREFS_KEY_CHEST_SCORE_NEXT_SORT
        , (PlayerPrefs.GetInt(PLAYERPREFS_KEY_CHEST_SCORE_NEXT_SORT) + 1) % cfg.Tables.tb.ChestLoop.DataList.Count);

        Debug.Log($"下一个宝箱位次：{PlayerPrefs.GetInt(PLAYERPREFS_KEY_CHEST_SCORE_NEXT_SORT)}");
    }



}