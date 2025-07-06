using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    public Text gameTimeUI;
    public Text earthHpText;
    public GameObject battleOver;
    public GameObject backBattleUI;
    public GameObject battleFail;
    public GameObject battleSuccess;
    public GameObject expProgressBar;//经验值进度条
    public Transform awardsContainer;//奖励列表
    public GameObject awardsEmpty;//奖励列表
    public Text expLvText;//当前等级
    public Dictionary<cfg.item.Item, int> awardsList = new();

    private float updateFpsInterval = 1f; // 更新帧率的间隔
    private float fpsCounter; // fps计数器
    private float updateFpsTimer; // 更新帧率计时器
    private float FPS; // 帧率

    public void Initialize()
    {
        foreach (Transform child in awardsContainer)
        {
            DestroyImmediate(child.gameObject);
        }

        battleOver.SetActive(false);
        battleFail.SetActive(false);
        battleSuccess.SetActive(false);
        RefreshEarthHp();
        RefreshExpLevel();

        backBattleUI.SetActive(true);
        gameObject.SetActive(true);

    }

    public void UnRegister()
    {
        gameObject.SetActive(false);
        backBattleUI.SetActive(false);
    }

    void Update()
    {

        fpsCounter++;
        updateFpsTimer += Time.deltaTime;
        if (updateFpsTimer >= updateFpsInterval)
        {
            FPS = fpsCounter / updateFpsTimer;
            updateFpsTimer = 0;
            fpsCounter = 0;
        }
        gameTimeUI.text = $"游戏时长：{(int)BattleManager.Instance.GameTime}秒  FPS:{FPS}   剩余敌人数量：{BattleManager.Instance.activeEnemysCount}";
    }

    /// <summary>
    /// 更新地球血量
    /// </summary>
    public void RefreshEarthHp()
    {
        earthHpText.text = $"{BattleManager.Instance.currentEarthHp} / {BattleManager.Instance.dataInitEarthHp}";
    }

    /// <summary>
    /// 游戏失败时的UI
    /// </summary>
    public void BattleFail()
    {
        battleOver.SetActive(true);
        battleFail.SetActive(true);
        AddAwardList();
    }
    /// <summary>
    /// 游戏胜利时的UI
    /// </summary>
    public void BattleSuccess()
    {
        battleOver.SetActive(true);
        battleSuccess.SetActive(true);
        AddAwardList();

    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    public void BackToMain()
    {
        Debug.Log("BackToMain");
        GameManager.Instance.SwitchGameStateToMainView();
    }


    /// <summary>
    /// 更新经验等级
    /// </summary>
    public void RefreshExpLevel()
    {
        var _currentLv = BattleManager.Instance.currentLv;
        float _currentExp = BattleManager.Instance.currentExp;
        float _nextExp = BattleManager.Instance.nextExp;
        expLvText.text = _currentLv.ToString();
        expProgressBar.transform.localScale = new Vector3(_currentExp / _nextExp, 1, 1);
    }

    void AddAwardList()
    {
        foreach (Transform a in awardsContainer)
        {
            DestroyImmediate(a.gameObject);
        }

        awardsEmpty.SetActive(awardsList.Count == 0);

        if (awardsList.Count != 0) StartCoroutine(CorAddAwardList());
    }

    IEnumerator CorAddAwardList()
    {
        var itemObj = Resources.Load<GameObject>("Prefabs/Common/Item");
        var wait = new WaitForSecondsRealtime(0.5f);
        foreach (var award in awardsList)
        {
            yield return wait;
            if (award.Value == 0) continue;
            Instantiate(itemObj, awardsContainer).GetComponent<ItemUI>().Initialize(award.Key, award.Value);

            Debug.Log("in BUI:" + award.Key + "  " + award.Value);
        }
    }

}
