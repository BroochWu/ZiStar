using System;
using UnityEngine;
using UnityEngine.UI;

public class TreasureDetailUI : MonoBehaviour
{
    public Text textRemainTime;

    public void CloseThisPage()
    {
        Destroy(gameObject);
    }

    void Update()
    {
        var str = TimeSpan.FromSeconds(ChestsRewardSystem.currentRemainSeconds).ToString(@"mm\:ss");
        textRemainTime.text = $"距离下次发放剩余  {str}";
    }

}