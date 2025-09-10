using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance;
    [Header("=====两个页签对应的层=====")]
    public ShopLayerDraw layerDraw;
    public GameObject layerShop;
    [Header("=====底部页签=====")]
    public Toggle tabLayerDraw;
    public Toggle tabLayerShop;
    public ToggleGroup toggleGroup;

    public GameObject reddotTabShop;


    void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    void OnEnable()
    {
        // tabLayerDraw.Select();
        // tabLayerDraw.isOn = true;
        Debug.Log("change success");
        tabLayerDraw.isOn = true;
        tabLayerShop.isOn = false;

        tabLayerDraw.GetComponent<PersistentSelectedToggle>().UpdateVisualState();
        tabLayerShop.GetComponent<PersistentSelectedToggle>().UpdateVisualState();

        ShowTab(layerDraw.gameObject);

        //打开shopUI的时候更新红点
        RedDotManager.Instance.shopRedDotController.Initialize();
        // scrollRect.verticalNormalizedPosition = 1;
        // tabLayerDraw.group.NotifyToggleOn(tabLayerDraw);
    }

    void ShowTab(GameObject _tab)
    {
        layerDraw.gameObject.SetActive(_tab == layerDraw.gameObject);
        layerShop.SetActive(_tab == layerShop);
    }


    // 更新主界面商城页签红点显示
    public void SetShopRedDot(bool showRedDot)
    {
        // 实现您的UI更新逻辑
        // 例如：shopTabButton.redDotObject.SetActive(showRedDot);
        Debug.Log($"更新主界面商城页签红点显示: {showRedDot}");
        reddotTabShop.SetActive(showRedDot);
    }

}