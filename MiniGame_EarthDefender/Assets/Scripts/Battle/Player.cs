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
    public GameObject earthSprite;

    public Dictionary<Weapon, int> battleEquipedWeapon { get; private set; } = new();//局中使用的装备列表及其本局伤害量
    public bool canMove;


    Camera mainCam;
    Vector2 targetTransform;
    float angle;
    const float EARTH_ROTATE_SPEED = 0.5f;

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
        if (GameManager.Instance == null) return;


        earthSprite.transform.Rotate(Vector3.back * Time.deltaTime * EARTH_ROTATE_SPEED);

        //如果不是正在战斗，就不要进行后面的判断了
        if ((GameManager.Instance.gameState != GameManager.GameState.BATTLE) || (BattleManager.Instance.battleState != BattleState.ISBATTLEING))
        {
            if (guideLine.activeInHierarchy)
                guideLine.SetActive(false);
            return;
        }

        //鼠标摁下、游戏没结束、没点到UI，就可以更换镜头
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject() && canMove)
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
        //需要注意这是错的！新武器要在游戏中解锁！
        // foreach (var weaponId in DataManager.Instance.GetPreequippedWeaponList())
        // {
        //     if (weaponId == -1) continue;
        //     AddWeaponInBattle(weaponId);
        // }

        //默认只携带基础枪
        AddWeaponInBattle(1);
    }

    /// <summary>
    /// 添加武器
    /// </summary>
    public void AddWeaponInBattle(int weaponId)
    {
        // 创建武器对象
        GameObject weaponObj = new GameObject($"Weapon_{weaponId}");
        weaponObj.transform.SetParent(transform);

        // 添加武器组件
        Weapon weapon = weaponObj.AddComponent<Weapon>();
        weapon.Initialize(cfg.Tables.tb.Weapon.Get(weaponId), shootPath.transform, 1);

        battleEquipedWeapon.Add(weapon, 0);

    }
    /// <summary>
    /// 移除所有武器
    /// </summary>
    public void RemoveAllWeapons()
    {
        if (battleEquipedWeapon.Count == 0) return;

        foreach (var weapon in battleEquipedWeapon?.Keys)
        {
            Debug.Log($"{weapon.name} 本局造成了 {battleEquipedWeapon[weapon]} 的总伤害");
            Destroy(weapon.gameObject);
        }
        battleEquipedWeapon.Clear();


    }

}
