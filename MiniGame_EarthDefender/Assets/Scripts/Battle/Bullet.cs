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
    public int bulletState = 0;//子弹阶段，用来判断效果生成对象

    private BulletParentType bulletParentType;
    private float timer;
    private Vector3 currentDirection;
    private cfg.Enums.Bullet.TrackType trackType = cfg.Enums.Bullet.TrackType.NULL;

    // public bool canCollide = true;//子弹是否可以碰撞
    // public Bullet bulletFormer;//生成之前的子弹


    void Start()
    {
        // initScale = transform.localScale;
        currentDirection = transform.up;
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
        SetCfgBasicData();
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
        SetCfgBasicData();
        //来自表
    }

    /// <summary>
    /// 设置表属性
    /// </summary>
    void SetCfgBasicData()
    {
        speed = _bulletConfig.Speed;
        baseBulletPenetrate = _bulletConfig.PenetrateCount;
        bulletPenetrateInterval = _bulletConfig.PenetrateSep;
        lifeTime = _bulletConfig.LifeTime;
        trackType = _bulletConfig.TrackType;
    }

    void Update()
    {
        //根据跟踪类型，决定跟踪方式
        switch (trackType)
        {
            case cfg.Enums.Bullet.TrackType.SLERP:
                BulletTrack();
                break;
            case cfg.Enums.Bullet.TrackType.NULL:
                transform.Translate(currentDirection * speed * Time.deltaTime);
                break;
            default:
                break;
        }
        // 移动
        //更新旋转使其朝向移动方向

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

    //子弹跟踪
    void BulletTrack()
    {
        //如果生成时间小于跟踪开始时间，则不跟踪
        if (timer < _bulletConfig.TrackStartTime)
            return;

        // 寻找目标
        if (BattleManager.Instance.activeEnemys.Count > 0)
        {
            Vector3 targetPosition = BattleManager.Instance.activeEnemys[0].transform.position;
            Vector3 targetDirection = (targetPosition - transform.position).normalized;

            // 使用旋转步长来平滑转向
            float rotateSpeed = 5f; // 控制转向速度，单位是度/秒
            float step = rotateSpeed * Time.deltaTime;
            currentDirection = Vector3.RotateTowards(currentDirection, targetDirection, step, 0.02f);

            // 也可以使用插值，但RotateTowards更易于控制最大旋转角度
            // currentDirection = Vector3.Slerp(currentDirection, targetDirection, rotateSpeed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(Vector3.forward, currentDirection);
            transform.Translate(currentDirection * speed * Time.deltaTime, Space.World);
        }

    }

    /// <summary>
    /// 重置子弹状态
    /// </summary>
    public void ResetState()
    {
        //重置初始朝向
        currentDirection = Vector3.up;

        //重置可穿透数量
        if (parentWeapon != null)
        {
            finalBulletPenetrate = baseBulletPenetrate;
        }
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

    //开启协程记录每个CD
    IEnumerator CorAddToListCollisionCD(GameObject _which, float _while)
    {
        yield return new WaitForSeconds(_while);
        listCollisionCd.Remove(_which);
    }

    //设置为子弹已回收
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