using UnityEngine;
using UnityEngine.UI;

public class DevelopUI : MonoBehaviour
{
    public Text atkLevelText;
    public Text atkValueText;
    public Text atkLevelUpCostText;
    public Image atkLevelUpCostImg;
    public Text hpLevelText;
    public Text hpValueText;
    public Text hpLevelUpCostText;
    public Image hpLevelUpCostImg;

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
        RefreshAtkResCost(false);
        //数据
        atkLevelText.text = $"等级：{nowAtkLevel}";
        atkValueText.text = DataManager.Instance.GetPlayerBasicAtk().ToString();

    }
    void RefreshPlayerHp()
    {
        nowHpLevel = PlayerPrefs.GetInt("playerData_hp_level");
        RefreshHpResCost(false);
        hpLevelText.text = $"等级：{nowHpLevel}";
        hpValueText.text = DataManager.Instance.GetPlayerBasicHp().ToString();

    }

    public void TryLevelUpAtk()
    {
        nowAtkLevel = PlayerPrefs.GetInt("playerData_atk_level");
        RefreshAtkResCost(true);
        RefreshPlayerAtk();

    }
    public void TryLevelUpHp()
    {
        nowHpLevel = PlayerPrefs.GetInt("playerData_hp_level");
        RefreshHpResCost(true);
        RefreshPlayerHp();

    }

    void RefreshAtkResCost(bool cost)
    {
        var item = cfg.Tables.tb.PlayerAttrLevel.Get(nowAtkLevel).BasicAtk.ItemRequire;
        atkLevelUpCostImg.sprite = Resources.Load<Sprite>("Images/" + item.Id_Ref.ImagePath);
        //需要消耗资源，并且成功升级了
        var isRed = true;
        // nowAtkLevel += 1;
        if (DataManager.Instance.CheckOrSetPLBasicAtkLevel(nowAtkLevel + 1, false))
        {
            if (DataManager.Instance.CheckOrCostResource(item.Id_Ref, item.Number, false))
            {
                isRed = false;
                if (cost)
                {
                    DataManager.Instance.CheckOrSetPLBasicAtkLevel(nowAtkLevel + 1, true);
                    DataManager.Instance.CheckOrCostResource(item.Id_Ref, item.Number, true);
                }
            }
        }

        SetRequireResText(
            atkLevelUpCostText,
            item.Number,
            DataManager.Instance.GetResourceCount(item.Id_Ref),
            isRed);
    }
    void RefreshHpResCost(bool cost)
    {
        var item = cfg.Tables.tb.PlayerAttrLevel.Get(nowHpLevel).BasicHp.ItemRequire;
        hpLevelUpCostImg.sprite = Resources.Load<Sprite>("Images/" + item.Id_Ref.ImagePath);
        var isRed = true;

        //需要消耗资源、未满级、资源足够，则消耗资源并成功升级
        if (DataManager.Instance.CheckOrSetPLBasicHpLevel(nowHpLevel + 1, false))
        {
            if (DataManager.Instance.CheckOrCostResource(item.Id_Ref, item.Number, false))
            {
                isRed = false;
                if (cost)
                {
                    DataManager.Instance.CheckOrSetPLBasicHpLevel(nowHpLevel + 1, true);
                    DataManager.Instance.CheckOrCostResource(item.Id_Ref, item.Number, true);

                }
            }
        }

        SetRequireResText(
            hpLevelUpCostText,
            item.Number,
            DataManager.Instance.GetResourceCount(item.Id_Ref),
            isRed);
    }

    void SetRequireResText(Text _text, int _costItemCount, int _nowItemCount, bool isRed)
    {
        _text.text = _costItemCount.ToString();
        _text.color = isRed ? Color.red : Color.black;
    }


}