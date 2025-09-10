using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BottomTabsUI : MonoBehaviour
{
    public static BottomTabsUI Instance;
    public Button buttonShop;
    public Button buttonDevelop;
    public Button buttonCommand;
    public Button buttonWeapons;

    // 引用商城红点控制器
    public GameObject reddotTabMainShop;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        CheckNow();
    }

    void Start()
    {
        // 在Start中检查红点，确保RedDotManager已经初始化完成
        CheckRedDot();
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
        //Debug.Log(currentLayer);
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
            case UILayer.SHOPLAYER:
                SetButtonActive(buttonShop, true);
                break;
        }
    }

    /// <summary>
    /// 重置所有按钮状态为普通
    /// </summary>
    void ResetAllButtonStates()
    {
        SetButtonActive(buttonShop, false);
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
    public void SwitchToShop()
    {
        // UIManager.Instance.CommonToast("暂未开发");
        UIManager.Instance.SwitchLayer(UILayer.SHOPLAYER);
        CheckNow();
    }

    // 更新主界面商城页签红点显示
    public void SetShopRedDot(bool showRedDot)
    {
        // 实现您的UI更新逻辑
        // 例如：shopTabButton.redDotObject.SetActive(showRedDot);
        Debug.Log($"更新主界面商城页签红点显示: {showRedDot}");
        reddotTabMainShop.SetActive(showRedDot);
    }

    void CheckRedDot()
    {
        // 安全地访问RedDotManager
        if (RedDotManager.Instance != null && RedDotManager.Instance.shopRedDotController != null)
        {
            RedDotManager.Instance.shopRedDotController.Initialize();
        }
        else
        {
            // 如果RedDotManager还没有初始化，延迟一段时间再尝试
            StartCoroutine(DelayedCheckRedDot());
        }
    }

    IEnumerator DelayedCheckRedDot()
    {
        // 等待直到RedDotManager初始化完成
        while (RedDotManager.Instance == null || RedDotManager.Instance.shopRedDotController == null)
        {
            yield return null;
        }

        RedDotManager.Instance.shopRedDotController.Initialize();
    }
}