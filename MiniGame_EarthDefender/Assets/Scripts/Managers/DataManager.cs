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
    /// 当玩家要消耗资源时进行检测
    /// </summary>
    /// <param name="item"></param>
    /// <param name="count"></param>
    public bool CostResource(cfg.item.Item item, int count)
    {
        if (!PlayerPrefs.HasKey($"item_{item.Id}")) return false;

        var nowHas = PlayerPrefs.GetInt($"item_{item.Id}");
        if (count > nowHas) return false;

        var newValue = nowHas - count;
        PlayerPrefs.SetInt($"item_{item.Id}", newValue);

        return true;
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

}