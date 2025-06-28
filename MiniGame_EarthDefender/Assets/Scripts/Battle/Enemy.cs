using UnityEngine;

public class Enemy : MonoBehaviour
{


    void Update()
    {
        transform.position -= Time.deltaTime * Vector3.down;
    }
}
