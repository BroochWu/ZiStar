using UnityEngine;
using UnityEngine.UI;


namespace cfg.item
{
    public partial class Item
    {
        public Sprite Image
        {
            get
            {
                return Resources.Load<Sprite>("Images/" + ImagePath);
            }
        }
    }
}


public class ItemUI : MonoBehaviour
{
    private cfg.item.Item item;

    public ItemInfoUI prefabItemInfo;
    [Header("=====内部组件=====")]
    public Text numberText;
    public Image image;
    public Image imageQualityBg;



    // public readonly cfg.item.Item item;
    // public readonly int number;


    public void Initialize(cfg.item.Item _item, int number)
    {
        item = _item;

        numberText.text = number.ToString();
        image.sprite = _item.Image;
        imageQualityBg.color = Utility.SetQualityColor(_item.Quality, false);
    }

    public void OpenItemInfoUI()
    {
        Instantiate(prefabItemInfo, UIManager.Instance.dynamicContainer)
        .Initialize(item);
    }
}