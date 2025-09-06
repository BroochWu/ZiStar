using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("=====两个页签对应的层=====")]
    public ShopLayerDraw layerDraw;
    public GameObject layerShop;
    [Header("=====底部页签=====")]
    public Toggle tabLayerDraw;
    public Toggle tabLayerShop;
    public ToggleGroup toggleGroup;
    public ScrollRect scrollRect;



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
        scrollRect.verticalNormalizedPosition = 1;
        // tabLayerDraw.group.NotifyToggleOn(tabLayerDraw);
    }

    void ShowTab(GameObject _tab)
    {
        layerDraw.gameObject.SetActive(_tab == layerDraw.gameObject);
        layerShop.SetActive(_tab == layerShop);
    }

}