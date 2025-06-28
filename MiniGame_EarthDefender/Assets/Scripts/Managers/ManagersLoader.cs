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
         managers.AddComponent<BattleManager>();
        // managers.AddComponent<UIManager>();


        Debug.Log(cfg.Tables.tb.GlobalParam.Get("maincharacter_idle_count") != null ? "tables加载成功" : "tables加载失败");


        // 设置为跨场景持久化
        DontDestroyOnLoad(managers);
    }
}
