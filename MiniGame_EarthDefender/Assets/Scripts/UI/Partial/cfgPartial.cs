// 在一个单独的文件中，例如 Item_RequireExtensions.cs
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

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


namespace cfg.weapon
{
    public partial class Weapon : Luban.BeanBase
    {
        //补充武器的解锁状态
        public enum CellState
        {
            NORMAL = 1,
            LOCK = 2,//未解锁
            NEXTUNLOCK = 3 //下一个即将解锁
        }

        public Sprite ImageIcon//道具图标
        {
            get
            {
                return Resources.Load<Sprite>("Images/" + ImageIconPath);
            }
        }
        public int currentLevel//当前等级
        {
            get
            {
                return DataManager.Instance.GetWeaponLevel(Id);
            }
            set
            {
                PlayerPrefs.SetInt($"weapon_level_{Id}", value);
            }
        }

        public float basicAdditionAtk //基础的武器伤害倍率
        {
            get
            {
                return currentLevel * 0.05f;
            }
        }

        public CellState weaponState //武器当前是否解锁
        {
            get
            {
                if (!Utility.CondCheck(UnlockCond.CondType, UnlockCond.StringParams, UnlockCond.IntParams))
                {
                    if (Id == DataManager.Instance.nextUnlockDungeonPassedWeapon)
                    {
                        Debug.Log("下一个要解锁的是：" + Id);
                        return CellState.NEXTUNLOCK;
                    }
                    else
                        return CellState.LOCK;
                }
                return CellState.NORMAL;

            }
        }
        public Dictionary<item.Item, int> levelUpConsumes //武器升级消耗
        {
            get
            {
                Dictionary<item.Item, int> tempDic = new()
                {
                    { Tables.tb.Item.Get(1), Mathf.Min(5000,currentLevel*5) },//金币 - 等级*5，最高5000
                    { Piece_Ref, 3 + 2 * currentLevel } //碎片 - 3+2*（等级-1）
                };
                return tempDic;
            }
        }

        private int curGlobalBonusLv//当前全局奖励等级(武器每5级提升1档)
        {
            get
            { return currentLevel / 5; }
        }

        public int curGlobalBonusNum //当前全局奖励数额（暂定每5档送3%）
        {
            get
            {
                return curGlobalBonusLv * Tables.tb.GlobalParam.Get("weapon_global_bonus_atk_per_level").IntValue;
            }
        }

        // public int nextGlobalBonusNum//下一档奖励
        // {
        //     get
        //     {
        //         return (currentLevel + 1) / 5 * 3;
        //     }
        // }

    }

}



namespace cfg.card
{
    public partial class Card
    {
        public Sprite imgBg
        {
            get
            {
                return Resources.Load<Sprite>("Images/" + ImageTricardiconPath);
            }
        }
    }
}

namespace cfg.com
{
    public partial class RedDot
    {
        public int value//红点值
        {
            get
            {
                return PlayerPrefs.GetInt($"RedDot_{Id}");
            }
            set
            {
                PlayerPrefs.SetInt($"RedDot_{Id}", value);
            }
        }
    }
}
