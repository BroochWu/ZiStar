using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;



    public GameObject rotationTarget;
    public GameObject cursorObj;
    public GameObject shootPath;

    public List<Weapon> equipedWeapon;



    Camera mainCam;
    Vector2 targetTransform;
    float angle;

    void Awake()
    {
        if (instance != null)
        {

            Destroy(gameObject);
            Debug.Log("实例已存在");
            return;
        }
        instance = this;

        mainCam = Camera.main;
        if (cursorObj == null) cursorObj = GameObject.Find("target");
    }

    void Start()
    {
#if !UNITY_EDITOR
        cursorObj.GetComponent<SpriteRenderer>().enabled = false;
#endif

        BattleStart();


    }

    // Update is called once per frame
    void Update()
    {
        LookTarget();
    }


    /// <summary>
    /// 看着目标
    /// </summary>
    void LookTarget()
    {
        cursorObj.transform.position = (Vector2)mainCam.ScreenToWorldPoint(Input.mousePosition);

        // 计算方向向量
        targetTransform = cursorObj.transform.position - rotationTarget.transform.position;

        // 计算旋转角度（绕Z轴）
        angle = Mathf.Atan2(targetTransform.y, targetTransform.x) * Mathf.Rad2Deg - 90f;
        //Debug.Log(angle);
        // 应用旋转
        if (angle >= 30)
        {
            angle = 30;
        }
        else if (angle <= -30)
        {
            angle = -30;
        }
        rotationTarget.transform.rotation = Quaternion.Euler(0, 0, angle);

    }

    /// <summary>
    /// 战斗开始时的操作
    /// </summary>
    void BattleStart()
    {
        AddWeapon(1);
    }

    /// <summary>
    /// 添加武器
    /// </summary>
    void AddWeapon(int weaponId)
    {
        // 创建武器对象
        GameObject weaponObj = new GameObject($"Weapon_{weaponId}");
        weaponObj.transform.SetParent(transform);
        
        // 添加武器组件
        Weapon weapon = weaponObj.AddComponent<Weapon>();
        weapon.Initialize(weaponId, shootPath.transform);
        
        equipedWeapon.Add(weapon);

    }

}
