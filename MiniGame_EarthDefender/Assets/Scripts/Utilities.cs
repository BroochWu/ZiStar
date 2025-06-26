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
                return new cfg.Tables(file =>
                JSON.Parse(File.ReadAllText(
                    Application.streamingAssetsPath + $"/Luban/Output/Json/{file}.json")
                    )
                    );
            }

        }




    }
}

