//跟随

using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    void LateUpdate()
    {
        if (target != null)
            transform.position = target.position + offset;
    }

}