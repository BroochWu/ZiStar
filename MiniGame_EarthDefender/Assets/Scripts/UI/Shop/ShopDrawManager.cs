// ShopDrawManager.cs
using SimpleJSON;
using UnityEngine;
using System.Collections.Generic;
using System;

public class ShopDrawManager
{
    private static ShopDrawManager _instance;
    public static ShopDrawManager instance => _instance ??= new ShopDrawManager();

    // 单抽消耗
    public cfg.Beans.Item_Require RegularDrawPerConsume
    {
        get => cfg.Beans.Item_Require.Create(2, 1);// 假设2是钻石的物品ID
    }

    // 连抽设置
    private const int RDNumMin = 5;
    private const int RDNumMax = 50;
    private const int RDNumStep = 5;

    // 广告抽卡设置
    private const int ADNumMin = 10;
    private const int ADNumMax = 50;

    public bool canRegularDraw
    {
        get => DataManager.Instance.CheckRes(RegularDrawPerConsume.Id_Ref, RegularDrawTotalConsume.Number);
    }

    // 广告抽卡冷却时间（秒）
    private const float ADDrawCooldown = 180f;
    private DateTime lastAdDrawTime = DateTime.MinValue;

    //已解锁武器的碎片ID列表
    private List<int> UnlockWeaponFragmentIdList = new();

    // 抽卡等级设置
    private int[] drawLevelThresholds = { 0, 100, 300, 600, 1000 }; // 各级别所需的抽卡次数
    // 抽卡概率配置（示例，实际应根据需求调整）
    private Dictionary<int, float[]> drawProbabilities = new Dictionary<int, float[]>
    {
        { 1, new float[] { 0.7f, 0.25f, 0.05f } }, // 等级1概率：普通, 稀有, 史诗, 传说
        { 2, new float[] { 0.6f, 0.3f, 0.1f } },
        { 3, new float[] { 0.5f, 0.35f, 0.15f } },
        { 4, new float[] { 0.4f, 0.4f, 0.2f } },
        { 5, new float[] { 0.3f, 0.45f, 0.25f } }
    };

    public int RegularDrawNum
    {
        get
        {
            // 根据当前钻石数量计算最大可连抽次数
            // Debug.Log(RegularDrawPerConsume.Id_Ref + " " + RegularDrawPerConsume.Id + " " + RegularDrawPerConsume.Number);
            int maxPossibleDraws = DataManager.Instance.GetItemCount(RegularDrawPerConsume.Id_Ref) / RegularDrawPerConsume.Number;

            // 限制在5-50之间，且为5的倍数
            int clampedDraws = Mathf.Clamp(maxPossibleDraws, RDNumMin, RDNumMax);
            clampedDraws = clampedDraws - (clampedDraws % RDNumStep);

            return Mathf.Max(clampedDraws, RDNumMin);
        }
    }

    public int AdDrawNum
    {
        get
        {
            // 10+当前已观看广告次数，上限50
            return Mathf.Min(ADNumMin + DataManager.Instance.AdDrawAdCount, ADNumMax);
        }
    }

    public cfg.Beans.Item_Require RegularDrawTotalConsume
    {
        get => cfg.Beans.Item_Require.Create(RegularDrawPerConsume.Id, RegularDrawPerConsume.Number * RegularDrawNum);
    }

    public int DrawLevel
    {
        get
        {
            int totalDraws = DataManager.Instance.TotalDrawCount;
            for (int i = drawLevelThresholds.Length - 1; i >= 0; i--)
            {
                if (totalDraws >= drawLevelThresholds[i])
                    return i + 1;
            }
            return 1;
        }
    }

    public float AdDrawCooldownRemaining//广告抽卡剩余冷却时间
    {
        get
        {
            if (IsAdDrawAvailable()) return 0;
            TimeSpan cooldownRemaining = lastAdDrawTime.AddSeconds(ADDrawCooldown) - DateTime.Now;
            return (float)cooldownRemaining.TotalSeconds;
        }
    }


    public bool TryRegularDraw()
    {
        // 检查钻石是否足够
        if (!canRegularDraw)
            return false;

        // 消耗钻石
        DataManager.Instance.CostResource(RegularDrawPerConsume.Id_Ref, RegularDrawTotalConsume.Number);
        return true;
    }

    public bool TryAdDraw()
    {
        if (!IsAdDrawAvailable())
            return false;

        // 记录广告抽卡时间
        lastAdDrawTime = DateTime.Now;

        // 增加广告抽卡次数（错误的，临时的，应该是成功观看以后）
        DataManager.Instance.AdDrawAdCount++;

        return true;
    }

    public bool IsAdDrawAvailable()
    {
        return (DateTime.Now - lastAdDrawTime).TotalSeconds >= ADDrawCooldown;
    }

    public List<Rewards> DrawCards(int count)
    {
        List<Rewards> results = new();
        int currentLevel = DrawLevel;
        float[] probabilities = drawProbabilities[currentLevel];

        for (int i = 0; i < count; i++)
        {
            // 根据概率随机品质
            cfg.Enums.Com.Quality quality = cfg.Enums.Com.Quality.GREEN;



            float rand = UnityEngine.Random.value;
            float cumulative = 0;
            for (int q = 0; q < probabilities.Length; q++)
            {
                cumulative += probabilities[q];
                if (rand <= cumulative)
                {
                    quality = (cfg.Enums.Com.Quality)(q + 1);
                    break;
                }
            }



            // 根据品质随机选择碎片（这里需要根据实际游戏设计实现）
            var item = GetRandomFragmentByQuality(quality);
            Debug.Log("抽到了：" + item.TextName);

            // 添加碎片到结果列表
            results.Add(new Rewards() { rewardItem = item, gainNumber = 1 });


            // 更新玩家数据
            DataManager.Instance.GainResource(item, 1);
        }

        // 更新总抽卡次数
        DataManager.Instance.TotalDrawCount += count;

        return results;
    }

    /// <summary>
    /// 根据品质返回碎片ID
    /// </summary>
    /// <param name="quality"></param>
    /// <returns></returns>
    private cfg.item.Item GetRandomFragmentByQuality(cfg.Enums.Com.Quality quality)
    {

        UpdateUnlockWeaponFragmentIdList();

        List<cfg.item.Item> matchQualityItems = new();

        foreach (var itemId in UnlockWeaponFragmentIdList)
        {
            var item = cfg.Tables.tb.Item.Get(itemId);
            if (item.Quality == quality)
            {

                matchQualityItems.Add(item);
            }
        }

        if (matchQualityItems.Count > 0)
        {
            return Utility.GetRandomByList(matchQualityItems);

        }
        else if (quality != cfg.Enums.Com.Quality.GREEN)
        {
            //如果没有这个品质就从绿色里随一个
            return GetRandomFragmentByQuality(cfg.Enums.Com.Quality.GREEN);
        }
        else
        {
            return cfg.Tables.tb.Item.Get(2001);
        }


    }


    private void UpdateUnlockWeaponFragmentIdList()
    {

        UnlockWeaponFragmentIdList.Clear();

        foreach (var weapon in cfg.Tables.tb.Weapon.DataList)
        {
            if (weapon.weaponState == cfg.weapon.Weapon.CellState.NORMAL)
            {
                UnlockWeaponFragmentIdList.Add(weapon.Piece);
            }
        }
    }
}