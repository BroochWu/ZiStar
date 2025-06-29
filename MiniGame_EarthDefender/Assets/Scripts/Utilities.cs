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
                //                 #if UNITY_ANDROID && !UNITY_EDITOR
                //                 return new Tables(file=>Json.Parse(File.ReadAllText()))
                // #endif
                return new cfg.Tables(file =>
                JSON.Parse(File.ReadAllText(
                    Application.dataPath + $"/Resources/Luban/Output/Json/{file}.json")
                    )
                    );
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
    /// <summary>
    /// 2D朝向
    /// </summary>
    /// <param name="whom">谁要转</param>
    /// <param name="target">转向谁</param>
    public static void LookTarget2D(Transform whom, Transform target, float rotationSpeed)
    {
        // 计算方向向量
        Vector2 direction = target.position - whom.position;

        // 计算旋转角度
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        // 应用旋转（2D游戏使用Z轴旋转）
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        // 平滑旋转
        whom.rotation = Quaternion.Slerp(
            whom.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

    }

}
