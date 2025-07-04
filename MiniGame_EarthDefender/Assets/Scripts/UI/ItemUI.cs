using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public Text numberText;
    public Image image;



    // public readonly cfg.item.Item item;
    // public readonly int number;


    public void Initialize(cfg.item.Item _item, int number)
    {
        numberText.text = number.ToString();
        image.sprite = Resources.Load<Sprite>("Images/" + _item.ImagePath);
    }
}