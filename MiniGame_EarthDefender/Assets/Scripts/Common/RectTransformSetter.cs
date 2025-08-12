using UnityEngine;

public class RectTransformSetter : MonoBehaviour
{
    public bool setPos;
    public bool setScale;

    public RectTransform target;

    void OnDrawGizmos()
    {
        if (setPos)
            GetComponent<RectTransform>().position = target.position;
        if (setScale)
            GetComponent<RectTransform>().sizeDelta = target.sizeDelta;
    }
}