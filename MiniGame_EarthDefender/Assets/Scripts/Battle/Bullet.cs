using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    public string bulletType;
    private Weapon parent;
    private float lifetime;
    private float timer;
    private bool isReleased = false; // 标记是否已释放

    public void Initialize(Weapon sourceWeapon)
    {
        parent = sourceWeapon;
        lifetime = parent.bulletReleaseTime;
        timer = 0f;
        isReleased = false; // 重置释放标记

        transform.localScale = Vector3.one * parent.bulletScale / 10000f;
    }

    public void ResetState()
    {
        timer = 0f;
        isReleased = false; // 重置释放标记
    }

    void Update()
    {
        if (isReleased) return; // 已释放则不再处理
        
        // 移动子弹
        transform.Translate(Vector3.up * parent.bulletSpeed * Time.deltaTime);

        // 更新生存时间
        timer += Time.deltaTime;

        // 检查是否超过生存时间
        if (timer >= lifetime)
        {
            ReturnToPool();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isReleased) return; // 已释放则不再处理碰撞
        
        Debug.Log("碰撞到" + other.tag);
        
        // 碰撞处理
        if (other.CompareTag("Enemy") || other.CompareTag("Boundary"))
        {
            ReturnToPool();
        }
    }

    void ReturnToPool()
    {
        // 如果已经释放过，直接返回
        if (isReleased) return;
        isReleased = true; // 标记为已释放
        
        // 确保BattleManager存在
        if (BattleManager.Instance == null)
        {
            Destroy(gameObject);
            return;
        }
        
        // 确保子弹类型有效
        if (string.IsNullOrEmpty(bulletType))
        {
            Destroy(gameObject);
            return;
        }

        // 获取对象池
        IObjectPool<GameObject> pool = BattleManager.Instance.GetBulletPool(bulletType);
        if (pool != null)
        {
            try
            {
                pool.Release(gameObject);
            }
            catch (System.InvalidOperationException ex)
            {
                Debug.LogError($"释放子弹到对象池失败: {ex.Message}");
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}