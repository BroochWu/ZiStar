using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WeaponCellUI : MonoBehaviour
{
    enum CellState
    {
        NORMAL,
        LOCK,//未解锁
    }

    private CellState weaponCellState;
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
        StartCoroutine(SetInitAnim());

        this.weapon = weapon;
        id = weapon.Id;
        quality = weapon.InitQuality;
        weaponName = weapon.TextName;

        nameText.text = weaponName;

        CheckCellState();

        SetQualityUI();

    }

    /// <summary>
    /// 根据品质改UI
    /// </summary>
    void SetQualityUI()
    {
        int qualityid = 1;
        Color qualityColor;

        var config = cfg.Tables.tb.Color;

        switch (quality)
        {
            case cfg.Enums.Com.Quality.BLUE:
                qualityid = 101;
                break;
            case cfg.Enums.Com.Quality.PURPLE:
                qualityid = 102;
                break;
            case cfg.Enums.Com.Quality.ORANGE:
                qualityid = 103;
                break;
        }
        Debug.Log(config.Get(qualityid).ColorLightbg);
        ColorUtility.TryParseHtmlString(config.Get(qualityid).ColorLightbg, out qualityColor);
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

    /// <summary>
    /// 检查格子状态
    /// </summary>
    void CheckCellState()
    {
        var _isLock = DataManager.Instance.GetWeaponLevel(id) <= 0;
        if (_isLock)
        {
            weaponCellState = CellState.LOCK;
            lockUI.SetActive(true);
        }
        else
        {
            weaponCellState = CellState.NORMAL;
            lockUI.SetActive(false);
        }


    }


    public void OpenDetailInfo()
    {
        if (weaponCellState == CellState.NORMAL)
        {
            Instantiate(UIManager.Instance.weaponsLayer.WeaponDetailInfoPrefab, UIManager.Instance.dynamicContainer)
            .GetComponent<WeaponDetailInfo>().Initialize(weapon);
        }
        else if (weaponCellState == CellState.LOCK)
        {
            UIManager.Instance.CommonToast($"<color={cfg.Tables.tb.Color.Get(1).ColorDarkbg}>【{weaponName}】</color>未解锁，收集碎片以解锁武器");
        }

    }

}