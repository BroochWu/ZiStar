//专门管理玩家数据类

using cfg.Beans;
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
        RefreshTopPLPanel(item.Id);
    }


    /// <summary>
    /// 刷新顶部资源栏
    /// </summary>
    /// <param name="itemId"></param>
    void RefreshTopPLPanel(int itemId)
    {
        if (itemId == 1 || itemId == 2) UIManager.Instance.topPLPanel.Refresh();
    }

    /// <summary>
    /// 当玩家要消耗资源时进行检测
    /// </summary>
    /// <param name="item"></param>
    /// <param name="count"></param>
    public bool CheckOrCostResource(cfg.item.Item item, int count, bool wantToCost)
    {
        var nowHas = GetResourceCount(item);
        if (count > nowHas)
        {
            return false;
        }

        if (wantToCost)
        {
            var newValue = nowHas - count;
            PlayerPrefs.SetInt($"item_{item.Id}", newValue);
            Debug.Log($"{item.TextName} 剩余数量 {newValue}");
        }

        RefreshTopPLPanel(item.Id);
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
    public int GetResourceCount(int itemId)
    {
        if (!PlayerPrefs.HasKey($"item_{itemId}"))
        {
            Debug.LogWarning($"没有找到道具 {itemId} ！返回 0 个");
            return 0;
        }
        return PlayerPrefs.GetInt($"item_{itemId}");
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

    public bool CheckOrSetPLBasicHpLevel(int newLv, bool wantToLevelUp)
    {
        if (cfg.Tables.tb.PlayerAttrLevel.GetOrDefault(newLv)?.BasicHp == null)
        {
            return false;
        }

        if (!wantToLevelUp) return true;

        PlayerPrefs.SetInt("playerData_hp_level", newLv);

        return true;
    }

    public bool CheckOrSetPLBasicAtkLevel(int newLv, bool wantToLevelUp)
    {
        if (cfg.Tables.tb.PlayerAttrLevel.GetOrDefault(newLv)?.BasicAtk == null)
        {
            if (wantToLevelUp) UIManager.Instance.CommonToast("已满级！");
            return false;
        }

        if (!wantToLevelUp) return true;

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