using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WeaponsUI : MonoBehaviour
{
    public List<GameObject> equippedWeaponsSlots;
    public Transform weaponsListContainer;
    public GameObject WeaponPrefab;
    public GameObject WeaponDetailInfoPrefab;
    public Text textTotalGlobalAtkBonus;
    private cfg.Tbweapon.Weapon config;


    public void Initialize()
    {
        config = cfg.Tables.tb.Weapon;

        var colorStr = cfg.Tables.tb.Color.Get(1).ColorLightbg;
        textTotalGlobalAtkBonus.text =
        $"总伤害加成：<color={colorStr}>+"
        + (DataManager.Instance.TotalWeaponsGlobalAtkBonus / 100).ToString()
        + "%</color>";

        // 装配已穿戴武器
        RefreshEquippedWeapons();
        // 更新背包
        StartCoroutine(CInitializeWeaponsBag());
    }

    IEnumerator CInitializeWeaponsBag()
    {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.03f);

        //新排序如下：
        //已解锁>未解锁
        //未解锁按照配表顺序
        //已解锁按照稀有度降序
        var unlockedWeapons = config.DataList.Where(weapon => weapon.weaponState == cfg.weapon.Weapon.CellState.NORMAL)
        .OrderByDescending(weapon => weapon.InitQuality);



        var lockedWeapons = config.DataList.Where(weapon => weapon.weaponState >= cfg.weapon.Weapon.CellState.LOCK)
        .OrderByDescending(weapon => weapon.weaponState)
        .ThenBy(weapon => weapon.Id);

        var sortedWeapons = unlockedWeapons.Concat(lockedWeapons);
        //装备列表

        foreach (Transform child in weaponsListContainer)
        {
            Destroy(child.gameObject);
        }

        //更新下一个要解锁的武器
        DataManager.Instance.RefreshNextUnlockDungeonPassedWeapon();
        foreach (var i in sortedWeapons)
        {
            var weaponCell = Instantiate(WeaponPrefab, weaponsListContainer);
            weaponCell.GetComponent<WeaponCellUI>().Initialize(i);
            yield return wait;
        }
    }



    public void RefreshEquippedWeapons()
    {

        int a = 0;
        foreach (var slot in equippedWeaponsSlots)
        {
            var weaponId = DataManager.Instance.GetPreequippedWeaponList()[a];
            if (a >= DataManager.EQUIP_SLOT_COUNT) break;
            var maybeNull = config.GetOrDefault(weaponId);
            slot.GetComponentInChildren<WeaponCellUI>().Initialize(maybeNull);
            a++;
        }
    }
}