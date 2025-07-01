using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    public Text gameTimeUI;
    public Text earthHpText;
    public GameObject battleOver;

    private float updateFpsInterval = 1f; // 更新帧率的间隔
    private float fpsCounter; // fps计数器
    private float updateFpsTimer; // 更新帧率计时器
    private float FPS; // 帧率

    public void Initialize()
    {
        battleOver.SetActive(false);
        RefreshEarthHp();
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
        gameTimeUI.text = $"游戏时长：{(int)BattleManager.Instance.GameTime}秒  FPS:{FPS}";
    }

    /// <summary>
    /// 更新地球血量
    /// </summary>
    public void RefreshEarthHp()
    {
        Debug.Log("地球血量已更新");
        earthHpText.text = $"{BattleManager.Instance.currentEarthHp} / {BattleManager.Instance.dataInitEarthHp}";
    }

    /// <summary>
    /// 游戏结束时的UI
    /// </summary>
    public void BattleOver()
    {
        battleOver.SetActive(true);
    }

}
