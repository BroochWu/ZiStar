using UnityEngine;

public class Bullet : MonoBehaviour
{
    enum BulletParent
    {
        PLAYER,
        ENEMY
    }

    private BulletParent bulletParent;

    public string bulletType;
    public int bulletDamage;
    private float lifeTime;
    private float timer;
    private float speed;

    public bool isReleased;//检测子弹是否已经被释放了

    public void Initialize(Weapon parent)
    {
        bulletParent = BulletParent.PLAYER;
        isReleased = false;
        speed = parent.bulletSpeed;
        lifeTime = parent.bulletReleaseTime;
        timer = 0f;
        transform.localScale = Vector3.one * parent.bulletScale / 10000f;

        //子弹继承武器的伤害
        bulletDamage = parent.damage;
    }
    public void Initialize(Enemy parent)
    {

        bulletParent = BulletParent.ENEMY;
        isReleased = false;
        speed = 1;
        lifeTime = Random.Range(15, 20) / 10f;
        timer = 0f;
        transform.SetPositionAndRotation(parent.transform.position, parent.transform.rotation);

        //敌人的子弹移动后自动销毁就行，不需要碰撞
    }


    void Update()
    {
        // 移动子弹
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        if (!isReleased)
        {
            timer += Time.deltaTime;
            switch (bulletParent)
            {
                case BulletParent.PLAYER:
                    break;

                case BulletParent.ENEMY:
                    break;

                default:
                    Debug.LogError("子弹没有父类？");
                    break;
            }

            // 检查是否超过生存时间
            if (timer >= lifeTime)
            {
                ObjectPoolManager.Instance.ReleaseBullet(gameObject);
                isReleased = true;
            }

        }
    }

    /// <summary>
    /// 重置子弹状态
    /// </summary>
    public void ResetState()
    {
        //暂时没啥重置的好像
    }
}