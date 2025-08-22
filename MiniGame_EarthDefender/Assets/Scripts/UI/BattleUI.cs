using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [Header("=====装配脚本=====")]
    public TriCardUI triCardUI;

    [Header("=====具体内容=====")]
    public Text gameTimeUI;
    public Text earthHpText;
    public GameObject battleOver;
    public GameObject backBattleUI;
    public GameObject battleFail;
    public GameObject battleSuccess;
    public GameObject battleStopWindow;
    public GameObject expProgressBar;//经验值进度条
    public Transform awardsContainer;//奖励列表
    public Transform damageTextsContainer;//伤害数字
    public DamageStatisticsPanel damageStatisticsPanel;//伤害统计



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

        // battleStopWindow.SetActive(false);
        BattleStop(false);
        triCardUI.gameObject.SetActive(false);
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
#if UNITY_EDITOR
        gameTimeUI.text = $"游戏时长：{(int)BattleManager.Instance.GameTime}秒  FPS:{FPS}   剩余敌人数量：{BattleManager.Instance.activeEnemysCount}"
        + $"\nisPlayingAvg:{AvgManager.Instance.isPlayingAvg}";
#else
        gameTimeUI.gameObject.SetActive(false);
#endif
    }

    /// <summary>
    /// 更新地球血量
    /// </summary>
    public void RefreshEarthHp()
    {
        earthHpText.text = $"{BattleManager.Instance.currentEarthHp} / {BattleManager.Instance.dataInitEarthHp}";
    }

    /// <summary>
    /// 根据战斗结果，显示战斗结束UI
    /// </summary>
    public void BattleOver()
    {
        switch (BattleManager.Instance.battleState)
        {
            case BattleState.BATTLEFAIL:
                BattleFail();
                break;
            case BattleState.BATTLESUCCESS:
                BattleSuccess();
                break;
            default:
                Debug.LogWarning("啊？战斗没结束吗");
                break;
        }
    }

    /// <summary>
    /// 游戏失败时的UI
    /// </summary>
    private void BattleFail()
    {
        battleStopWindow.SetActive(false);
        battleOver.SetActive(true);
        battleFail.SetActive(true);
        damageStatisticsPanel.Initialize();
        AddAwardList();
    }
    /// <summary>
    /// 游戏胜利时的UI
    /// </summary>
    private void BattleSuccess()
    {
        battleOver.SetActive(true);
        battleSuccess.SetActive(true);
        damageStatisticsPanel.Initialize();
        AddAwardList();

    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    public void BackToMain()
    {
        Debug.Log("BackToMain");
        GameManager.Instance.SwitchGameStateToMainView();

        //删除所有特效字体
        foreach (Transform dtx in damageTextsContainer)
        {
            Destroy(dtx.gameObject);
        }
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
        // var itemObj = Resources.Load<GameObject>("Prefabs/Common/Item");
        var itemObj = UIManager.Instance.itemObj;
        var wait = new WaitForSecondsRealtime(0.5f);
        foreach (var award in awardsList)
        {
            yield return wait;
            if (award.Value == 0) continue;
            Instantiate(itemObj, awardsContainer).GetComponent<ItemUI>().Initialize(award.Key, award.Value);

        }
    }


    public void BattleStop(bool _is)
    {
        if (_is)
        {
            Time.timeScale = 0;
            battleStopWindow.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            battleStopWindow.SetActive(false);
        }
    }

    // public void CloseUI(GameObject name)
    // {
    //     // switch (name)
    //     // {
    //     //     case "BattleStop":
    //     //         battleStopWindow.SetActive(false);
    //     //         break;
    //     // }
    //     name.SetActive(false);
    // }

}
