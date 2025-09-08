using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TriCardUI : MonoBehaviour
{
    public Transform cardsContainer;
    public List<GameObject> cardSlots;
    public Animator anim;//状态机
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
        for (int i = 0; i <= 2; i++)
        {
            cardSlots[i].GetComponentInChildren<CardUI>().Initialize(cards[i], i);
        }
        // cardSlots[1].GetComponentInChildren<CardUI>().Initialize(cards[1]);
        // cardSlots[2].GetComponentInChildren<CardUI>().Initialize(cards[2]);
        // GetComponent<Animator>().Update(0f);
        await Task.Delay(100);
        TriCard.Instance.canChooseCard = true;
        gameObject.SetActive(true);
    }


    public void PlayEndTriAnims(int _slot)
    {
        anim.Play("Disappear");

        for (int i = 0; i <= 2; i++)
        {
            if (i == _slot)
            {
                Debug.Log(i + "号位的卡牌不播放消失动效");
                cardSlots[i].GetComponentInChildren<Animator>().Play("Chosen");
                continue;
            }
            cardSlots[i].GetComponentInChildren<Animator>().Play("TriPerCard_Disappear");
            Debug.Log("抽中的slot是" + _slot);
        }
        StartCoroutine(CPlayEndTriAnims());
    }

    IEnumerator CPlayEndTriAnims()
    {
        yield return new WaitForSecondsRealtime(1f);
        BattleManager.Instance.EndTri();
    }


    // public void EndTri()
    // {
    //     BattleManager.Instance.EndTri();
    // }



    public void ButtonRefresh()
    {
        // anim.Play(null);
        //播动画
        anim.Play("Appear", 0, anim.GetFloat("RefreshFrame"));

        //后面可以在换一批的时候把高品质的权重多加点
        //总权重重置
        // foreach (var card in TriCard.Instance.listCardsThree)
        // {
        //     TriCard.Instance.totalWeight += card.Weight;
        // }

        // TriCard.Instance.RebackTempRemove();

        TriCard.Instance.GetTriCards();
    }

    public void ButtonGetAll()
    {
        TriCard.Instance.SetCardEffectAll();



        anim.Play("Disappear");

        for (int i = 0; i <= 2; i++)
        {
            cardSlots[i].GetComponentInChildren<Animator>().Play("Chosen");
        }


        StartCoroutine(CPlayEndTriAnims());

    }

}