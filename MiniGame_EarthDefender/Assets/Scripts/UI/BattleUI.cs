using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    public Text gameTimeUI;

    // Update is called once per frame
    void Update()
    {
        gameTimeUI.text = $"游戏时长：{(int)BattleManager.Instance.GameTime}秒  FPS:{Application.targetFrameRate}";
    }
}
