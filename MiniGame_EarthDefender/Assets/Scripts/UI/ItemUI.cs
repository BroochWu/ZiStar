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
    public Text numberText;
    public Image image;
    public Image imageQualityBg;



    // public readonly cfg.item.Item item;
    // public readonly int number;


    public void Initialize(cfg.item.Item _item, int number)
    {
        numberText.text = number.ToString();
        image.sprite = _item.Image;
        imageQualityBg.color = Utility.SetQualityColor(_item.Quality, false);
    }
}