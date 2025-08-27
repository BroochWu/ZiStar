using UnityEngine;
using UnityEngine.UI;

namespace cfg.card
{
    public partial class Card
    {
        public Sprite imgBg
        {
            get
            {
                return Resources.Load<Sprite>(ImageTricardiconPath);
            }
        }
    }
}



public class CardUI : MonoBehaviour
{
    private cfg.card.Card me;
    private cfg.Enums.Com.Quality quality;
    public Text textName;
    public Text textDesc;
    public Text textSpecial;
    public Text textTargetName;
    public Image qualityBg;
    public Image icon;

    [Header("资源装配")]
    public Sprite spriteTricardBgBlue;
    public Sprite spriteTricardBgPurple;
    public Sprite spriteTricardBgOrange;
    public Sprite spriteTricardBgDoubleBless;

    public void Initialize(cfg.card.Card card)
    {
        me = card;
        textName.text = card.TextName;
        textDesc.text = card.TextDesc;
        textTargetName.text = card.TextTargetName;

        quality = card.Quality;
        SetQualityUI();

        icon.sprite = card.imgBg;

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
        // qualityBg.color = Utility.SetQualityColor(quality, true);

        //根据品质选取背景图
        switch (quality)
        {
            case cfg.Enums.Com.Quality.BLUE:
                qualityBg.sprite = spriteTricardBgBlue;
                break;
            case cfg.Enums.Com.Quality.PURPLE:
                qualityBg.sprite = spriteTricardBgPurple;
                break;
            case cfg.Enums.Com.Quality.ORANGE:
                qualityBg.sprite = spriteTricardBgOrange;
                break;
            default:
                Debug.Log("你没配品质，傻逼");
                qualityBg.sprite = spriteTricardBgBlue;
                break;
        }


    }
}