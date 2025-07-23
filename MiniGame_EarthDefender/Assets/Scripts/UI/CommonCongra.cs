using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonCongra : MonoBehaviour
{
    public Transform itemsContainer;

    public void StartAddItemList(Dictionary<cfg.item.Item, int> items)
    {

        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }

        StartCoroutine(AddItemList(items));
    }

    IEnumerator AddItemList(Dictionary<cfg.item.Item, int> items)
    {
        var wait = new WaitForSecondsRealtime(0.1f);
        foreach (var item in items)
        {
            Instantiate(UIManager.Instance.itemObj, itemsContainer)
            .GetComponent<ItemUI>()
            .Initialize(item.Key, item.Value);
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