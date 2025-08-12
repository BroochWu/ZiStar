// TopPLPanelGroupUI.cs
using UnityEngine;

public class TopPLPanelGroupUI : MonoBehaviour
{
    protected virtual void Start()
    {
        // 注册到UI管理器
        UIManager.Instance.RegisterTopPanelGroup(this);
        Refresh();
    }

    // 刷新所有子面板
    public void Refresh()
    {
        RefreshChild();
    }

    // 虚方法，由子类实现具体刷新逻辑
    public virtual void RefreshChild()
    {
        // 基类不做具体实现
    }

    // void OnEnable()
    // {
    //     Refresh();
    // }
}