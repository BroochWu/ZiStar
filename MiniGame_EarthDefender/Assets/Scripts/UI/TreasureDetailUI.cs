using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasureDetailUI : MonoBehaviour
{
    public Text textRemainTime;//剩余时间
    public Text textRemainChestCount;//剩余可领箱子
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
        RefreshAllChests();
    }

    void Update()
    {
        var str = TimeSpan.FromSeconds(ChestsRewardSystem.currentRemainSeconds).ToString(@"mm\:ss");
        textRemainTime.text = $"距离下次发放剩余  {str}";
        textRemainChestCount.text = $"当前可领：<size=40> {ChestsRewardSystem.nowRemainChests} /{ChestsRewardSystem.MAX_CHESTS} </size>";
    }

    // public void RefreshUI()
    // {
    // }

    public void GainChest()
    {
        ChestsRewardSystem.GainAndResetChestsRewardAction();
        RefreshAllChests();
    }

    public void UseChest()
    {
        //使用当前正在看的
        nowLookChest.UseChest();
    }

    public void RefreshAllChests()
    {
        foreach (var a in treasureChests)
        {
            a.RefreshUI();
        }
        nowLookChest.RefreshImage();
    }

}