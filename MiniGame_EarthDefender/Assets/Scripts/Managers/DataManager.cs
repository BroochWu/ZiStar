//专门管理玩家数据类

using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }


    /// <summary>
    /// 获取奖励
    /// </summary>
    /// <param name="item"></param>
    /// <param name="count"></param>
    public void GainResource(cfg.item.Item item, int count)
    {
        var nowHas = 0;
        if (PlayerPrefs.HasKey($"item_{item.Id}"))
        {
            nowHas = PlayerPrefs.GetInt($"item_{item.Id}");
        }
        PlayerPrefs.SetInt($"item_{item.Id}", nowHas + count);
    }

    /// <summary>
    /// 当玩家要消耗资源时进行检测
    /// </summary>
    /// <param name="item"></param>
    /// <param name="count"></param>
    public bool CostResource(cfg.item.Item item, int count)
    {
        var nowHas = GetResourceCount(item);
        if (count > nowHas)
        {
            Debug.LogWarning("资源不足！");
            return false;
        }

        var newValue = nowHas - count;
        PlayerPrefs.SetInt($"item_{item.Id}", newValue);
        Debug.Log($"{item.TextName} 剩余数量 {newValue}");
        return true;
    }

    /// <summary>
    /// 获取指定道具id的数量
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int GetResourceCount(cfg.item.Item item)
    {
        if (!PlayerPrefs.HasKey($"item_{item.Id}"))
        {
            Debug.LogWarning($"没有找到道具 {item.Id} ！返回 0 个");
            return 0;
        }
        return PlayerPrefs.GetInt($"item_{item.Id}");
    }

    /// <summary>
    /// 这里只记具体值
    /// </summary>
    /// <returns></returns>
    public int GetPlayerBasicHp()
    {
        var playerConfig = cfg.Tables.tb.PlayerAttrLevel;
        return playerConfig.Get(PlayerPrefs.GetInt("playerData_hp_level")).BasicHp.Value;
    }
    public int GetPlayerBasicAtk()
    {
        var playerConfig = cfg.Tables.tb.PlayerAttrLevel;
        return playerConfig.Get(PlayerPrefs.GetInt("playerData_atk_level")).BasicAtk.Value;
    }

    public bool SetPlayerBasicHpLevel(int newLv, bool isCostEnough)
    {
        if (!isCostEnough)
        {
            // Debug.LogWarning("资源不足，升级失败！");
            UIManager.Instance.CommonToast("资源不足，升级失败！");
            return false;
        }
        if (cfg.Tables.tb.PlayerAttrLevel.GetOrDefault(newLv)?.BasicHp == null)
        {
            // Debug.LogWarning("已满级！");
            UIManager.Instance.CommonToast("已满级！");
            return false;
        }
        PlayerPrefs.SetInt("playerData_hp_level", newLv);
        return true;
    }
    public bool SetPlayerBasicAtkLevel(int newLv, bool isCostEnough)
    {
        if (!isCostEnough)
        {
            UIManager.Instance.CommonToast("资源不足，升级失败！");
            return false;
        }
        if (cfg.Tables.tb.PlayerAttrLevel.GetOrDefault(newLv)?.BasicAtk == null)
        {
            UIManager.Instance.CommonToast("已满级！");
            return false;
        }
        PlayerPrefs.SetInt("playerData_atk_level", newLv);
        return true;
    }

    /// <summary>
    /// 首次加载的内容
    /// </summary>
    public void FirstLoad()
    {
        var playerData = cfg.Tables.tb.PlayerData;
        //初始化数据加载
        foreach (var data in playerData.DataList)
        {
            switch (data.ParamType)
            {
                case cfg.Enums.Com.ParamType.INT:
                    PlayerPrefs.SetInt(data.DataStr, int.Parse(data.ParamValueInit));
                    break;
                case cfg.Enums.Com.ParamType.STRING:
                    PlayerPrefs.SetString(data.DataStr, data.ParamValueInit);
                    break;
            }

        }

    }



}