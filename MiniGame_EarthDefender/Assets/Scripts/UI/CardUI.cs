using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    private cfg.card.Card me;
    private cfg.Enums.Com.Quality quality;
    public Text textName;
    public Text textDesc;
    public Text textSpecial;
    public Image qualityBg;
    public Image icon;

    public void Initialize(cfg.card.Card card)
    {
        me = card;
        textName.text = card.TextName;
        textDesc.text = card.TextDesc;
        quality = card.Quality;


        SetQualityUI();
        textSpecial.gameObject.SetActive(card.UnlockCondsInbattle.Count == 2);
        //icon
    }

    public void ChooseMe()
    {
        if (TriCard.Instance.canChooseCard)
        {
            TriCard.Instance.SetCardEffect(me);
        }
        else
        {
            Debug.LogWarning("canChooseCard:" + TriCard.Instance.canChooseCard);
        }
    }


    /// <summary>
    /// 根据品质改UI
    /// </summary>
    void SetQualityUI()
    {
        // int qualityid = 1;
        // Color qualityColor;

        // var config = cfg.Tables.tb.Color;

        // switch (quality)
        // {
        //     case cfg.Enums.Com.Quality.BLUE:
        //         qualityid = 101;
        //         break;
        //     case cfg.Enums.Com.Quality.PURPLE:
        //         qualityid = 102;
        //         break;
        //     case cfg.Enums.Com.Quality.ORANGE:
        //         qualityid = 103;
        //         break;
        // }
        // ColorUtility.TryParseHtmlString(config.Get(qualityid).ColorLightbg, out qualityColor);
        qualityBg.color = Utility.SetQualityColor(quality,true);

    }
}