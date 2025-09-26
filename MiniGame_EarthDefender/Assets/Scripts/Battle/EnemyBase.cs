// EnemyBase.cs - 基类
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public EnemyUI enemyUI;

    [Header("敌人属性")]
    public int enemyId;
    public int enemyExp;

    // 静态缓存，所有敌人共享
    protected static Transform _earth;
    protected static int _enemyStopDisSqr;
    protected static float _hitDuration = 0.1f;


    // 保护字段，子类可访问
    protected cfg.enemy.Enemy _dynamicConfig;
    private cfg.enemy.EnemyLevel levelData;
    protected cfg.enemy.EnemyLevel _levelData => levelData ??= _dynamicConfig.LevelId_Ref;
    protected int _enemyLevel;
    protected cfg.Enums.Enemy.Type _enemyType;

    // 状态变量
    protected bool _isReleased;
    protected int _currentHp;
    protected float _moveSpeed;
    protected float _rotationSpeed;

    // 属性
    public int bulletId { get; protected set; }
    public int Damage { get; protected set; }
    public int InitHp { get; protected set; }
    public bool IsReleased { get => _isReleased; set => _isReleased = value; }
    //挂在这里的子弹
    public List<Bullet> mountBullets = new();

    // 抽象方法，子类必须实现
    public abstract void Initialize(cfg.enemy.Enemy enemy, int enemyLevel, Quaternion initDir, Portal parent);
    protected abstract void UpdateBehavior();




    // 虚方法，子类可重写
    protected virtual void Awake()
    {
        enemyUI = GetComponent<EnemyUI>();
        // 静态初始化只执行一次
        if (_earth == null)
        {
            _earth = Player.instance.rotationTarget.transform;
            _enemyStopDisSqr = cfg.Tables.tb.GlobalParam.Get("enemy_stop_distance").IntValue;
        }

    }

    protected virtual void FixedUpdate()
    {
        if (_isReleased) return;
        UpdateBehavior();
    }

    public virtual void TakeDamage(int damage, Weapon source)
    {
        //如果伤害量是0，不显示（后面可能会有无敌之类的显示？到时候再说吧）
        if (damage == 0) return;

        //如果已经释放，无法造成伤害
        if (_dynamicConfig == null || _isReleased) return;


        var dtx = ObjectPoolManager.Instance.GetVFX(VFXType.DAMAGETEXT);
        dtx.GetComponent<VFX>().InitializeAsDTX(damage, transform.position);

        // 统计伤害
        if (Player.instance.battleEquipedWeapon.ContainsKey(source))
        {
            Player.instance.battleEquipedWeapon[source] += BattleManager.Instance.CalDamage(damage, _currentHp, out int newHp);
            _currentHp = newHp;
        }
        else
        {
            Debug.LogWarning("未知的武器来源");
        }

        if (_currentHp <= 0)
        {
            OnDie();
        }
        else
        {
            // 显示血条并更新
            enemyUI.hpBar.SetActive(true);
            var hpRenderer = enemyUI.hpLight.GetComponent<SpriteRenderer>();
            hpRenderer.size = new Vector2((float)_currentHp / InitHp, hpRenderer.size.y);

            // 启动受击效果
            StartCoroutine(enemyUI.OnHitEffect(_hitDuration));
        }
    }

    // 在 OnDie 方法中确保清理
    protected virtual void OnDie()
    {
        // 先清理所有附着子弹
        UnregistAllMountBullets();

        enemyUI.hpBar.SetActive(false);

        // 爆炸效果
        var bomb = ObjectPoolManager.Instance.GetVFX(VFXType.BOMB);
        bomb.GetComponent<VFX>().InitializeAsBomb(transform.position);

        ObjectPoolManager.Instance.ReleaseEnemy(this);
    }

    // 注册挂载的子弹（跟随本enemy）
    public void RegistMountBullets(Bullet bullet)
    {
        mountBullets.Add(bullet);

        bullet.transform.SetParent(enemyUI.battleObjContainer);
        bullet.transform.localPosition = Vector3.zero;
        bullet.OnAttachEnemy(this);

    }


    // 修改 UnregistAllMountBullets 方法
    public void UnregistAllMountBullets()
    {
        // 先创建副本避免在遍历时修改集合
        var bulletsToRelease = new List<Bullet>(mountBullets);
        mountBullets.Clear();

        foreach (Bullet bullet in bulletsToRelease)
        {
            if (bullet != null)
            {
                bullet.ReleaseOnEnemy();
            }
        }
    }


    // 新增方法：单独卸载一个子弹
    public void UnregistMountBullet(Bullet bullet)
    {
        mountBullets.Remove(bullet);
    }


    // protected IEnumerator OnHitEffect()
    // {
    //     sprite.sortingOrder = 50;
    //     _spriteMaterial.color = Color.red;

    //     yield return new WaitForSeconds(_hitDuration);

    //     sprite.sortingOrder = _initOrder;
    //     _spriteMaterial.color = Color.white;
    // }

    protected virtual void ResetAttributes()
    {
        // mountBullets.Clear();

        var baseDamage = (int)(_levelData.Damage + (_enemyLevel - 1) * _levelData.DamageMulti / 10000f);
        var baseHp = (int)(_levelData.Hp + (_enemyLevel - 1) * _levelData.HpMulti / 10000f);

        // Damage = baseDamage;
        // InitHp = baseHp;

        // 计算属性
        //杂兵每过X秒成长N%

        //if判断敌人类型

        float additionMulti = (int)(BattleManager.Instance.GameTime / 5) * 1f;

        Damage = (int)(baseDamage * (1 + additionMulti));
        InitHp = (int)(baseHp * (1 + additionMulti));

        _currentHp = InitHp;
        enemyUI.ResetAttributes();
    }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
    }
    public void SetBasicEssentials(cfg.enemy.Enemy enemyBasic)
    {
        _dynamicConfig = enemyBasic;
        enemyId = _dynamicConfig.Id;
        _enemyType = _dynamicConfig.EnemyType;
        enemyExp = _dynamicConfig.exp;
        bulletId = _dynamicConfig.Id;

        // 预计算速度值
        _moveSpeed = _dynamicConfig.MultiMoveSpeed * 0.0001f;
        _rotationSpeed = _dynamicConfig.MultiAngle * 0.0001f;
    }
}