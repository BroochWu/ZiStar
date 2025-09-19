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

    private BulletParentType bulletParentType;
    public Weapon parentWeapon { get; private set; }

    public string bulletType;
    public int bulletDamage;
    private float lifeTime;
    private float timer;
    private float speed;

    public int bulletPenetrate;//子弹可碰撞次数（可穿透数量）
    public float bulletPenetrateInterval;//子弹穿透同一个单位造成连续伤害的伤害间隔

    public bool isReleased;//检测子弹是否已经被释放了
    public List<GameObject> listCollisionCd = new();

    public void Initialize(Weapon parent)
    {
        bulletParentType = BulletParentType.PLAYER;
        parentWeapon = parent;

        speed = parent.bulletSpeed;
        lifeTime = parent.bulletReleaseTime;
        transform.localScale = Vector3.one * parent.bulletScale / 10000f;


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
}