using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    public static Player instance;



    public GameObject rotationTarget;
    public GameObject cursorObj;
    public GameObject shootPath;
    public GameObject guideLine;

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
        guideLine.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //如果不是正在战斗，就不要进行后面的判断了
        if ((GameManager.Instance.gameState != GameManager.GameState.BATTLE) || (BattleManager.Instance.battleState != BattleState.ISBATTLEING))
            return;

        //鼠标摁下、游戏没结束、没点到UI，就可以更换镜头
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            guideLine.SetActive(true);
            LookCursor();
        }
        else
        {
            guideLine.SetActive(false);

        }
    }


    /// <summary>
    /// 看着鼠标位置
    /// </summary>
    void LookCursor()
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
    public void BattleStart()
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
        weapon.Initialize(cfg.Tables.tb.Weapon.Get(weaponId), shootPath.transform, 1);

        equipedWeapon.Add(weapon);

    }
    /// <summary>
    /// 移除所有武器
    /// </summary>
    public void RemoveAllWeapons()
    {
        foreach (var weapon in equipedWeapon)
        {
            Destroy(weapon.gameObject);
        }
        equipedWeapon.Clear();

    }

}
