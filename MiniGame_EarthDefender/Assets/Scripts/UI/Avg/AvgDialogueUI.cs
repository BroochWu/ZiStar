using UnityEngine;
using UnityEngine.UI;

public class AvgDialogueUI : MonoBehaviour
{
    public Text textAvg;

    public void Initialize(string _avgText, Color _textColor)
    {
        textAvg.text = _avgText;
        textAvg.color = _textColor;
    }
}