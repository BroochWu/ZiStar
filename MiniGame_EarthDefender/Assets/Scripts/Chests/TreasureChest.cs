using UnityEngine;
using UnityEngine.UI;

public class TreasureChest : MonoBehaviour
{
    public int itemId;
    public cfg.item.Item item => cfg.Tables.tb.Item.Get(itemId);
    public int score;

    private Toggle _toggle;

    void Awake()
    {
        TryGetComponent(out _toggle);
        TryGetComponent<PersistentSelectedToggle>(out var persist);
        if (persist != null) persist.normalSprite = Resources.Load<Sprite>($"Images/{GetComponent<TreasureChest>().item.ImagePath}");
    }
    void OnEnable()
    {
        // 订阅Toggle值变化事件
        _toggle?.onValueChanged.AddListener(OnToggleValueChanged);
    }
    void OnDisable()
    {
        // 订阅Toggle值变化事件
        _toggle?.onValueChanged.RemoveListener(OnToggleValueChanged);
    }

    public void RefreshUI()
    {
        GetComponentInChildren<Text>().text = 'x' + DataManager.Instance.GetItemCount(item).ToString();
    }

    public void RefreshImage()
    {
        GetComponent<Image>().sprite = Resources.Load<Sprite>($"Images/{GetComponent<TreasureChest>().item.ImagePath}");
    }

    void OnToggleValueChanged(bool _isOn)
    {
        if (_isOn)
        {
            GetComponentInParent<TreasureDetailUI>().SetNowLookChest(this);
        }
    }


}