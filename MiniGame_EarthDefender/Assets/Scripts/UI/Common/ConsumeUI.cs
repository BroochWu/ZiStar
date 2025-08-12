using UnityEngine;
using UnityEngine.UI;

public class ConsumeUI : MonoBehaviour
{
    public Image imageIcon;
    public Text textCurAndNeed;//消耗/需要

    public void Initialize(cfg.item.Item _item, int _count)
    {
        imageIcon.sprite = _item.Image;

        var nowHas = DataManager.Instance.GetItemCount(_item);
        string str = $"{_count} /{Utility.BigNumber(nowHas)}";
        textCurAndNeed.text = str;
        textCurAndNeed.color = nowHas >= _count ? Color.green : Color.red;

    }
}