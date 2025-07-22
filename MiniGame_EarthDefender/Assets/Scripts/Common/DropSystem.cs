using System;
using UnityEngine;

public static class DropSystem
{

    public static void LetsDrop(cfg.com.Drop _drop, int _dropCount)
    {
        for (int i = 1; i <= _dropCount; i++)
        {
            DoDrop(_drop);
        }
    }

    /// <summary>
    /// 确定要掉落，概率不在这里管理
    /// </summary>
    /// <param name="_dropDraw"></param>
    static void ConfirmCheckResType(cfg.Beans.Item_Draw _dropDraw)
    {
        var num = _dropDraw.Number;
        
        switch (_dropDraw.ResType)
        {
            case cfg.Enums.Com.ResourceType.ITEM:
                var newItem = cfg.Tables.tb.Item.Get(_dropDraw.Id);
                DataManager.Instance.GainResource(newItem, num);
                break;
            case cfg.Enums.Com.ResourceType.DROP:
                var drop = cfg.Tables.tb.Drop.Get(_dropDraw.Id);
                LetsDrop(drop, _dropDraw.Number);
                break;
            default:
                Debug.LogError("掉落错误");
                break;
        }
    }


    /// <summary>
    /// 根据概率，掉落掉落包/道具
    /// </summary>
    public static void DoDrop(cfg.com.Drop drop)
    {
        switch (drop.DropType)
        {
            //概率，则依次判断，可能掉落多个
            //权重，则一行只有一个
            case cfg.Enums.Drop.DropType.PROP:
                foreach (var a in drop.DropList)
                {
                    if (a.Prop == 10000 && UnityEngine.Random.Range(0, 10000) <= a.Prop)
                    {
                        ConfirmCheckResType(a);
                    }
                }
                break;
            case cfg.Enums.Drop.DropType.WEIGHT:
                int totalWeight = 0;
                foreach (var b in drop.DropList)
                {
                    totalWeight += (int)b.Prop;
                }
                var randomNum = UnityEngine.Random.Range(0, totalWeight);
                foreach (var c in drop.DropList)
                {
                    randomNum -= (int)c.Prop;
                    if (randomNum <= 0)
                    {
                        ConfirmCheckResType(c);
                        return;
                    }
                }
                break;
            default:
                Debug.LogError("掉落包配置错误 " + drop.Id);
                break;
        }
    }
}