using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;
    public GameObject rotationTarget;
    public GameObject cursorObj;

    Camera mainCam;
    Vector2 targetTransform;

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

    }

    void Start()
    {
#if !UNITY_EDITOR
        cursorObj.GetComponent<SpriteRenderer>().enabled = false;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        cursorObj.transform.position = (Vector2)mainCam.ScreenToWorldPoint(Input.mousePosition);

        // 计算方向向量
        Vector3 direction = cursorObj.transform.position - rotationTarget.transform.position;

        // 计算旋转角度（绕Z轴）
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Debug.Log(angle);
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
}
