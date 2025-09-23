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

    private cfg.weapon.Bullet _bulletConfig;
    //基础值
    private int baseBulletPenetrate;//子弹可碰撞次数（可穿透数量）

    //最终值
    public float lifeTime;
    public float speed;
    public int finalBulletPenetrate;//子弹可碰撞次数（可穿透数量）
    public float bulletPenetrateInterval;//子弹穿透同一个单位造成连续伤害的伤害间隔
    public int bulletDamage;
    public Weapon parentWeapon { get; private set; }
    public string bulletType;
    public bool isReleased;//检测子弹是否已经被释放了
    public List<GameObject> listCollisionCd = new();
    private BulletParentType bulletParentType;
    private float timer;
    public int bulletState = 0;//子弹阶段，用来判断效果生成对象
    // private Vector3 initScale;

    // public bool canCollide = true;//子弹是否可以碰撞
    // public Bullet bulletFormer;//生成之前的子弹


    void Start()
    {
        // initScale = transform.localScale;
    }
    /// <summary>
    /// 由武器直接生成
    /// </summary>
    /// <param name="parent"></param>
    public void Initialize(Weapon parent)
    {
        isReleased = false;
        timer = 0f;

        //来自父
        bulletParentType = BulletParentType.PLAYER;

        parentWeapon = parent;
        bulletDamage = parent.finalAttack;

        _bulletConfig = parent.bulletConfig;

        //来自表
        speed = _bulletConfig.Speed;
        baseBulletPenetrate = _bulletConfig.PenetrateCount;
        bulletPenetrateInterval = _bulletConfig.PenetrateSep;
        lifeTime = _bulletConfig.LifeTime;
    }

    /// <summary>
    /// 由敌人生成
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    public void Initialize<T>(T parent) where T : EnemyBase
    {
        isReleased = false;
        timer = 0f;

        bulletParentType = BulletParentType.ENEMY;
        transform.SetPositionAndRotation(parent.transform.position, parent.transform.rotation);
        bulletDamage = parent.Damage;


        speed = 1;
        lifeTime = 1.5f;
        //敌人的子弹移动后自动销毁就行，不需要碰撞
    }

    /// <summary>
    /// 派生子弹，但继承最初的父类
    /// </summary>
    /// <param name="_parent"></param>
    /// <param name="_bullet"></param>
    public void Initialize(Weapon _parent, cfg.weapon.Bullet _bullet)
    {
        isReleased = false;
        timer = 0f;

        //来自父
        bulletParentType = BulletParentType.PLAYER;

        parentWeapon = _parent;
        bulletDamage = _parent.finalAttack;

        _bulletConfig = _bullet;//子弹是重写的

        //来自表
        speed = _bulletConfig.Speed;
        baseBulletPenetrate = _bulletConfig.PenetrateCount;
        bulletPenetrateInterval = _bulletConfig.PenetrateSep;
        lifeTime = _bulletConfig.LifeTime;
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
            finalBulletPenetrate = baseBulletPenetrate;
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
        if (_bulletConfig.NextBullet != null)
        {

            var a = _bulletConfig.NextBullet_Ref;
            if (a != null)
            {
                // Debug.LogError(bulletNext.name);
                var child = ObjectPoolManager.Instance.GetBullet(a.BulletPrefab);
                child.transform.SetPositionAndRotation(transform.position, transform.rotation);

                if (bulletParentType == BulletParentType.PLAYER)
                {
                    var childComponent = child.GetComponent<Bullet>();
                    childComponent.Initialize(parentWeapon, a);
                    childComponent.bulletState = bulletState + 1;
                }

                else Debug.LogError("暂不支持此种子弹的派生");
            }
            else
            {
                throw new System.Exception("检查配置！");
            }
        }
    }
}