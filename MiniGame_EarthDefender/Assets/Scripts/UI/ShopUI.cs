using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("=====两个页签对应的层=====")]
    public GameObject layerDraw;
    public GameObject layerShop;
    [Header("=====底部页签=====")]
    public Toggle tabLayerDraw;
    public Toggle tabLayerShop;
    public ScrollRect scrollRect;



    void OnEnable()
    {
        // tabLayerDraw.Select();
        // tabLayerDraw.isOn = true;
        Debug.Log("change success");
        tabLayerDraw.isOn = true;
        ShowTab(layerDraw);
        scrollRect.verticalNormalizedPosition = 1;
    }

    void ShowTab(GameObject _tab)
    {
        layerDraw.SetActive(_tab == layerDraw);
        layerShop.SetActive(_tab == layerShop);
    }

}