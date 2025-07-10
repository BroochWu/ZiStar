using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BottomTabsUI : MonoBehaviour
{
    public Button buttonDevelop;
    public Button buttonCommand;
    public Button buttonWeapons;

    void Awake()
    {
        CheckNow();
    }

    /// <summary>
    /// 检测当前处于哪一个界面并高亮对应的按钮
    /// </summary>
    void CheckNow()
    {
        // 获取当前UI层
        var currentLayer = UIManager.Instance.GetUILayer();

        // 重置所有按钮状态
        ResetAllButtonStates();
        Debug.Log(currentLayer);
        // 根据当前层高亮对应按钮
        switch (currentLayer)
        {
            case UILayer.MAINLAYER:
                SetButtonActive(buttonCommand, true);
                break;
            case UILayer.DEVELOPLAYER:
                SetButtonActive(buttonDevelop, true);
                break;
            case UILayer.WEAPONSLAYER:
                SetButtonActive(buttonWeapons, true);
                break;
        }
    }

    /// <summary>
    /// 重置所有按钮状态为普通
    /// </summary>
    void ResetAllButtonStates()
    {
        SetButtonActive(buttonCommand, false);
        SetButtonActive(buttonDevelop, false);
        SetButtonActive(buttonWeapons, false);
    }


    /// <summary>
    /// 设置按钮为激活状态
    /// </summary>
    void SetButtonActive(Button button, bool _isActive)
    {
        button.GetComponent<Image>().color = _isActive ? Color.blue : Color.white;
    }


    public void SwitchToCommand()
    {
        UIManager.Instance.SwitchLayer(UILayer.MAINLAYER);
        CheckNow();
    }
    public void SwitchToDevelop()
    {
        UIManager.Instance.SwitchLayer(UILayer.DEVELOPLAYER);
        CheckNow();
    }
    public void SwitchToWeapons()
    {
        // UIManager.Instance.CommonToast("暂未开发");
        UIManager.Instance.SwitchLayer(UILayer.WEAPONSLAYER);
        CheckNow();
    }
}
