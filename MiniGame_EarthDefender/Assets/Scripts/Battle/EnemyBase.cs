// EnemyBase.cs - 基类
using UnityEngine;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour
{
    // 公共字段和属性
    [Header("组件引用")]
    public GameObject hpBar;
    public GameObject hpLight;
    public SpriteRenderer sprite;

    [Header("敌人属性")]
    public int enemyId;
    public int enemyExp;

    // 静态缓存，所有敌人共享
    protected static Transform _earth;
    protected static int _enemyStopDisSqr;
    protected static float _hitDuration = 0.1f;

    // 保护字段，子类可访问
    protected cfg.enemy.Enemy _dynamicConfig;
    protected cfg.enemy.EnemyLevel _levelData;
    protected int _enemyLevel;
    protected cfg.Enums.Enemy.Type _enemyType;

    // 状态变量
    protected bool _isReleased;
    protected int _currentHp;
    protected float _moveSpeed;
    protected float _rotationSpeed;
    protected int _initOrder = 20;
    protected Material _spriteMaterial;

    // 属性
    public int Damage { get; protected set; }
    public int InitHp { get; protected set; }
    public bool IsReleased { get => _isReleased; set => _isReleased = value; }

    // 抽象方法，子类必须实现
    public abstract void Initialize(cfg.enemy.Enemy enemy, int enemyLevel, Quaternion initDir, Portal parent);
    protected abstract void UpdateBehavior();

    // 虚方法，子类可重写
    protected virtual void Awake()
    {
        // 静态初始化只执行一次
        if (_earth == null)
        {
            _earth = Player.instance.rotationTarget.transform;
            _enemyStopDisSqr = cfg.Tables.tb.GlobalParam.Get("enemy_stop_distance").IntValue;
        }

        // 缓存组件引用
        if (hpBar == null) hpBar = transform.Find("root/HpBar").gameObject;
        if (hpLight == null) hpLight = transform.Find("root/HpBar/Hp").gameObject;
        if (sprite == null) sprite = transform.Find("root/Sprite").GetComponent<SpriteRenderer>();

        hpBar.SetActive(false);
        _spriteMaterial = sprite.material;
    }

    protected virtual void FixedUpdate()
    {
        if (_isReleased) return;
        UpdateBehavior();
    }

    public virtual void TakeDamage(int damage, Weapon source)
    {
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
            hpBar.SetActive(true);
            var hpRenderer = hpLight.GetComponent<SpriteRenderer>();
            hpRenderer.size = new Vector2((float)_currentHp / InitHp, hpRenderer.size.y);

            // 启动受击效果
            StartCoroutine(OnHitEffect());
        }
    }

    protected virtual void OnDie()
    {
        hpBar.SetActive(false);
        // 爆炸效果
        var bomb = ObjectPoolManager.Instance.GetVFX(VFXType.BOMB);
        bomb.GetComponent<VFX>().InitializeAsBomb(transform.position);

        ObjectPoolManager.Instance.ReleaseEnemy(this);
        BattleManager.Instance.UnregisterEnemy(this);
    }

    protected IEnumerator OnHitEffect()
    {
        sprite.sortingOrder = 50;
        _spriteMaterial.color = Color.red;

        yield return new WaitForSeconds(_hitDuration);

        sprite.sortingOrder = _initOrder;
        _spriteMaterial.color = Color.white;
    }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
    }
    public  void SetBasicEssentials(cfg.enemy.Enemy enemyBasic)
    {
        _dynamicConfig = enemyBasic;
        enemyId = _dynamicConfig.Id;
        _enemyType = _dynamicConfig.EnemyType;
        enemyExp = _dynamicConfig.exp;

        // 预计算速度值
        _moveSpeed = _dynamicConfig.MultiMoveSpeed * 0.0001f;
        _rotationSpeed = _dynamicConfig.MultiAngle * 0.0001f;
    }
}