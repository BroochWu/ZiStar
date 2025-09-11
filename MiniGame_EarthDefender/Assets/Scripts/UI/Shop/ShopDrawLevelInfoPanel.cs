using UnityEngine;
using UnityEngine.UI;

public class ShopDrawLevelInfoPanel : MonoBehaviour
{
    public Image imageQuality;
    public Text textQuality;
    public Text textProb;

    public void Initialize(cfg.Enums.Com.Quality _quality, float _prob)
    {
        if (_quality == cfg.Enums.Com.Quality.NULL || _prob == 0)
            Destroy(gameObject);

        imageQuality.color = Utility.SetQualityColor(_quality, true);
        textQuality.text = Utility.GetQualityName(_quality);
        textProb.text = _prob.ToString();
    }
}