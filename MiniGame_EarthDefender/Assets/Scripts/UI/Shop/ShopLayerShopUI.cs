using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopLayerShopUI : MonoBehaviour
{
    [Header("=====预制体索引=====")]
    public ShopCellUI prefabShopCell;

    [Header("=====资源装配=====")]
    public Transform DiscountCellsContainer;
    public Transform RegularCellsContainer;

    [Header("=====UI元素=====")]
    public Text refreshTimeText;
    public Button refreshButton;
    public ScrollRect scrollRect;

    private const int DISCOUNT_SHOP_ID = 0;//特惠商店ID
    private const int REGULAR_SHOP_ID = 1;//常驻商店ID

    private List<ShopCellUI> discountCells = new List<ShopCellUI>();
    private List<ShopCellUI> regularCells = new List<ShopCellUI>();

    void OnEnable()
    {
        UpdateShopUI();
        scrollRect.verticalNormalizedPosition = 1;
    }

    void OnDisable()
    {
        Debug.Log("OnDisable");
        RedDotManager.Instance.shopRedDotController.OnLeaveShopTab();
    }

    void Start()
    {
        InitializeShop();
        UpdateShopUI();
    }

    void InitializeShop()
    {
        // 初始化特惠商店
        InitializeDiscountShop();

        // 初始化常规商店
        InitializeRegularShop();

        // 设置刷新按钮
        // refreshButton.onClick.AddListener(OnRefreshButtonClicked);
    }

    void InitializeDiscountShop()
    {

        // 清空容器
        foreach (Transform child in DiscountCellsContainer)
        {
            Destroy(child.gameObject);
        }
        discountCells.Clear();

        // 创建特惠商店格子
        for (int i = 0; i < 8; i++)
        {
            var cellUI = Instantiate(prefabShopCell, DiscountCellsContainer);

            if (cellUI != null)
            {
                discountCells.Add(cellUI);

                // 第一格固定是看广告领钻石
                if (i == 0)
                {
                    cellUI.InitializeAsAdReward(DISCOUNT_SHOP_ID, i);
                }
                else
                {
                    // 其他格子根据已解锁碎片随机生成
                    cellUI.InitializeAsRandomItem(DISCOUNT_SHOP_ID, i);
                }
            }
        }
    }

    void InitializeRegularShop()
    {
        // 清空容器
        foreach (Transform child in RegularCellsContainer)
        {
            Destroy(child.gameObject);
        }
        regularCells.Clear();

        // 创建常规商店格子
        for (int i = 0; i < 3; i++)
        {
            var cellUI = Instantiate(prefabShopCell, RegularCellsContainer);

            if (cellUI != null)
            {
                regularCells.Add(cellUI);

                // 根据索引初始化不同的商品
                switch (i)
                {
                    case 0:
                        cellUI.InitializeAsAdCoin(REGULAR_SHOP_ID, i);
                        break;
                    case 1:
                        cellUI.InitializeAsDiamondToCoin(100, 4000, REGULAR_SHOP_ID, i);
                        break;
                    case 2:
                        cellUI.InitializeAsDiamondToCoin(500, 5000, REGULAR_SHOP_ID, i);
                        break;
                }
            }
        }
    }

    void UpdateShopUI()
    {
        // 更新刷新时间
        UpdateRefreshTime();
    }

    void UpdateRefreshTime()
    {
        System.TimeSpan timeToNextRefresh = ShopShoppingManager.Instance.GetTimeToNextRefresh();

        //如果看着它时间归零，立刻刷新
        if (timeToNextRefresh.Seconds <= 0) ShopShoppingManager.Instance.CheckAutoRefreshInGaming();

        refreshTimeText.text = string.Format("下次刷新: {0:D2}:{1:D2}:{2:D2}",
            timeToNextRefresh.Hours, timeToNextRefresh.Minutes, timeToNextRefresh.Seconds);
    }


    public void OnRefreshButtonClicked()
    {
        if (!AdManager.PlayAd()) return;
        ShopShoppingManager.Instance.RefreshDiscountShop();
        InitializeDiscountShop(); // 重新初始化特惠商店
    }

    void Update()
    {
        // 每秒更新一次刷新时间
        if (Time.time % 1 < Time.deltaTime)
        {
            UpdateRefreshTime();
        }
    }
}