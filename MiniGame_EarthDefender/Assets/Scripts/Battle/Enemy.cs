using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class Enemy : MonoBehaviour
{
    enum EnemyState { MOVE, ATTACK }

    // 静态缓存，所有敌人共享
    private static Transform _earth;
    private static int _enemyStopDisSqr; // 使用平方距离避免开方运算
    private static float _hitDuration = 0.1f;
    private static WaitForSeconds attackSep = new WaitForSeconds(0.5f);

    [Header("组件引用")]
    public GameObject hpBar;
    public GameObject hpLight;
    public SpriteRenderer sprite;

    [Header("敌人属性")]
    public int enemyId;
    public cfg.enemy.Enemy config;
    private int enemyLevel;
    private string bulletType;


    // 状态变量
    public bool isReleased;
    private EnemyState _state;
    private int _currentHp;
    private float _moveSpeed;
    private float _rotationSpeed;
    private int _initOrder = 20;
    private Material _spriteMaterial; // 缓存材质

    // 属性优化
    public int Damage { get; private set; }
    public int InitHp { get; private set; }

    void Awake()
    {
        // 静态初始化只执行一次
        if (_earth == null)
        {
            _earth = Player.instance.rotationTarget.transform;
            int stopDistance = cfg.Tables.tb.GlobalParam.Get("enemy_stop_distance").IntValue;
            _enemyStopDisSqr = stopDistance * stopDistance; // 预计算平方值
        }

        // 缓存组件引用
        if (hpBar == null) hpBar = transform.Find("root/HpBar").gameObject;
        if (hpLight == null) hpLight = transform.Find("root/HpBar/Hp").gameObject;
        if (sprite == null) sprite = transform.Find("root/Sprite").GetComponent<SpriteRenderer>();

        hpBar.SetActive(false);
        _spriteMaterial = sprite.material; // 缓存材质实例
    }

    public void Initialize(cfg.enemy.Enemy enemy, int enemyLevel, Quaternion initDir, Portal parent)
    {
        isReleased = false;
        sprite.sortingOrder = _initOrder;
        transform.SetPositionAndRotation(parent.transform.position, initDir);

        config = enemy;
        this.enemyLevel = enemyLevel;
        enemyId = config.Id;
        bulletType = config.PrefabBullet;

        // 预计算速度值
        _moveSpeed = config.MultiMoveSpeed * 0.0001f;
        _rotationSpeed = enemy.MultiAngle * 0.0001f;

        ResetState();
    }

    public void ResetState()
    {
        _state = EnemyState.MOVE;
        _spriteMaterial.color = Color.white;
        hpBar.SetActive(false);

        if (config == null) return;

        // 预计算属性
        var levelData = cfg.Tables.tb.EnemyLevel.Get(config.LevelId, enemyLevel);
        InitHp = levelData.Hp;
        _currentHp = InitHp;
        Damage = levelData.Damage;
    }

    void FixedUpdate()
    {
        if (isReleased || _state != EnemyState.MOVE) return;

        // 移动逻辑
        transform.position += transform.up * _moveSpeed;

        // // 旋转朝向（每帧执行）
        // LookTarget2D(true);

        Utility.LookTarget2D(transform, _earth, _rotationSpeed, true);


        // Vector2 direction = _earth.position - transform.position;
        // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
        // Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
        // transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed);

        // 优化距离检查：每5帧检查一次，每次检查顺便把层级-1，以让受击的永远在最前面
        if (Time.frameCount % 5 == 0)
        {
            if ((transform.position - _earth.position).sqrMagnitude <= _enemyStopDisSqr)
            {
                _state = EnemyState.ATTACK;
                StartCoroutine(Attack());
            }

            sprite.sortingOrder = Mathf.Max(sprite.sortingOrder - 1, _initOrder);
        }



    }

    /// <summary>
    /// 开始攻击协程
    /// </summary>
    /// <returns></returns>
    IEnumerator Attack()
    {
        while (true)
        {
            var obj = ObjectPoolManager.Instance.GetBullet(bulletType);
            obj.GetOrAddComponent<Bullet>().Initialize(this);

            yield return attackSep;
        }
    }

    public void TakeDamage(int damage)
    {
        if (config == null || isReleased) return;

        _currentHp = BattleManager.Instance.CalDamage(damage, _currentHp);

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

            // 直接启动受击效果协程（少量实例）
            StartCoroutine(OnHitEffect());
        }
    }

    void OnDie()
    {
        hpBar.SetActive(false);
        ObjectPoolManager.Instance.ReleaseEnemy(gameObject);
        BattleManager.Instance.UnregisterEnemy(this);
    }

    /// <summary>
    /// 优化的2D朝向方法
    /// </summary>
    private void LookTarget2D(bool _stopWhileLook)
    {
        // 计算方向向量（无平方根计算）
        Vector3 direction = _earth.position - transform.position;

        // 计算旋转角度（使用Atan2）
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        // 计算当前角度差异
        float angleDiff = Mathf.DeltaAngle(transform.eulerAngles.z, targetAngle);

        // 检查是否需要停止
        if (_stopWhileLook && Mathf.Abs(angleDiff) <= 0.1f)
        {
            return;
        }

        // 应用旋转（使用更高效的LerpAngle）
        float newAngle = Mathf.LerpAngle(
            transform.eulerAngles.z,
            targetAngle,
            _rotationSpeed * Time.fixedDeltaTime
        );

        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }


    IEnumerator OnHitEffect()
    {
        sprite.sortingOrder = 50;
        _spriteMaterial.color = Color.red;

        // 使用静态缓存的等待时间
        yield return new WaitForSeconds(_hitDuration);

        sprite.sortingOrder = _initOrder;
        _spriteMaterial.color = Color.white;
    }
}