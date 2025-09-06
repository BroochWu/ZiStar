// 在一个单独的文件中，例如 Item_RequireExtensions.cs
using SimpleJSON;

namespace cfg.Beans
{
    public partial class Item_Require
    {
        private static Item_Require TempCreate(int id, int number)
        {
            JSONObject json = new JSONObject();
            json["id"] = id;
            json["number"] = number;
            return new Item_Require(json);
        }
        
        public static Item_Require Create(int id, int number)
        {
            var itemRequire = TempCreate(id, number);
            itemRequire.ResolveRef(cfg.Tables.tb);
            return itemRequire;
        }
    }
}