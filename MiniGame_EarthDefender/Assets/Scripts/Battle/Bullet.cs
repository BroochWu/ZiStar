using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    enum BulletParentType
    {
        PLAYER,
        ENEMY
    }
    [Header("必填！！！")]
    public float lifeTime;
    public Bullet bulletNext;//生成之后的子弹


    [HideInInspector] public int bulletDamage;
    [HideInInspector] public Weapon parentWeapon { get; private set; }

    [HideInInspector] public string bulletType;

    [HideInInspector] public int bulletPenetrate;//子弹可碰撞次数（可穿透数量）
    [HideInInspector] public float bulletPenetrateInterval;//子弹穿透同一个单位造成连续伤害的伤害间隔

    [HideInInspector] public bool isReleased;//检测子弹是否已经被释放了
    [HideInInspector] public List<GameObject> listCollisionCd = new();
    private BulletParentType bulletParentType;
    private float timer;
    private float speed;
    // private Vector3 initScale;

    // public bool canCollide = true;//子弹是否可以碰撞
    // public Bullet bulletFormer;//生成之前的子弹


    void Start()
    {
        // initScale = transform.localScale;
    }

    public void Initialize(Weapon parent)
    {
        bulletParentType = BulletParentType.PLAYER;
        parentWeapon = parent;

        speed = parent.bulletSpeed;
        // transform.localScale = initScale * (float)(parent.bulletScale / 10000f);


        isReleased = false;
        timer = 0f;
        bulletDamage = parent.attack;
        bulletPenetrate = parent.config.Penetrate;
        bulletPenetrateInterval = parent.config.PenetrateInterval;
    }
    public void Initialize<T>(T parent) where T : EnemyBase
    {

        bulletParentType = BulletParentType.ENEMY;
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


                switch (bulletParentType)
                {
                    case BulletParentType.PLAYER:
                        break;

                    case BulletParentType.ENEMY:

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

        if (parentWeapon != null)
        {
            bulletPenetrate = parentWeapon.config.Penetrate;
        }
        // transform.localScale = initScale * parentWeapon.config.BulletScale / 10000f;
        //清空冷却池
        listCollisionCd.Clear();
    }

    /// <summary>
    /// 添加到冷却池，以保证一个子弹不会重复触发
    /// </summary>
    /// <param name="_which"></param>
    /// <param name="_while"></param>
    /// <returns>true:没碰撞过，false:正在冷却中</returns>
    public bool AddToListCollisionCD(GameObject _which, float _while)
    {
        if (listCollisionCd.Contains(_which)) return false;

        listCollisionCd.Add(_which);
        StartCoroutine(CorAddToListCollisionCD(_which, _while));
        return true;
    }

    IEnumerator CorAddToListCollisionCD(GameObject _which, float _while)
    {
        yield return new WaitForSeconds(_while);
        listCollisionCd.Remove(_which);
    }

    public void SetRelease()
    {
        //这里要小心，不要在父类的坐标被重置以后赋值
        isReleased = true;
        if (bulletNext != null)
        {
            Debug.LogError(bulletNext.name);
            var child = ObjectPoolManager.Instance.GetBullet(bulletNext.name);
            child.transform.SetPositionAndRotation(transform.position, transform.rotation);
            if (bulletParentType == BulletParentType.PLAYER) child.GetComponent<Bullet>().Initialize(parentWeapon);
            else Debug.LogError("暂不支持此种子弹的派生");
        }
    }
}