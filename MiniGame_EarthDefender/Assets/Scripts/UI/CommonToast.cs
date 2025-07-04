using UnityEngine;
using UnityEngine.UI;

public class CommonToast : MonoBehaviour
{
    public static CommonToast Instance;
    public Text _text;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }


    public void Initialize(string desc)
    {
        _text.text = desc;
        Invoke("DestroyThis", 2);
    }

    void DestroyThis()
    {
        Destroy(gameObject);
    }
}