using System.Collections.Generic;
using UnityEngine;
public class TriCard
{
    private static TriCard _instance;
    public static TriCard Instance => _instance ??= new TriCard();
    public Dictionary<cfg.card.Card, int> listCardsAvailable = new();//可被抽取的卡牌，及剩余抽取次数
    public List<cfg.card.Card> listCardsThree = new();//最后抽到的三张牌
    public int totalWeight;

    public void Initialize()
    {
        //获取可抽取的卡牌列表
        listCardsAvailable.Clear();
        totalWeight = 0;
        foreach (var card in cfg.Tables.tb.Card.DataMap)
        {
            //if条件
            listCardsAvailable.Add(card.Value, card.Value.DrawCount);
            totalWeight += card.Value.Weight;
        }
    }

    public void GetTriCards()
    {
        //3选1
        //从可抽取的卡牌列表中，根据权重随机3个
        listCardsThree.Clear();

        var finalNum = Random.Range(0, totalWeight);
        while (listCardsThree.Count <= 3)
        {
            foreach (var cardAndDrawCount in listCardsAvailable)
            {
                finalNum -= cardAndDrawCount.Key.Weight;
                if (finalNum <= 0)
                {
                    listCardsThree.Add(cardAndDrawCount.Key);
                }
            }
        }

        UIManager.Instance.battleLayer.triCardUI.Initialize(listCardsThree);
    }


    public void SetCardEffect(cfg.card.Card card)
    {
        UIManager.Instance.CommonToast($"假装生效成功 {card.TextName}");
        MinusCardDrawCount(card);
        BattleManager.Instance.EndTri();
    }

    /// <summary>
    /// 扣除并计算单局可被抽取的次数，如果<=0就移除
    /// </summary>
    void MinusCardDrawCount(cfg.card.Card card)
    {
        if (card.DrawCount <= -1)
        {
            Debug.Log($"{card.TextName} 剩余可抽取次数为-1，跳过扣除阶段 ");
            return;
        }

        int newDrawCount;
        listCardsAvailable.TryGetValue(card, out newDrawCount);
        newDrawCount -= 1;
        if (newDrawCount <= 0)
        {
            RemoveCard(card);
        }
        else
        {
            listCardsAvailable[card] = newDrawCount;
        }
        Debug.Log($"已扣除，{card.TextName} 剩余可抽取次数 {newDrawCount} ");
    }


    /// <summary>
    /// 本局游戏去掉某张卡
    /// </summary>
    void RemoveCard(cfg.card.Card card)
    {
        listCardsAvailable.Remove(card);
    }

}