using UnityEngine;

public class Bullet : MonoBehaviour
{
    enum BulletParent
    {
        PLAYER,
        ENEMY
    }

    private BulletParent bulletParent;
    public Weapon parentWeapon { get; private set; }

    public string bulletType;
    public int bulletDamage;
    private float lifeTime;
    private float timer;
    private float speed;

    public bool isReleased;//检测子弹是否已经被释放了

    public void Initialize(Weapon parent)
    {
        bulletParent = BulletParent.PLAYER;
        parentWeapon = parent;

        speed = parent.bulletSpeed;
        lifeTime = parent.bulletReleaseTime;
        transform.localScale = Vector3.one * parent.bulletScale / 10000f;


        isReleased = false;
        timer = 0f;
        bulletDamage = parent.attack;
    }
    public void Initialize(EnemyBase parent)
    {

        bulletParent = BulletParent.ENEMY;
        speed = 1;
        lifeTime = 1.5f;
        transform.SetPositionAndRotation(parent.transform.position, parent.transform.rotation);


        isReleased = false;
        timer = 0f;
        bulletDamage = parent.Damage;
        //敌人的子弹移动后自动销毁就行，不需要碰撞
    }


    void Update()
    {
        // 移动子弹
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        if (!isReleased)
        {
            timer += Time.deltaTime;

            // 检查是否超过生存时间
            if (timer >= lifeTime)
            {
                ObjectPoolManager.Instance.ReleaseBullet(gameObject);


                switch (bulletParent)
                {
                    case BulletParent.PLAYER:
                        break;

                    case BulletParent.ENEMY:

                        BattleManager.Instance.CalDamageEarthSuffer(bulletDamage);
                        break;

                    default:
                        Debug.LogError("子弹没有父类？");
                        break;
                }


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