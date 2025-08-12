using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public enum UILayer
{
    NULL,
    BATTLELAYER,
    MAINLAYER,
    DEVELOPLAYER,
    WEAPONSLAYER
}


public class UIManager : MonoBehaviour
{

    public static UIManager Instance;
    [Header("=====各个基础层=====")]
    public BattleUI battleLayer;
    public BackBattleUI backbattleLayer;
    public MainUI mainLayer;
    public DevelopUI developLayer;
    public WeaponsUI weaponsLayer;
    [Header("=====更高的玩家层=====")]
    public GameObject bottomTabs;
    public TopPLPanelGroupUI topPLPanel;
    [Header("=====最高的动态层=====")]
    public Transform tipsContainer;//提示UI
    public Transform dynamicContainer;//动态UI


    public GameObject CommonToastObj;
    public GameObject commonCongraGainObj;
    public GameObject itemObj;
    public ItemInfoUI itemInfoObj;


    // 注册的面板组
    private List<TopPLPanelGroupUI> topPanelGroups = new List<TopPLPanelGroupUI>();


    public UILayer uiLayer { get; private set; } = UILayer.NULL;

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
                SetTopPanels(true);
                mainLayer.gameObject.SetActive(true);
                mainLayer.Initialize();
                break;
            case UILayer.DEVELOPLAYER:
                SetTopPanels(true);
                SetBottomTabs(true);
                developLayer.gameObject.SetActive(true);
                developLayer.Initialize();
                break;
            case UILayer.WEAPONSLAYER:
                SetTopPanels(true);
                SetBottomTabs(true);
                weaponsLayer.gameObject.SetActive(true);
                _ = weaponsLayer.Initialize();
                break;
            default:
                uiLayer = UILayer.NULL;
                Debug.LogWarning("没找到UI层");
                break;
        }
        Debug.Log("当前UI：" + uiLayer);

    }


    void HideLayer(UILayer uiLayer)
    {
        switch (uiLayer)
        {
            case UILayer.BATTLELAYER:
                SetBottomTabs(false);
                SetTopPanels(false);
                battleLayer.UnRegister();
                break;
            case UILayer.MAINLAYER:
                mainLayer.gameObject.SetActive(false);
                break;
            case UILayer.DEVELOPLAYER:
                developLayer.gameObject.SetActive(false);
                break;
            case UILayer.WEAPONSLAYER:
                weaponsLayer.gameObject.SetActive(false);
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
    void SetTopPanels(bool _bool)
    {
        topPLPanel.gameObject.SetActive(_bool);
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
        var a = Instantiate(CommonToastObj, tipsContainer).GetComponent<CommonToast>();
        a.Initialize(desc);

    }

    public void CommonCongra(List<Rewards> items)
    {
        if (items.Count == 0)
            return;

        var a = Instantiate(commonCongraGainObj, dynamicContainer).GetComponent<CommonCongra>();
        a.StartAddItemList(items);
    }


    public void OpenInDynamic(GameObject _uiPrefab)
    {
        Instantiate(_uiPrefab, dynamicContainer);
    }



    public void OpenItemInfoUI(cfg.item.Item _item)
    {
        Instantiate(itemInfoObj, dynamicContainer)
        .Initialize(_item);
    }
    #region  顶层相关面板组

    // 注册面板组
    public void RegisterTopPanelGroup(TopPLPanelGroupUI panelGroup)
    {
        if (!topPanelGroups.Contains(panelGroup))
        {
            topPanelGroups.Add(panelGroup);
        }
    }

    // 取消注册面板组
    public void UnregisterTopPanelGroup(TopPLPanelGroupUI panelGroup)
    {
        if (topPanelGroups.Contains(panelGroup))
        {
            topPanelGroups.Remove(panelGroup);
        }
    }

    // 刷新所有顶部面板
    public void RefreshAllTopPanels()
    {
        foreach (var panelGroup in topPanelGroups)
        {
            panelGroup.Refresh();
        }
    }
    #endregion



}
