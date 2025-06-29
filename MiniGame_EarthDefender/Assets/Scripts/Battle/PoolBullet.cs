using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolBullet
{

    public PoolBullet instance { get; private set; }

    // 子弹对象池字典（按子弹类型）
    private Dictionary<string, IObjectPool<GameObject>> bulletPools =
        new Dictionary<string, IObjectPool<GameObject>>();

    // 子弹预制体缓存
    private Dictionary<string, GameObject> bulletPrefabs =
        new Dictionary<string, GameObject>();


    void Awake()
    {

        if (instance != null)
        {
            Debug.LogWarning("实例已存在");
            return;
        }


        instance = this;

    }



    /// <summary>
    /// 获取指定类型的子弹对象池
    /// </summary>
    public IObjectPool<GameObject> GetBulletPool(string bulletType)
    {
        // 如果池已存在，直接返回
        if (bulletPools.TryGetValue(bulletType, out var pool))
        {
            return pool;
        }

        // 确保预制体已加载
        if (!bulletPrefabs.ContainsKey(bulletType))
        {
            LoadBulletPrefab(bulletType);
        }

        // 创建新对象池
        var newPool = CreateBulletPool(bulletType);
        bulletPools.Add(bulletType, newPool);
        return newPool;
    }

    /// <summary>
    /// 加载子弹预制体
    /// </summary>
    private void LoadBulletPrefab(string bulletType)
    {
        GameObject prefab = Resources.Load<GameObject>($"Prefabs/Bullets/{bulletType}");
        if (prefab != null)
        {
            bulletPrefabs.Add(bulletType, prefab);
        }
        else
        {
            Debug.LogError($"Bullet prefab not found: Prefabs/Bullets/{bulletType}");
        }
    }

    /// <summary>
    /// 创建子弹对象池
    /// </summary>
    private IObjectPool<GameObject> CreateBulletPool(string bulletType)
    {
        if (!bulletPrefabs.TryGetValue(bulletType, out GameObject prefab))
        {
            Debug.LogError($"Bullet prefab not loaded: {bulletType}");
            return null;
        }

        return new ObjectPool<GameObject>(
            createFunc: () => CreateBulletInstance(prefab, bulletType),
            actionOnGet: (bullet) => OnGetBullet(bullet),
            actionOnRelease: (bullet) => OnReleaseBullet(bullet),
            actionOnDestroy: (bullet) => Object.Destroy(bullet),
            collectionCheck: true, // 启用集合检查
            defaultCapacity: 20,
            maxSize: 100
        );
    }

    /// <summary>
    /// 创建子弹实例
    /// </summary>
    private GameObject CreateBulletInstance(GameObject prefab, string bulletType)
    {
        GameObject bullet = Object.Instantiate(prefab, BattleManager.Instance.bulletPath);
        bullet.GetComponent<Bullet>().bulletType = bulletType;

        return bullet;
    }

    /// <summary>
    /// 从池中获取子弹时的操作
    /// </summary>
    private void OnGetBullet(GameObject bullet)
    {
        if (bullet == null) return;

        bullet.SetActive(true);

        // 重置子弹状态
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.ResetState();
        }
    }

    /// <summary>
    /// 释放子弹回池时的操作
    /// </summary>
    private void OnReleaseBullet(GameObject bullet)
    {
        if (bullet == null) return;

        bullet.SetActive(false);
        bullet.transform.position = Player.instance.shootPath.transform.position;
    }

    /// <summary>
    /// 预热对象池
    /// </summary>
    public void WarmUpPool(string bulletType, int count)
    {
        if (!bulletPools.TryGetValue(bulletType, out var pool))
        {
            pool = GetBulletPool(bulletType);
        }

        List<GameObject> tempList = new List<GameObject>(count);
        for (int i = 0; i < count; i++)
        {
            tempList.Add(pool.Get());
        }

        foreach (var bullet in tempList)
        {
            pool.Release(bullet);
        }
    }

    /// <summary>
    /// 清理所有对象池
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var pool in bulletPools.Values)
        {
            pool.Clear();
        }
        bulletPools.Clear();
    }



}
