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

        var mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        GetComponent<RectTransform>().pivot =
        (mousePos.x >= 0.5f ? Vector2.right * 0.9f : Vector2.right * 0.1f)
        + (mousePos.y >= 0.5f ? Vector2.up * 0.9f : Vector2.up * 0.1f);


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