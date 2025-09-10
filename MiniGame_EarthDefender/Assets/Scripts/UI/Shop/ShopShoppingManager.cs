using UnityEngine;
using System;
using System.Collections.Generic;

public class ShopShoppingManager : MonoBehaviour
{
    public static ShopShoppingManager Instance { get; private set; }

    // 商店状态
    public bool IsUnlocked { get; private set; }

    // 刷新相关
    private DateTime lastRefreshTime;
    // 特惠商店种子
    private int discountShopSeed;

    // 购买记录
    private HashSet<int> purchasedDiscountItems = new HashSet<int>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }

    public void Initialize()
    {
        LoadShopData();
        InitializeShop();
    }

    void LoadShopData()
    {
        // 从PlayerPrefs加载商店数据
        IsUnlocked = PlayerPrefs.GetInt("Shop_Unlocked", 0) == 1;

        // 加载刷新时间
        string refreshTimeStr = PlayerPrefs.GetString("Shop_LastRefresh", "");
        if (!string.IsNullOrEmpty(refreshTimeStr))
        {
            lastRefreshTime = DateTime.Parse(refreshTimeStr);
        }
        else
        {
            lastRefreshTime = DateTime.Now;
        }

        discountShopSeed = PlayerPrefs.GetInt("Shop_DiscountSeed", (int)lastRefreshTime.Ticks);

        // 加载购买记录
        string purchasedItems = PlayerPrefs.GetString("Shop_PurchasedItems", "");
        if (!string.IsNullOrEmpty(purchasedItems))
        {
            string[] items = purchasedItems.Split(',');
            foreach (string item in items)
            {
                if (int.TryParse(item, out int itemId))
                {
                    purchasedDiscountItems.Add(itemId);
                }
            }
        }
    }

    void SaveShopData()
    {
        // 保存商店数据到PlayerPrefs
        PlayerPrefs.SetInt("Shop_Unlocked", IsUnlocked ? 1 : 0);
        PlayerPrefs.SetString("Shop_LastRefresh", lastRefreshTime.ToString());
        PlayerPrefs.SetInt("Shop_DiscountSeed", discountShopSeed);

        // 保存购买记录
        List<string> purchasedItems = new List<string>();
        foreach (int itemId in purchasedDiscountItems)
        {
            purchasedItems.Add(itemId.ToString());
        }
        PlayerPrefs.SetString("Shop_PurchasedItems", string.Join(",", purchasedItems));

        PlayerPrefs.Save();
    }

    void InitializeShop()
    {
        // 检查是否需要自动刷新
        CheckAutoRefresh();
    }

    void CheckAutoRefresh()
    {
        DateTime now = DateTime.Now;

        // 检查是否到了刷新时间 (0点或12点)
        if ((now.Hour == 0 && lastRefreshTime.Hour != 0) ||
            (now.Hour == 12 && lastRefreshTime.Hour != 12))
        {
            RefreshDiscountShop();
            RedDotManager.Instance.OnShopAutoRefresh();
            lastRefreshTime = now;
            SaveShopData();
        }
    }

    public void RefreshDiscountShop()
    {
        // 使用新的种子重新生成特惠商店
        discountShopSeed = (int)DateTime.Now.Ticks;
        purchasedDiscountItems.Clear();

        SaveShopData();
    }


    public TimeSpan GetTimeToNextRefresh()
    {
        DateTime now = DateTime.Now;
        DateTime nextRefresh;

        // 计算下一个刷新时间 (0点或12点)
        if (now.Hour < 12)
        {
            nextRefresh = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0);
        }
        else
        {
            nextRefresh = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0).AddDays(1);
        }

        return nextRefresh - now;
    }


    public bool IsItemPurchased(int itemId)
    {
        return purchasedDiscountItems.Contains(itemId);
    }

    public void MarkItemAsPurchased(int itemId)
    {
        purchasedDiscountItems.Add(itemId);
        SaveShopData();
    }

    public int GetDiscountShopSeed()
    {
        return discountShopSeed;
    }

    // 解锁商店
    public void UnlockShop()
    {
        IsUnlocked = true;
        SaveShopData();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveShopData();
        }
    }

    void OnApplicationQuit()
    {
        SaveShopData();
    }
}