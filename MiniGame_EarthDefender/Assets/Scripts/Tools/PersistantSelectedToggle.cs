using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Toggle))]
public class PersistentSelectedToggle : MonoBehaviour, IPointerClickHandler
{
    private Toggle _toggle;
    // private ToggleGroup _group;
    public bool isON { get { return _toggle.isOn; } }

    // 选中的sprite
    public GameObject target;
    public Sprite selectedSprite;
    public Sprite normalSprite;
    public Color selectedColor = Color.white;
    public Color normalColor = Color.white;

    void Awake()
    {
        if (target == null) target = this.gameObject;
        _toggle = GetComponent<Toggle>();
        // _group = _toggle.group;
        // normalSprite = Resources.Load<Sprite>($"Images/{GetComponent<TreasureChest>().item.ImagePath}");

        // 如果组内没有选中的Toggle，强制选中当前
        // if (!_group.AnyTogglesOn())
        // {
        //     _toggle.isOn = true;
        //     Debug.Log("已强制选中当前");
        // }

        // 确保初始状态正确
        UpdateVisualState();
    }

    void OnEnable()
    {

        // 订阅Toggle值变化事件
        _toggle.onValueChanged.AddListener(OnToggleValueChanged);

    }

    void OnDisable()
    {
        _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }

    public void OnToggleValueChanged(bool isOn)
    {
        UpdateVisualState();
    }

    // 手动更新视觉状态
    public void UpdateVisualState()
    {
        if (_toggle == null)
        {
            Debug.LogWarning("toggle is null");
            return;
        }

        // 根据是否选中更新视觉
        if (_toggle.isOn)
        {
            SetSelectedAppearance();
        }
        else
        {
            SetNormalAppearance();
        }
    }

    private void SetSelectedAppearance()
    {
        // 设置选中状态的外观
        if (target.TryGetComponent<Image>(out var image))
        {
            if (selectedSprite != null) image.sprite = selectedSprite;
            if (selectedColor != null) image.color = selectedColor;
            Debug.Log("SetSelectedAppearance " + name);
        }
    }

    private void SetNormalAppearance()
    {
        // 设置普通状态的外观
        if (target.TryGetComponent<Image>(out var image))
        {
            if (normalSprite != null) image.sprite = normalSprite;
            if (normalColor != null) image.color = normalColor;
            Debug.Log("SetNormalAppearance" + name);
        }

    }

    // 处理点击事件
    public void OnPointerClick(PointerEventData eventData)
    {
        // 如果当前未选中，则选中
        if (!_toggle.isOn)
        {
            _toggle.isOn = true;
        }
        // 如果已选中，不允许取消选中
        else
        {
            // 保持选中状态
            _toggle.isOn = true;
        }
    }
}