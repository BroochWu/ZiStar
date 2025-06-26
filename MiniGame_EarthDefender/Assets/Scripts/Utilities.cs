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

