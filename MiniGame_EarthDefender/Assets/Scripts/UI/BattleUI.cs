using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    public Text gameTimeUI;
    public Text earthHpText;
    public GameObject battleFail;
    public GameObject battleSuccess;
    public GameObject expProgressBar;//经验值进度条
    public Text expLvText;//当前等级

    private float updateFpsInterval = 1f; // 更新帧率的间隔
    private float fpsCounter; // fps计数器
    private float updateFpsTimer; // 更新帧率计时器
    private float FPS; // 帧率

    public void Initialize()
    {
        battleFail.SetActive(false);
        battleSuccess.SetActive(false);
        RefreshEarthHp();
        RefreshExpLevel();
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
        battleFail.SetActive(true);
    }
    /// <summary>
    /// 游戏胜利时的UI
    /// </summary>
    public void BattleSuccess()
    {
        battleSuccess.SetActive(true);

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

}
