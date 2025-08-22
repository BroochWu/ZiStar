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
            if (!bullet.activeInHierarchy)
                continue;
            // 获取可能碰撞的敌人对象
            var potentialCollisions = new List<GameObject>();
            quadTree.Retrieve(potentialCollisions, bullet);

            foreach (var obj in potentialCollisions)
            {
                if (!obj.activeInHierarchy)
                    continue;

                if (obj.CompareTag("Enemy") && IsColliding(bullet, obj))
                {

                    // 处理碰撞
                    var bulletConfig = bullet.GetComponent<Bullet>();
                    obj.GetComponent<Enemy>().TakeDamage(
                        bulletConfig.bulletDamage, bulletConfig.parentWeapon


                    );
                    // 回收子弹
                    ObjectPoolManager.Instance.ReleaseBullet(bullet);

                }
            }
        }
    }

    bool IsColliding(GameObject a, GameObject b)
    {
        if (a == null || b == null) return false;
        if (a.activeInHierarchy && b.activeInHierarchy == false) return false;

        SimpleCollider colA = a.GetComponent<SimpleCollider>();
        SimpleCollider colB = b.GetComponent<SimpleCollider>();

        if (colA == null || colB == null) return false;

        // 简化的圆形碰撞检测（完全忽略Z轴）
        Vector2 posA = new Vector2(a.transform.position.x, a.transform.position.y);
        Vector2 posB = new Vector2(b.transform.position.x, b.transform.position.y);

        float distanceSqr = (posA - posB).sqrMagnitude;
        float radiusSum = (colA.Size + colB.Size) / 2f;
        float minDistanceSqr = radiusSum * radiusSum;

        return distanceSqr < minDistanceSqr;
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