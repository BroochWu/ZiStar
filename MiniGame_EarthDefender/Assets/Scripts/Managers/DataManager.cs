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
    const string PLAYERPREFS_KEY_MAX_DUNGEON = "dungeon_passed_level";
    const string PLAYERPREFS_KEY_AD_DRAW_COUNT = "ad_draw_count";
    const string PLAYERPREFS_KEY_TOTAL_DRAW_COUNT = "total_draw_count";



    // 道具数量变化事件
    public static event Action<int> OnItemCountChanged;


    private int[] preequippedWeapons = new int[EQUIP_SLOT_COUNT];
    public int dungeonPassedLevel //已通关的最大等级
    {
        get
        {
            return PlayerPrefs.GetInt(PLAYERPREFS_KEY_MAX_DUNGEON);
        }
        set
        {
            //这是最大等级，仅当更大时才记录
            if (value > PlayerPrefs.GetInt(PLAYERPREFS_KEY_MAX_DUNGEON))
                PlayerPrefs.SetInt(PLAYERPREFS_KEY_MAX_DUNGEON, value);
        }
    }
    public int nextUnlockDungeonPassedWeapon //下一个用已通关关卡解锁的主线武器
    {
        get; private set;
    }

    public int nowAtkLevel
    {
        get
        {
            return PlayerPrefs.GetInt("playerData_atk_level");
        }
        set
        {
            PlayerPrefs.SetInt("playerData_atk_level", value);
        }
    }
    public int nowHpLevel
    {
        get
        {
            return PlayerPrefs.GetInt("playerData_hp_level");
        }
        set
        {
            PlayerPrefs.SetInt("playerData_hp_level", value);
        }
    }

    public int TotalWeaponsGlobalAtkBonus//武器总额外全局攻击加成
    {
        get
        {
            int a = 0;
            foreach (var weapon in cfg.Tables.tb.Weapon.DataList)
            {
                a += weapon.curGlobalBonusNum;
            }
            return a;
        }
    }

    public int AdDrawAdCount//广告抽卡的看广告次数
    {
        get
        {
            return PlayerPrefs.GetInt(PLAYERPREFS_KEY_AD_DRAW_COUNT, 0);
        }
        set
        {
            PlayerPrefs.SetInt(PLAYERPREFS_KEY_AD_DRAW_COUNT, value);
        }
    }

    public int TotalDrawCount
    {
        get => PlayerPrefs.GetInt(PLAYERPREFS_KEY_TOTAL_DRAW_COUNT, 0);
        set => PlayerPrefs.SetInt(PLAYERPREFS_KEY_TOTAL_DRAW_COUNT, value);
    }



    public List<Rewards> rewardList { get; private set; } = new();//奖励列表










    void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }


    void Start()
    {
        GetPreequippedWeaponList();
    }


    #region 武器预穿戴相关

    /// <summary>
    /// 获取局外预装备
    /// </summary>
    /// <returns></returns>
    public int[] GetPreequippedWeaponList()
    {
        // 默认值：所有槽位为-1（未装备）
        preequippedWeapons = Enumerable.Repeat(-1, EQUIP_SLOT_COUNT).ToArray();

        if (PlayerPrefs.HasKey("preequipped_weapons"))
        {
            string[] weaponStrings = PlayerPrefs.GetString("preequipped_weapons").Split(',');

            // 只加载有效数据（最多5个）
            for (int i = 0; i < Mathf.Min(weaponStrings.Length, EQUIP_SLOT_COUNT); i++)
            {
                if (int.TryParse(weaponStrings[i], out int weaponId))
                {
                    preequippedWeapons[i] = weaponId;
                }
            }
        }
        return preequippedWeapons;
        // equippedWeaponsList = Array.ConvertAll(PlayerPrefs.GetString("equipped_weapons").Split(','), int.Parse).ToList();
    }

    /// <summary>
    /// 预装备
    /// </summary>
    /// <param name="slotIndex">格子索引</param>
    /// <param name="weaponId">武器id</param>
    /// <returns></returns>
    public bool PreequipWeapon(int slotIndex, int weaponId)
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
        if (IsWeaponPreequipped(weaponId) > 0)
        {
            UIManager.Instance.CommonToast($"武器 {weaponId} 已装备在其他槽位");
            return false;
        }

        // 更新装备槽
        preequippedWeapons[slotIndex] = weaponId;
        SavePreequippedWeapons();

        Debug.Log($"槽位 {slotIndex} 装备武器: {weaponId}");
        return true;
    }
    /// <summary>
    /// 卸下指定槽位的预武器
    /// </summary>
    public bool UnequipPreweaponBySlot(int slotIndex)
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

        if (preequippedWeapons[slotIndex] == -1)
        {
            Debug.Log($"槽位 {slotIndex} 未装备武器");
            return false;
        }

        int weaponId = preequippedWeapons[slotIndex];
        preequippedWeapons[slotIndex] = -1;
        SavePreequippedWeapons();

        Debug.Log($"槽位 {slotIndex} 卸下武器: {weaponId}");
        return true;
    }

    /// <summary>
    /// 检查预装备中的武器是否已装备,并且返回对应的位置（slot）-1就是没穿
    /// </summary>
    public int IsWeaponPreequipped(int weaponId)
    {
        // 遍历所有装备槽位
        for (int slotIndex = 0; slotIndex < EQUIP_SLOT_COUNT; slotIndex++)
        {
            // 检查当前槽位是否装备了指定武器
            if (preequippedWeapons[slotIndex] == weaponId)
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
            if (preequippedWeapons[i] == -1)
            {
                return i;
            }
        }
        return -1; // 没有空闲槽位
    }

    /// <summary>
    /// 保存已装备武器列表
    /// </summary>
    private void SavePreequippedWeapons()
    {
        string saveStr = string.Join(",", preequippedWeapons);
        PlayerPrefs.SetString("preequipped_weapons", saveStr);
        Debug.Log("保存装备武器: " + saveStr);
    }

    #endregion



    #region 资源的获取和消耗
    /// <summary>
    /// 获取奖励
    /// </summary>
    /// <param name="item"></param>
    /// <param name="count"></param>
    public void GainResource(cfg.item.Item item, int count)
    {
        var nowHas = PlayerPrefs.GetInt($"item_{item.Id}",0);
        PlayerPrefs.SetInt($"item_{item.Id}", nowHas + count);

        //刷新顶栏
        // RefreshTopPLPanel(item.Id);
        OnItemCountChanged?.Invoke(item.Id);

    }



    public bool CheckRes(cfg.item.Item _item, int count)
    {
        var nowHas = GetResourceCount(_item);

        return nowHas >= count;

    }

    /// <summary>
    /// 当玩家要消耗资源时进行检测
    /// </summary>
    /// <param name="item"></param>
    /// <param name="count"></param>
    public bool CostResource(cfg.item.Item item, int count)
    {
        var nowHas = GetResourceCount(item);

        if (CheckRes(item, count))
        {
            var newValue = nowHas - count;
            PlayerPrefs.SetInt($"item_{item.Id}", newValue);
            OnItemCountChanged?.Invoke(item.Id);
            return true;
        }
        //Debug.Log($"{item.TextName} 剩余数量 {newValue}");
        else
        {
            UIManager.Instance.CommonToast("数量不足，使用失败！");
            return false;
        }

    }

    public bool CostResource(Dictionary<cfg.item.Item, int> _kv)
    {
        //组，如果任意一个不满足则返回false并提示
        foreach (var k in _kv)
        {
            var nowHas = GetResourceCount(k.Key);
            if (!CheckRes(k.Key, k.Value))
            {
                UIManager.Instance.CommonToast("数量不足，使用失败！");
                return false;
            }
        }

        //如果都满足则进入扣除环节
        foreach (var k in _kv)
        {
            var nowHas = GetResourceCount(k.Key);

            var newValue = nowHas - k.Value;
            PlayerPrefs.SetInt($"item_{k.Key.Id}", newValue);
            OnItemCountChanged?.Invoke(k.Key.Id);
        }
        return true;

    }


    public bool CostResource(List<cfg.Beans.Item_Require> _requires)
    {
        //组，如果任意一个不满足则返回false并提示
        foreach (var k in _requires)
        {
            var nowHas = GetResourceCount(k.Id_Ref);
            if (!CheckRes(k.Id_Ref, k.Number))
            {
                UIManager.Instance.CommonToast("数量不足，使用失败！");
                return false;
            }
        }

        //如果都满足则进入扣除环节
        foreach (var k in _requires)
        {
            var nowHas = GetResourceCount(k.Id);

            var newValue = nowHas - k.Number;
            PlayerPrefs.SetInt($"item_{k.Id}", newValue);
            OnItemCountChanged?.Invoke(k.Id);
        }

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
            if (CostResource(_item, 1))
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
        return PlayerPrefs.GetInt($"item_{item.Id}", 0);
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

    #endregion

    #region 养成相关


    #region 地球养成
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


    public bool PLBasicAtkLevelUp(int newLv)
    {
        //是否满级
        var levelConfig = cfg.Tables.tb.PlayerAttrLevel.GetOrDefault(newLv)?.BasicAtk;
        if (levelConfig == null)
        {
            UIManager.Instance.CommonToast("已满级！");
            return false;
        }

        if (CostResource(new List<cfg.Beans.Item_Require> { levelConfig.ItemRequire }))
        {
            nowAtkLevel = newLv;
            return true;
        }
        else
        {
            return false;
        }

    }

    public bool PLBasicHpLevelUp(int newLv)
    {
        //是否满级
        var levelConfig = cfg.Tables.tb.PlayerAttrLevel.GetOrDefault(newLv)?.BasicHp;
        if (levelConfig == null)
        {
            UIManager.Instance.CommonToast("已满级！");
            return false;
        }

        if (CostResource(new List<cfg.Beans.Item_Require> { levelConfig.ItemRequire }))
        {
            nowHpLevel = newLv;
            return true;
        }
        else
        {
            return false;
        }

    }






    #endregion

    #region 武器养成

    public bool TryWeaponLevelUp(cfg.weapon.Weapon _weapon)
    {
        //首先判断资源是否满足
        //如果满足就成功升级并消耗资源（需要注意等都满足了再执行扣除）
        //否则就返回失败
        // foreach (var consumeKV in _weapon.levelUpConsumes)
        // {
        //     if (!CheckRes(consumeKV.Key, consumeKV.Value))
        //     {
        //         return false;
        //     }
        // }

        // //资源都满足，开始扣除
        // foreach (var consumeKV in _weapon.levelUpConsumes)
        // {
        //     CostResource(consumeKV.Key, consumeKV.Value);
        // }

        //生效效果
        if (CostResource(_weapon.levelUpConsumes))
        {
            WeaponLevelUp(_weapon);
            return true;
        }
        else
        {
            return false;
        }


    }
    private void WeaponLevelUp(cfg.weapon.Weapon weapon)
    {
        weapon.currentLevel += 1;
    }

    #endregion

    #endregion


    #region  其他数据加载

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



    public void RefreshNextUnlockDungeonPassedWeapon()
    {

        foreach (var a in cfg.Tables.tb.Weapon.DataList)
        {
            var unlock = a.UnlockCond;
            if (unlock.CondType == cfg.Enums.Com.CondType.DUNGEON_PASS)
            {
                if (dungeonPassedLevel >= unlock.IntParams[0])
                {
                    continue;
                }
                else
                {
                    nextUnlockDungeonPassedWeapon = a.Id;
                    return;
                }
            }
        }
        //如果全都已解锁，就是不存在下一个已解锁的武器
        nextUnlockDungeonPassedWeapon = -1;
    }

    #endregion
}