using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum CostType
{
    Coin,
    Diamond,
    Ad
}

public class ShopItem
{
    public int id;
    public cfg.item.Item reward;
    public int rewardCount;
    public CostType costType;
    public int costCount;
    public string name;
    public bool isSoldOut;
    public bool isInfinity;
}




public class ShopCellUI : MonoBehaviour
{
    [Header("引用")]
    public Sprite imgAd;
    [Header("UI元素")]
    public Image iconReward;
    public Image iconCost;
    public Text itemName;
    public Text itemPrice;
    public Text textAmountCount;
    public Button buyButton;
    public GameObject soldOutOverlay;

    private ShopItem itemData;

    private static cfg.item.Item Coin => cfg.Tables.tb.Item.Get(1);
    private static cfg.item.Item Diamond => cfg.Tables.tb.Item.Get(2);

    // 各个品质的随机商品数量
    private Dictionary<int, int[]> listAmounts = new Dictionary<int, int[]>
    {
        { 1, new int[] { 10,15,20 } }, // 等级1概率：绿蓝紫橙，无红
        { 2, new int[] { 10,8,5} },
        { 3, new int[] { 5,3 } },
        { 4, new int[] { 1,2 } }
    };

    // 各个品质的单个碎片消耗
    private Dictionary<int, int> listCost = new Dictionary<int, int>
    {
        { 1, 10 }, // 等级1概率：绿蓝紫橙，无红
        { 2, 25 },
        { 3, 40},
        { 4, 100 }
    };



    public void InitializeAsAdReward()
    {
        // 设置广告奖励商品

        itemData = new ShopItem
        {
            id = -1, // 特殊ID表示广告奖励
            reward = Diamond,
            rewardCount = 100,
            costType = CostType.Ad,
            costCount = 0,
            name = "海量钻石",
            isInfinity = true,
        };

        UpdateUI();
    }

    public void InitializeAsAdCoin()
    {
        // 设置广告金币奖励

        itemData = new ShopItem
        {
            id = -2, // 特殊ID表示广告金币
            reward = Coin,
            rewardCount = 1000,
            costType = CostType.Ad,
            costCount = 0,
            name = "海量金币",
            isInfinity = true,
        };

        UpdateUI();
    }

    public void InitializeAsDiamondToCoin(int diamondCost, int coinAmount)
    {
        // 设置钻石兑换金币

        itemData = new ShopItem
        {
            id = -3, // 特殊ID表示钻石兑换
            reward = Coin,
            rewardCount = coinAmount,
            costType = CostType.Diamond,
            costCount = diamondCost,
            name = "钻石兑换",
            isInfinity = true,
        };

        UpdateUI();
    }

    public void InitializeAsRandomItem()
    {
        // 使用种子生成随机商品
        iconCost.enabled = true;
        int seed = ShopShoppingManager.Instance.GetDiscountShopSeed() + this.GetHashCode();
        UnityEngine.Random.InitState(seed);

        // 获取已解锁的碎片列表
        // 这里需要根据您的游戏实际数据结构来实现
        // List<Fragment> unlockedFragments = DataManager.Instance.GetUnlockedFragments();
        var unlockedFragments = ShopDrawManager.instance.UpdateUnlockWeaponFragmentIdList();

        // 模拟随机选择碎片
        // 实际实现时，您需要替换为真实的碎片数据
        int randomFragmentId = Utility.GetRandomByList(unlockedFragments);
        var randomFragment = cfg.Tables.tb.Item.Get(randomFragmentId);

        int qualityid = (int)randomFragment.Quality;

        int randomAmount = listAmounts[qualityid][UnityEngine.Random.Range(0, listAmounts[qualityid].Length)];
        int randomPrice = randomAmount * listCost[qualityid];

        itemData = new ShopItem
        {
            id = randomFragmentId,
            reward = randomFragment,
            rewardCount = randomAmount,
            costType = CostType.Coin,
            costCount = randomPrice,
            name = randomFragment.TextName,
            isInfinity = false,
        };

        // 检查是否已购买
        if (ShopShoppingManager.Instance.IsItemPurchased(itemData.id))
        {
            itemData.isSoldOut = true;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        //更新数量显示
        if (itemData.rewardCount == 1)
        {
            textAmountCount.enabled = false;
        }
        else
        {
            textAmountCount.text = $"×{itemData.rewardCount}";
        }

        // 更新UI显示
        itemName.text = itemData.name;
        iconReward.sprite = itemData.reward.Image;

        // 设置价格文本
        string priceText = "";
        switch (itemData.costType)
        {
            case CostType.Coin:
                priceText = $"{itemData.costCount}";
                iconCost.sprite = cfg.Tables.tb.Item.Get(1).Image;
                break;
            case CostType.Diamond:
                priceText = $"{itemData.costCount}";
                iconCost.sprite = cfg.Tables.tb.Item.Get(2).Image;
                break;
            case CostType.Ad:
                priceText = "免费";
                iconCost.sprite = imgAd;
                break;
        }
        itemPrice.text = priceText;

        // 设置图标 (需要根据您的资源来设置)
        // itemIcon.sprite = GetItemIcon(itemData.type, itemData.id);

        // 设置购买按钮状态
        buyButton.interactable = !itemData.isSoldOut;
        soldOutOverlay.SetActive(itemData.isSoldOut);

        // 添加购买按钮点击事件
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyButtonClicked);
    }

    void OnBuyButtonClicked()
    {
        // 处理购买逻辑
        if (itemData.isSoldOut) return;

        // 检查是否足够支付
        bool canAfford = false;
        switch (itemData.costType)
        {
            case CostType.Coin:
                canAfford = DataManager.Instance.CheckRes(Coin, itemData.costCount);
                break;
            case CostType.Diamond:
                canAfford = DataManager.Instance.CheckRes(Diamond, itemData.costCount);
                break;
            case CostType.Ad:
                canAfford = true; // 广告总是可以观看
                break;
        }

        if (!canAfford)
        {
            Debug.Log("货币不足");
            return;
        }

        // 扣除货币
        switch (itemData.costType)
        {
            case CostType.Coin:
                DataManager.Instance.CostResource(Coin, itemData.costCount);
                break;
            case CostType.Diamond:
                DataManager.Instance.CostResource(Diamond, itemData.costCount);
                break;
            case CostType.Ad:
                // 播放广告
                if (!AdManager.PlayAd()) return;
                break;
        }

        // 发放奖励
        DataManager.Instance.GainResource(itemData.reward, itemData.rewardCount);
        UIManager.Instance.CommonCongra(new List<Rewards> { new Rewards { rewardItem = itemData.reward, gainNumber = itemData.rewardCount } });

        // 标记为已购买
        if (!itemData.isInfinity) itemData.isSoldOut = true;
        if (itemData.id > 0) // 只记录非特殊商品的购买
        {
            ShopShoppingManager.Instance.MarkItemAsPurchased(itemData.id);
        }


        // 更新UI
        UpdateUI();

        Debug.Log($"购买了 {itemData.name}");
    }
}