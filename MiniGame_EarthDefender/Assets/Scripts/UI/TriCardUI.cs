using System.Collections.Generic;
using UnityEngine;

public class TriCardUI : MonoBehaviour
{
    public Transform cardsContainer;
    public List<GameObject> cardSlots;


    public void Initialize()
    {
        // gameObject.GetComponent<Animator>().Play("Appear");
        gameObject.SetActive(true);

        if (cardSlots == null)
        {
            cardSlots.Clear();
            foreach (Transform slot in cardsContainer)
            {
                cardSlots.Add(slot.gameObject);
            }
        }

        var card1 = cfg.Tables.tb.Card.Get(1);
        var card2 = cfg.Tables.tb.Card.Get(2);
        var card3 = cfg.Tables.tb.Card.Get(5);
        Debug.Log(card1 + "  " + card2 + "  " + card3);
        cardSlots[0].GetComponentInChildren<CardUI>().Initialize(card1);
        cardSlots[1].GetComponentInChildren<CardUI>().Initialize(card2);
        cardSlots[2].GetComponentInChildren<CardUI>().Initialize(card3);
    }
}