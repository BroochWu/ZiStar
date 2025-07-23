using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasureDetailUI : MonoBehaviour
{
    public Text textRemainTime;//剩余时间
    public Text textRemainChestCount;//剩余可领箱子
    public Text textChestScore;//宝箱积分
    public Image ImgNextChest;//下一个积分宝箱的图片
    public GameObject progressRemainTime;//剩余时间进度条
    public List<TreasureChest> treasureChests;
    public TreasureChest nowLookChest;
    // void Start()
    // {
    //     RefreshUI();
    // }

    public void CloseThisPage()
    {
        Destroy(gameObject);
    }

    public void SetNowLookChest(TreasureChest treasureChest)
    {

        nowLookChest.itemId = treasureChest.itemId;
        nowLookChest.score = treasureChest.score;
        nowLookChest.RefreshImage();

        Debug.Log("当前正在选择：" + nowLookChest.itemId);

    }

    void Start()
    {
        RefreshAll();
    }

    void Update()
    {
        if (ChestsRewardSystem.nowRemainChests >= ChestsRewardSystem.MAX_CHESTS)
        {
            textRemainTime.text = "已满！请领取奖励！";
            progressRemainTime.transform.localScale = Vector3.one;
        }
        else
        {
            var str = TimeSpan.FromSeconds(ChestsRewardSystem.currentRemainSeconds);
            textRemainTime.text = $"距离下次发放剩余  {str:mm\\:ss}";
            progressRemainTime.transform.localScale =
             new Vector3((ChestsRewardSystem.PENDING_TIME - ChestsRewardSystem.currentRemainSeconds) * 1f / ChestsRewardSystem.PENDING_TIME, 1, 1);
        }
        textRemainChestCount.text = $"当前可领：<size=40> {ChestsRewardSystem.nowRemainChests} /{ChestsRewardSystem.MAX_CHESTS} </size>";
    }

    // public void RefreshUI()
    // {
    // }

    public void GainChest()
    {
        ChestsRewardSystem.GainAndResetChestsRewardAction();
        RefreshAll();
    }

    public void UseChest()
    {
        //使用当前正在看的
        nowLookChest.UseChest();
    }

    public void RefreshAll()
    {
        foreach (var a in treasureChests)
        {
            a.RefreshUI();
        }
        nowLookChest.RefreshImage();
        RefreshChestScore();
    }

    public void RefreshChestScore()
    {
        var current = ChestsRewardSystem.currentChestScore;
        var next = ChestsRewardSystem.nextChest.Score;
        var colorStr = cfg.Tables.tb.Color.Get(current < next ? 3 : 1).ColorDarkbg;
        var numerator = $"<color={colorStr}>{current}</color>";
        textChestScore.text = $"{numerator} / {next}";
        ImgNextChest.sprite = ChestsRewardSystem.nextChest.Reward_Ref.Image;
    }

    public void GetChestLoop()
    {
        ChestsRewardSystem.UseChestScore();
        RefreshAll();
    }

}