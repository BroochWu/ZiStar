using UnityEngine;

public class MainUI : MonoBehaviour
{
    public void Initialize()
    {
    }

    public void BattleStart()
    {
        GameManager.Instance.SwitchGameStateToBattle(1);
    }
}