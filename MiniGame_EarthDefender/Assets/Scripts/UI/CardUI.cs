using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    private cfg.card.Card me;
    public Text textName;
    public Text textDesc;
    public Text textSpecial;
    public Image icon;

    public void Initialize(cfg.card.Card card)
    {
        me = card;
        textName.text = card.TextName;
        textDesc.text = card.TextDesc;
        textSpecial.gameObject.SetActive(card.UnlockConds.Count == 2);
        //icon

        GetComponent<Button>().onClick.AddListener(ChooseMe);
    }

    void ChooseMe()
    {
        TriCard.Instance.SetCardEffect(me);
    }
}