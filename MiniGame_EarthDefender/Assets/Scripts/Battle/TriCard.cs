using System.Collections.Generic;
using UnityEngine;
public static class TriCard
{
    public static List<cfg.card.Card> listCardsAvailable = new();
    public static void Initialize()
    {
        //获取可抽取的卡牌列表
        listCardsAvailable.Clear();
        foreach (var card in cfg.Tables.tb.Card.DataMap)
        {
            //if条件
            listCardsAvailable.Add(card.Value);
        }
    }

    public static void GetTriCards()
    {
        Time.timeScale = 0;
        UIManager.Instance.battleLayer.triCardUI.Initialize();
    }
}