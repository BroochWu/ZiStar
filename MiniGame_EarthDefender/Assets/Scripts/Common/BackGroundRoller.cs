using System.Collections;
using UnityEngine;

public class BackGroundRoller : MonoBehaviour
{
    #region 旧代码
    // public float multi = 0.1f;
    // public Material mat;
    // void Awake()
    // {
    //     mat = GetComponent<SpriteRenderer>().material;
    // }
    // // Update is called once per frame
    // void Update()
    // {
    //     if (GameManager.Instance == null) return;

    //     mat.mainTextureOffset += Time.deltaTime * (Vector2.right + Vector2.up) * multi;
    //     if (GameManager.Instance.gameState == GameManager.GameState.BATTLE)
    //     {
    //         mat.mainTextureScale = Vector2.Lerp(mat.mainTextureScale, 30 * (Vector2.right + Vector2.up), 0.05f);
    //     }
    //     else
    //     {
    //         mat.mainTextureScale = Vector2.Lerp(mat.mainTextureScale, 12 * (Vector2.right + Vector2.up), 0.05f);
    //     }
    // }
    #endregion

    const float MULTI_SPEED = 0.1f;
    const float RANDOM_SCALE_RANGE_MIN = 0.5f;//随机缩放最小值
    const float RANDOM_SCALE_RANGE_MAX = 1f;//随机缩放最大值
    float randomScale;
    Quaternion randomRotate;
    Vector3 initPos;

    void Start()
    {
        Reset();
    }

    void Reset()
    {
        //随机大小
        randomScale = Random.Range(RANDOM_SCALE_RANGE_MIN, RANDOM_SCALE_RANGE_MAX);
        transform.localScale = Vector3.one * randomScale;

        //随机位置
        initPos = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(0f, 1f) > 0.5f ? 1.1f : -0.1f, Random.Range(0.4f, 0.6f)));
        initPos.z = 0;
        transform.position = initPos;
        // Debug.Log($"ip:{initPos},tp:{transform.position}");

        //随机自转角度
        randomRotate = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
        transform.rotation = randomRotate;


        //开始移动
        StartCoroutine(Moving());
    }

    IEnumerator Moving()
    {
        var multi = new Vector3(MULTI_SPEED * (initPos.x > 0 ? -1 : 1), 0, 0);
        while (
            (transform.position.x <= Camera.main.ViewportToWorldPoint(new Vector2(1.4f, 0.5f)).x)
            && (transform.position.x >= Camera.main.ViewportToWorldPoint(new Vector2(-0.4f, 0.5f)).x))
        {
            transform.position += multi * Time.deltaTime;
            yield return null;
        }
        //移动完成，重置位置再生成
        Reset();
    }

}
