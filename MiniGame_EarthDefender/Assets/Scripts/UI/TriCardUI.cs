using System.Collections.Generic;
using UnityEngine;

public class TriCardUI : MonoBehaviour
{
    public Transform cardsContainer;
    public List<GameObject> cardSlots;


    public void Initialize(List<cfg.card.Card> cards)
    {
        // gameObject.GetComponent<Animator>().Play("Appear");

        if (cardSlots == null)
        {
            cardSlots.Clear();
            foreach (Transform slot in cardsContainer)
            {
                cardSlots.Add(slot.gameObject);
            }
        }

        Debug.Log(cards[0] + "  " + cards[1] + "  " + cards[2]);
        cardSlots[0].GetComponentInChildren<CardUI>().Initialize(cards[0]);
        cardSlots[1].GetComponentInChildren<CardUI>().Initialize(cards[1]);
        cardSlots[2].GetComponentInChildren<CardUI>().Initialize(cards[2]);
        // await Task.Delay(100);
        GetComponent<Animator>().Update(0f);
        gameObject.SetActive(true);
    }
}