using UnityEngine;

public class WeaponDetailInfo : MonoBehaviour
{
    private cfg.weapon.Weapon weapon;

    public void CloseThisWindow()
    {
        Destroy(this.gameObject);
    }

    public void TryEquipWeapon()
    {
        //如果有空位，就自动上空位
        //如果没有空位，就发起换武器的功能
        var fistSlot = DataManager.Instance.GetFirstEmptySlot();
        if (fistSlot != -1)
        {
            DataManager.Instance.EquipWeapon(fistSlot, weapon.Id);
            CloseThisWindow();
            UIManager.Instance.CommonToast("装备成功");
            UIManager.Instance.weaponsLayer.RefreshEquippedWeapons();
        }
        else
        {
            UIManager.Instance.CommonToast("没空位力");
        }
    }


    public void Initialize(cfg.weapon.Weapon weapon)
    {
        this.weapon = weapon;
    }

}