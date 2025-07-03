using UnityEngine;
using UnityEngine.UI;

public class DevelopUI : MonoBehaviour
{
    public Text atkLevelText;
    public Text atkValueText;
    public Text atkLevelUpCostText;
    public Text hpLevelText;
    public Text hpValueText;
    public Text hpLevelUpCostText;
    public void Initialize()
    {
        RefreshAll();
    }

    void RefreshAll()
    {
        RefreshPlayerHp();
        RefreshPlayerAtk();
    }

    void RefreshPlayerHp()
    {
        var nowHpLevel = PlayerPrefs.GetInt("playerData_hp_level");
        hpLevelText.text = $"等级：{nowHpLevel}";
        hpValueText.text = DataManager.Instance.GetPlayerBasicHp().ToString();
        hpLevelUpCostText.text = cfg.Tables.tb.PlayerAttrLevel.Get(nowHpLevel).BasicHp.ItemRequire.Require.ToString();
    }
    void RefreshPlayerAtk()
    {
        var nowAtkLevel = PlayerPrefs.GetInt("playerData_atk_level");
        atkLevelText.text = $"等级：{nowAtkLevel}";
        atkValueText.text = DataManager.Instance.GetPlayerBasicAtk().ToString();
        atkLevelUpCostText.text = cfg.Tables.tb.PlayerAttrLevel.Get(nowAtkLevel).BasicAtk.ItemRequire.Require.ToString();
    }
}