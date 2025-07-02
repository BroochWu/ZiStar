using UnityEngine;

public class EntityManager : MonoBehaviour
{
    public InstancedRenderer enemyRenderer;
    public InstancedRenderer bulletRenderer;

    void Start()
    {
        enemyRenderer.Initialize(200);
        bulletRenderer.Initialize(200);
    }

    void LateUpdate()
    {
        // 收集所有敌人的矩阵
        foreach (Transform enemy in BattleManager.Instance.EnemyPath.transform)
        {
            enemyRenderer.AddInstance(enemy.transform.localToWorldMatrix);
        }

        // 收集所有子弹的矩阵
        foreach (Transform bullet in BattleManager.Instance.BulletsPath.transform)
        {
            bulletRenderer.AddInstance(bullet.transform.localToWorldMatrix);
        }

        // 渲染
        enemyRenderer.Render();
        bulletRenderer.Render();
    }
}