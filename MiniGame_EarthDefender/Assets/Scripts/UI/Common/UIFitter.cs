using UnityEngine;

public class UIFitter : MonoBehaviour
{
    public enum UIFitState
    {
        STABLEHEIGHT
    }

    public UIFitState uIFitState;
    public float basicScale;

    void OnDrawGizmos()
    {
        float ratio = (float)(Screen.height * 1f / 1334f);
        Debug.Log(Screen.height + " " + Screen.width +  " " + ratio);
        if (uIFitState == UIFitState.STABLEHEIGHT)
        {
            transform.localScale = Vector3.one * basicScale * ratio;
        }
    }
}