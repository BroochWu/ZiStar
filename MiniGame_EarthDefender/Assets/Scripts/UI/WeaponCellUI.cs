using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WeaponCellUI : MonoBehaviour
{
    private cfg.weapon.Weapon weapon;
    public GameObject empty;
    public Image iconBg;
    public Text nameText;
    private int id;
    private cfg.Enums.Com.Quality quality;
    private string weaponName;
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

        SetQualityUI();

    }

    /// <summary>
    /// 根据品质改UI
    /// </summary>
    void SetQualityUI()
    {
        string qualityColorStr = "#000000";
        Color qualityColor;

        switch (quality)
        {
            case cfg.Enums.Com.Quality.BLUE:
                qualityColorStr = "#3EBFF6";
                break;
            case cfg.Enums.Com.Quality.PURPLE:
                qualityColorStr = "#BC3EF6";
                break;
            case cfg.Enums.Com.Quality.ORANGE:
                qualityColorStr = "#F6B63E";
                break;
        }
        ColorUtility.TryParseHtmlString(qualityColorStr, out qualityColor);
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
        Instantiate(UIManager.Instance.weaponsLayer.WeaponDetailInfoPrefab, UIManager.Instance.dynamicContainer)
        .GetComponent<WeaponDetailInfo>().Initialize(weapon);

    }

}