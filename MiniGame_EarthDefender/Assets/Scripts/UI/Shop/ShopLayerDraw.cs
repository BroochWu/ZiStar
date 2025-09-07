// ShopLayerDraw.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine.EventSystems;

public class ShopLayerDraw : MonoBehaviour
{
    [Header("=====UI装配=====")]
    public Button btnRegularDraw;//常规抽卡
    public Button btnAdDraw;//广告抽卡

    public Text textRegularDrawDesc;
    public Text textAdDrawDesc;
    public Text textRegularDrawConsume;
    public Text textAdDrawConsume;

    public Image imgRegularConsume;

    private void Start()
    {
        // DataManager.Instance.GainResource(cfg.Tables.tb.Item.Get(2), 10000000);

        // 添加按钮事件监听
        btnRegularDraw.onClick.AddListener(ButtonRegularDrawEvent);
        btnAdDraw.onClick.AddListener(ButtonAdDrawEvent);

        // 初始化UI
        RefreshUI();
    }

    public void RefreshUI()
    {
        // 更新UI文本
        UpdateRegularDrawUI();
        UpdateAdDrawUI();
    }

    void UpdateRegularDrawUI()
    {
        textRegularDrawDesc.text = $"{ShopDrawManager.instance.RegularDrawNum} 连抽";
        textRegularDrawConsume.text = $"{ShopDrawManager.instance.RegularDrawTotalConsume.Number}";

        // 根据钻石是否足够设置按钮交互状态
        btnRegularDraw.interactable = ShopDrawManager.instance.canRegularDraw;

        imgRegularConsume.sprite = ShopDrawManager.instance.RegularDrawPerConsume.Id_Ref.Image;
    }

    void UpdateAdDrawUI()
    {
        textAdDrawDesc.text = $"{ShopDrawManager.instance.AdDrawNum} 连抽";

        // 更新广告按钮状态
        btnAdDraw.interactable = ShopDrawManager.instance.IsAdDrawAvailable();
        if (!btnAdDraw.interactable)
        {
            // 显示冷却时间
            textAdDrawConsume.text = $"冷却中({Mathf.CeilToInt(ShopDrawManager.instance.AdDrawCooldownRemaining)}s)";
        }
        else
        {
            textAdDrawConsume.text = "观看广告";
        }
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
        else
        {
            // 钻石不足
            Debug.Log("钻石不足");
            // 这里可以添加钻石不足的提示
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
            LetUsDraw(ShopDrawManager.instance.AdDrawNum, true);
            StartCoroutine(UpdateAdButtonCooldown());
        }
        else
        {
            // 广告抽卡不可用
            Debug.Log("广告抽卡冷却中");
        }
    }

    IEnumerator UpdateAdButtonCooldown()
    {
        while (!ShopDrawManager.instance.IsAdDrawAvailable())
        {
            UpdateAdDrawUI();
            yield return new WaitForSeconds(1f);
        }
        UpdateAdDrawUI();
    }


    /// <summary>
    /// 开始抽卡
    /// </summary>
    public async void LetUsDraw(int drawCount, bool isAdDraw)
    {
        // 显示加载UI
        // UIManager.Instance.ShowLoading(true);
        // GameManager.Instance.DisableEventSystem();

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
            // 可以在这里显示错误提示
        }
        finally
        {
            // 隐藏加载UI
            // UIManager.Instance.ShowLoading(false);
            // GameManager.Instance.EnableEventSystem();
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
}