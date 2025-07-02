using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    public Text dungeonName;
    private int nowChooseDungeonId;
    public void Initialize()
    {
        var passedDungeonId = PlayerPrefs.GetInt("dungeon_passed_level");
        Debug.Log(passedDungeonId);

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
        Debug.Log(dungeon);


        dungeonName.text = $"第{nowChooseDungeonId}关";

    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void BattleStart()
    {
        GameManager.Instance.SwitchGameStateToBattle(nowChooseDungeonId);
    }
}