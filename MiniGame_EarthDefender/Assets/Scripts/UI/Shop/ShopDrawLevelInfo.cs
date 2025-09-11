using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopDrawLevelInfo : MonoBehaviour
{
    [Header("=====组件索引=====")]
    public Text textCurrentLevel;
    public Transform contentPanels;
    public List<ShopDrawLevelInfoPanel> shopDrawLevelInfoPanels;


    void Start()
    {
        var curLv = ShopDrawManager.instance.DrawLevel;
        var curLvConfig = cfg.Tables.tb.DrawLevel.Get(curLv);

        textCurrentLevel.text = curLv.ToString();
        for (int i = 0; i < shopDrawLevelInfoPanels.Count; i++)
        {
            var quality = (cfg.Enums.Com.Quality)(i + 1);
            var prob = curLvConfig.Probs[i];
            Debug.Log(quality);
            Debug.Log(prob);
            Debug.Log(shopDrawLevelInfoPanels[i].name);
            shopDrawLevelInfoPanels[i].Initialize(quality, prob);
        }
    }
}
