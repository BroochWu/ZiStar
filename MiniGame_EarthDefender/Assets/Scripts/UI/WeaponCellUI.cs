using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//每一个武器格子的UI
public class WeaponCellUI : MonoBehaviour
{
    private cfg.weapon.Weapon weapon;
    [Header("=====锁定的内容=====")]
    public GameObject lockUI;
    public Image lockBg;
    public Text lockText;
    public Image lockImg;

    [Header("=====资源索引=====")]
    public Sprite lockImgRes1;
    public Sprite lockImgRes2;

    [Header("=====组件索引=====")]
    public GameObject empty;
    public Text weaponLevelText;
    public Image iconBg;
    public Text nameText;
    public GameObject weaponPieceObj;


    public string weaponName;

    private int id;
    private float initScale;
    private cfg.Enums.Com.Quality quality;



    public void Initialize(cfg.weapon.Weapon weapon)
    {
        empty.SetActive(weapon == null);
        nameText.gameObject.SetActive(weapon != null);
        if (weapon == null)
        {
            weaponPieceObj.SetActive(false);
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

        Debug.Log("weaponstate:" + weapon.Id + "  " + weapon.weaponState);
        //判断格子状态
        switch (weapon.weaponState)
        {
            case cfg.weapon.Weapon.CellState.LOCK:
                //非即将解锁全黑
                weaponLevelText.gameObject.SetActive(false);
                weaponPieceObj.SetActive(false);

                lockUI.SetActive(true);

                lockBg.color = Color.black;
                lockImg.sprite = lockImgRes1;
                nameText.gameObject.SetActive(false);
                lockText.gameObject.SetActive(false);
                break;
            case cfg.weapon.Weapon.CellState.NEXTUNLOCK:
                //即将解锁
                //下一个要解锁的
                //只能看到未解锁的武器中，下一个的名称和解锁条件
                Debug.Log("this is next unlock  " + weapon.Id);

                weaponLevelText.gameObject.SetActive(false);
                weaponPieceObj.SetActive(false);

                lockUI.SetActive(true);
                lockText.text = $"通过\n<size=40> 第{weapon.UnlockCond.IntParams[0]}关 </size>";
                lockImg.sprite = lockImgRes2;
                break;
            case cfg.weapon.Weapon.CellState.NORMAL:
                //正常已解锁

                weaponLevelText.text = $"等级 {weapon.currentLevel}";

                //碎片剩余数量/需求数量
                var pieceItemCount = Utility.BigNumber(DataManager.Instance.GetItemCount(weapon.Piece_Ref));
                var pieceItemNeed = weapon.levelUpConsumes[weapon.Piece_Ref];
                weaponPieceObj.GetComponentInChildren<Text>().text = $"{pieceItemCount}/{pieceItemNeed}";

                //装配完成，可以可见
                weaponLevelText.gameObject.SetActive(true);
                weaponPieceObj.SetActive(true);
                lockUI.SetActive(false);
                break;
        }
        // if (weapon.weaponState == cfg.weapon.Weapon.CellState.LOCK)
        // {
        //     weaponLevelText.gameObject.SetActive(false);
        //     weaponPieceObj.SetActive(false);

        //     lockUI.SetActive(true);

        //     if (weapon.weaponState == cfg.weapon.Weapon.CellState.NEXTUNLOCK)
        //     {
        //         //下一个要解锁的
        //         //只能看到未解锁的武器中，下一个的名称和解锁条件
        //         lockText.text = $"通过\n<size=40> 第{weapon.UnlockCond.IntParams[0]}关 </size>";
        //         lockImg.sprite = lockImgRes2;
        //     }
        //     else
        //     {
        //         //否则全黑
        //         lockBg.color = Color.black;
        //         lockImg.sprite = lockImgRes1;
        //         nameText.gameObject.SetActive(false);
        //         lockText.gameObject.SetActive(false);
        //     }
        // }
        // else
        // {
        //     weaponLevelText.text = $"等级 {weapon.currentLevel}";

        //     //碎片剩余数量/需求数量
        //     var pieceItemCount = Utility.BigNumber(DataManager.Instance.GetItemCount(weapon.Piece_Ref));
        //     var pieceItemNeed = weapon.levelUpConsumes[weapon.Piece_Ref];
        //     weaponPieceObj.GetComponentInChildren<Text>().text = $"{pieceItemCount}/{pieceItemNeed}";

        //     //装配完成，可以可见
        //     weaponLevelText.gameObject.SetActive(true);
        //     weaponPieceObj.SetActive(true);
        //     lockUI.SetActive(false);
        // }


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


    /// <summary>
    /// 打开详情页
    /// </summary>
    public void OpenDetailInfo()
    {
        string str = "";
        foreach (var a in weapon.levelUpConsumes)
        {
            str += a.ToString();
        }
        Debug.Log(str);



        switch (weapon.weaponState)
        {
            case cfg.weapon.Weapon.CellState.NORMAL:

                Instantiate(UIManager.Instance.weaponsLayer.WeaponDetailInfoPrefab, UIManager.Instance.dynamicContainer)
                .GetComponent<WeaponDetailInfo>().Initialize(weapon);
                break;
            case cfg.weapon.Weapon.CellState.LOCK:
            case cfg.weapon.Weapon.CellState.NEXTUNLOCK:
                Utility.CondCheck(weapon.UnlockCond.CondType, weapon.UnlockCond.StringParams, weapon.UnlockCond.IntParams, out string toastStr);
                UIManager.Instance.CommonToast(toastStr);
                break;

        }

    }

}