using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AvgDialogueUI : MonoBehaviour
{
    public Text textAvg;

    public void Initialize(cfg.avg.AvgEvent _avgEvent)
    {
        textAvg.text = _avgEvent.TextStr;
        Color textColor;
        ColorUtility.TryParseHtmlString(_avgEvent.TextColor_Ref.ColorDarkbg, out textColor);
        textAvg.color = textColor;

        StartCoroutine(DestroyAvgDialogue(_avgEvent.TimeDuration));

    }

    IEnumerator DestroyAvgDialogue(float _targetTime)
    {
        var elapsedTime = 0f;
        while (elapsedTime <= _targetTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
        //后面看需求可以做销毁渐隐动画
    }

    void OnDestroy()
    {
        //销毁以后播放下一行
        AvgPlayer.Instance.NextAvgEvent();
    }

}