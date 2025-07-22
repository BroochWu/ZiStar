using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasureDetailUI : MonoBehaviour
{
    public Text textRemainTime;//剩余时间
    public Text textRemainChestCount;//剩余可领箱子
    public List<TreasureChest> treasureChests;

    // void Start()
    // {
    //     RefreshUI();
    // }

    public void CloseThisPage()
    {
        Destroy(gameObject);
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

    void RefreshAllChests()
    {
        foreach (var a in treasureChests)
        {
            a.RefreshUI();
        }
    }

}