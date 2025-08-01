//专门管理玩家数据类

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public struct Rewards
{
    public cfg.item.Item rewardItem;
    public int gainNumber;
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;
    public const int EQUIP_SLOT_COUNT = 5;
    const string PLAYERPREFS_KEY_LAST_LOAD_TIME = "rewardchest_last_load_time";
    private int[] equippedWeapons = new int[EQUIP_SLOT_COUNT];

    public List<Rewards> rewardList { get; private set; } = new();//奖励列表

    void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }


    void Start()
    {
        GetEquippedWeaponList();
    }


    #region 武器穿戴相关

    public int[] GetEquippedWeaponList()
    {
        // 默认值：所有槽位为-1（未装备）
        equippedWeapons = Enumerable.Repeat(-1, EQUIP_SLOT_COUNT).ToArray();

        if (PlayerPrefs.HasKey("equipped_weapons"))
        {
            string[] weaponStrings = PlayerPrefs.GetString("equipped_weapons").Split(',');

            // 只加载有效数据（最多5个）
            for (int i = 0; i < Mathf.Min(weaponStrings.Length, EQUIP_SLOT_COUNT); i++)
            {
                if (int.TryParse(weaponStrings[i], out int weaponId))
                {
                    equippedWeapons[i] = weaponId;
                }
            }
        }
        return equippedWeapons;
        // equippedWeaponsList = Array.ConvertAll(PlayerPrefs.GetString("equipped_weapons").Split(','), int.Parse).ToList();
    }

    public bool EquipWeapon(int slotIndex, int weaponId)
    {
        // GetEquippedWeaponList();
        // equippedWeaponsList[equippedWeaponsList.FindIndex(which => which == _old)] = _new;
        // SetEquippedWeaponList(equippedWeaponsList);
        if (slotIndex < 0 || slotIndex >= EQUIP_SLOT_COUNT)
        {
            UIManager.Instance.CommonToast($"无效的槽位索引: {slotIndex}");
            return false;
        }

        // 检查是否已装备
        if (IsWeaponEquipped(weaponId) > 0)
        {
            UIManager.Instance.CommonToast($"武器 {weaponId} 已装备在其他槽位");
            return false;
        }

        // 更新装备槽
        equippedWeapons[slotIndex] = weaponId;
        SaveEquippedWeapons();

        Debug.Log($"槽位 {slotIndex} 装备武器: {weaponId}");
        return true;
    }
    /// <summary>
    /// 卸下指定槽位的武器
    /// </summary>
    public bool UnequipWeaponBySlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= EQUIP_SLOT_COUNT)
        {
            Debug.LogError($"无效的槽位索引: {slotIndex}");
            return false;
        }


        if (slotIndex == 0)
        {
            UIManager.Instance.CommonToast($"下士！帝国派发的离子枪无法被卸载！");
            return false;
        }

        if (equippedWeapons[slotIndex] == -1)
        {
            Debug.Log($"槽位 {slotIndex} 未装备武器");
            return false;
        }

        int weaponId = equippedWeapons[slotIndex];
        equippedWeapons[slotIndex] = -1;
        SaveEquippedWeapons();

        Debug.Log($"槽位 {slotIndex} 卸下武器: {weaponId}");
        return true;
    }

    /// <summary>
    /// 检查武器是否已装备,并且返回对应的位置（slot）
    /// </summary>
    public int IsWeaponEquipped(int weaponId)
    {
        // 遍历所有装备槽位
        for (int slotIndex = 0; slotIndex < EQUIP_SLOT_COUNT; slotIndex++)
        {
            // 检查当前槽位是否装备了指定武器
            if (equippedWeapons[slotIndex] == weaponId)
            {
                return slotIndex; // 返回找到的槽位索引
            }
        }

        return -1;
    }


    /// <summary>
    /// 获取第一个空闲槽位
    /// </summary>
    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < EQUIP_SLOT_COUNT; i++)
        {
            if (equippedWeapons[i] == -1)
            {
                return i;
            }
        }
        return -1; // 没有空闲槽位
    }

    /// <summary>
    /// 保存已装备武器列表
    /// </summary>
    private void SaveEquippedWeapons()
    {
        string saveStr = string.Join(",", equippedWeapons);
        PlayerPrefs.SetString("equipped_weapons", saveStr);
        Debug.Log("保存装备武器: " + saveStr);
    }

    #endregion




    /// <summary>
    /// 获取奖励
    /// </summary>
    /// <param name="item"></param>
    /// <param name="count"></param>
    public void GainResource(cfg.item.Item item, int count)
    {
        var nowHas = PlayerPrefs.GetInt($"item_{item.Id}");
        PlayerPrefs.SetInt($"item_{item.Id}", nowHas + count);

        // //加入临时的奖励列表，以便恭喜获得
        // rewardList.Add(new Rewards() { rewardItem = item, gainNumber = count });

        //刷新顶栏
        RefreshTopPLPanel(item.Id);

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
            UIManager.Instance.CommonToast("消耗大于剩余量，资源消耗失败！");
            return false;
        }

        if (wantToCost)
        {
            var newValue = nowHas - count;
            PlayerPrefs.SetInt($"item_{item.Id}", newValue);
            //Debug.Log($"{item.TextName} 剩余数量 {newValue}");
        }

        RefreshTopPLPanel(item.Id);
        return true;
    }


    /// <summary>
    /// 在道具结构体内管理道具使用和掉落
    /// </summary>
    /// <param name="_item"></param>
    /// <param name="_useNum"></param>
    public bool UseItemInItemStruct(cfg.item.Item _item, int _useNum)
    {

        if (_item.UseChange.Count == 0)
        {
            Debug.LogError("并不存在使用参数，无法使用：" + _item.Id);
            return false;
        }

        //记录道具每一次使用的产出
        Dictionary<cfg.item.Item, int> sessionRewards = new();
        //使用道具
        for (int i = 1; i <= _useNum; i++)
        {
            sessionRewards.Clear();
            //扣除道具(获得奖励后再扣除?)
            if (CheckOrCostResource(_item, 1, true))
            {

                foreach (var draw in _item.UseChange)
                {
                    //开始计算所得
                    //如果概率失败就直接返回(Item表里一定是概率)
                    if (draw.Prop != 10000 && UnityEngine.Random.Range(0, 10000) > draw.Prop)
                    {
                        return false;
                    }
                    //否则如果是掉道具，直接发放
                    //如果是掉掉落包，由掉落包系统接手管理
                    switch (draw.ResType)
                    {
                        case cfg.Enums.Com.ResourceType.ITEM:
                            GainResource(_item, draw.Number);
                            sessionRewards.Add(_item, draw.Number);
                            break;
                        case cfg.Enums.Com.ResourceType.DROP:
                            var _drop = cfg.Tables.tb.Drop.Get(draw.Id);
                            DropSystem.LetsDrop(_drop, draw.Number, ref sessionRewards);
                            break;
                        default:
                            Debug.LogError("报错了");
                            break;
                    }
                }
            }

            foreach (var reward in sessionRewards)
            {
                rewardList.Add(new Rewards() { rewardItem = reward.Key, gainNumber = reward.Value });
            }

        }

        return true;

    }

    public int GetItemCount(cfg.item.Item item)
    {
        return PlayerPrefs.GetInt($"item_{item.Id}");
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
    public int GetWeaponLevel(int weaponId)
    {
        // return PlayerPrefs.HasKey($"weapon_level_{weaponId}") ?
        // PlayerPrefs.GetInt($"weapon_level_{weaponId}") : 0;
        return PlayerPrefs.GetInt($"weapon_level_{weaponId}");
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

    /// <summary>
    /// 设置最后一次登录时间
    /// </summary>
    public void SetLastLoadTime()
    {
        var current = DateTime.Now;
        var lastStr = PlayerPrefs.GetString(PLAYERPREFS_KEY_LAST_LOAD_TIME);
        Debug.Log(current + "\n" + lastStr);
        if (lastStr != "")
        {
            DateTime.TryParse(lastStr, out var last);
            ChestsRewardSystem.SetChestRewardsOnLoad(last, current);
        }
        PlayerPrefs.SetString(PLAYERPREFS_KEY_LAST_LOAD_TIME, current.ToString());
    }

}