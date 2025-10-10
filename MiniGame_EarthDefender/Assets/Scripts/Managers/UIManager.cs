using System;
using System.Collections.Generic;
using UnityEngine;

public enum UILayer
{
    NULL = 0,
    BATTLELAYER = -1,
    SHOPLAYER = 1,
    DEVELOPLAYER = 2,
    MAINLAYER = 3,
    WEAPONSLAYER = 4,
}


public class UIManager : MonoBehaviour
{

    public static UIManager Instance;
    [Header("=====各个基础层=====")]
    public MainHall MainHallUI;
    public BattleUI battleLayer;
    public BackBattleUI backbattleLayer;
    public MainUI mainLayer;
    public DevelopUI developLayer;
    public WeaponsUI weaponsLayer;
    public ShopUI shopLayer;
    [Header("=====更高的玩家层=====")]
    public BottomTabsUI bottomTabs;
    public TopPLPanelGroupUI topPLPanel;
    [Header("=====最高的动态层=====")]
    public Transform tipsContainer;//提示UI
    public Transform avgContainer;//剧情UI
    public Transform dynamicContainer;//动态UI


    public GameObject CommonToastObj;
    public GameObject commonCongraGainObj;
    public GameObject itemObj;
    public ItemInfoUI itemInfoObj;

    [Header("=====对话框AVG预制体=====")]
    public AvgDialogueUI avgDialoguePrefab;//对话avg预制体


    // 注册的面板组
    private List<TopPLPanelGroupUI> topPanelGroups = new List<TopPLPanelGroupUI>();


    public UILayer uiLayer { get; private set; } = UILayer.NULL;

    void Awake()
    {
        if (Instance != null) return;
        Instance = this;



    }

    void Start()
    {

        foreach (Transform child in avgContainer)
        {
            DestroyImmediate(child.gameObject);
        }

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
                SetBottomTabs(false);
                SetTopPanels(false);
                battleLayer.Initialize();
                break;
            case UILayer.MAINLAYER:
                SetBottomTabs(true);
                SetTopPanels(true);
                mainLayer.gameObject.SetActive(true);
                mainLayer.Initialize();
                MainHallUI.MoveBg(0);
                break;
            case UILayer.DEVELOPLAYER:
                SetTopPanels(true);
                SetBottomTabs(true);
                developLayer.gameObject.SetActive(true);
                developLayer.Initialize();
                MainHallUI.MoveBg(1);
                break;
            case UILayer.WEAPONSLAYER:
                SetTopPanels(true);
                SetBottomTabs(true);
                weaponsLayer.gameObject.SetActive(true);
                weaponsLayer.Initialize();
                MainHallUI.MoveBg(2);
                break;
            case UILayer.SHOPLAYER:
                SetTopPanels(true);
                SetBottomTabs(true);
                shopLayer.gameObject.SetActive(true);
                MainHallUI.MoveBg(3);
                break;
            default:
                uiLayer = UILayer.NULL;
                Debug.LogWarning("没找到UI层");
                break;
        }

        //判断AVG事件
        AvgManager.Instance?.CheckAndTriggerAvgs(cfg.Enums.Com.TriggerType.UI_STATE);
        Debug.Log("当前UI：" + uiLayer);

    }


    void HideLayer(UILayer uiLayer)
    {
        switch (uiLayer)
        {
            case UILayer.BATTLELAYER:
                SetBottomTabs(false);
                SetTopPanels(false);
                // battleLayer.UnRegister();
                break;
            case UILayer.MAINLAYER:
                //仅主界面内部切换的时候直接隐藏，否则播动效隐藏
                if (GameManager.Instance.gameState == GameManager.GameState.MAINVIEW) mainLayer.gameObject.SetActive(false);
                break;
            case UILayer.DEVELOPLAYER:
                developLayer.gameObject.SetActive(false);
                break;
            case UILayer.WEAPONSLAYER:
                weaponsLayer.gameObject.SetActive(false);
                break;
            case UILayer.SHOPLAYER:
                shopLayer.gameObject.SetActive(false);
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
        {
            HideLayer(this.uiLayer);
        }

        ShowLayer(uiLayer);


    }

    /// <summary>
    /// 设置是否显示底部页签
    /// </summary>
    /// <param name="_bool"></param>
    void SetBottomTabs(bool _bool)
    {
        if (_bool) bottomTabs.gameObject.SetActive(true);
        //这里false由动画决定隐藏时间
        bottomTabs.Show(_bool);
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


    public void OpenInDynamic<T>(T _uiPrefab) where T : MonoBehaviour
    {
        Instantiate(_uiPrefab.gameObject, dynamicContainer);
    }
    public void OpenInDynamic(GameObject uiPrefab)
    {
        Instantiate(uiPrefab, dynamicContainer);
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




    /// <summary>
    /// 显示对话AVG
    /// </summary>
    /// <param name="_avgEvent"></param>
    public void ShowAvgDialogue(cfg.avg.AvgEvent _avgEvent)
    {
        Debug.Log("ShowAvgDialogue");
        //如果存在实例就立刻销毁，不会触发OnDestroy
        if (AvgManager.dialogueAvgInstance != null)
        {
            Destroy(AvgManager.dialogueAvgInstance.gameObject);
        }

        AvgManager.dialogueAvgInstance = Instantiate(avgDialoguePrefab, avgContainer);
        AvgManager.dialogueAvgInstance.Initialize(_avgEvent);
    }

}
