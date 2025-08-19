using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace cfg.weapon
{
    public partial class Weapon : Luban.BeanBase
    {
        //补充武器的解锁状态
        public enum CellState
        {
            NORMAL = 1,
            LOCK = 2,//未解锁
            NEXTUNLOCK = 3 //下一个即将解锁
        }

        public Sprite ImageIcon//道具图标
        {
            get
            {
                return Resources.Load<Sprite>("Images/" + ImageIconPath);
            }
        }
        public int currentLevel//当前等级
        {
            get
            {
                return DataManager.Instance.GetWeaponLevel(Id);
            }
            set
            {
                PlayerPrefs.SetInt($"weapon_level_{Id}", value);
            }
        }

        public float basicAdditionAtk //基础的武器伤害倍率
        {
            get
            {
                return currentLevel * 0.05f;
            }
        }

        public CellState weaponState //武器当前是否解锁
        {
            get
            {
                if (!Utility.CondCheck(UnlockCond.CondType, UnlockCond.IntParams))
                {
                    if (Id == DataManager.Instance.nextUnlockDungeonPassedWeapon)
                    {
                        Debug.Log("下一个要解锁的是：" + Id);
                        return CellState.NEXTUNLOCK;
                    }
                    else
                        return CellState.LOCK;
                }
                return CellState.NORMAL;

            }
        }
        public Dictionary<item.Item, int> levelUpConsumes //武器升级消耗
        {
            get
            {
                Dictionary<item.Item, int> tempDic = new()
                {
                    { Tables.tb.Item.Get(1), Mathf.Min(5000,currentLevel*5) },//金币 - 等级*5，最高5000
                    { Piece_Ref, 3 + 2 * currentLevel } //碎片 - 3+2*（等级-1）
                };
                return tempDic;
            }
        }

        private int curGlobalBonusLv//当前全局奖励等级
        {
            get
            { return currentLevel / 5; }
        }

        public int curGlobalBonusNum //当前全局奖励数额（暂定每5级送3%）
        {
            get
            {
                return curGlobalBonusLv * 300;
            }
        }

        // public int nextGlobalBonusNum//下一档奖励
        // {
        //     get
        //     {
        //         return (currentLevel + 1) / 5 * 3;
        //     }
        // }

    }

}



public class Weapon : MonoBehaviour
{
    // public static int globalDamageMultiInOneBattle { get; private set; }//所有武器共同生效的，单局游戏全局伤害加成
    private int localDamageMultiInOneBattle; //仅这个武器生效
    private int rowCount;
    private float rowSpace;
    private int columnCount;
    private WaitForSeconds columnSpace;
    private Transform bulletInitTransform;
    private Coroutine corShootBullet;
    private WaitForSeconds rateOfFire;
    private int weaponLevel;


    public int weaponId;
    public int attack;
    public float bulletSpeed { get; private set; }
    public int bulletScale { get; private set; }
    public float bulletReleaseTime { get; private set; }
    private cfg.weapon.Weapon thisWeapon;
    private string bulletType;

    public void Initialize(cfg.weapon.Weapon weapon, Transform bulletInitTransform, int weaponLevel)
    {

        // 加载配置
        weaponId = weapon.Id;
        this.thisWeapon = weapon;
        this.bulletInitTransform = bulletInitTransform;
        this.weaponLevel = weaponLevel;
        //初始化武器伤害
        // this.attack = GetWeaponAttack();

        if (thisWeapon == null)
        {
            Debug.LogError($"Weapon config not found for ID: {thisWeapon.Id}");
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
            + thisWeapon.basicAdditionAtk //基础武器伤害倍率
                                          // + DataManager.Instance.TotalWeaponsGlobalAtkBonus / 100f //武器带来的全局加成量（删除，叠加在单局加成里）
            + BattleManager.Instance.globalDamageMultiInOneBattle / 10000f  //单局加成（基本上是卡牌带来的）
            + localDamageMultiInOneBattle / 10000f //单局本武器独特加成（基本上也是卡牌带来的）
            )
            );
        attack = final;

        return final;
    }

    /// <summary>
    /// 更新数据
    /// </summary>
    void UpdateData()
    {
        // 初始化武器参数(这些未来可能都不是固定读表的)
        rateOfFire = new WaitForSeconds(1f / thisWeapon.RateOfFire);
        bulletReleaseTime = thisWeapon.MaxLifetime;
        rowCount = thisWeapon.RowCount;
        rowSpace = thisWeapon.RowSpace;
        columnCount = thisWeapon.ColumnCount;
        columnSpace = new WaitForSeconds(thisWeapon.ColumnSpace);
        bulletSpeed = thisWeapon.BulletSpeed;
        bulletScale = thisWeapon.BulletScale;
        bulletType = thisWeapon.BulletPrefab;
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
            Quaternion rotation = Player.instance.rotationTarget.transform.rotation;

            // 获取发射方向
            Vector3 fireDirection = rotation * Vector3.up;

            // 计算垂直于发射方向的向量
            Vector3 perpendicular = Vector3.Cross(fireDirection, Vector3.forward).normalized;

            // 计算起始位置（居中分布）
            if (rowCount > 1)
            {
                float totalWidth = rowSpace * (rowCount - 1);
                Vector3 startOffset = perpendicular * (totalWidth / 2f);
                spawnPos = bulletInitTransform.position - startOffset;
            }

            var oldSpawnPos = spawnPos;
            // 生成子弹行
            for (int j = 0; j < columnCount; j++)
            {
                spawnPos = oldSpawnPos;
                // 生成子弹列
                for (int i = 0; i < rowCount; i++)
                {
                    // 从对象池获取子弹
                    GameObject bullet = GetBulletFromPool();

                    // 设置子弹位置和旋转
                    bullet.transform.position = spawnPos;
                    bullet.transform.rotation = rotation;

                    // 初始化子弹
                    Bullet bulletComponent = bullet.GetComponent<Bullet>();
                    if (bulletComponent != null)
                    {
                        bulletComponent.Initialize(this);
                    }
                    spawnPos += perpendicular * rowSpace;
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
        return ObjectPoolManager.Instance.GetBullet(bulletType);
    }

    /// <summary>
    /// 武器卸载时的行为
    /// </summary>
    public void UnEquip()
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
        Debug.Log($"当前 {thisWeapon.TextName} 基础伤害加成：" + localDamageMultiInOneBattle);
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