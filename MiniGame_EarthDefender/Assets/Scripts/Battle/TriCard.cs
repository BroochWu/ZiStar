using System.Collections.Generic;
using UnityEngine;
public class TriCard
{
    private static TriCard _instance;
    public static TriCard Instance => _instance ??= new TriCard();
    public Dictionary<cfg.card.Card, int> listCardsAvailable = new();//可被抽取的卡牌，及剩余抽取次数
    public Dictionary<cfg.card.Card, int> listTempRemove = new();//不放回重抽时临时去除的列表
    public List<cfg.card.Card> listCardsThree = new();//最后抽到的三张牌
    public int totalWeight;
    public bool canChooseCard;

    const int MAX_DRAW_COUNT = 10; //每张卡牌有10次重抽次数
    int nowReDrawCount = 0;//当前重抽次数，重抽每一张的时候重置

    public void InitializeBeforeBattle()
    {
        //获取可抽取的卡牌列表
        listCardsAvailable.Clear();
        totalWeight = 0;
        foreach (var card in cfg.Tables.tb.Card.DataList)
        {
            //if条件
            if (!CheckCardBeforeBattle(card))
                continue;

            listCardsAvailable.Add(card, card.DrawCount);
            totalWeight += card.Weight;
        }
    }

    public void GetTriCards()
    {
        //3选1
        //从可抽取的卡牌列表中，根据权重随机3个


        //重置3张卡牌、临时去除的重抽列表
        listCardsThree.Clear();
        listTempRemove.Clear();

        while (listCardsThree.Count <= 3)
        {
            //抽3张，并且3张卡牌绝对不一样
            var cardResult = DrawOneCard();

            listCardsThree.Add(cardResult);
            totalWeight -= cardResult.Weight;
            listTempRemove.Add(cardResult, listCardsAvailable[cardResult]);
            listCardsAvailable.Remove(cardResult);
        }

        _ = UIManager.Instance.battleLayer.triCardUI.Initialize(listCardsThree);
        //事后把不放回重抽时被移除的列表添加回去（基于不存在第二张牌突然满足了条件的情况）
        foreach (var a in listTempRemove)
        {
            listCardsAvailable.Add(a.Key, a.Value);
            totalWeight += a.Key.Weight;
        }
        listTempRemove.Clear();
    }

    cfg.card.Card DrawOneCard()
    {
        int finalNum = Random.Range(0, totalWeight);
        //这俩是如果重抽的时候要用到的
        cfg.card.Card a = null;
        int b = 0;

        //重置每次抽取时的当前重抽次数
        nowReDrawCount = 0;

        while (nowReDrawCount <= MAX_DRAW_COUNT)
        {
            foreach (var cardAndDrawCount in listCardsAvailable)
            {
                finalNum -= cardAndDrawCount.Key.Weight;
                if (finalNum <= 0)
                {
                    if (CheckCardCondInBattle(cardAndDrawCount.Key))
                    {
                        //如果条件满足就添加并抽下一个(返回该道具)
                        return cardAndDrawCount.Key;
                    }
                    else
                    {
                        //如果条件不满足就重抽
                        //重抽不放回
                        //最多重抽10次，否则固定拿id==1的填充

                        nowReDrawCount += 1;
                        Debug.LogWarning($"抽到了{cardAndDrawCount.Key.TextName},但是条件不满足重抽，第{listCardsThree.Count + 1}张卡牌的剩余重抽次数：{MAX_DRAW_COUNT - nowReDrawCount}");
                        a = cardAndDrawCount.Key;
                        b = cardAndDrawCount.Value;
                        break;
                    }
                }
            }
            listCardsAvailable.Remove(a);
            totalWeight -= a.Weight;
            listTempRemove.Add(a, b);

        }
        return cfg.Tables.tb.Card.Get(1);
    }




    public void SetCardEffect(cfg.card.Card card)
    {
        // UIManager.Instance.CommonToast($"假装生效成功 {card.TextName}");
        if (!TriCardEffect.TakeEffect(card))
        {
            Debug.LogError("生效失败！");
        }
        else
        {
            MinusCardDrawCount(card);
        }

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
        totalWeight -= card.Weight;
    }

    /// <summary>
    /// 战斗中检测卡牌是否可用
    /// </summary>
    bool CheckCardCondInBattle(cfg.card.Card card)
    {
        if (card.UnlockCondsInbattle.Count == 0) return true;

        if (!Utility.CondListCheck(card.UnlockCondsInbattle)) return false;

        return true;
    }

    /// <summary>
    /// 战斗前检测卡牌是否可用
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    bool CheckCardBeforeBattle(cfg.card.Card card)
    {
        if (card.UnlockCondsBfbattle.Count == 0) return true;
        //后面可能配全局表里，现在就算了
        foreach (var cond in card.UnlockCondsBfbattle)
        {
            switch (cond.CondType)
            {
                case cfg.Enums.Com.CondType.NULL:
                    break;
                case cfg.Enums.Com.CondType.WEAPONLEVEL:
                    if (DataManager.Instance.GetWeaponLevel(cond.IntParams[0]) >= cond.IntParams[1])
                    {
                        break;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    Debug.LogError($"配错表了，卡牌{card.Id}战斗前的加载条件配了{cond.CondType}");
                    return false;
            }
        }
        return true;
    }


}