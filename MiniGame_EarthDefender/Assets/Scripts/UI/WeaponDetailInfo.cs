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

    public GameObject levelUpCardPanelPrefab;


    public Button buttonEquip;
    public Transform levelUpContainer;


    private ButtonState buttonState;


    public void Initialize(cfg.weapon.Weapon weapon)
    {
        this.weapon = weapon;
        // var slotId = DataManager.Instance.IsWeaponEquipped(weapon.Id);
        // DataManager.Instance.UnequipWeaponBySlot(slotId);





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
        buttonEquip.onClick.AddListener(OnEquipButtonClicked);



        var levelConfig = cfg.Tables.tb.WeaponLevel;
        for (int i = 2; i <= levelConfig.DataList[weapon.LevelId].LevelupUnlockCard; i++)
        {
            
        }
    }


    public void CloseThisWindow()
    {
        UIManager.Instance.weaponsLayer.RefreshEquippedWeapons();
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
                CloseThisWindow();
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
                CloseThisWindow();
            }
        }
    }




    private void OnEquipButtonClicked()
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
}