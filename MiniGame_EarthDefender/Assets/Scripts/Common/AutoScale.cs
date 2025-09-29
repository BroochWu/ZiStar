using UnityEngine;


//自动缩放
public class AutoScale : MonoBehaviour
{
    public float scaleStep = 0.2f;

    private Vector3 incrementScale;//缩放增量
    public Vector3 initScale = Vector3.one;//初始缩放

    void OnEnable()
    {
        transform.localScale = initScale;
    }

    void Update()
    {
        incrementScale = scaleStep * Time.deltaTime * Vector3.one;
        transform.localScale += initScale + incrementScale;
    }

    void OnDrawGizmos()
    {
        transform.localScale = initScale;
    }
}