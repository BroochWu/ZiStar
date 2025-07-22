using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WeaponCellUI : MonoBehaviour
{

    private cfg.weapon.Weapon weapon;
    public GameObject empty;
    public GameObject lockUI;
    public Image iconBg;
    public Text nameText;
    private int id;
    private cfg.Enums.Com.Quality quality;
    public string weaponName;
    private float initScale;

    public void Initialize(cfg.weapon.Weapon weapon)
    {
        empty.SetActive(weapon == null);
        nameText.gameObject.SetActive(weapon != null);
        if (weapon == null)
        {
            return;
        }


        initScale = 0.5f;
        Debug.Log("Start Init");

        if (UIManager.Instance.uiLayer == UILayer.WEAPONSLAYER)
            StartCoroutine(SetInitAnim());

        this.weapon = weapon;
        id = weapon.Id;
        quality = weapon.InitQuality;
        weaponName = weapon.TextName;

        nameText.text = weaponName;

        lockUI.SetActive(weapon.weaponState == cfg.weapon.Weapon.CellState.LOCK);

        SetQualityUI();

    }

    /// <summary>
    /// 根据品质改UI
    /// </summary>
    void SetQualityUI()
    {
        Color qualityColor = Utility.SetQualityColor(quality, true);

        nameText.color = qualityColor;
        iconBg.color = qualityColor;

    }

    IEnumerator SetInitAnim()
    {
        while (initScale <= 0.99f)
        {
            initScale = Mathf.Lerp(initScale, 1.0f, 0.2f);
            transform.localScale = Vector3.one * initScale;
            yield return null;
        }
    }



    public void OpenDetailInfo()
    {
        if (weapon.weaponState == cfg.weapon.Weapon.CellState.NORMAL)
        {
            Instantiate(UIManager.Instance.weaponsLayer.WeaponDetailInfoPrefab, UIManager.Instance.dynamicContainer)
            .GetComponent<WeaponDetailInfo>().Initialize(weapon);
        }
        else if (weapon.weaponState == cfg.weapon.Weapon.CellState.LOCK)
        {
            UIManager.Instance.CommonToast($"<color={cfg.Tables.tb.Color.Get(1).ColorDarkbg}>【{weaponName}】</color>未解锁，收集碎片以解锁武器");
        }

    }

}