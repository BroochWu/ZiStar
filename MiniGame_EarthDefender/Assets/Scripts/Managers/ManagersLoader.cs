using UnityEngine;

public class ManagersLoader : MonoBehaviour
{
    public ManagersLoader Instance;
    void Awake()
    {

        Debug.Log("ManagersLoader");
        // 设置为跨场景持久化
        DontDestroyOnLoad(this);
    }
}
