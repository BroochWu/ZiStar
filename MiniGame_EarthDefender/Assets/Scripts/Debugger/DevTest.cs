
//测试脚本
#if UNITY_EDITOR


using UnityEngine;
public class DevTest : MonoBehaviour
{
    [Header("===== ctrl+shift+1测试剧情 =====")]

    [Tooltip("ctrl+shift+1测试剧情")]
    public int test_avg_story_id;
    public float test_time_scale;
    public int test_dungeon_pass_id;

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl)) Debug.Log("按下了lctrl");
        if (Input.GetKey(KeyCode.Alpha1)) Debug.Log("按下了1");

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.LogWarning($"测试avg{test_avg_story_id}");
                AvgManager.Instance.TestAvg(test_avg_story_id);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.LogWarning($"时间流速调整为{test_time_scale}");
                Time.timeScale = test_time_scale;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.LogWarning($"通关设置为{test_dungeon_pass_id}");
                DataManager.Instance.dungeonPassedLevel = test_dungeon_pass_id;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Debug.LogWarning("无限血触发成功");
                BattleManager.Instance.currentEarthHp = 99999999;
                // BattleManager.Instance.globalDamageMultiInOneBattle = 99999999;
            }
        }
    }
}




#endif