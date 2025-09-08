using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CommonToast : MonoBehaviour
{
    public static CommonToast Instance;
    public Text _text;
    private Coroutine _destroyCoroutine;

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
        
        // 如果已经有协程在运行，先停止它
        if (_destroyCoroutine != null)
        {
            StopCoroutine(_destroyCoroutine);
        }
        
        // 启动新的协程使用真实时间
        _destroyCoroutine = StartCoroutine(DestroyAfterSeconds(2f));
    }

    IEnumerator DestroyAfterSeconds(float seconds)
    {
        // 使用 WaitForSecondsRealtime 而不是 WaitForSeconds
        yield return new WaitForSecondsRealtime(seconds);
        DestroyThis();
    }

    void DestroyThis()
    {
        Destroy(gameObject);
    }
    
    void OnDestroy()
    {
        // 确保在对象销毁时停止协程
        if (_destroyCoroutine != null)
        {
            StopCoroutine(_destroyCoroutine);
        }
    }
}