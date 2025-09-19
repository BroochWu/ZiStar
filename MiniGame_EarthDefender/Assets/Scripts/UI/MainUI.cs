using System;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    public static MainUI Instance;
    public Text dungeonName;
    public Text textChestButtonRemainTime;
    public GameObject objChestButtonRedPoint;

    private int nowChooseDungeonId;
    private Text textChestButtonRedPoint => objChestButtonRedPoint.GetComponentInChildren<Text>();




    void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }


    public void Initialize()
    {
        var passedDungeonId = PlayerPrefs.GetInt("dungeon_passed_level");

        var dungeon = cfg.Tables.tb.Dungeon.GetOrDefault(passedDungeonId + 1);
        if (dungeon != null)
        {
            //否则，将自动选择下一关
            nowChooseDungeonId = passedDungeonId + 1;
        }
        else
        {
            //如果没找到下一关的数据，则返回已通过的最高关卡
            nowChooseDungeonId = passedDungeonId;
        }


        dungeonName.text = $"第<color=#fec678> {nowChooseDungeonId} </color>关";

        //宝箱UI
        SetChestsUI();
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void BattleStart()
    {
        GameManager.Instance.SwitchGameStateToBattle(nowChooseDungeonId);
    }

    void Update()
    {
        //每30帧检测一次
        if (Time.frameCount % 30 != 0)
            return;
        SetChestsUI();
    }

    void SetChestsUI()
    {
        var str = TimeSpan.FromSeconds(ChestsRewardSystem.currentRemainSeconds).ToString(@"mm\:ss");
        var count = ChestsRewardSystem.nowRemainChests;

        objChestButtonRedPoint.SetActive(count != 0);
        textChestButtonRedPoint.text = count.ToString();
        if (count < ChestsRewardSystem.MAX_CHESTS)
        {
            textChestButtonRemainTime.text = $"在线宝箱\n{str}";
        }
        else
        {
            textChestButtonRemainTime.text = $"在线宝箱\n<color={cfg.Tables.tb.Color.Get(1).ColorDarkbg}>满</color>";
        }

    }


}