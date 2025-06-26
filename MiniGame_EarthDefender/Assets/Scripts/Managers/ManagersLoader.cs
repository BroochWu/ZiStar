using UnityEngine;

public class ManagersLoader : MonoBehaviour
{
    void Awake()
    {
        // 确保只存在一个管理器容器
        if (GameObject.Find("Managers")) return;

        // 创建管理器容器
        GameObject managers = new GameObject("Managers");



        // 添加管理器组件
        // managers.AddComponent<GameManager>();
        // managers.AddComponent<UIManager>();




        // 设置为跨场景持久化
        DontDestroyOnLoad(managers);
    }
}
