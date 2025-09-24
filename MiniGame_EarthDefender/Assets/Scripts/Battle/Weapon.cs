using System.Collections;
using Unity.Mathematics;
using UnityEngine;



public class Weapon : MonoBehaviour
{
    // public static int globalDamageMultiInOneBattle { get; private set; }//所有武器共同生效的，单局游戏全局伤害加成
    private cfg.weapon.Weapon _config;
    public cfg.weapon.Weapon weaponConfig => _config ??= cfg.Tables.tb.Weapon.GetOrDefault(weaponId);
    private cfg.weapon.Bullet _bulletConfig;
    public cfg.weapon.Bullet bulletConfig => _bulletConfig ??= weaponConfig.BulletId_Ref;
    private string bulletName;
    //发射
    private int rowCount;
    // private float rowSpace;//行间距由子弹自己控制
    private int columnCount;
    private WaitForSeconds columnSpace;


    private Transform bulletInitTransform;
    private Coroutine corShootBullet;
    private WaitForSeconds rateOfFire;//发射频率
    private int weaponLevel;//武器等级

    private int localDamageMultiInOneBattle; //仅这个武器生效

    public int weaponId;
    public int finalAttack;//最终伤害
    // public float bulletSpeed { get; private set; }
    // public int bulletScale { get; private set; }
    public float battleWeaponDamage { get; private set; }//武器单局造成的伤害


    public void Initialize(cfg.weapon.Weapon weapon, Transform bulletInitTransform, int weaponLevel)
    {
        _config = weapon;

        // 加载配置
        weaponId = weapon.Id;
        this.bulletInitTransform = bulletInitTransform;
        this.weaponLevel = weaponLevel;

        //重置武器造成的伤害
        battleWeaponDamage = 0;

        //初始化武器伤害
        // this.attack = GetWeaponAttack();

        if (weaponConfig == null)
        {
            Debug.LogError($"Weapon config not found for ID: {weaponConfig.Id}");
            return;
        }

        UpdateData();
    }


    /// <summary>
    /// 获取武器伤害
    /// </summary>
    /// <returns></returns>
    public int GetAndSetWeaponAttack()
    {
        //武器伤害=基础值（来源于账号养成）×（1+武器升级加成+全局伤害加成+单体伤害加成）
        var atkLv = PlayerPrefs.GetInt("playerData_atk_level");
        var basicValue = cfg.Tables.tb.PlayerAttrLevel.Get(atkLv).BasicAtk.Value;

        //伤害加成率 = 武器等级 × 3%
        // var additionValue = cfg.Tables.tb.WeaponLevel.Get(thisWeapon.LevelId, weaponLevel).DamageMulti / 10000f;



        int final = (int)(
            basicValue *
            (1
            + weaponConfig.basicAdditionAtk //基础武器伤害倍率
                                            // + DataManager.Instance.TotalWeaponsGlobalAtkBonus / 100f //武器带来的全局加成量（删除，叠加在单局加成里）
            + BattleManager.Instance.globalDamageMultiInOneBattle / 10000f  //单局加成（基本上是卡牌带来的）
            + localDamageMultiInOneBattle / 10000f //单局本武器独特加成（基本上也是卡牌带来的）
            )
            );
        finalAttack = final;

        return final;
    }

    /// <summary>
    /// 更新数据
    /// </summary>
    void UpdateData()
    {
        // 初始化武器参数(这些未来可能都不是固定读表的)
        rateOfFire = new WaitForSeconds(1f / weaponConfig.RateOfFire);
        // bulletReleaseTime = config.MaxLifetime[currentStateCount];
        rowCount = weaponConfig.RowCount;
        columnCount = weaponConfig.ColumnCount;
        columnSpace = new WaitForSeconds(weaponConfig.ColumnSpace);
        // bulletSpeed = config.BulletSpeed;
        // bulletScale = config.BulletScale;
        bulletName = weaponConfig.BulletId_Ref.BulletPrefab;
        GetAndSetWeaponAttack();

        // 预热对象池（可选）
        // BattleManager.Instance?.poolBullet.WarmUpPool(bulletType, rowCount * 5);

        // 启动射击协程
        corShootBullet = StartCoroutine(ShootBullet());

    }

    /// <summary>
    /// 武器发射子弹
    /// </summary>
    public IEnumerator ShootBullet()
    {
        while (true)
        {
            yield return rateOfFire;

            var spawnPos = bulletInitTransform.position;
            Quaternion baseRotation = Player.instance.rotationTarget.transform.rotation;

            // 获取发射方向
            Vector3 fireDirection = baseRotation * Vector3.up;

            // 计算垂直于发射方向的向量
            Vector3 perpendicular = Vector3.Cross(fireDirection, Vector3.forward).normalized;

            // 计算起始位置（居中分布）
            if (columnCount > 1)
            {
                float totalWidth = bulletConfig.RowSpace * (columnCount - 1);
                Vector3 startOffset = perpendicular * (totalWidth / 2f);
                spawnPos = bulletInitTransform.position - startOffset;

            }

            var oldSpawnPos = spawnPos;
            // 生成子弹行
            for (int j = 0; j < rowCount; j++)
            {
                spawnPos = oldSpawnPos;
                // 生成子弹列
                for (int i = 0; i < columnCount; i++)
                {
                    // 计算当前子弹的角度偏移（居中对称分布）
                    float angleOffset = 0f;
                    if (columnCount > 1)
                    {
                        // 例如：3列时，角度偏移为 [-角度间隔, 0, +角度间隔]
                        angleOffset = bulletConfig.RowAngleSep * (i - (columnCount - 1) / 2f);
                    }


                    // 从对象池获取子弹
                    GameObject bullet = GetBulletFromPool();

                    // 设置子弹位置和旋转
                    bullet.transform.position = spawnPos;
                    // 应用基础旋转 + 角度偏移（绕Z轴旋转）
                    bullet.transform.rotation = baseRotation * Quaternion.Euler(0, 0, angleOffset);

                    // 初始化子弹
                    Bullet bulletComponent = bullet.GetComponent<Bullet>();
                    if (bulletComponent != null)
                    {
                        bulletComponent.Initialize(this);
                    }
                    spawnPos += perpendicular * bulletConfig.RowSpace;
                }
                yield return columnSpace;

            }
        }
    }

    /// <summary>
    /// 从对象池获取子弹
    /// </summary>
    private GameObject GetBulletFromPool()
    {
        return ObjectPoolManager.Instance.GetBullet(bulletName);
    }

    /// <summary>
    /// 战斗中武器卸载时的行为
    /// </summary>
    public void BattleUnEquip()
    {
        if (corShootBullet != null)
        {
            StopCoroutine(corShootBullet);
            corShootBullet = null;
        }
    }

    /// <summary>
    /// 本次作战增加伤害加成倍率
    /// </summary>
    /// <param name="number"></param>
    public void PlusLocalDamageMultiInOneBattle(int number)
    {
        localDamageMultiInOneBattle += number;
        Debug.Log($"当前 {weaponConfig.TextName} 基础伤害加成：" + localDamageMultiInOneBattle);
        GetAndSetWeaponAttack();
    }

    /// <summary>
    /// 增加武器行数
    /// </summary>
    /// <param name="num"></param>
    public void PlusRowCount(int num)
    {
        rowCount += num;
    }

    /// <summary>
    /// 增加武器列数
    /// </summary>
    /// <param name="num"></param>
    public void PlusColumnCount(int num)
    {
        columnCount += num;
    }
}