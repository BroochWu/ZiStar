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
            if (!bullet.activeSelf) continue;
            // 获取可能碰撞的敌人对象
            var potentialCollisions = new List<GameObject>();
            quadTree.Retrieve(potentialCollisions, bullet);

            foreach (var obj in potentialCollisions)
            {
                if (!obj.activeSelf) continue;

                if (obj.CompareTag("Enemy") && IsColliding(bullet, obj))
                {

                    // 处理碰撞
                    obj.GetComponent<Enemy>().TakeDamage(
                        bullet.GetComponent<Bullet>().bulletDamage
                    );
                    // 回收子弹
                    ObjectPoolManager.Instance.ReleaseBullet(bullet);

                }
            }
        }
    }

    bool IsColliding(GameObject a, GameObject b)
    {
        // 简化的圆形碰撞检测
        SimpleCollider colA = a.GetComponent<SimpleCollider>();
        SimpleCollider colB = b.GetComponent<SimpleCollider>();

        float distance = Vector2.Distance(a.transform.position, b.transform.position);
        return distance < (colA.Size + colB.Size) / 2f;
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
}