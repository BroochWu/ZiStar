using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TriCardUI : MonoBehaviour
{
    public Transform cardsContainer;
    public List<GameObject> cardSlots;
    public Animator anim;//换一批
    public Button buttonRefresh;//换一批
    public Button buttonGetAll;//全都要


    public async Task Initialize(List<cfg.card.Card> cards)
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

        //Debug.Log(cards[0] + "  \n" + cards[1] + "  \n" + cards[2]);
        cardSlots[0].GetComponentInChildren<CardUI>().Initialize(cards[0]);
        cardSlots[1].GetComponentInChildren<CardUI>().Initialize(cards[1]);
        cardSlots[2].GetComponentInChildren<CardUI>().Initialize(cards[2]);
        // GetComponent<Animator>().Update(0f);
        await Task.Delay(100);
        TriCard.Instance.canChooseCard = true;
        gameObject.SetActive(true);
    }


    public void ButtonRefresh()
    {
        // anim.Play(null);
        anim.Play("Appear", 0, anim.GetFloat("RefreshFrame"));
        //后面可以在换一批的时候把高品质的权重多加点
        TriCard.Instance.GetTriCards();
    }
}