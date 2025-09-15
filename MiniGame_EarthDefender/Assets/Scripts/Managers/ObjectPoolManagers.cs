using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }
    // 对象池容器
    private Dictionary<string, ObjectPool<GameObject>> bulletPools =
        new Dictionary<string, ObjectPool<GameObject>>();

    private Dictionary<int, ObjectPool<GameObject>> enemyPools =
        new Dictionary<int, ObjectPool<GameObject>>();

    private Dictionary<VFXType, ObjectPool<GameObject>> vfxPools =
        new Dictionary<VFXType, ObjectPool<GameObject>>();

    // 预制体缓存
    private Dictionary<string, GameObject> prefabCache =
        new Dictionary<string, GameObject>();

    private static cfg.enemy.Enemy enemyCache;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize()
    {
        PreloadEnemyType(1001, 40);

    }


    #region  ================= 子弹池管理 =================

    /// <summary>
    /// 预加载子弹类型
    /// </summary>
    public void PreloadBulletType(string bulletType, int prewarmCount = 20)
    {
        if (!bulletPools.ContainsKey(bulletType))
        {
            CreateBulletPool(bulletType);
            WarmUpBulletPool(bulletType, prewarmCount);
        }
    }

    /// <summary>
    /// 获取子弹
    /// </summary>
    public GameObject GetBullet(string bulletType)
    {
        if (!bulletPools.ContainsKey(bulletType))
        {
            CreateBulletPool(bulletType);
        }

        return bulletPools[bulletType].Get();
    }

    /// <summary>
    /// 回收子弹
    /// </summary>
    public void ReleaseBullet(GameObject bullet)
    {
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent.isReleased) return;

        string bulletType = bulletComponent.bulletType;
        // if (bulletPools.ContainsKey(bulletType))
        // {
        bulletPools[bulletType].Release(bullet);
        bullet.transform.position = Player.instance.shootPath.transform.position;
        // }
        // else
        // {
        //     Debug.LogWarning($"找不到子弹类型 {bulletType} 的对象池");
        //     Destroy(bullet);
        // }
    }

    // 创建子弹对象池
    private void CreateBulletPool(string bulletType)
    {
        string prefabPath = $"Prefabs/Bullets/{bulletType}";
        GameObject prefab = LoadPrefab(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"无法创建子弹池: 预制体 {prefabPath} 未找到");
            return;
        }

        bulletPools[bulletType] = new ObjectPool<GameObject>(
            createFunc: () => CreateBulletInstance(prefab, bulletType),
            actionOnGet: (obj) => OnGetBullet(obj),
            actionOnRelease: (obj) => OnReleaseBullet(obj),
            actionOnDestroy: (obj) => Destroy(obj),
            collectionCheck: true,
            defaultCapacity: 20,
            maxSize: 100
        );
    }

    // 创建子弹实例
    private GameObject CreateBulletInstance(GameObject prefab, string bulletType)
    {
        GameObject bullet = Instantiate(prefab, BattleManager.Instance.BulletsPath);
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.bulletType = bulletType;
        }
        else
        {
            Debug.LogWarning($"子弹预制体 {bulletType} 缺少Bullet组件");
        }
        return bullet;
    }

    // 预热子弹池
    private void WarmUpBulletPool(string bulletType, int count)
    {
        if (!bulletPools.ContainsKey(bulletType)) return;

        List<GameObject> tempList = new List<GameObject>();

        for (int i = 0; i < count; i++)
        {
            tempList.Add(GetBullet(bulletType));
        }

        foreach (GameObject bullet in tempList)
        {
            ReleaseBullet(bullet);
        }
    }
    #endregion

    #region  ================= 敌人池管理 =================

    /// <summary>
    /// 预加载敌人类型
    /// </summary>
    public void PreloadEnemyType(int enemyId, int prewarmCount)
    {
        if (!enemyPools.ContainsKey(enemyId))
        {
            CreateEnemyPool(enemyId);
            WarmUpEnemyPool(enemyId, prewarmCount);
        }
    }

    /// <summary>
    /// 获取敌人
    /// </summary>
    public GameObject GetEnemy(int enemyId)
    {
        if (!enemyPools.ContainsKey(enemyId))
        {
            CreateEnemyPool(enemyId);
        }

        return enemyPools[enemyId].Get();
    }

    /// <summary>
    /// 回收敌人
    /// </summary>
    public void ReleaseEnemy(Enemy enemy)
    {
        int enemyId = enemy.enemyId; // 假设Enemy类中有config

        if (enemyPools.ContainsKey(enemyId))
        {
            enemyPools[enemyId].Release(enemy.gameObject);
            enemy.isReleased = true;
        }
        else
        {
            Debug.LogWarning($"找不到敌人类型 {enemyId} 的对象池");
            Destroy(enemy);
        }
    }

    // 创建敌人对象池
    private void CreateEnemyPool(int enemyId)
    {
        // 从配置获取敌人预制体路径
        string prefabPath = GetEnemyPrefabPath(enemyId);
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError($"无法创建敌人池: 敌人ID {enemyId} 无效");
            return;
        }

        GameObject prefab = LoadPrefab(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"无法创建敌人池: 预制体 {prefabPath} 未找到");
            return;
        }

        enemyPools[enemyId] = new ObjectPool<GameObject>(
            createFunc: () => CreateEnemyInstance(prefab, enemyId),
            actionOnGet: (obj) => OnGetEnemy(obj),
            actionOnRelease: (obj) => OnReleaseEnemy(obj),
            actionOnDestroy: (obj) => Destroy(obj),
            collectionCheck: true,
            defaultCapacity: 20,
            maxSize: 150
        );
    }

    // 创建敌人实例
    private GameObject CreateEnemyInstance(GameObject prefab, int enemyId)
    {
        GameObject enemy = Instantiate(prefab, BattleManager.Instance.EnemyPath);
        // enemy.transform.SetParent(enemyContainer);
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            // 设置敌人ID以便回收
            if (enemyCache?.Id != enemyId)
            {
                enemyCache = cfg.Tables.tb.Enemy.Get(enemyId);
            }
            // enemyComponent.enemyId = enemyId;
            enemyComponent.SetEnemyBasicEssentials(enemyCache);
            Debug.Log($"CreateEnemyInstance");
        }
        else
        {
            Debug.LogWarning($"敌人预制体 {prefab.name} 缺少Enemy组件");
        }
        return enemy;
    }

    // 预热敌人池
    private void WarmUpEnemyPool(int enemyId, int count)
    {
        if (!enemyPools.ContainsKey(enemyId))
        {
            Debug.Log("releasenemy fail");
            return;
        }
        List<GameObject> tempList = new();
        for (int i = 0; i < count; i++)
        {

            tempList.Add(GetEnemy(enemyId));
        }
        foreach (var a in tempList)
        {
            ReleaseEnemy(a.GetComponent<Enemy>());
        }
    }
    #endregion

    #region  ================特效池管理=================

    /// <summary>
    /// 预加载特效类型
    /// </summary>
    public void PreloadVFXType(VFXType VFXtype, int prewarmCount)
    {
        if (!vfxPools.ContainsKey(VFXtype))
        {
            CreateVFXPool(VFXtype);
            WarmUpVFXPool(VFXtype, prewarmCount);
        }
    }

    /// <summary>
    /// 获取特效
    /// </summary>
    public GameObject GetVFX(VFXType _type)
    {
        if (!vfxPools.ContainsKey(_type))
        {
            CreateVFXPool(_type);
        }

        return vfxPools[_type].Get();
    }

    /// <summary>
    /// 回收特效
    /// </summary>
    public void ReleaseVFX(GameObject obj)
    {
        VFX component = obj.GetComponent<VFX>();

        VFXType type = component.vFXType; // 假设Enemy类中有config

        if (vfxPools.ContainsKey(type))
        {
            vfxPools[type].Release(obj);
        }
        else
        {
            Debug.LogWarning($"找不到类型 {type} 的对象池");
            Destroy(obj);
        }
    }

    // 创建特效对象池
    private void CreateVFXPool(VFXType _type)
    {
        var prefabPath = "";
        switch (_type)
        {
            case VFXType.DAMAGETEXT:
                prefabPath = "Prefabs/Common/DamageText";
                break;
            case VFXType.BOMB:
                prefabPath = "Prefabs/Common/VfxBoom";
                break;
        }
        GameObject prefab = LoadPrefab(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"无法创建: 预制体未找到");
            return;
        }

        vfxPools[_type] = new ObjectPool<GameObject>(
            createFunc: () => CreateVFXInstance(prefab, _type),
            actionOnGet: (obj) => OnGetVFX(obj),
            actionOnRelease: (obj) => OnReleaseVFX(obj),
            actionOnDestroy: (obj) => Destroy(obj),
            collectionCheck: true,
            defaultCapacity: 20,
            maxSize: 150
        );
    }

    // 创建特效实例
    private GameObject CreateVFXInstance(GameObject prefab, VFXType _type)
    {
        // Transform instantiateTrans = UIManager.Instance.battleLayer.UIVFXsContainer;
        Transform instantiateTrans = null;
        switch (_type)
        {
            case VFXType.DAMAGETEXT:
                instantiateTrans = UIManager.Instance.battleLayer.UIVFXsContainer;
                break;
            case VFXType.BOMB:
                instantiateTrans = BattleManager.Instance.VfxsPath;
                break;
        }
        GameObject VFXIns = Instantiate(prefab, instantiateTrans);
        VFXIns.GetOrAddComponent<VFX>().vFXType = _type;
        // 设置特效类型ID以便回收

        return VFXIns;
    }

    // 预热特效池
    private void WarmUpVFXPool(VFXType vFXType, int count)
    {
        if (!vfxPools.ContainsKey(vFXType)) return;

        List<GameObject> tempList = new List<GameObject>();

        for (int i = 0; i < count; i++)
        {
            tempList.Add(GetVFX(vFXType));
        }

        foreach (GameObject obj in tempList)
        {
            ReleaseVFX(obj);
        }
    }
    #endregion

    #region  ================= 通用方法 =================

    // 加载预制体（带缓存）
    private GameObject LoadPrefab(string path)
    {
        if (prefabCache.TryGetValue(path, out GameObject prefab))
        {
            return prefab;
        }

        prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"预制体加载失败: {path}");
            return null;
        }

        prefabCache[path] = prefab;
        return prefab;
    }

    // 从敌人ID获取预制体路径
    private string GetEnemyPrefabPath(int enemyId)
    {
        // 这里需要根据您的配置系统实现
        // 示例：从配置表获取敌人预制体路径
        var enemyConfig = cfg.Tables.tb.Enemy.Get(enemyId);
        if (enemyConfig != null)
        {
            return $"Prefabs/Enemys/{enemyConfig.Prefab}";
        }
        return string.Empty;
    }

    // 从池中获取时的操作（子弹）
    private void OnGetBullet(GameObject bullet)
    {
        bullet.SetActive(true);

        // 重置子弹状态
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.ResetState();
        }
    }

    // 释放回池时的操作（子弹）
    private void OnReleaseBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bullet.GetComponent<Bullet>().isReleased = true;
        bullet.transform.position = Vector3.zero;
    }

    // 从池中获取时的操作（敌人）
    private void OnGetEnemy(GameObject enemy)
    {
        enemy.SetActive(true);

        // 重置敌人状态（别搞了，预热的时候可能没有）
        // if (enemyComponent != null)
        // {
        //     enemyComponent.ResetAttr();
        // }
    }

    // 从池中获取时的操作（特效）
    private void OnGetVFX(GameObject obj)
    {

    }

    // 从池中移除时的操作（特效）
    private void OnReleaseVFX(GameObject obj)
    {
        obj.SetActive(false);
    }

    // 释放回池时的操作（敌人）
    private void OnReleaseEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
        enemy.transform.position = Vector3.zero;

        // 重置敌人组件
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.hpBar.SetActive(false);
        }
    }



    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        // 清理子弹池
        foreach (var pool in bulletPools.Values)
        {
            pool.Clear();
        }
        bulletPools.Clear();

        // 清理特效池
        foreach (var pool in vfxPools.Values)
        {
            pool.Clear();
        }
        vfxPools.Clear();

        foreach (var enemy in BattleManager.Instance.activeEnemys)
        {
            ReleaseEnemy(enemy);
        }
    }

    /// <summary>
    /// 清理所有对象池
    /// </summary>
    public void ClearAllPools()
    {
        // 清理子弹池
        foreach (var pool in bulletPools.Values)
        {
            pool.Clear();
        }
        bulletPools.Clear();

        // 清理敌人池
        foreach (var pool in enemyPools.Values)
        {
            pool.Clear();
        }
        enemyPools.Clear();

        // 清理特效池
        foreach (var pool in vfxPools.Values)
        {
            pool.Clear();
        }
        vfxPools.Clear();


        // 清理预制体缓存
        prefabCache.Clear();
    }
    #endregion

}