using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AvgDialogueUI : MonoBehaviour
{
    public Text textAvg;
    public Animator anim;

    // Vector3 INIT_POS_OFFSET = new Vector3(-20, 0, 0);
    // Vector3 initPos;

    const float animTime = 0.5f;

    public void Initialize(cfg.avg.AvgEvent _avgEvent)
    {
        // initPos = transform.position;

        textAvg.text = _avgEvent.TextStr;
        Color textColor;
        ColorUtility.TryParseHtmlString(_avgEvent.TextColor_Ref.ColorDarkbg, out textColor);
        textAvg.color = textColor;

        StartCoroutine(DestroyAvgDialogue(_avgEvent.TimeDuration));

    }

    // void Update()
    // {
    //     INIT_POS_OFFSET = Vector3.Lerp(INIT_POS_OFFSET, Vector3.zero, 0.1f);
    //     transform.position = initPos + INIT_POS_OFFSET;
    // }

    IEnumerator DestroyAvgDialogue(float _targetTime)
    {

        //判断什么时候销毁
        var elapsedTime = 0f;
        var wait = new WaitForSecondsRealtime(0.02f);
        while (elapsedTime <= _targetTime)
        {
            Debug.Log("elapsedTime:" + elapsedTime);
            elapsedTime += wait.waitTime;
            //返回真实等待时间，不会因为游戏暂停而停止
            yield return wait;
        }

        //销毁动画
        // elapsedTime = 0;
        anim.Play("AVGDialogueDisappear");
        // while (elapsedTime <= animTime)
        // {
        //     Debug.Log("正在执行销毁动画");
        //     elapsedTime += Time.deltaTime;

        //     yield return null;
        // }

        //后面看需求可以做销毁渐隐动画
    }

    public void DestroyThis()
    {

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        //销毁以后播放下一行
        if (AvgManager.Instance.isPlayingAvg != null) AvgPlayer.Instance.NextAvgEvent();
    }



}