using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class ShopShoppingManager : MonoBehaviour
{
    public static ShopShoppingManager Instance { get; private set; }

    private const string PLAYERPREFS_UNLOCK_SHOP = "unlock_shop";


    // public event Action OnShopUnlockCheck;
    // 商店状态
    public bool IsUnlocked
    {
        get
        {
            // // OnShopUnlockCheck.Invoke();
            var saveUnlock = PlayerPrefs.GetInt(PLAYERPREFS_UNLOCK_SHOP,0);
            bool isUnlock = saveUnlock == 1;
            if (isUnlock)
            {
                //如果已经解锁了，就再也不用判断是否解锁了
                return true;
            }

            var newResult = DataManager.Instance.dungeonPassedLevel >= cfg.Tables.tb.GlobalParam.Get(PLAYERPREFS_UNLOCK_SHOP).IntValue;
            if (isUnlock != newResult)
            {
                //首次解锁
                PlayerPrefs.SetInt(PLAYERPREFS_UNLOCK_SHOP, 1);
                InitializeShop();
            }
            return newResult;
            // return false;
        }
    }
    private Coroutine coroutineCheckingAutoRefreshInGaming;

    // 刷新相关
    private DateTime lastRefreshTime;
    // 特惠商店种子
    private int discountShopSeed;

    // 购买记录(商店ID，格子ID)
    private List<KeyValuePair<int, int>> purchasedDiscountItems = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }
    void Start()
    {
        Initialize();
    }

    void StartCheckingRefreshInGaming()
    {
        if (coroutineCheckingAutoRefreshInGaming == null)
        {
            coroutineCheckingAutoRefreshInGaming = StartCoroutine(CheckingRefreshInGaming());
        }
        else
        {
            Debug.LogWarning("协程已存在");
        }

    }

    IEnumerator CheckingRefreshInGaming()
    {
        while (true)
        {
            if (Time.frameCount % 300 == 0)
            {
                //每300帧判断一次自动刷新
                CheckAutoRefreshInGaming();
            }
            yield return null;
        }
    }

    public void Initialize()
    {

        //加载完了看看解锁没
        if (!IsUnlocked)
        {
            Debug.LogWarning("商店功能未解锁");
            return;
        }

        InitializeShop();
    }


    void SaveShopData()
    {
        // 保存商店数据到PlayerPrefs
        // PlayerPrefs.SetInt("Shop_Unlocked", IsUnlocked ? 1 : 0);
        PlayerPrefs.SetString("Shop_LastRefresh", lastRefreshTime.ToString());
        PlayerPrefs.SetInt("Shop_DiscountSeed", discountShopSeed);

        // 保存购买记录
        List<string> purchasedItems = new List<string>();
        foreach (var item in purchasedDiscountItems)
        {
            purchasedItems.Add(item.Key + "_" + item.Value);
        }
        PlayerPrefs.SetString("Shop_PurchasedSlots", string.Join(",", purchasedItems));

        PlayerPrefs.Save();
    }

    void LoadShopData()
    {
        // 从PlayerPrefs加载商店数据
        // IsUnlocked = PlayerPrefs.GetInt("Shop_Unlocked", 0) == 1;
        // Debug.Log("isShopUnlocked:" + IsUnlocked);

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
        string purchasedItems = PlayerPrefs.GetString("Shop_PurchasedSlots", "");
        if (!string.IsNullOrEmpty(purchasedItems))
        {
            string[] items = purchasedItems.Split(',');
            foreach (string kvpair in items)
            {
                int.TryParse(kvpair.Split('_')[0], out int shopId);
                int.TryParse(kvpair.Split('_')[1], out int slotId);

                purchasedDiscountItems.Add(new KeyValuePair<int, int>(shopId, slotId));
            }
        }
    }


    void InitializeShop()
    {

        // if (!IsUnlocked) return;


        //加载商店数据
        LoadShopData();
        // 检查是否需要自动刷新，复杂判断登录时的关系
        CheckAutoRefreshOnLoading();
        // 启动协程，在线检查是否需要自动刷新，简单判断
        StartCheckingRefreshInGaming();


    }

    public void CheckAutoRefreshInGaming()
    {
        DateTime now = DateTime.Now;

        // 检查是否到了刷新时间 (0点或12点)
        // if (true)
        if ((now.Hour == 0 && lastRefreshTime.Hour != 0) ||
            (now.Hour == 12 && lastRefreshTime.Hour != 12))
        {
            RefreshDiscountShop();
        }
    }

    void CheckAutoRefreshOnLoading()
    {
        Debug.Log("检查自动刷新 - 上次刷新时间: " + lastRefreshTime);
        DateTime now = DateTime.Now;

        // 计算今天和昨天的0点和12点
        DateTime todayMidnight = DateTime.Today;
        DateTime todayNoon = DateTime.Today.AddHours(12);
        DateTime yesterdayMidnight = DateTime.Today.AddDays(-1);
        DateTime yesterdayNoon = DateTime.Today.AddDays(-1).AddHours(12);

        // 检查是否需要执行0点刷新
        bool shouldRefreshMidnight =
            (now >= todayMidnight && lastRefreshTime < todayMidnight) || // 今天0点后且上次刷新在今天0点前
            (now >= yesterdayMidnight && lastRefreshTime < yesterdayMidnight && now.Hour >= 0); // 或昨天0点后且上次刷新在昨天0点前

        // 检查是否需要执行12点刷新
        bool shouldRefreshNoon =
            (now >= todayNoon && lastRefreshTime < todayNoon) || // 今天12点后且上次刷新在今天12点前
            (now >= yesterdayNoon && lastRefreshTime < yesterdayNoon && now.Hour >= 12); // 或昨天12点后且上次刷新在昨天12点前

        // 如果需要刷新
        if (shouldRefreshMidnight || shouldRefreshNoon)
        {
            RefreshDiscountShop();
        }
    }


    public void RefreshDiscountShop()
    {
        // 使用新的种子重新生成特惠商店
        var now = DateTime.Now;
        discountShopSeed = (int)now.Ticks;
        purchasedDiscountItems.Clear();

        RedDotManager.Instance.shopRedDotController.OnAutoRefreshTriggered();//触发红点
        lastRefreshTime = now;

        SaveShopData();
        Debug.Log("checkRefreshsuccess");
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


    public bool IsShopSlotPurchased(KeyValuePair<int, int> _shopSlotIndex)
    {
        return purchasedDiscountItems.Contains(_shopSlotIndex);
    }

    public void MarkItemAsPurchased(int _shopId, int itemId)
    {
        var index = new KeyValuePair<int, int>(_shopId, itemId);
        purchasedDiscountItems.Add(index);
        SaveShopData();
    }

    public int GetDiscountShopSeed()
    {
        return discountShopSeed;
    }

    // // 解锁商店
    // public void UnlockShop()
    // {
    //     IsUnlocked = true;
    //     SaveShopData();
    // }

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