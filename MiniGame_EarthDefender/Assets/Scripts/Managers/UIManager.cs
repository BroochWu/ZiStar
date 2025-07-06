using System;
using Unity.Burst.Intrinsics;
using UnityEngine;

public enum UILayer
{
    NULL,
    BATTLELAYER,
    MAINLAYER,
    DEVELOPLAYER,
}


public class UIManager : MonoBehaviour
{

    public static UIManager Instance;
    public BattleUI battleLayer;
    public MainUI mainLayer;
    public DevelopUI developLayer;
    public GameObject bottomTabs;
    public Transform dynamicContainer;//动态UI
    public GameObject CommonToastObj;
    private UILayer uiLayer = UILayer.NULL;

    void Awake()
    {
        if (Instance != null) return;
        Instance = this;


        ShowLayer(UILayer.NULL);

    }


    /// <summary>
    /// 显示哪一层
    /// </summary>
    /// <param name="uiLayer">如果为Null，则隐藏所有</param>
    void ShowLayer(UILayer uiLayer)
    {

        SetUILayer(uiLayer);
        switch (uiLayer)
        {
            case UILayer.NULL:
                //如果要ShowNull，则代表要隐藏所有Layer
                foreach (UILayer i in Enum.GetValues(typeof(UILayer)))
                {
                    if (i == UILayer.NULL) continue;
                    HideLayer(i);
                }
                break;

            case UILayer.BATTLELAYER:
                battleLayer.Initialize();
                break;
            case UILayer.MAINLAYER:
                SetBottomTabs(true);
                mainLayer.gameObject.SetActive(true);
                mainLayer.Initialize();
                break;
            case UILayer.DEVELOPLAYER:
                SetBottomTabs(true);
                developLayer.gameObject.SetActive(true);
                developLayer.Initialize();
                break;
            default:
                uiLayer = UILayer.NULL;
                Debug.LogWarning("没找到UI层");
                break;
        }

    }


    void HideLayer(UILayer uiLayer)
    {
        switch (uiLayer)
        {
            case UILayer.BATTLELAYER:
                SetBottomTabs(false);
                battleLayer.UnRegister();
                break;
            case UILayer.MAINLAYER:
                mainLayer.gameObject.SetActive(false);
                break;
            case UILayer.DEVELOPLAYER:
                developLayer.gameObject.SetActive(false);
                break;
            default:
                Debug.LogWarning($"没找到 {uiLayer} 层");
                break;
        }
    }


    /// <summary>
    /// 切换，关闭当前层，隐藏上一层
    /// </summary>
    /// <param name="uiLayer"></param>
    public void SwitchLayer(UILayer uiLayer)
    {
        if (uiLayer == this.uiLayer)
        {
            Debug.LogWarning("已在此层了");
            return;
        }
        //隐藏当前层，并打开下一层
        if (this.uiLayer != UILayer.NULL)
            HideLayer(this.uiLayer);
        ShowLayer(uiLayer);


    }

    /// <summary>
    /// 设置是否显示底部页签
    /// </summary>
    /// <param name="_bool"></param>
    void SetBottomTabs(bool _bool)
    {
        bottomTabs.SetActive(_bool);
    }


    public UILayer GetUILayer()
    {
        return uiLayer;
    }

    void SetUILayer(UILayer uiLayer)
    {
        this.uiLayer = uiLayer;
    }

    public void CommonToast(string desc)
    {
        var a = Instantiate(CommonToastObj, dynamicContainer).GetComponent<CommonToast>();
        a.Initialize(desc);

    }

}
