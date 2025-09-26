using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    public static CollisionManager Instance;

    public Rect worldBounds = new Rect(-100, -100, 200, 200);
    private QuadTree quadTree;

    private List<GameObject> enemies = new List<GameObject>();
    private List<GameObject> bullets = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        quadTree = new QuadTree(0, worldBounds);
    }

    void Initialize()
    {

    }

    void LateUpdate()
    {
        // 每帧重建四叉树（或增量更新）
        quadTree.Clear();

        // 插入所有对象
        foreach (var enemy in enemies) quadTree.Insert(enemy);
        foreach (var bullet in bullets) quadTree.Insert(bullet);

        // 检测碰撞
        DetectCollisions();
    }

    void DetectCollisions()
    {
        // 检测子弹与敌人的碰撞
        foreach (var bullet in bullets)
        {
            //如果子弹为隐藏态，不检测碰撞
            if (!bullet.activeInHierarchy)
                continue;

            // 获取子弹的碰撞体
            // 如果子弹没有碰撞体，不检测
            var bulletCollider = bullet.GetComponent<SimpleCollider>();
            if (bulletCollider == null) continue;

            // 获取子弹的边界框
            Rect bulletBounds = bulletCollider.GetBounds();


            //如果子弹已经释放了，不检测
            var bulletConfig = bullet.GetComponent<Bullet>();
            if (bulletConfig.isReleased) return;

            // 获取可能碰撞的敌人对象
            var potentialCollisions = new List<GameObject>();
            quadTree.Retrieve(potentialCollisions, bulletBounds);

            // 如果是非单体伤害，找到列表
            // 如果是单体伤害，锁定单体目标
            if (bulletConfig.isSingleCol)
            {
                switch (bulletConfig.trackType)
                {
                    case cfg.Enums.Bullet.TrackType.SLERP:
                        if (bulletConfig.attachedEnemy == null)
                        {
                            //如果没有挂载目标，则黏住第一个碰撞的单位
                            foreach (var a in potentialCollisions)
                            {
                                var component = a.GetComponent<EnemyBase>();
                                if (component == null)
                                {
                                    //如果ta没有enemy脚本（不能碰撞），就判断下一个（应该也不会触发）
                                    continue;
                                }
                                else if (!component.IsReleased)
                                {
                                    //否则只要能碰撞，就直接打断
                                    potentialCollisions = new List<GameObject>() { potentialCollisions[0] };
                                    break;
                                }
                            }
                        }
                        else
                        {
                            //如果有挂载目标，只对挂载目标造成伤害
                            potentialCollisions = new List<GameObject>() { bulletConfig.attachedEnemy.gameObject };
                        }
                        break;
                    case cfg.Enums.Bullet.TrackType.LASER:
                        return;
                        //     if (bulletConfig.trackTarget != null)
                        //     {
                        //         potentialCollisions = new List<GameObject>() { bulletConfig.trackTarget.gameObject };
                        //     }
                        //     else
                        //     {
                        //         //如果镭射武器没有挂载目标，就返回，不过应该也不会碰撞到，以防万一
                        //         return;
                        //     }
                        break;

                }
            }

            if (potentialCollisions.Count == 0)
                return;

            foreach (var obj in potentialCollisions)
            {
                // 隐藏的不计数
                if (!obj.activeInHierarchy)
                    continue;

                if (obj.CompareTag("Enemy") && IsColliding(bullet, obj))
                {
                    // 处理碰撞

                    // 判断目前是不是和该实例的碰撞正在冷却中
                    if (bulletConfig.AddToListCollisionCD(obj, bulletConfig.bulletPenetrateInterval))
                    {
                        obj.GetComponent<EnemyBase>().TakeDamage(
                            bulletConfig.bulletFinalDamage, bulletConfig.parentWeapon
                        );

                        bulletConfig.OnHIt(obj);


                    }
                }
            }
        }
    }


    /// <summary>
    /// 单体指向型碰撞（否则需要碰撞到才触发）
    /// </summary>
    public void SingleDirectionalCol(Bullet _bullet, EnemyBase _enemy)
    {
        if (_enemy.IsReleased)
        {
            ObjectPoolManager.Instance.ReleaseBullet(_bullet.gameObject);
        }
        if (_bullet.isReleased)
        {
            return;
        }
        if (_enemy.CompareTag("Enemy"))
        {
            // 处理碰撞

            // 判断目前是不是和该实例的碰撞正在冷却中
            if (_bullet.AddToListCollisionCD(_enemy.gameObject, _bullet.bulletPenetrateInterval))
            {
                _enemy.TakeDamage(
                    _bullet.bulletFinalDamage, _bullet.parentWeapon
                );

                _bullet.OnHIt(_enemy.gameObject);


            }
        }

    }



    bool IsColliding(SimpleCollider colA, GameObject b)
    {
        if (colA == null || b == null) return false;
        if (!colA.gameObject.activeInHierarchy || !b.activeInHierarchy) return false;

        SimpleCollider colB = b.GetComponent<SimpleCollider>();
        if (colB == null) return false;

        // 使用分离轴定理进行精确的旋转碰撞检测
        var result = colA.CheckCollision(colB);
        Debug.Log("碰撞结果：" + result);
        return result;
    }

    // 重载方法，保持向后兼容
    bool IsColliding(GameObject a, GameObject b)
    {
        if (a == null || b == null) return false;

        SimpleCollider colA = a.GetComponent<SimpleCollider>();
        if (colA == null) return false;

        return IsColliding(colA, b);
    }

    public void RegisterEnemy(GameObject enemy)
    {
        if (!enemies.Contains(enemy)) enemies.Add(enemy);
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        enemies.Remove(enemy);
    }

    public void RegisterBullet(GameObject bullet)
    {
        if (!bullets.Contains(bullet)) bullets.Add(bullet);
    }

    public void UnregisterBullet(GameObject bullet)
    {
        bullets.Remove(bullet);
    }

    // 调试绘制四叉树
    void OnDrawGizmos()
    {
        if (quadTree != null)
        {
            DrawQuadTree(quadTree);
        }
    }

    void DrawQuadTree(QuadTree qt)
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(qt.Bounds.center.x, qt.Bounds.center.y, 0),
                            new Vector3(qt.Bounds.width, qt.Bounds.height, 0));

        if (qt.Nodes != null)
        {
            foreach (var node in qt.Nodes)
            {
                if (node != null) DrawQuadTree(node);
            }
        }
    }
}

// 添加辅助属性到QuadTree类
public partial class QuadTree
{
    public Rect Bounds => bounds;
    public QuadTree[] Nodes => nodes;
}