using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class TriCard
{
    private static TriCard _instance;
    public static TriCard Instance => _instance ??= new TriCard();
    public Dictionary<cfg.card.Card, int> listCardsAvailable = new();//可被抽取的卡牌，及剩余抽取次数
    public Dictionary<cfg.card.Card, int> listTempRemove = new();//不放回重抽时临时去除的卡牌+可抽取次数
    public List<cfg.card.Card> listCardsThree = new();//最后抽到的三张牌
    public int totalWeight;
    int totalWeightTempMinus;//暂时扣除的总权重
    public bool canChooseCard;

    const int MAX_DRAW_COUNT = 10; //每张卡牌有10次重抽次数
    int nowReDrawCount = 0;//当前重抽次数，重抽每一张的时候重置


    /// <summary>
    /// (战斗初始化时)初始化三选一
    /// </summary>
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

        //保证抽出3张不一样的牌以供选择
        while (listCardsThree.Count <= 3)
        {
            //抽3张，并且3张卡牌绝对不一样
            var cardResult = DrawOneCard();

            //抽出一张牌后，将临时去除的列表复原（错误的！此时不能复原，否则会出现反复抽取的情况，第一张牌是A，第二张牌是B，第三张牌又抽到A，须在三张牌抽完后再复原临时去除的列表）
            // RebackTempRemove();

            listCardsThree.Add(cardResult);

            //而后，再临时去除已生成的卡牌，保证不会生成相同的卡牌（除了id==1）
            //等等，这里要小心，别出现反复调用的问题
            //这里有问题，lca已经被移除了本项，是找不到的，因此我坐在方法内部了
            // AddToTempRemove(cardResult, listCardsAvailable[cardResult]);

            // totalWeight -= cardResult.Weight;
            // listTempRemove.Add(cardResult, listCardsAvailable[cardResult]);

            // //可获得的卡牌列表中删除抽卡结果
            // listCardsAvailable.Remove(cardResult);
        }

        //全部抽完后把不放回重抽时被移除的列表添加回去（基于不存在第二张牌突然满足了条件的情况）
        RebackTempRemove();

        //可以初始化UI了
        _ = UIManager.Instance.battleLayer.triCardUI.Initialize(listCardsThree);

        // foreach (var a in listTempRemove)
        // {
        //     listCardsAvailable.Add(a.Key, a.Value);
        //     totalWeight += a.Key.Weight;
        // }
        // listTempRemove.Clear();
    }

    cfg.card.Card DrawOneCard()
    {
        //决定一个权重值
        int finalNum = Random.Range(0, totalWeight);


        //这俩是如果重抽的时候要用到的
        //重随时去除的卡和其权重
        //重置每次抽取时的当前重抽次数

        //准备好，接下来可能会触发重抽
        //最多重新抽10次
        nowReDrawCount = 0;


        while (nowReDrawCount <= MAX_DRAW_COUNT)
        {
            foreach (var cardAndRemainCount in listCardsAvailable)
            {
                //当前权重减去卡牌权重
                finalNum -= cardAndRemainCount.Key.Weight;


                //当前权重值如果<=0视为抽到此牌
                if (finalNum <= 0)
                {
                    if (CheckCardCondInBattle(cardAndRemainCount.Key))
                    {
                        //如果条件满足
                        //添加并抽下一个(返回该道具)
                        //抽到以后其他卡牌不会抽到本牌
                        AddToTempRemove(cardAndRemainCount.Key, cardAndRemainCount.Value);
                        Debug.Log("已抽中：" + cardAndRemainCount.Key.TextName);
                        return cardAndRemainCount.Key;
                    }
                    else
                    {
                        //如果条件不满足
                        //重抽
                        //最多重抽10次，否则固定拿id==1的填充

                        //如果重抽达到上限，指连续抽到无法被抽取的卡牌，则取id==1（这个可以重复）
                        Debug.LogWarning($"抽到了{cardAndRemainCount.Key.TextName},但是条件不满足重抽，第{listCardsThree.Count + 1}张卡牌的剩余重抽次数：{MAX_DRAW_COUNT - nowReDrawCount}");

                        nowReDrawCount += 1;

                        //重抽不放回
                        AddToTempRemove(cardAndRemainCount.Key, cardAndRemainCount.Value);
                        // totalWeight -= cardAndRemainCount.Key.Weight;
                        // totalWeightTempMinus += cardAndRemainCount.Key.Weight;
                        // listCardsAvailable.Remove(cardAndRemainCount.Key);
                        // listTempRemove.Add(cardAndRemainCount.Key, cardAndRemainCount.Value);

                        break;
                    }
                }
            }
            if (finalNum > 0)
            {
                Debug.LogWarning("算法有误，finalNum大于0，返回卡牌1");
                break;
            }
        }
        return cfg.Tables.tb.Card.Get(1);
    }

    void AddToTempRemove(cfg.card.Card _tempCard, int _tempDrawCount)
    {
        totalWeight -= _tempCard.Weight;
        totalWeightTempMinus += _tempCard.Weight;

        listCardsAvailable.Remove(_tempCard);
        listTempRemove.Add(_tempCard, _tempDrawCount);

    }

    void RebackTempRemove()
    {
        totalWeight += totalWeightTempMinus;
        listCardsAvailable.AddRange(listTempRemove);

        totalWeightTempMinus = 0;
        listTempRemove.Clear();
    }


    public void SetCardEffect(cfg.card.Card card, int _slot)
    {
        // UIManager.Instance.CommonToast($"假装生效成功 {card.TextName}");
        if (TriCardEffect.TakeEffect(card))
        {
            MinusCardDrawCount(card);
            // UIManager.Instance.battleLayer.triCardUI.cardSlots[_slot].GetComponentInChildren<Animator>().Play("TriPerCard_Disappear");
        }
        else
        {
            Debug.LogError("生效失败！");
        }

        UIManager.Instance.battleLayer.triCardUI.PlayEndTriAnims(_slot);

        // BattleManager.Instance.EndTri();

    }


    /// <summary>
    /// 把所有3张卡牌都生效
    /// </summary>
    public void SetCardEffectAll()
    {
        canChooseCard = false;

        foreach (var a in UIManager.Instance.battleLayer.triCardUI.cardSlots)
        {
            var card = a.GetComponentInChildren<CardUI>().card;
            if (TriCardEffect.TakeEffect(card)) MinusCardDrawCount(card);
        }
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

        listCardsAvailable.TryGetValue(card, out int newDrawCount);

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