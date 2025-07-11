using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WeaponCellUI : MonoBehaviour
{
    public GameObject empty;
    public Image iconBg;
    public Text nameText;
    private int id;
    private cfg.Enums.Com.Quality quality;
    private string name;
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

        id = weapon.Id;
        quality = weapon.InitQuality;
        name = weapon.TextName;

        nameText.text = name;

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

}