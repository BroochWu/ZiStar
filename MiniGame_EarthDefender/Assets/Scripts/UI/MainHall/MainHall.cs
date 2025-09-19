using System.Collections;
using UnityEngine;

public class MainHall : MonoBehaviour
{

    public RectTransform Bg;
    public Animator animator;
    private const int BG_MOVE_SEP = 600;
    private Vector2 baseAP = new Vector2(0, 0);
    private Coroutine corMoveBg;

    void Awake()
    {
        animator ??= GetComponent<Animator>();
    }

    // void Start()
    // {
    //     Debug.LogError(Bg.anchoredPosition);
    // }

    public void MoveBg(int _step)
    {
        return;
        var target = baseAP + Vector2.left * (_step - 2) * BG_MOVE_SEP;
        if (corMoveBg != null) StopCoroutine(corMoveBg);
        corMoveBg = StartCoroutine(CorMoveBg(target));
    }

    IEnumerator CorMoveBg(Vector2 _target)
    {
        float timer = 0;
        while (timer <= 0.5f)
        {
            timer += Time.deltaTime;
            Bg.anchoredPosition = Vector2.Lerp(Bg.anchoredPosition, _target, 0.4f);
            yield return null;
        }
    }

}