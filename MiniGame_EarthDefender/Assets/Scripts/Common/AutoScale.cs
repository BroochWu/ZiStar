using UnityEngine;


//自动缩放
public class AutoScale : MonoBehaviour
{
    public float scaleStep = 0.2f;

    private float finalScaleMulti;

    void OnEnable()
    {
        transform.localScale = Vector3.one;
    }

    void Update()
    {
        finalScaleMulti = scaleStep * Time.deltaTime;
        transform.localScale += Vector3.one * finalScaleMulti;
    }
}