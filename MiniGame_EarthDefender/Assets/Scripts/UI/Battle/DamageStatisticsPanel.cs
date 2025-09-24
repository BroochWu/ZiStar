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
        // string str1 = null;
        // foreach (var a in BattleManager.Instance.sortedWeaponDamageStatisticsList)
        // {
        //     str1 += a.Key.config.TextName + "  " + a.Value;
        // }
        // Debug.Log(str1);

        mvpWeapon = BattleManager.Instance.sortedWeaponDamageStatisticsList[0].Key;
        mvpWeaponDamage = BattleManager.Instance.sortedWeaponDamageStatisticsList[0].Value;
        imageMvpWeapon.sprite = mvpWeapon.weaponConfig.ImageIcon;
        textMvpWeaponName.text = mvpWeapon.weaponConfig.TextName;
        if (mvpWeaponDamage == 0)
        {
            textMvpWeaponDamageCount.text = "造成伤害……0？？居然是0耶……";
            // 好像通过通用方式实现了
            // var avgId = Utility.GetRandomByList(cfg.Tables.tb.GlobalParam.Get("avg_while_total_damage_is_0").IntListValue);
            // AvgManager.Instance.TriggerAvg(avgId);
        }
        else
        {
            textMvpWeaponDamageCount.text = "造成伤害："
            + Utility.BigNumber(mvpWeaponDamage)
            + $"<b> 【{(float)(mvpWeaponDamage * 1f / BattleManager.Instance.totalDamage) * 100:F1}%】</b>";
        }
    }
}