using System.Collections;
using UnityEngine;

public class Bullet : Weapon
{
    Weapon parent;

    public void Initialize(Weapon parent)
    {
        this.parent = parent;
    }

    void Start()
    {
        transform.localScale *= parent.bulletScale / 10000f;
        StartCoroutine(WaitForDestroyBullet());
    }

    void Update()
    {
        BulletMove();
    }

    /// <summary>
    /// 子弹移动
    /// </summary>
    void BulletMove()
    {
        //按照出射时的方向
        // transform.position += Quaternion(Player.instance.rotationTarget.transform.rotation)* parent.bulletSpeed * Time.deltaTime;
        transform.Translate(Vector3.up * parent.bulletSpeed * Time.deltaTime);

    }
    /// <summary>
    /// 销毁子弹
    /// </summary>
    IEnumerator WaitForDestroyBullet()
    {
        yield return parent.bulletReleaseTime;
        Debug.LogWarning("子弹已销毁，回头子弹销毁记得写对象池");
        Destroy(gameObject);
    }

}
