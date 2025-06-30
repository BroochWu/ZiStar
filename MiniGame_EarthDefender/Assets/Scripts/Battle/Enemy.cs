using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("索引")]
    public GameObject hpBar;//血条栏组
    public GameObject hpLight;//会掉的那个血条
    public SpriteRenderer sprite;//形象


    [Header("识别")]
    public int enemyId; // 用于对象池回收

    //敌人配置
    public cfg.enemy.Enemy config;

    // 敌人属性
    WaitForSeconds HitWait = new WaitForSeconds(0.1f);//受击变色时间
    public int Damage { get; private set; }
    public int InitHp { get; private set; }//初始血量
    int currentHp;
    Transform Earth;
    int enemyLevel;




    float rotationSpeed;


    public void Initialize(cfg.enemy.Enemy enemy, int enemyLevel, Quaternion initDir, Portal parent)
    {
        transform.rotation = initDir;
        transform.position = parent.transform.position;

        config = enemy;
        this.enemyLevel = enemyLevel;
        enemyId = config.Id;

        rotationSpeed = enemy.AngleDamp / 10000f;

        //重置状态
        ResetState();

    }

    /// <summary>
    /// 重置敌人状态
    /// </summary>
    public void ResetState()
    {

        // 重置材质颜色
        if (sprite != null) sprite.material.color = Color.white;

        // 隐藏血条
        if (hpBar != null) hpBar.SetActive(false);


        //接下来重置数据
        if (config == null) return;

        //初始化血量
        InitHp = cfg.Tables.tb.EnemyLevel.Get(config.LevelId, enemyLevel).Hp;
        // 重置生命值
        currentHp = InitHp;
        //初始化伤害
        Damage = cfg.Tables.tb.EnemyLevel.Get(config.LevelId, enemyLevel).Damage;


    }


    void Awake()
    {

        if (hpBar == null)
        {
            hpBar = transform.Find("root/HpBar").gameObject;
            Debug.Log("未找到HpBar，重新加载");
        }
        hpBar.SetActive(false);



        if (hpLight == null)
        {
            hpLight = transform.Find("root/HpBar/Hp").gameObject;
            Debug.Log("未找到HpLight，重新加载");
        }


        if (sprite == null)
        {
            sprite = transform.Find("root/Sprite").GetComponent<SpriteRenderer>();
            Debug.Log("未找到sprite，重新加载");
        }

        Earth = Player.instance.rotationTarget.transform;
    }


    void Update()
    {
        if (config == null) return;


        //朝向地球的方向
        Utility.LookTarget2D(transform, Earth, rotationSpeed);



        //敌人最终目标是地球半径某处，抵达即停止并攻击
        if (Vector3.Distance(transform.position, Earth.position) >= cfg.Tables.tb.GlobalParam.Get("enemy_stop_distance").IntValue)
        {
            //Debug.Log("当前距离：" + Vector3.Distance(transform.position, Player.instance.rotationTarget.transform.position));
            transform.position += transform.up * Time.deltaTime * config.MultiMoveSpeed / 10000;

        }

    }

    public void TakeDamage(int damage)
    {
        if (config == null) return;


        StartCoroutine(OnHit());

        currentHp = BattleManager.Instance.CalDamage(damage, currentHp);
        if (currentHp > 0)
        {
            hpBar.SetActive(true);
            //Debug.Log("血条长度 " + HpLight.GetComponent<SpriteRenderer>().size);
            hpLight.GetComponent<SpriteRenderer>().size = (float)currentHp / InitHp * Vector2.right + Vector2.up;
        }
        else
        {
            //死亡
            OnDie();
        }
    }
    // void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (config == null) return;


    //     // 示例：当敌人被子弹击中
    //     if (other.CompareTag("PlayerBullet"))
    //     {

    //         StartCoroutine(OnHit());

    //         //Debug.Log(other.GetComponent<Bullet>().bulletDamage + "  " + currentHp);
    //         Bullet bullet = other.GetComponent<Bullet>();
    //         currentHp = BattleManager.Instance.CalDamage(bullet.bulletDamage, currentHp);
    //         if (currentHp > 0)
    //         {
    //             hpBar.SetActive(true);
    //             //Debug.Log("血条长度 " + HpLight.GetComponent<SpriteRenderer>().size);
    //             hpLight.GetComponent<SpriteRenderer>().size = (float)currentHp / InitHp * Vector2.right + Vector2.up;
    //         }
    //         else
    //         {
    //             //死亡
    //             OnDie();
    //         }
    //     }
    // }


    /// <summary>
    /// 敌人死亡时
    /// </summary>
    void OnDie()
    {
        hpBar.SetActive(false);
        BattleManager.Instance.ReturnEnemy(this);
        // Debug.LogWarning("后面记得把敌人写进对象池啊");
    }


    /// <summary>
    /// 受击协程表现
    /// </summary>
    /// <returns></returns>
    IEnumerator OnHit()
    {
        if (!enabled) yield break;
        
        sprite.material.color = Color.red;
        yield return HitWait;
        sprite.material.color = Color.white;
    }
}
