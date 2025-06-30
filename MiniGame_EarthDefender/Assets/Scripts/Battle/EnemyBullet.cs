using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    // private string bulletType;
    private float lifeTime = 0.5f;
    private bool isReleased;
    private float durationTime;

    public void Initialize(Enemy parent)
    {
        transform.SetPositionAndRotation(parent.transform.position, parent.transform.rotation);
        isReleased = false;
        durationTime = 0;
    }
    void Update()
    {
        transform.position += transform.up * Time.deltaTime;
        durationTime += Time.deltaTime;
        Debug.Log($"lifetime:{lifeTime},durationTime:{durationTime},isreleased:{isReleased}");
        if ((lifeTime <= durationTime) && !isReleased)
        {
            ObjectPoolManager.Instance.ReleaseBullet(gameObject);
            isReleased = true;
            Debug.Log("release enemybullet");
        }
    }
}
