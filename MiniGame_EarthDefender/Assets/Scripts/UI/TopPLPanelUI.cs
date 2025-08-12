// TopPLPanelUI.cs
using UnityEngine;
using UnityEngine.UI;

public class TopPLPanelUI : TopPLPanelGroupUI
{
    [Header("UI 组件")]
    public Image imageIcon;
    public Text textCount;
    public Button buttonPanel; // 点击面板可执行操作

    [Header("配置")]
    public int itemId;

    private cfg.item.Item _item
    {
        get
        {
            return cfg.Tables.tb.Item.Get(itemId);
        }
    }

    void Awake()
    {
        RefreshChild();
    }

    protected override void Start()
    {
        // 确保基类初始化
        base.Start();

        // 设置图标
        if (imageIcon != null && _item != null)
        {
            imageIcon.sprite = _item.Image;
        }

        // 添加点击事件
        if (buttonPanel != null)
        {
            buttonPanel.onClick.AddListener(OnPanelClicked);
        }

        // 初始刷新
        RefreshChild();
    }

    // 实现具体刷新逻辑
    public override void RefreshChild()
    {
        // 获取当前道具数量并更新UI
        int count = DataManager.Instance.GetResourceCount(_item.Id);
        textCount.text = count.ToString();

        // 根据数量设置不同颜色
        if (count <= 0)
        {
            textCount.color = Color.red;
        }
        else if (count < 10)
        {
            textCount.color = Color.yellow;
        }
        else
        {
            textCount.color = Color.white;
        }
    }

    // 面板点击事件
    private void OnPanelClicked()
    {
        // 打开道具详情或商店
        Debug.Log($"点击了道具面板: {_item.TextName}");
        UIManager.Instance.OpenItemInfoUI(_item);
    }

    // 添加道具数量变化的监听
    private void OnEnable()
    {
        DataManager.OnItemCountChanged += OnItemCountChanged;
    }

    private void OnDisable()
    {
        DataManager.OnItemCountChanged -= OnItemCountChanged;
    }

    // 当道具数量变化时刷新
    private void OnItemCountChanged(int changedItemId)
    {
        if (changedItemId == itemId)
        {
            RefreshChild();
        }
    }
}