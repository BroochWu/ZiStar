
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class WeaponsUI : MonoBehaviour
{
    public List<GameObject> equippedWeaponsSlots;
    public Transform weaponsListContainer;
    public GameObject WeaponPrefab;
    public GameObject WeaponDetailInfoPrefab;
    private cfg.Tbweapon.Weapon config;


    public async Task Initialize()
    {
        config = cfg.Tables.tb.Weapon;

        // 装配已穿戴武器
        RefreshEquippedWeapons();




        // 获取武器列表
        // 排序：
        // 已解锁>未解锁
        // 稀有度降序
        // id降序
        // var sortedWeapons = config.DataList
        // .OrderBy(weapon => weapon.weaponState)
        // .ThenByDescending(weapon => weapon.InitQuality)
        // .ThenByDescending(weapon => weapon.Id)
        // .ToList();

        //新排序如下：
        //已解锁>未解锁
        //未解锁按照配表顺序
        //已解锁按照稀有度降序
        var unlockedWeapons = config.DataList.Where(weapon => weapon.weaponState == cfg.weapon.Weapon.CellState.NORMAL)
        .OrderByDescending(weapon => weapon.InitQuality);

        var lockedWeapons = config.DataList.Where(weapon => weapon.weaponState == cfg.weapon.Weapon.CellState.LOCK)
        .OrderBy(weapon => weapon.Id);

        var sortedWeapons = unlockedWeapons.Concat(lockedWeapons);
        //装备列表
        // //未避免反复销毁生成，这里先遍历现有的组件
        // //如果<=列表数，则直接在现有的上面改
        // //如果>，则再生成
        // int count = 0;
        // foreach (Transform child in weaponsListContainer)
        // {
        //     if (count >= sortedWeapons.Count)
        //     {
        //         Destroy(child.gameObject);
        //         break;
        //     }
        //     // Debug.Log(count + " " + config.DataList[count]);
        //     child.GetComponent<WeaponCellUI>().Initialize(sortedWeapons[count]);
        //     count++;
        //     await Task.Delay(50);
        // }

        // //如果格子不够再新增
        // for (int j = count; j < sortedWeapons.Count; j++)
        // {
        //     // Debug.Log(count + " " + config.DataList[j]);
        //     var weaponCell = Instantiate(WeaponPrefab, weaponsListContainer);
        //     weaponCell.GetComponent<WeaponCellUI>().Initialize(sortedWeapons[j]);
        //     await Task.Delay(50);
        // }

        foreach (Transform child in weaponsListContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (var i in sortedWeapons)
        {
            var weaponCell = Instantiate(WeaponPrefab, weaponsListContainer);
            weaponCell.GetComponent<WeaponCellUI>().Initialize(i);
            await Task.Delay(30);
        }
    }



    public void RefreshEquippedWeapons()
    {

        int a = 0;
        foreach (var slot in equippedWeaponsSlots)
        {
            var weaponId = DataManager.Instance.GetEquippedWeaponList()[a];
            if (a >= DataManager.EQUIP_SLOT_COUNT) break;
            var maybeNull = config.GetOrDefault(weaponId);
            slot.GetComponentInChildren<WeaponCellUI>().Initialize(maybeNull);
            a++;
        }
    }
}