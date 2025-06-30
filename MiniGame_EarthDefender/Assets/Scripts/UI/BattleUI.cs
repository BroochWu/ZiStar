using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    public Text gameTimeUI;

    private float updateFpsInterval = 1f; // 更新帧率的间隔
    private float fpsCounter; // fps计数器
    private float updateFpsTimer; // 更新帧率计时器
    private float FPS; // 帧率

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
}
