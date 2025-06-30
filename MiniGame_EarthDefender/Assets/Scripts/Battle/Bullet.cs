using UnityEngine;

public class Bullet : MonoBehaviour
{
    public string bulletType;
    public int bulletDamage;
    private Weapon parent;
    private float lifetime;
    private float timer;

    public bool isReleased;//检测子弹是否已经被释放了

    public void Initialize(Weapon sourceWeapon)
    {
        isReleased = false;
        parent = sourceWeapon;
        lifetime = parent.bulletReleaseTime;
        timer = 0f;
        transform.localScale = Vector3.one * parent.bulletScale / 10000f;

        //子弹继承武器的伤害
        bulletDamage = parent.damage;
    }
    void Update()
    {
        // 移动子弹
        transform.Translate(Vector3.up * parent.bulletSpeed * Time.deltaTime);

        if (!isReleased)
        {
            // 更新生存时间
            timer += Time.deltaTime;

            // 检查是否超过生存时间
            if (timer >= lifetime)
            {
                BattleManager.Instance.ReturnBullet(this);
            }
        }
    }

    // void OnTriggerEnter2D(Collider2D other)
    // {
    //     // 碰撞处理
    //     if (other.CompareTag("Enemy") || other.CompareTag("Boundary"))
    //     {
    //         if (!isReleased) BattleManager.Instance.ReturnBullet(this);
    //     }
    // }

    /// <summary>
    /// 重置子弹状态
    /// </summary>
    public void ResetState()
    {
        //暂时没啥重置的好像
    }
}