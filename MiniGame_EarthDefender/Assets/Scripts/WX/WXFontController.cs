
#if UNITY_WEBGL

using UnityEngine;
using UnityEngine.UI;


public class WXFontController : MonoBehaviour
{
    public Text[] allTexts;

    void Start()
    {
        allTexts = Resources.FindObjectsOfTypeAll<Text>();
        string fallBackFont = null;
        WeChatWASM.WX.GetWXFont(fallBackFont, (font) =>
        {
            for (int i = 0; i < allTexts.Length; i++)
                allTexts[i].font ??= font;
        });
    }
}
#endif