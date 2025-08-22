using UnityEngine;
using UnityEngine.UI;

public class DamageStatisticsPanel : MonoBehaviour
{
    [Header("=====资源装配=====")]
    public Image imageMvpWeapon;
    public Text textMvpWeaponName;
    public Text textMvpWeaponDamageCount;

    [Header("=====数据=====")]
    private Weapon mvpWeapon;
    private int mvpWeaponDamage;

    public void Initialize()
    {
        mvpWeapon = BattleManager.Instance.sortedWeaponDamageStatisticsList[0].Key;
        mvpWeaponDamage = BattleManager.Instance.sortedWeaponDamageStatisticsList[0].Value;
        imageMvpWeapon.sprite = mvpWeapon.config.ImageIcon;
        textMvpWeaponName.text = mvpWeapon.config.TextName;
        if (mvpWeaponDamage == 0)
        {
            textMvpWeaponDamageCount.text = "造成伤害……0？？居然是0耶……";
            AvgManager.Instance.TriggerAvg(5);
        }
        else
        {
            textMvpWeaponDamageCount.text = "造成伤害：" + Utility.BigNumber(mvpWeaponDamage) + $"[{mvpWeaponDamage / BattleManager.Instance.totalDamage * 100f:F1}%]";
        }
    }
}