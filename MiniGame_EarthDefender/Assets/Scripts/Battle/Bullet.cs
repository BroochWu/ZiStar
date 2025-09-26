using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    enum BulletParentType//子弹来源于玩家还是敌人
    {
        PLAYER,
        ENEMY
    }
    enum BulletDestroyReason
    {
        NULL,
        LIFETIME,//到达时间
        HIT,//碰撞
        ENEMYDIE //敌人死亡
    }

    private cfg.weapon.Bullet _bulletConfig;
    private const float ROTATE_SPEED = 2f; // 控制转向速度，单位是度/秒


    //基础设置
    private bool _isReleased;
    public bool isReleased//检测子弹是否已经被释放了
    {
        get => _isReleased;
        private set => _isReleased = value;
    }
    private cfg.Enums.Bullet.Container _parentContainer = cfg.Enums.Bullet.Container.NULL;
    public cfg.Enums.Bullet.Container parentContainer => _parentContainer == cfg.Enums.Bullet.Container.NULL ? _bulletConfig.ParentContainer : _parentContainer;


    private BulletParentType bulletParentType;//子弹来源
    private BulletDestroyReason bulletDestroyReason = BulletDestroyReason.NULL;//销毁原因
    private cfg.Enums.Bullet.TrackType trackType = cfg.Enums.Bullet.TrackType.NULL;//跟踪模式


    public int bulletId;
    public Weapon parentWeapon { get; private set; }


    public bool isSingleCol//是否单体伤害
    {
        get { return parentContainer == cfg.Enums.Bullet.Container.ENEMY; }
    }


    //基础数值
    private int baseBulletPenetrate;//子弹可碰撞次数（可穿透数量）



    //最终值
    public float lifeTime;
    public float speed;
    public float bulletPenetrateInterval;//子弹穿透同一个单位造成连续伤害的伤害间隔
    public float bulletPenetrateDamageMulti;//子弹穿透单位的伤害衰减
    private int bulletBaseDamage;
    public int bulletFinalDamage
    {
        get
        {
            return
            (int)(bulletBaseDamage
            * finalDamagePenetrateMulti
            * (float)(_bulletConfig.DamageMulti / 10000f));
        }
    }
    private float finalDamagePenetrateMulti => Mathf.Max(Mathf.Pow(bulletPenetrateDamageMulti, hitTime), 0.4f);//伤害递减
    public bool isInfinityPenetrate { get; private set; }//是否无敌

    //动态内容
    public List<GameObject> listCollisionCd = new();
    public int bulletState = 0;//子弹阶段，用来判断效果生成对象
    private float timer;//子弹持续时间
    private Vector3 currentDirection;//当前朝向
    private SimpleCollider bulletCol;
    private int hitTime;//发生碰撞的次数

    public int finalBulletPenetrate;//子弹可碰撞次数（可穿透数量）
    [SerializeField] public GameObject destroyEnemy;//导致子弹销毁的敌人
    [SerializeField] public EnemyBase attachedEnemy;//子弹吸附单位


    // public bool canCollide = true;//子弹是否可以碰撞
    // public Bullet bulletFormer;//生成之前的子弹

    void Start()
    {
        // initScale = transform.localScale;
        if (!CompareTag("PlayerBullet")) Debug.LogError("子弹的标签不对，这样无法碰撞");
        currentDirection = transform.up;
    }

    // void OnDisable()
    // {
    //     if (isReleased)
    //     {
    //         Debug.LogWarning("检测到隐藏时未释放本子弹，已释放");
    //         ObjectPoolManager.Instance.ReleaseBullet(gameObject);
    //     }
    // }
    /// <summary>
    /// 由武器直接生成
    /// </summary>
    /// <param name="parent"></param>
    public void Initialize(Weapon parent)
    {
        isReleased = false;
        timer = 0f;
        bulletCol = GetComponent<SimpleCollider>();

        //来自父
        bulletParentType = BulletParentType.PLAYER;

        parentWeapon = parent;
        bulletBaseDamage = parent.finalAttack;

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
        _bulletConfig = cfg.Tables.tb.Bullet.Get(parent.bulletId);

        bulletParentType = BulletParentType.ENEMY;
        transform.SetPositionAndRotation(parent.transform.position, parent.transform.rotation);
        bulletBaseDamage = parent.Damage;


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
        bulletCol = GetComponent<SimpleCollider>();

        //来自父
        bulletParentType = BulletParentType.PLAYER;

        parentWeapon = _parent;
        bulletBaseDamage = _parent.finalAttack;


        _bulletConfig = _bullet;//子弹是重写的
                                // if (_bulletConfig.ParentContainer == cfg.Enums.Bullet.Container.ENEMY)
                                // {
                                // }


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
        bulletPenetrateDamageMulti = _bulletConfig.PenetrateDamageMulti / 10000f;

        isInfinityPenetrate = _bulletConfig.PenetrateCount == -1;

        bulletCol.enabled = false;
        // Debug.LogError($"{bulletType},BD:{bulletDamage}");

        InitData();
    }

    void Update()
    {
        //子弹未加载完成之前不启用
        if (_bulletConfig == null) return;

        //根据跟踪类型，决定跟踪方式
        switch (trackType)
        {
            case cfg.Enums.Bullet.TrackType.SLERP:
                try
                {
                    MoveAsTrack();

                }
                catch (Exception ex)
                {
                    Debug.LogWarning("欲进行跟踪，但因为未知原因，跟踪失败了" + ex);
                    MoveAsNormal();
                }
                break;
            case cfg.Enums.Bullet.TrackType.NULL:
                MoveAsNormal();
                break;
            default:
                break;
        }
        // 移动
        //更新旋转使其朝向移动方向

        if (!isReleased)
        {
            timer += Time.deltaTime;

            if (timer >= _bulletConfig.UncolTime && _bulletConfig.CanCol && bulletCol.enabled == false)
            {
                bulletCol.enabled = true;//初始false
            }

            // 检查是否超过生存时间
            if (timer >= lifeTime)
            {
                bulletDestroyReason = BulletDestroyReason.LIFETIME;
                ObjectPoolManager.Instance.ReleaseBullet(gameObject);


                switch (bulletParentType)
                {
                    case BulletParentType.PLAYER:
                        break;

                    case BulletParentType.ENEMY:

                        BattleManager.Instance.CalDamageEarthSuffer(bulletFinalDamage);
                        break;

                    default:
                        Debug.LogError("子弹没有父类？");
                        break;
                }


            }

        }
    }

    //子弹跟踪
    void MoveAsTrack()
    {
        //如果生成时间小于跟踪开始时间，则不跟踪，往前跑
        if (timer < _bulletConfig.TrackStartTime || BattleManager.Instance.activeEnemys.Count <= 0)
        {
            MoveAsNormal();
            return;
        }
        else
        {
            // 寻找目标
            var trackTarget = BattleManager.Instance.activeEnemys[0];
            if (trackTarget == null)
            {
                Debug.LogWarning("未能找到跟踪目标");
                MoveAsNormal();
                return;
            }
            Vector3 targetPosition = trackTarget.transform.position;
            Vector3 targetDirection = (targetPosition - transform.position).normalized;

            // 使用旋转步长来平滑转向
            float step = ROTATE_SPEED * Time.deltaTime;
            currentDirection = Vector3.RotateTowards(currentDirection, targetDirection, step, 0.02f);

            // 也可以使用插值，但RotateTowards更易于控制最大旋转角度
            // currentDirection = Vector3.Slerp(currentDirection, targetDirection, rotateSpeed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(Vector3.forward, currentDirection);
            transform.Translate(currentDirection * speed * Time.deltaTime, Space.World);
        }

    }

    void MoveAsNormal()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    /// <summary>
    /// 初始化数据（Initialize的时候调用）
    /// </summary>
    void InitData()
    {

        //重置可穿透数量
        if (parentWeapon != null)
        {
            finalBulletPenetrate = baseBulletPenetrate;
        }

        //重置已撞击次数
        hitTime = 0;
        //重置导致死亡的单位
        destroyEnemy = null;
    }

    /// <summary>
    /// 重置子弹数据(release回收的时候调用)
    /// </summary>
    public void ResetData()
    {
        InitData();


        //重置初始朝向
        // currentDirection = Vector3.up;

        gameObject.SetActive(false);
        // transform.position = Vector3.zero;
        // transform.rotation = Quaternion.identity;

        //清空冷却池
        listCollisionCd.Clear();

        //重置跟踪单位
        attachedEnemy = null;

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

    // 修改 SetRelease 方法中的附着逻辑
    public void SetRelease()
    {
        isReleased = true;

        // 根据不同的结束类型，判断生成什么子弹
        cfg.weapon.Bullet nextBullet = null;
        int rowCount = 0;

        switch (bulletDestroyReason)
        {
            case BulletDestroyReason.LIFETIME:
                nextBullet = _bulletConfig.NextLifetimeBullet_Ref;
                rowCount = _bulletConfig.NextLifetimeBulletRow;
                break;
            case BulletDestroyReason.HIT:
                nextBullet = _bulletConfig.NextColBullet_Ref;
                rowCount = _bulletConfig.NextColBulletRow;
                break;
            case BulletDestroyReason.ENEMYDIE:
                nextBullet = null;
                break;
        }

        // 尝试生成子子弹
        if (nextBullet != null && bulletParentType == BulletParentType.PLAYER)
        {
            for (int i = 0; i < rowCount; i++)
            {
                var child = ObjectPoolManager.Instance.GetBullet(nextBullet.Id);
                var childComponent = child.GetComponent<Bullet>();

                switch (nextBullet.ParentContainer)
                {
                    case cfg.Enums.Bullet.Container.NORMAL:
                        //一般的，则继承父级的位置和旋转
                        var angleOffset = 360 / rowCount * (i - (rowCount - 1) / 2f);
                        child.transform.SetPositionAndRotation(transform.position,
                            transform.rotation * Quaternion.Euler(0, 0, angleOffset));
                        break;

                    case cfg.Enums.Bullet.Container.ENEMY:
                        //父级如果是敌人的，需要判断附着在哪个敌人身上
                        //获取碰撞物体的enemy组件
                        if (destroyEnemy == null)
                        {
                            Debug.LogWarning("目标不存在，无法附着");
                            continue;// 跳过这个子弹的初始化
                        }

                        var enemyComponent = destroyEnemy.GetComponent<EnemyBase>();
                        if (enemyComponent == null)
                        {
                            Debug.LogWarning("目标没脚本");
                            continue;// 跳过这个子弹的初始化
                        }

                        if (!destroyEnemy.activeInHierarchy
                                || enemyComponent.IsReleased)
                        {
                            Debug.LogWarning("目标已死亡，无法附着");
                            ObjectPoolManager.Instance.ReleaseBullet(child);
                            continue; // 跳过这个子弹的初始化
                        }

                        //成功附着在目标身上
                        enemyComponent.RegistMountBullets(childComponent);
                        break;
                }

                //初始化子物体
                childComponent.Initialize(parentWeapon, nextBullet);
                childComponent.bulletState = bulletState + 1;
            }
        }

        ResetData();

    }



    // 修改 OnHIt 方法，记录附着的敌人
    public void OnHIt(GameObject _colObj)
    {
        // 注册在目标的子弹挂载列表
        // if (parentContainer == cfg.Enums.Bullet.Container.ENEMY)
        // {
        //     if (attachedEnemy = null)
        //     {
        //         var enemyComponent = _colObj.GetComponent<EnemyBase>();
        //         enemyComponent.RegistMountBullets(this);
        //         attachedEnemy = enemyComponent; // 记录附着的敌人
        //     }
        // }

        hitTime += 1;

        if (!isInfinityPenetrate && hitTime >= finalBulletPenetrate)
        {
            bulletDestroyReason = BulletDestroyReason.HIT;
            destroyEnemy = _colObj;

            ObjectPoolManager.Instance.ReleaseBullet(gameObject);
        }
    }

    // 修改 ReleaseOnEnemy 方法
    public void ReleaseOnEnemy()
    {
        // 先解除父子关系
        if (attachedEnemy != null)
        {
            attachedEnemy.UnregistMountBullet(this);
            attachedEnemy = null;
        }
        // transform.SetParent(BattleManager.Instance.BulletsPath);

        bulletDestroyReason = BulletDestroyReason.ENEMYDIE;
        ObjectPoolManager.Instance.ReleaseBullet(gameObject);
    }


    public void OnAttachEnemy(EnemyBase _enemy)
    {
        attachedEnemy = _enemy;
    }

    // // 新增方法：从敌人身上分离
    // public void DetachFromEnemy()
    // {
    //     if (attachedEnemy != null)
    //     {
    //     }
    // }

}