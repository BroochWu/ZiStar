using System;
using UnityEngine;

public enum UILayer
{
    NULL,
    BATTLELAYER,
    MAINLAYER,
}


public class UIManager : MonoBehaviour
{

    public static UIManager Instance;
    public BattleUI battleLayer;
    public MainUI mainLayer;
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
                battleLayer.gameObject.SetActive(true);
                battleLayer.Initialize();
                break;
            case UILayer.MAINLAYER:
                mainLayer.gameObject.SetActive(true);
                mainLayer.Initialize();
                break;
            default:
                uiLayer = UILayer.NULL;
                Debug.LogWarning("没找到UI层");
                break;
        }

        this.uiLayer = uiLayer;
    }


    void HideLayer(UILayer uiLayer)
    {
        switch (uiLayer)
        {
            case UILayer.BATTLELAYER:
                battleLayer.gameObject.SetActive(false);
                break;
            case UILayer.MAINLAYER:
                mainLayer.gameObject.SetActive(false);
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
        //隐藏当前层，并打开下一层
        if (this.uiLayer != UILayer.NULL) HideLayer(this.uiLayer);
        ShowLayer(uiLayer);


    }
}
