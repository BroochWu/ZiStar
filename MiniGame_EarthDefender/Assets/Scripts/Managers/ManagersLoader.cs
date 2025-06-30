using Unity.VisualScripting;
using UnityEngine;

public class ManagersLoader : MonoBehaviour
{
    void Awake()
    {
        // 确保只存在一个管理器容器
        if (GameObject.Find("Loader")) return;

        // 创建管理器容器
        GameObject managers = new GameObject("Loader");



        // 添加管理器组件
        managers.GetOrAddComponent<BattleManager>();
        // managers.AddComponent<UIManager>();


        // 设置为跨场景持久化
        DontDestroyOnLoad(managers);
    }
}
