using UnityEngine;

public class SimpleCollider : MonoBehaviour
{
    public float Size = 0.5f; // 碰撞体大小
    
    void Start()
    {
        // 注册到碰撞管理器
        if (CompareTag("Enemy"))
            CollisionManager.Instance.RegisterEnemy(gameObject);
        else if (CompareTag("PlayerBullet"))
            CollisionManager.Instance.RegisterBullet(gameObject);
    }
    
    void OnDestroy()
    {
        // 注销
        if (CompareTag("Enemy") && CollisionManager.Instance != null)
            CollisionManager.Instance.UnregisterEnemy(gameObject);
        else if (CompareTag("PlayerBullet") && CollisionManager.Instance != null)
            CollisionManager.Instance.UnregisterBullet(gameObject);
    }
    
    // 可视化调试
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, Size / 2f);
    }
}