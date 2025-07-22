using UnityEngine;
using UnityEngine.UI;

public class TreasureChest : MonoBehaviour
{
    public int itemId;
    private cfg.item.Item item => cfg.Tables.tb.Item.Get(itemId);
    public int score;

    public void RefreshUI()
    { 
        GetComponentInChildren<Text>().text = 'x' + DataManager.Instance.GetItemCount(item).ToString();
    }

}