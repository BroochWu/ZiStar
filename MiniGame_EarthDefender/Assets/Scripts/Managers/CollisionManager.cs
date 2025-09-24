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
            // 如果是非单体伤害，找到列表
            // 如果是单体伤害，锁定单体目标
            var potentialCollisions = new List<GameObject>();
            quadTree.Retrieve(potentialCollisions, bulletBounds);

            if (bulletConfig.isSingleCol)
            {
                if (bulletConfig.colObj == null)
                {
                    foreach (var a in potentialCollisions)
                    {
                        var component = a.GetComponent<EnemyBase>();
                        if (component == null)
                        {
                            return;
                        }
                        else if (!component.IsReleased)
                        {
                            potentialCollisions = new List<GameObject>() { potentialCollisions[0] };
                            break;
                        }
                    }
                }
                else
                {
                    potentialCollisions = new List<GameObject>() { bulletConfig.colObj };
                }
            }

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

    bool IsColliding(GameObject a, GameObject b)
    {
        if (a == null || b == null) return false;
        if (!a.activeInHierarchy || !b.activeInHierarchy) return false;

        SimpleCollider colA = a.GetComponent<SimpleCollider>();
        SimpleCollider colB = b.GetComponent<SimpleCollider>();

        if (colA == null || colB == null) return false;

        // 获取两个碰撞体的边界框
        Rect rectA = colA.GetBounds();
        Rect rectB = colB.GetBounds();

        // 检查两个矩形是否相交
        return rectA.Overlaps(rectB);
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