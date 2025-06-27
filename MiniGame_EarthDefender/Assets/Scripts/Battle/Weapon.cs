using System.Collections;
using UnityEngine;

//武器类
public class Weapon : MonoBehaviour
{

    public int weaponId;//武器的id，指定自己是什么武器
    int rowCount;//行数
    float rowSpace;//行间距
    GameObject bulletPrefab;
    Transform bulletInitTranform;//子弹初始坐标
    public float bulletSpeed;
    public int bulletScale;
    Coroutine corShootBullet;
    WaitForSeconds rateOfFire;
    public WaitForSeconds bulletReleaseTime;
    cfg.weapon.Weapon thisWeapon;





    public void Initialize(int weaponId, Transform bulletInitTranform)
    {
        this.weaponId = weaponId;
        // bulletInitPos = Player.instance.shootPath.transform.position;

        this.bulletInitTranform = bulletInitTranform;



        thisWeapon = cfg.Tables.tb.Weapon.Get(weaponId);

        //重新装配一下luban的表数据，免得日后有改动

        rateOfFire = new WaitForSeconds(1f / thisWeapon.RateOfFire);
        bulletReleaseTime = new WaitForSeconds(thisWeapon.MaxLifetime);
        rowCount = thisWeapon.RowCount;
        rowSpace = thisWeapon.RowSpace;
        bulletSpeed = thisWeapon.BulletSpeed;
        bulletScale = thisWeapon.BulletScale;
        bulletPrefab = Resources.Load<GameObject>($"Prefabs/Bullets/{thisWeapon.BulletPrefab}");

        corShootBullet = Player.instance.StartCoroutine(ShootBullet());
    }

    /// <summary>
    /// 武器发射子弹
    /// </summary>
    /// <returns></returns>
    public IEnumerator ShootBullet()
    {
        while (true)
        {
            yield return rateOfFire;

            var spawnPos = bulletInitTranform.position;
            Quaternion rotation = Player.instance.rotationTarget.transform.rotation;
            Vector3 perpendicular = Vector3.Cross(rotation * Vector3.up, Vector3.forward).normalized;

            //计算同行子弹的生成坐标
            //1就基于原点，否则单数-d*数量*间隔，偶数-0.5d*数量*间隔
            //先找到最左边的点，然后逐个生成
            if (rowCount == 1)
            {
                break;
            }
            else
            {
                spawnPos = bulletInitTranform.position - (rowCount % 2 == 0 ? 0.5f : 1f) * perpendicular * rowSpace * (rowCount - 1);
            }
            // Debug.Log(spawnPos + "  " + bulletInitPos);
            for (int i = 1; i <= rowCount; i++)
            {

                //生成行
                GameObject bullet = Instantiate(bulletPrefab, spawnPos, rotation);

                bullet.GetComponent<Bullet>().Initialize(this);

                spawnPos += rowSpace * perpendicular;
            }

            yield return null;
        }
    }

    /// <summary>
    /// 武器卸载时的行为
    /// </summary>
    public void UnEquip()
    {
        if (corShootBullet != null)
        {
            Player.instance.StopCoroutine(corShootBullet);
            corShootBullet = null;
        }
    }

}
