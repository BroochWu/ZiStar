using UnityEngine;
using UnityEngine.UI;
public class ItemInfoUI : MonoBehaviour
{
    public Text TextName;
    public Text TextDesc;
    public Text NowHas;

    public void Initialize(cfg.item.Item item)
    {
        transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GetComponent<RectTransform>().pivot = Camera.main.ScreenToViewportPoint(Input.mousePosition).x >= 0.5f ? Vector2.right * 0.9f : Vector2.right * 0.1f;


        TextName.text = item.TextName;
        TextDesc.text = item.TextDesc;
        NowHas.text = $"当前持有：{DataManager.Instance.GetItemCount(item)}";
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Destroy(gameObject);
        }
    }
}