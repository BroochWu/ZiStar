using System.Collections;
using System.IO;
using SimpleJSON;
using UnityEngine;

namespace cfg
{
    public partial class Tables
    {
        public static Tables tb
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                // Android平台（非编辑器）使用Resources.Load
                return new Tables(file => 
                {
                    // 注意：Resources.Load的路径是相对于Resources文件夹的，不需要扩展名
                    TextAsset textAsset = Resources.Load<TextAsset>($"Luban/Output/Json/{file}");
                    if (textAsset == null)
                    {
                        throw new System.Exception($"Load config file failed: {file}");
                    }
                    return JSON.Parse(textAsset.text);
                });
#else
                // 其他平台（包括编辑器）使用原来的文件读取方式
                return new Tables(file =>
                    JSON.Parse(File.ReadAllText(
                        Application.dataPath + $"/Resources/Luban/Output/Json/{file}.json")
                    )
                );
#endif
            }
        }
    }
}

public static class ExternalTypeUtil
{
    public static UnityEngine.Vector2 NewVector2(cfg.Vector2 v)
    {
        return new UnityEngine.Vector2(v.X, v.Y);
    }

    public static UnityEngine.Vector3 NewVector3(cfg.Vector3 v)
    {
        return new UnityEngine.Vector3(v.X, v.Y, v.Z);
    }

    public static UnityEngine.Vector4 NewVector4(cfg.Vector4 v)
    {
        return new UnityEngine.Vector4(v.X, v.Y, v.Z, v.W);
    }
}



public static class Utility
{
    // /// <summary>
    // /// 2D朝向
    // /// </summary>
    // /// <param name="whom">谁要转</param>
    // /// <param name="target">转向谁</param>
    // /// <param name="watchTime">看多久（-1为无限）</param>
    // /// <param name="stopWhileLook">夹角小于0.1时自动停止</param>
    // public static IEnumerator LookTarget2D(Transform whom, Transform target, float rotationSpeed, float watchTime, bool stopWhileLook)
    // {
    //     var Timer = 0f;
    //     float rand = Random.Range(0, 1) * 0.2f + 0.1f;
    //     WaitForSeconds waitFor = new WaitForSeconds(rand);
    //     //-1代表一直跟着看
    //     if (watchTime == -1) Timer = int.MinValue;

    //     while (Timer <= watchTime)
    //     {
    //         // 计算方向向量
    //         Vector2 direction = target.position - whom.position;

    //         // 计算旋转角度
    //         float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

    //         Debug.Log($"旋转中，当前angle{angle}");

    //         // 应用旋转（2D游戏使用Z轴旋转）
    //         Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

    //         // 平滑旋转
    //         whom.rotation = Quaternion.Slerp(
    //             whom.rotation,
    //             targetRotation,
    //             rotationSpeed * Time.deltaTime
    //         );

    //         Debug.Log("ABS:" + Mathf.Abs(whom.rotation.z - angle));
    //         if (stopWhileLook && (Mathf.Abs(whom.rotation.z - angle) <= 0.1f)) break;


    //         Timer += Time.deltaTime;
    //         yield return waitFor;
    //     }
    //     Debug.Log($"协程 LookTarget2D 已停止");
    // }


    /// <summary>
    /// 优化的2D朝向方法
    /// </summary>
    public static void LookTarget2D(Transform whom, Transform target, float _rotationSpeed, bool _stopWhileLook)
    {
        // 计算方向向量（无平方根计算）
        Vector3 direction = target.position - whom.position;

        // 计算旋转角度（使用Atan2）
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        // 计算当前角度差异
        float angleDiff = Mathf.DeltaAngle(whom.eulerAngles.z, targetAngle);

        // 检查是否需要停止
        if (_stopWhileLook && Mathf.Abs(angleDiff) <= 0.1f)
        {
            return;
        }

        // 应用旋转（使用更高效的LerpAngle）
        float newAngle = Mathf.LerpAngle(
            whom.eulerAngles.z,
            targetAngle,
            _rotationSpeed
        );

        whom.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }


    public static Color SetQualityColor(cfg.Enums.Com.Quality _quality, bool _isLight)
    {
        int qualityid = 1;
        Color qualityColor;

        var config = cfg.Tables.tb.Color;

        switch (_quality)
        {
            case cfg.Enums.Com.Quality.BLUE:
                qualityid = 101;
                break;
            case cfg.Enums.Com.Quality.PURPLE:
                qualityid = 102;
                break;
            case cfg.Enums.Com.Quality.ORANGE:
                qualityid = 103;
                break;
        }
        ColorUtility.TryParseHtmlString(
            _isLight ?
            config.Get(qualityid).ColorLightbg : config.Get(qualityid).ColorDarkbg
            , out qualityColor);
            
        return qualityColor;

    }
}
