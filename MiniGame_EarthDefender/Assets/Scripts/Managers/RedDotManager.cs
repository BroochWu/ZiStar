using System;
using System.Collections.Generic;
using UnityEngine;

// 红点系统管理器
public class RedDotManager : MonoBehaviour
{
    private static RedDotManager _instance;
    public static RedDotManager Instance => _instance;
    List<cfg.com.RedDot> _nodes => cfg.Tables.tb.RedDot.DataList;

    // 红点值变化事件
    public event Action<int, int> OnRedDotValueChanged;

    // 各个模块的红点控制器
    public ShopRedDotController shopRedDotController;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 先初始化所有红点控制器
        InitializeAllRedDotControllers();

        // 然后加载保存的红点状态
        LoadRedDotStates();
    }

    // 加载保存的红点状态
    void LoadRedDotStates()
    {
        foreach (var node in _nodes)
        {
            string key = $"RedDot_{node.Id}";
            node.value = PlayerPrefs.GetInt(key, 0);
            OnRedDotValueChanged?.Invoke(node.Id, node.value);
        }
        // 更新所有父节点的值
        UpdateAllParentNodes();
    }

    // 保存红点状态
    void SaveRedDotStates()
    {
        foreach (var node in _nodes)
        {
            string key = $"RedDot_{node.Id}";
            PlayerPrefs.SetInt(key, node.value);
        }
        PlayerPrefs.Save();
    }

    // 更新所有父节点的值
    void UpdateAllParentNodes()
    {
        foreach (var node in _nodes)
        {
            if (node.ChildIds.Count > 0)
            {
                int newValue = 0;
                foreach (int childId in node.ChildIds)
                {
                    var config = cfg.Tables.tb.RedDot.GetOrDefault(childId);
                    if (config != null)
                    {
                        //汇总求出子节点的值，以替代该父节点的值
                        newValue += config.value;
                    }
                }

                if (node.value != newValue)
                {
                    //如果值发生了变化
                    node.value = newValue;
                    OnRedDotValueChanged?.Invoke(node.Id, newValue);
                }
            }
        }
    }

    // 设置红点值
    public void SetValue(int nodeId, int value)
    {
        var config = cfg.Tables.tb.RedDot.GetOrDefault(nodeId);
        if (config == null)
        {
            Debug.LogWarning($"红点节点不存在: {nodeId}");
            return;
        }

        if (config.value != value)
        {
            config.value = value;
            OnRedDotValueChanged?.Invoke(nodeId, value);
            SaveRedDotStates(); // 保存状态
        }

        // 如果这个节点有父节点，需要更新父节点
        UpdateParentNodes(nodeId);
    }

    // 获取红点值
    public int GetValue(int nodeId)
    {
        var config = cfg.Tables.tb.RedDot.GetOrDefault(nodeId);
        if (config == null)
        {
            Debug.LogWarning($"红点节点不存在: {nodeId}");
            return 0;
        }

        return config.value;
    }

    // 更新父节点
    private void UpdateParentNodes(int childNodeId)
    {
        foreach (var node in _nodes)
        {
            if (node.ChildIds.Contains(childNodeId))
            {
                // 计算所有子节点的值总和
                int newValue = 0;
                foreach (int childId in node.ChildIds)
                {
                    newValue += GetValue(childId);
                }

                // 只有当值确实改变时才更新
                if (node.value != newValue)
                {
                    node.value = newValue;
                    OnRedDotValueChanged?.Invoke(node.Id, newValue);
                    SaveRedDotStates(); // 保存状态

                    // 递归更新上级节点
                    UpdateParentNodes(node.Id);
                }
            }
        }
    }

    // 增加红点值
    public void AddValue(int nodeId, int value = 1)
    {
        SetValue(nodeId, GetValue(nodeId) + value);
    }

    // 减少红点值
    public void SubtractValue(int nodeId, int value = 1)
    {
        SetValue(nodeId, Mathf.Max(0, GetValue(nodeId) - value));
    }

    // 重置红点值
    public void ResetValue(int nodeId)
    {
        SetValue(nodeId, 0);
    }

    // 检查是否有红点
    public bool HasRedDot(int nodeId)
    {
        return GetValue(nodeId) > 0;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveRedDotStates();
        }
    }

    void OnApplicationQuit()
    {
        SaveRedDotStates();
    }

    void InitializeAllRedDotControllers()
    {
        // 初始化商城红点控制器
        shopRedDotController ??= gameObject.AddComponent<ShopRedDotController>();
        shopRedDotController.Initialize();
    }

    // 获取商城红点控制器
    public ShopRedDotController GetShopRedDotController()
    {
        return shopRedDotController;
    }

    // 当自动刷新发生时调用
    public void OnShopAutoRefresh()
    {
        if (shopRedDotController != null)
        {
            shopRedDotController.OnAutoRefreshTriggered();
        }
    }
}
