using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject HpBar;//血条栏组
    public GameObject HpLight;//会掉的那个血条
    public SpriteRenderer sprite;//形象
    int enemyLevel;
    WaitForSeconds HitWait = new WaitForSeconds(0.1f);
    public int Damage { get; private set; }
    public int InitHp { get; private set; }
    int currentHp;


    GameObject prefab;
    Transform Earth;

    float rotationSpeed;
    cfg.enemy.Enemy config;


    public void Initialize(cfg.enemy.Enemy enemy, int enemyLevel)
    {
        config = enemy;
        this.enemyLevel = enemyLevel;

        rotationSpeed = enemy.AngleDamp / 10000f;
        this.prefab = Resources.Load<GameObject>($"Prefabs/Enemys/{enemy.Prefab}");
    }

    void Awake()
    {
        Earth = Player.instance.rotationTarget.transform;
    }

    void Start()
    {

        if (HpBar == null)
        {
            HpBar = transform.Find("HpBar").gameObject;
            Debug.Log("未找到HpBar，重新加载");
        }
        HpBar.SetActive(false);



        if (HpLight == null)
        {
            HpLight = transform.Find("HpBar/Hp").gameObject;
            Debug.Log("未找到HpLight，重新加载");
        }


        if (sprite == null)
        {
            sprite = transform.Find("Sprite").GetComponent<SpriteRenderer>();
            Debug.Log("未找到sprite，重新加载");
        }



        //初始化伤害
        Damage = cfg.Tables.tb.EnemyLevel.Get(config.LevelId, enemyLevel).Damage;
        //初始化血量
        InitHp = cfg.Tables.tb.EnemyLevel.Get(config.LevelId, enemyLevel).Hp;
        currentHp = InitHp;


    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"检测到鼠标左键，此时的{HpBar.activeInHierarchy}");
            HpBar.SetActive(!HpBar.activeInHierarchy);
        }


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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (config == null) return;


        // 示例：当敌人被子弹击中
        if (other.CompareTag("PlayerBullet"))
        {

            StartCoroutine(OnHit());

            //Debug.Log(other.GetComponent<Bullet>().bulletDamage + "  " + currentHp);
            currentHp = BattleManager.Instance.CalDamage(other.GetComponent<Bullet>().bulletDamage, currentHp);
            if (currentHp > 0)
            {
                HpBar.SetActive(true);
                //Debug.Log("血条长度 " + HpLight.GetComponent<SpriteRenderer>().size);
                HpLight.GetComponent<SpriteRenderer>().size = (float)currentHp / InitHp * Vector2.right + Vector2.up;
            }
            else
            {
                //死亡
                OnDie();
            }
        }
    }

    /// <summary>
    /// 敌人死亡时
    /// </summary>
    void OnDie()
    {
        HpBar.SetActive(false);
        Destroy(gameObject);
        Debug.LogWarning("后面记得把敌人写进对象池啊");
    }


    /// <summary>
    /// 受击协程表现
    /// </summary>
    /// <returns></returns>
    IEnumerator OnHit()
    {
        sprite.material.color = Color.red;
        yield return HitWait;
        sprite.material.color = Color.white;
    }
}
