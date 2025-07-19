using UnityEngine;
using UnityEngine.UI;

public class WeaponDetailInfoLvupCardPanelUI : MonoBehaviour
{
    public Text textDesc;
    public Image bg;
    public GameObject lockUI;
    public bool isLock
    {
        set
        { SetLockUI(value); }
    }

    public void Initialize(cfg.weapon.WeaponLevel level)
    {
        bg.color = Utility.SetQualityColor(level.LevelupUnlockCard_Ref.Quality, true); ;

        var str1 = $"{level.Level}级解锁：";
        textDesc.text = str1 + level.LevelupUnlockCard_Ref.TextDesc;

        isLock = DataManager.Instance.GetWeaponLevel(level.Id) <= level.Level;
        

    }

    void SetLockUI(bool _islocked)
    {
        lockUI.SetActive(_islocked);
    }
}
