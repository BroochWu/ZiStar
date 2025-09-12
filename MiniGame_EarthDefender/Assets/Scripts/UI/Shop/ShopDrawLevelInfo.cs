using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopDrawLevelInfo : MonoBehaviour
{
    [Header("=====组件索引=====")]
    public Text textCurrentLevel;
    public Transform contentPanels;
    public List<ShopDrawLevelInfoPanel> shopDrawLevelInfoPanels;
    public Button btnLeftArrow;
    public Button btnRightArrow;
    private int nowLookLv;
    private int minLv => cfg.Tables.tb.DrawLevel.DataList[0].Id;
    private int maxLv => cfg.Tables.tb.DrawLevel.DataList[cfg.Tables.tb.DrawLevel.DataList.Count - 1].Id;

    void Start()
    {
        //点开的时候默认展示当前等级的信息
        RefreshUI(ShopDrawManager.instance.DrawLevel);
        btnLeftArrow.onClick.AddListener(OnLeftButtonClick);
        btnRightArrow.onClick.AddListener(OnRightButtonClick);
    }

    bool RefreshUI(int _lv)
    {

        if (!IsLvConfigExist(_lv, out cfg.shop.drawLevel config))
        {
            return false;
        }


        nowLookLv = _lv;
        RefreshButtons();

        textCurrentLevel.text = _lv.ToString();

        var totalWeight = 0;
        foreach (var item in config.Probs)
        {
            totalWeight += item;
        }



        for (int i = shopDrawLevelInfoPanels.Count - 1; i >= 0; i--)
        {
            var quality = (cfg.Enums.Com.Quality)(i + 1);
            float prob = config.Probs[i] * 1f / totalWeight;
            shopDrawLevelInfoPanels[i].Initialize(quality, prob);
        }
        return true;
    }

    void RefreshButtons()
    {
        btnLeftArrow.gameObject.SetActive(nowLookLv != minLv);
        btnRightArrow.gameObject.SetActive(nowLookLv != maxLv);
    }

    bool IsLvConfigExist(int _lv, out cfg.shop.drawLevel config)
    {
        config = cfg.Tables.tb.DrawLevel.GetOrDefault(_lv);
        return config != null;
    }

    public void OnLeftButtonClick()
    {
        RefreshUI(nowLookLv - 1);
    }

    public void OnRightButtonClick()
    {
        RefreshUI(nowLookLv + 1);
    }
}
