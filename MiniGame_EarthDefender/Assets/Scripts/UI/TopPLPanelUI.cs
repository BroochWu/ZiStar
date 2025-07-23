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
        goldCount.text = Utility.BigNumber(DataManager.Instance.GetResourceCount(1));
        diamondCount.text = Utility.BigNumber(DataManager.Instance.GetResourceCount(2));
    }
}
