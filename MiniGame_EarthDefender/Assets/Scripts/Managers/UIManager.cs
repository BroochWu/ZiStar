using UnityEngine;

public enum UILayer
{
    NULL,
    BATTLELAYER,
}


public class UIManager : MonoBehaviour
{

    public static UIManager Instance;
    public BattleUI battleLayer;
    private UILayer uiLayer = UILayer.NULL;

    void Awake()
    {
        if (Instance != null) return;
        Instance = this;


        ShowLayer(UILayer.NULL);

    }


    /// <summary>
    /// 显示哪一层
    /// </summary>
    /// <param name="uiLayer">如果为Null，则隐藏所有</param>
    void ShowLayer(UILayer uiLayer)
    {

        switch (uiLayer)
        {
            case UILayer.NULL:
                //如果要ShowNull，则代表要隐藏所有Layer
                HideLayer(UILayer.BATTLELAYER);
                break;
            case UILayer.BATTLELAYER:
                battleLayer.gameObject.SetActive(true);
                break;
            default:
                uiLayer = UILayer.NULL;
                Debug.LogWarning("没找到UI层");
                break;
        }

        this.uiLayer = uiLayer;
    }


    void HideLayer(UILayer uiLayer)
    {
        switch (uiLayer)
        {
            case UILayer.BATTLELAYER:
                battleLayer.gameObject.SetActive(false);
                break;
            default:
                Debug.LogWarning($"没找到 {uiLayer} 层");
                break;
        }
    }


    /// <summary>
    /// 切换
    /// </summary>
    /// <param name="uiLayer"></param>
    public void SwitchLayer(UILayer uiLayer)
    {
        //隐藏当前层，并打开下一层
        if (this.uiLayer != UILayer.NULL) HideLayer(this.uiLayer);
        ShowLayer(uiLayer);


    }
}
