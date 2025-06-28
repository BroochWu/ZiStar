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

