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
    const float MOVE_TIME = 30f;//移动时间
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
        initPos = new Vector3(Random.Range(0, 1) > 0.5f ? 20 : -20, 0, 0);
        transform.position = initPos;

        //随机自转角度
        randomRotate = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
        transform.rotation = randomRotate;


        //开始移动
        StartCoroutine(Moving());
    }

    IEnumerator Moving()
    {
        var releaseTime = 0f;
        var multi = new Vector3(MULTI_SPEED * initPos.x > 0 ? -1 : 1, 0, 0);
        while (releaseTime <= MOVE_TIME)
        {
            releaseTime += Time.deltaTime;
            transform.position += multi * Time.deltaTime;
            yield return null;
        }
        //移动完成，重置位置再生成
        Reset();
    }

}
