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

    private int nowHpLevel;
    private int nowAtkLevel;

    public void Initialize()
    {
        RefreshAll();
        Debug.Log($"{this.name} is initialize!");
    }

    void RefreshAll()
    {
        RefreshPlayerAtk();
        RefreshPlayerHp();
    }

    void RefreshPlayerAtk()
    {
        //当前等级
        nowAtkLevel = PlayerPrefs.GetInt("playerData_atk_level");
        //消耗资源
        SetAtkResText(false);
        //数据
        atkLevelText.text = $"等级：{nowAtkLevel}";
        atkValueText.text = DataManager.Instance.GetPlayerBasicAtk().ToString();

    }
    void RefreshPlayerHp()
    {
        nowHpLevel = PlayerPrefs.GetInt("playerData_hp_level");
        SetHpResText(false);
        hpLevelText.text = $"等级：{nowHpLevel}";
        hpValueText.text = DataManager.Instance.GetPlayerBasicHp().ToString();

    }

    public void TryLevelUpAtk()
    {
        nowAtkLevel = PlayerPrefs.GetInt("playerData_atk_level");
        SetAtkResText(true);
        RefreshPlayerAtk();

    }
    public void TryLevelUpHp()
    {
        nowHpLevel = PlayerPrefs.GetInt("playerData_hp_level");
        SetHpResText(true);
        RefreshPlayerHp();

    }

    void SetAtkResText(bool cost)
    {
        var item = cfg.Tables.tb.PlayerAttrLevel.Get(nowAtkLevel).BasicAtk.ItemRequire;
        //需要消耗资源，并且成功升级了
        if (cost)
        {
            // nowAtkLevel += 1;
            DataManager.Instance.SetPlayerBasicAtkLevel(nowAtkLevel + 1, DataManager.Instance.CostResource(item.Id_Ref, item.Require));
        }

        SetRequireResText(
            atkLevelUpCostText,
            item.Require,
            DataManager.Instance.GetResourceCount(item.Id_Ref));
    }
    void SetHpResText(bool cost)
    {
        var item = cfg.Tables.tb.PlayerAttrLevel.Get(nowHpLevel).BasicHp.ItemRequire;

        //需要消耗资源、未满级、资源足够，则消耗资源并成功升级
        if (cost)
        {
            // nowHpLevel += 1;;
            DataManager.Instance.SetPlayerBasicHpLevel(nowHpLevel + 1, DataManager.Instance.CostResource(item.Id_Ref, item.Require));
        }

        SetRequireResText(
            hpLevelUpCostText,
            item.Require,
            DataManager.Instance.GetResourceCount(item.Id_Ref));
    }

    void SetRequireResText(Text _text, int _costItemCount, int _nowItemCount)
    {
        _text.text = _costItemCount.ToString();
        _text.color = _nowItemCount >= _costItemCount ? Color.black : Color.red;
    }


}