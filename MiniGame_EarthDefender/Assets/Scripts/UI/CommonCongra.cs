using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonCongra : MonoBehaviour
{
    public Transform itemsContainer;

    public void StartAddItemList(List<Rewards> items)
    {

        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }

        StartCoroutine(AddItemList(items));
    }

    IEnumerator AddItemList(List<Rewards> items)
    {
        var wait = new WaitForSecondsRealtime(0.05f);
        foreach (var item in items)
        {
            if (item.gainNumber == 0) continue;
            Instantiate(UIManager.Instance.itemObj, itemsContainer)
            .GetComponent<ItemUI>()
            .Initialize(item.rewardItem, item.gainNumber);
            yield return wait;
        }
        DataManager.Instance.rewardList.Clear();
        Debug.Log("奖励列表已清除！");
    }

    public void CloseWindow()
    {
        Destroy(gameObject);
    }
}