using UnityEngine;
using UnityEngine.UI;

public class WXFontController : MonoBehaviour
{
    public Text[] allTexts;
    public Font chineseFont; // 在Inspector中分配你导入的中文字体

    void Start()
    {
        allTexts = Resources.FindObjectsOfTypeAll<Text>();

        // 先尝试微信API
#if !UNITY_EDITOR && UNITY_WEBGL
        WeChatWASM.WX.GetWXFont(null, (font) =>
        {
            if (font != null)
            {
                ApplyFontToAllTexts(font);
            }
            else
            {
                // 微信API失败，使用备用字体
                ApplyFallbackFont();
            }
        });
#else
        // 在编辑器中直接使用备用字体
        ApplyFallbackFont();
#endif
    }

    void ApplyFallbackFont()
    {
        if (chineseFont != null)
        {
            ApplyFontToAllTexts(chineseFont);
        }
        else
        {
            Debug.LogError("未设置备用中文字体！");
        }
    }

    void ApplyFontToAllTexts(Font font)
    {
        foreach (Text text in allTexts)
        {
            if (text != null)
            {
                if (text.font != chineseFont)
                {
                    text.font = font;
                    Debug.Log("替换掉的text名称为：" + text.font);
                }
            }
        }
        Debug.Log($"已为{allTexts.Length}个Text组件设置字体");
    }
}