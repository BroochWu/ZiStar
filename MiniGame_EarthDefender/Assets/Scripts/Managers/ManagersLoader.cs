using UnityEngine;

public class ManagersLoader : MonoBehaviour
{
    public ManagersLoader Instance;
    void Awake()
    {
        // 设置为跨场景持久化
        DontDestroyOnLoad(this);
    }
}
