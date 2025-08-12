using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponDetailInfo : MonoBehaviour
{
    enum ButtonState
    {
        EQUIP,
        UNEQUIP
    }
    private cfg.weapon.Weapon weapon;
    private ButtonState buttonState;
    private List<WeaponAttr> weaponAttrs = new();//属性组


    [Header("=====预制体=====")]
    public WeaponAttr weaponAttrInfoPrefab;//武器属性详情预制体（暂时只展示攻击力、攻速（冷却））
    public GameObject levelUpCardPanelPrefab;//升级详情展示预制体
    public ConsumeUI consumeUIPrefab;//消耗资源的通用预制体

    [Header("=====UI组件=====")]
    public Image imageWeaponIcon;//武器图标
    public Text textWeaponLevel;//武器等级
    public Transform weaponAttrInfoContainer;//武器属性详情容器

    public Text textLvUpCurLevel;//当前等级
    public Text textLvUpNextLevel;//下一级
    public Transform levelUpContainer;//升级内容
    public Transform levelUpConsumesContainer;//升级消耗资源内容

    public Button buttonLvUp;//升级按钮
    public Button buttonEquip;//穿戴/卸下按钮





    public void Initialize(cfg.weapon.Weapon weapon)
    {
        this.weapon = weapon;
        // var slotId = DataManager.Instance.IsWeaponEquipped(weapon.Id);
        // DataManager.Instance.UnequipWeaponBySlot(slotId);



        //升级变化
        // GenerateLevelCards();
        imageWeaponIcon.sprite = weapon.ImageIcon;

        RefreshLevel();

        InitWeaponAttrs();

        GenerateLevelCards();

        RefreshLevelUpConsumes();

        if (DataManager.Instance.IsWeaponEquipped(weapon.Id) != -1)
        {
            buttonState = ButtonState.UNEQUIP;
            buttonEquip.GetComponentInChildren<Text>().text = "卸下";
        }
        else
        {
            buttonState = ButtonState.EQUIP;
            buttonEquip.GetComponentInChildren<Text>().text = "装备";
        }


    }

    /// <summary>
    /// 生成底板上的X级解锁X技能
    /// </summary>
    void GenerateLevelCards()
    {
        foreach (Transform child in levelUpContainer)
        {
            Destroy(child.gameObject);
        }

        var levelConfig = cfg.Tables.tb.WeaponLevel;
        foreach (var level in levelConfig.DataList)
        {
            if (level.Id == weapon.LevelId)
            {
                if (level.LevelupUnlockCard == null)
                {
                    continue;
                }
                Instantiate(levelUpCardPanelPrefab, levelUpContainer)
                .GetComponent<WeaponDetailInfoLvupCardPanelUI>()
                .Initialize(level);
            }
        }

    }


    void RefreshLevelUpConsumes()
    {
        foreach (Transform child in levelUpConsumesContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var kv in weapon.levelUpConsumes)
        {
            Instantiate(consumeUIPrefab, levelUpConsumesContainer).Initialize(kv.Key, kv.Value);
        }
    }


    public void CloseThisWindow(bool _needRefresh)
    {
        if (_needRefresh) UIManager.Instance.weaponsLayer.RefreshEquippedWeapons();
        Destroy(this.gameObject);
    }

    public void TryEquipWeapon(bool isEquip)
    {
        //如果有空位，就自动上空位
        //如果没有空位，就发起换武器的功能
        if (isEquip)
        {
            var fistSlot = DataManager.Instance.GetFirstEmptySlot();
            if (fistSlot != -1)
            {
                DataManager.Instance.EquipWeapon(fistSlot, weapon.Id);
                UIManager.Instance.CommonToast("装备成功");
                CloseThisWindow(true);
            }
            else
            {
                UIManager.Instance.CommonToast("没空位力");
            }
        }
        else
        {
            var slotId = DataManager.Instance.IsWeaponEquipped(weapon.Id);
            if (DataManager.Instance.UnequipWeaponBySlot(slotId))
            {
                CloseThisWindow(true);
            }
        }
    }

    void InitWeaponAttrs()
    {
        weaponAttrs.Clear();
        foreach (Transform child in weaponAttrInfoContainer)
        {
            Destroy(child.gameObject);
        }

        var attr1 = Instantiate(weaponAttrInfoPrefab, weaponAttrInfoContainer);
        attr1.Initialize("攻击加成", (weapon.basicAdditionAtk * 100).ToString("f0") + "%");
        weaponAttrs.Add(attr1);

        var attr2 = Instantiate(weaponAttrInfoPrefab, weaponAttrInfoContainer);
        attr2.Initialize("射速", weapon.RateOfFire.ToString());
        weaponAttrs.Add(attr2);

    }

    /// <summary>
    /// 刷新武器属性
    /// </summary>
    void RefreshWeaponAttrs()
    {
        //攻击加成
        weaponAttrs[0].RefreshNum((weapon.basicAdditionAtk * 100).ToString("f0") + "%");
        //射速（会变吗？不会吧）
        weaponAttrs[1].RefreshNum(weapon.RateOfFire.ToString());
    }


    //刷新武器详情的UI
    private void RefreshAll()
    {

        //更新武器等级
        RefreshLevel();

        RefreshWeaponAttrs();

        GenerateLevelCards();

        RefreshLevelUpConsumes();


    }


    private void RefreshLevel()
    {
        //当前等级
        textWeaponLevel.text = weapon.currentLevel.ToString();

        //升级
        textLvUpCurLevel.text = weapon.currentLevel.ToString();
        textLvUpNextLevel.text = (weapon.currentLevel + 1).ToString();
    }


    #region 按钮组交互
    public void OnEquipButtonClicked()
    {
        switch (buttonState)
        {
            case ButtonState.UNEQUIP:
                TryEquipWeapon(false);
                break;

            case ButtonState.EQUIP:
                TryEquipWeapon(true);
                break;
        }
    }

    public void OnLvUpButtonClicked()
    {
        //暂时是无限的，不用考虑是否满级
        if (DataManager.Instance.TryWeaponLevelUp(weapon))
        {
            RefreshAll();
        }

    }

    #endregion
}