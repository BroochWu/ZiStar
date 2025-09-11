// ShopLayerDraw.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Linq;

public class ShopLayerDraw : MonoBehaviour
{
    [Header("=====UI装配=====")]
    public Text textCurrentExp;
    public Text textCurrentLv;
    public GameObject objCurrentExpProgress;

    public Button btnRegularDraw;//常规抽卡
    public Button btnAdDraw;//广告抽卡

    public Text textRegularDrawDesc;
    public Text textAdDrawDesc;
    public Text textRegularDrawConsume;
    public Text textAdDrawConsume;

    public Image imgRegularConsume;
    public ShopDrawLevelInfo popShopDrawLevelInfo;

    void OnEnable()
    {
        // DataManager.Instance.GainResource(cfg.Tables.tb.Item.Get(2), 10000000);


        // 初始化UI
        RefreshUI();
    }


    void Start()
    {
        // 添加按钮事件监听
        btnRegularDraw.onClick.AddListener(ButtonRegularDrawEvent);
        btnAdDraw.onClick.AddListener(ButtonAdDrawEvent);

    }

    public void RefreshUI()
    {
        // 更新UI文本
        UpdateRegularDrawUI();
        UpdateAdDrawUI();
        UpdateDrawLevel();
    }

    void UpdateDrawLevel()
    {
        var drawLevel = ShopDrawManager.instance.DrawLevel;
        textCurrentLv.text = "等级 " + drawLevel;

        var thresholds = cfg.Tables.tb.DrawLevel;

        var currentLevelRequire = thresholds.GetOrDefault(drawLevel - 1)?.NextExpRequire ?? 0;
        var N = DataManager.Instance.TotalDrawCount - currentLevelRequire;
        var D = thresholds.Get(drawLevel).NextExpRequire - currentLevelRequire;
        textCurrentExp.text = N + "/" + D;
        objCurrentExpProgress.transform.localScale = new Vector3(N * 1f / D, 1, 1);
    }

    void UpdateRegularDrawUI()
    {
        textRegularDrawDesc.text = $"{ShopDrawManager.instance.RegularDrawNum} 连抽";
        textRegularDrawConsume.text = $"{ShopDrawManager.instance.RegularDrawTotalConsume.Number}";

        // 根据钻石是否能抽设置消耗颜色
        textRegularDrawConsume.color = ShopDrawManager.instance.canRegularDraw ? Color.white : Color.red;

        imgRegularConsume.sprite = ShopDrawManager.instance.RegularDrawPerConsume.Id_Ref.Image;
    }

    void UpdateAdDrawUI()
    {
        textAdDrawDesc.text = $"{ShopDrawManager.instance.AdDrawNum} 连抽";

        // 更新广告按钮状态
        if (!ShopDrawManager.instance.IsAdDrawAvailable())
        {
            UpdateAdDrawCoolDownUI();
        }
        else
        {
            textAdDrawConsume.text = "观看广告";
        }
    }

    /// <summary>
    /// 显示冷却时间
    /// </summary>
    void UpdateAdDrawCoolDownUI()
    {
        textAdDrawConsume.text = $"冷却中({Mathf.CeilToInt(ShopDrawManager.instance.AdDrawCooldownRemaining)}s)";
    }

    /// <summary>
    /// 普通抽卡按钮
    /// </summary>
    public void ButtonRegularDrawEvent()
    {
        if (ShopDrawManager.instance.TryRegularDraw())
        {
            // 抽卡成功
            LetUsDraw(ShopDrawManager.instance.RegularDrawNum, false);
            RefreshUI();
        }
    }

    /// <summary>
    /// 广告抽卡按钮
    /// </summary>
    public void ButtonAdDrawEvent()
    {
        if (ShopDrawManager.instance.TryAdDraw())
        {
            // 广告抽卡成功
            // -1是为了如果广告播放成功，但是抽卡失败了，能将损失最小化，这里是先加了次数，再执行的抽卡
            LetUsDraw(ShopDrawManager.instance.AdDrawNum - 1, true);
            StartCoroutine(UpdateAdButtonCooldown());
        }
        else
        {
            // 广告抽卡不可用
            UIManager.Instance.CommonToast("莫急莫急，冷却中");
        }
    }

    IEnumerator UpdateAdButtonCooldown()
    {
        while (!ShopDrawManager.instance.IsAdDrawAvailable())
        {
            UpdateAdDrawCoolDownUI();
            yield return new WaitForSeconds(1f);
        }
        UpdateAdDrawUI();
    }

    /// <summary>
    /// 开始抽卡
    /// </summary>
    public async void LetUsDraw(int drawCount, bool isAdDraw)
    {
        // // 禁用按钮，防止重复点击
        // btnRegularDraw.interactable = false;
        // btnAdDraw.interactable = false;

        // // 显示加载UI
        // UIManager.Instance.ShowLoading(true);

        try
        {
            // 执行抽卡逻辑
            var rewards = await ShopDrawManager.instance.DrawCards(drawCount);

            // 显示抽卡结果
            Debug.Log($"抽卡完成，获得{rewards.Count}个碎片");

            UIManager.Instance.CommonCongra(rewards);

            // 刷新UI
            RefreshUI();
        }
        catch (Exception ex)
        {
            Debug.LogError($"抽卡过程中发生错误: {ex.Message}");
        }
        finally
        {
            // 隐藏加载UI
            // UIManager.Instance.ShowLoading(false);

            // 重新启用按钮
            // btnRegularDraw.interactable = true;
            // btnAdDraw.interactable = true;
        }
    }
    private void Update()
    {
        if (Time.frameCount % 30 != 0) return;
        // 实时更新钻石抽卡按钮状态
        // bool canDraw = ShopDrawManager.instance.canRegularDraw;
        // if (btnRegularDraw.interactable != canDraw)
        // {
        //     UpdateRegularDrawUI();
        // }

        // 实时更新广告按钮状态（如果需要）
        UpdateAdDrawUI();

    }

    public void OnShowDrawLevelInfoBtnClick()
    {
        UIManager.Instance.OpenInDynamic(popShopDrawLevelInfo);
    }
}