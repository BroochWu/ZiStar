using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum CostType
{
    item,
    Ad
}

public class ShopCell
{
    public int shopId;//隶属于哪个商店
    public int slotId;//第几个格子，用于刷新和保存
    public cfg.item.Item reward;//奖励的道具
    public int rewardCount;//奖励数量
    public CostType costType;//是什么类型的消耗
    public cfg.item.Item costItem;//消耗什么资源，广告不用赋值
    public int costCount;//消耗资源的数量，广告不用赋值也可以
    public string name;//格子商品的名字
    public bool isSoldOut;//是否售罄
    public bool isInfinity;//可无限购买（后面可以改成购买次数限制，但目前没有需求）
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

    private ShopCell itemData;

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



    public void InitializeAsAdReward(int _shopId, int _slot)
    {
        // 设置广告奖励商品

        itemData = new ShopCell
        {
            shopId = _shopId,
            reward = Diamond,
            rewardCount = 100,
            costType = CostType.Ad,
            costCount = 0,
            name = "海量钻石",
            isInfinity = true,
            slotId = _slot
        };

        UpdateUI();
    }

    public void InitializeAsAdCoin(int _shopId, int _slot)
    {
        // 设置广告金币奖励

        itemData = new ShopCell
        {
            shopId = _shopId,
            reward = Coin,
            rewardCount = 1000,
            costType = CostType.Ad,
            costCount = 0,
            name = "海量金币",
            isInfinity = true,
            slotId = _slot
        };

        UpdateUI();
    }

    public void InitializeAsDiamondToCoin(int diamondCost, int coinAmount, int _shopId, int _slot)
    {
        // 设置钻石兑换金币

        itemData = new ShopCell
        {
            shopId = _shopId,
            reward = Coin,
            rewardCount = coinAmount,
            costType = CostType.item,
            costItem = Diamond,
            costCount = diamondCost,
            name = "钻石兑换",
            isInfinity = true,
            slotId = _slot
        };

        UpdateUI();
    }

    public void InitializeAsRandomItem(int _shopId, int _slot)
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

        itemData = new ShopCell
        {
            shopId = _shopId,
            slotId = _slot,
            reward = randomFragment,
            rewardCount = randomAmount,
            costType = CostType.item,
            costItem = Coin,
            costCount = randomPrice,
            name = randomFragment.TextName,
            isInfinity = false,
        };

        // 检查是否已购买
        itemData.isSoldOut = ShopShoppingManager.Instance.IsShopSlotPurchased(new KeyValuePair<int, int>(itemData.shopId, itemData.slotId));


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
            case CostType.item:
                priceText = $"{itemData.costCount}";
                iconCost.sprite = itemData.costItem.Image;
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
        // buyButton.interactable = !itemData.isSoldOut;
        soldOutOverlay.SetActive(itemData.isSoldOut);

        // 添加购买按钮点击事件
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyButtonClicked);
    }

    void OnBuyButtonClicked()
    {
        // 处理购买逻辑
        if (itemData.isSoldOut)
        {
            UIManager.Instance.CommonToast("卖完力！等商店刷新吧！");
            return;
        }

        bool successBusiness = false;
        // 扣除货币
        switch (itemData.costType)
        {
            case CostType.item:
                successBusiness = DataManager.Instance.CostResource(itemData.costItem, itemData.costCount);
                break;
            case CostType.Ad:
                // 播放广告
                successBusiness = AdManager.PlayAd();
                break;
        }
        if (!successBusiness) return;

        // 发放奖励
        DataManager.Instance.GainResource(itemData.reward, itemData.rewardCount);
        UIManager.Instance.CommonCongra(new List<Rewards> { new Rewards { rewardItem = itemData.reward, gainNumber = itemData.rewardCount } });

        // 标记为已购买
        if (!itemData.isInfinity)
        {
            ShopShoppingManager.Instance.MarkItemAsPurchased(itemData.shopId, itemData.slotId);
            itemData.isSoldOut = true;
        }


        // 更新UI
        UpdateUI();

        Debug.Log($"购买了 {itemData.name}");
    }
}