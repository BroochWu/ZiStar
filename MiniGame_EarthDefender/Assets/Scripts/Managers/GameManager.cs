using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    void Awake()
    {
        if (Instance != null) return;
        Instance = this;

        BasicSetting();


    }

    void Start()
    {
        BattleStart(1);
    }


    //基础设置
    void BasicSetting()
    {

        Application.targetFrameRate = 120;
    }

    //战斗开始
    void BattleStart(int dungeonId)
    {
        BattleManager.Instance.Initialize(dungeonId);
    }
}
