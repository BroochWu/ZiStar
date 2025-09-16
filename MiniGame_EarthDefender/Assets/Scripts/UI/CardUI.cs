using UnityEngine;
using UnityEngine.UI;



public class CardUI : MonoBehaviour
{
    public cfg.card.Card card;
    private cfg.Enums.Com.Quality quality;
    private bool doubleBless;
    private int slot;

    [Header("组件装配")]
    public Animator anim;
    public Text textName;
    public Text textDesc;
    public Text textSpecial;
    public Text textTargetName;
    public Image qualityBg;
    public Image icon;
    public GameObject VfxNode;

    [Header("资源装配")]
    public Sprite spriteTricardBgBlue;
    public Sprite spriteTricardBgPurple;
    public Sprite spriteTricardBgOrange;
    public Sprite spriteTricardBgDoubleBless;

    public void Initialize(cfg.card.Card card, int _slot)
    {
        // gameObject.SetActive(false);
        // gameObject.SetActive(true);
        this.card = card;

        textTargetName.text = card.TextTargetName;

        if (card.CardType == cfg.Enums.Card.Type.WEAPONUNLOCK)
        {
            var weapon = cfg.Tables.tb.Weapon.Get(card.CardEffects[0].IntParams[0]);
            textName.text = string.Format(card.TextName, weapon.TextName);
            textDesc.text = string.Format(card.TextDesc, weapon.TextName);

        }
        else
        {
            textName.text = card.TextName;
            textDesc.text = card.TextDesc;
        }

        quality = card.Quality;

        icon.sprite = card.imgBg;

        slot = _slot;


        doubleBless = card.UnlockCondsInbattle.Count == 2;
        textSpecial.gameObject.SetActive(doubleBless);
        VfxNode.SetActive(doubleBless);


        if (doubleBless)
        {
            qualityBg.sprite = spriteTricardBgDoubleBless;
        }
        else
        {
            SetQualityUI();
        }
        //icon
    }

    public void SetInitAnim()
    {
        anim.Play("Idle", 0, slot * anim.GetFloat("IdleSpace"));
    }

    public void ChooseMe()
    {
        if (TriCard.Instance.canChooseCard)
        {
            TriCard.Instance.canChooseCard = false;
            TriCard.Instance.SetCardEffect(card, slot);
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