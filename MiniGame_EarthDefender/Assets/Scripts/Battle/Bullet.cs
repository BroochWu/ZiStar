using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    public string bulletType;
    private Weapon parent;//子弹所属的武器
    private float lifetime;
    private float timer;

    public void Initialize(Weapon sourceWeapon)
    {
        parent = sourceWeapon;
        lifetime = parent.bulletReleaseTime;
        timer = 0f;

        transform.localScale = Vector3.one * parent.bulletScale / 10000f;//万分数
    }

    /// <summary>
    /// 重置子弹状态
    /// </summary>
    public void ResetState()
    {
        timer = 0f;
    }

    void Update()
    {
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
        // 碰撞处理（示例）
        if (other.CompareTag("Enemy") || other.CompareTag("Boundary"))
        {
            ReturnToPool();
        }
    }

    /// <summary>
    /// 返回对象池
    /// </summary>
    void ReturnToPool()
    {
        if (BattleManager.Instance == null)
        {
            Destroy(gameObject);
            return;
        }

        // 获取对象池
        IObjectPool<GameObject> pool = BattleManager.Instance.GetBulletPool(bulletType);
        if (pool != null)
        {
            pool.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}