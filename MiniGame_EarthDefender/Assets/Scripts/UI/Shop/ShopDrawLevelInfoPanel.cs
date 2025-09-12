using UnityEngine;
using UnityEngine.UI;

public class ShopDrawLevelInfoPanel : MonoBehaviour
{
    public Image imageQuality;
    public Text textQuality;
    public Text textProb;

    public void Initialize(cfg.Enums.Com.Quality _quality, float _prob)
    {
        gameObject.SetActive(_quality != cfg.Enums.Com.Quality.NULL && _prob != 0);

        imageQuality.color = Utility.SetQualityColor(_quality, false);
        textQuality.text = Utility.GetQualityName(_quality);
        textProb.text = (_prob * 100f).ToString("0.00") + "%";
    }
}