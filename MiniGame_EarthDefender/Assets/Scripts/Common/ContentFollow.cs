using UnityEngine;

public class ContentFollow : MonoBehaviour
{
    public RectTransform target;
    public Vector2 offset;
    public float halfHeight;

    public void OnDrawGizmos()
    {
        // GetComponent<RectTransform>().position = (Vector2)target.position - Vector2.up * target.rect.height / 2 + offset;
        GetComponent<RectTransform>().anchoredPosition = target.anchoredPosition - Vector2.up * target.rect.height + offset;
        halfHeight = target.rect.height;
    }
}