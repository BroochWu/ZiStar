
using UnityEngine;

public class TrailUti : MonoBehaviour
{
    public TrailRenderer trail;
    public float delayTime=0.2f;
    private float timer;
    void Awake()
    {
        if (trail == null) trail = GetComponent<TrailRenderer>();
    }
    void OnEnable()
    {
        timer = 0;
    }
    void OnDisable()
    {
        trail.emitting = false;
    }
    void Update()
    {
        if (timer <= delayTime)
        {
            timer += Time.deltaTime;
            if (timer > delayTime)
            {

                trail.emitting = true;
            }
        }
    }
}