using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    private cfg.card.Card card;
    public Text textName;
    public Text textDesc;
    public Image icon;

    public void Initialize(cfg.card.Card card)
    {
        textName.text = card.TextName;
        textDesc.text = card.TextDesc;
        //icon
    }
}