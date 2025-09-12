using System.Collections;
using UnityEngine;

public class ShopRedDotController : MonoBehaviour
{
    // 红点节点ID（根据您的Excel表配置）
    private const int MAIN_SHOP_NODE_ID = 1;      // 主界面商城页签
    private const int SHOP_INNER_NODE_ID = 101;   // 商城内部页签

    // 是否已经初始化
    private bool _isInitialized = false;

    // 初始化方法
    public void Initialize()
    {
        if (!_isInitialized)
        {
            // 注册红点变化事件
            RedDotManager.Instance.OnRedDotValueChanged += OnRedDotValueChanged;
            _isInitialized = true;
        }


        // 初始化红点状态 - 延迟一帧执行，确保UI已经初始化
        UpdateRedDotUI();

    }

    void OnDestroy()
    {
        // 取消注册事件
        if (RedDotManager.Instance != null)
        {
            RedDotManager.Instance.OnRedDotValueChanged -= OnRedDotValueChanged;
        }
    }

    // 当自动刷新被触发时调用
    public void OnAutoRefreshTriggered()
    {
        // 设置商城内部页签红点
        RedDotManager.Instance.SetValue(SHOP_INNER_NODE_ID, 1);
        UpdateRedDotUI();
    }

    // 当玩家离开商品页签时调用
    public void OnLeaveShopTab()
    {
        // 重置商城内部页签红点
        if (RedDotManager.Instance.ResetValue(SHOP_INNER_NODE_ID))
            UpdateRedDotUI();
    }

    // 红点值变化事件处理
    private void OnRedDotValueChanged(int nodeId, int value)
    {
        // 只处理我们关心的节点
        if (nodeId == MAIN_SHOP_NODE_ID || nodeId == SHOP_INNER_NODE_ID)
        {
            UpdateRedDotUI();
        }
    }

    // 更新红点UI显示
    private void UpdateRedDotUI()
    {
        // 获取主界面商城页签红点状态
        bool mainShopHasRedDot = RedDotManager.Instance.HasRedDot(MAIN_SHOP_NODE_ID);

        // 获取商城内部页签红点状态
        bool shopInnerHasRedDot = RedDotManager.Instance.HasRedDot(SHOP_INNER_NODE_ID);
        // Debug.Log($"mainShopHasRedDot:{mainShopHasRedDot},shopInnerHasRedDot:{shopInnerHasRedDot}");

        // 更新UI显示
        UpdateMainShopRedDot(mainShopHasRedDot);
        UpdateShopInnerRedDot(shopInnerHasRedDot);
    }

    // 更新主界面商城页签红点显示
    private void UpdateMainShopRedDot(bool showRedDot)
    {
        // 安全地访问BottomTabsUI实例
        if (BottomTabsUI.Instance != null)
        {
            BottomTabsUI.Instance.SetShopRedDot(showRedDot);
        }
    }


    // 更新商城内部页签红点显示
    private void UpdateShopInnerRedDot(bool showRedDot)
    {
        // 安全地访问ShopUI实例
        if (ShopUI.Instance != null)
        {
            ShopUI.Instance.SetShopRedDot(showRedDot);
        }
    }
}