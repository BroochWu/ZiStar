using UnityEngine;
using UnityEngine.UI;

public class TopPLPanelUI : MonoBehaviour
{
    public Text goldCount;
    public Text diamondCount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Refresh();
    }

    // Update is called once per frame
    public void Refresh()
    {
        goldCount.text = DataManager.Instance.GetResourceCount(1).ToString();
        diamondCount.text = DataManager.Instance.GetResourceCount(2).ToString();
    }
}
