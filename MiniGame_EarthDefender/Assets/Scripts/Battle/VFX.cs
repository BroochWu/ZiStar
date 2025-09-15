using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum VFXType
{
    BOMB,
    DAMAGETEXT
}

public class VFX : MonoBehaviour
{
    [Tooltip("特效类型")]
    public VFXType vFXType = VFXType.BOMB;

    // 添加条件属性（仅用于代码提示，实际隐藏由Editor脚本处理）
    [ConditionalHide("vFXType", VFXType.DAMAGETEXT)]
    public Text damageText;
    public float damageLifeTime = 1.0f;

    // 爆炸特效
    // [ConditionalHide("vFXType", VFXType.BOMB)]

    // // 其他公共字段（始终显示）
    // public AnimationCurve movementCurve;

    /// <summary>
    /// 初始化 VFX
    /// </summary>
    public void InitializeAsDTX(int _number, Vector3 _initPos)
    {
        transform.position = _initPos;
        transform.localScale = Vector3.one * 2f;
        gameObject.SetActive(true);

        // // 只有伤害文本类型才设置文本
        // if (vFXType == VFXType.DAMAGETEXT)
        // {
        damageText.text = _number.ToString();
        // }

        // 根据类型播放特效
        PlayVFX();
    }

    public void InitializeAsBomb(Vector3 _initPos)
    {
        transform.position = _initPos;
        gameObject.SetActive(true);
        PlayVFX();
    }

    private void PlayVFX()
    {
        switch (vFXType)
        {
            case VFXType.BOMB:
                break;

            case VFXType.DAMAGETEXT:
                StartCoroutine(PlayDamageTextAnimation());
                break;
        }
    }

    private IEnumerator PlayDamageTextAnimation()
    {
        // var dirZ = Random.Range(0, 360) * Mathf.Deg2Rad;//弧度
        // var red = Mathf.Tan(dirZ);//求出tan
        // var moveDir = (Vector3.right + red * Vector3.up).normalized;
        // 随机初始方向（使用弧度制更高效）
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);
        Vector2 moveDir = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));

        float currentLifeTime = 0;
        var rand = Random.Range(5, 10);
        while (currentLifeTime < damageLifeTime)
        {
            currentLifeTime += Time.deltaTime;
            moveDir = Vector2.Lerp(moveDir, Vector2.zero, 0.1f);
            transform.position += (Vector3)moveDir * Time.deltaTime * rand;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, 3.5f * Time.deltaTime);
            yield return null;
        }
        // while (currentLifeTime < damageLifeTime + 1)
        // {
        //     damageText.color.a -= 3;
        //     yield return null;
        // }
        ObjectPoolManager.Instance.ReleaseVFX(gameObject);
    }



    public void ReleaseThis()
    {
        ObjectPoolManager.Instance.ReleaseVFX(gameObject);
    }

}





/// <summary>
/// 条件隐藏属性（用于代码提示）
/// </summary>
public class ConditionalHideAttribute : PropertyAttribute
{
    public string conditionalSourceField;
    public object enumValue;

    public ConditionalHideAttribute(string conditionalSourceField, object enumValue)
    {
        this.conditionalSourceField = conditionalSourceField;
        this.enumValue = enumValue;
    }
}