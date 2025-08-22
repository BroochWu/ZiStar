
//测试脚本
#if UNITY_EDITOR


using UnityEngine;
public class DevTest : MonoBehaviour
{
    [Header("===== ctrl+shift+1测试剧情 =====")]

    [Tooltip("ctrl+shift+1测试剧情")]
    public int test_avg_story_id;
    public float test_time_scale
    {
        set
        {
            Time.timeScale = value;
        }
    }


    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl)) Debug.Log("按下了lctrl");
        if (Input.GetKey(KeyCode.Alpha1)) Debug.Log("按下了1");

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("ctrl+shift+1触发成功");
                AvgManager.Instance.TestAvg(test_avg_story_id);
            }
        }
    }


}


#endif