using UnityEngine;

public class UtilAnimationMotions : MonoBehaviour
{
    public virtual void Disappear()
    {
        Destroy(gameObject);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}