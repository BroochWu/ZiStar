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
        //消耗资源
        RefreshAtkResCost();
        //数据
        atkLevelText.text = $"等级：{DataManager.Instance.nowAtkLevel}";
        atkValueText.text = DataManager.Instance.GetPlayerBasicAtk().ToString();

    }
    void RefreshPlayerHp()
    {
        RefreshHpResCost();
        hpLevelText.text = $"等级：{DataManager.Instance.nowHpLevel}";
        hpValueText.text = DataManager.Instance.GetPlayerBasicHp().ToString();

    }

    public void TryLevelUpAtk()
    {
        //尝试升级，成功后刷新UI
        if (DataManager.Instance.PLBasicAtkLevelUp(DataManager.Instance.nowAtkLevel + 1))
        {
            RefreshAll();
        }

    }
    public void TryLevelUpHp()
    {
        //尝试升级，成功后刷新UI
        if (DataManager.Instance.PLBasicHpLevelUp(DataManager.Instance.nowHpLevel + 1))
        {
            RefreshAll();
        }

    }

    void RefreshAtkResCost()
    {
        var item = cfg.Tables.tb.PlayerAttrLevel.Get(DataManager.Instance.nowAtkLevel).BasicAtk.ItemRequire;
        atkLevelUpCostImg.sprite = item.Id_Ref.Image;

        var isRed = !DataManager.Instance.CheckRes(item.Id_Ref, item.Number);

        SetRequireResText(
            atkLevelUpCostText,
            item.Number,
            DataManager.Instance.GetResourceCount(item.Id_Ref),
            isRed);
    }


    void RefreshHpResCost()
    {
        var item = cfg.Tables.tb.PlayerAttrLevel.Get(DataManager.Instance.nowHpLevel).BasicHp.ItemRequire;
        hpLevelUpCostImg.sprite = item.Id_Ref.Image;

        var isRed = !DataManager.Instance.CheckRes(item.Id_Ref, item.Number);

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