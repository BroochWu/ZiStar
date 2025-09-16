using System.Collections.Generic;
using UnityEngine;
namespace cfg
{
    public partial class Tables
    {
        private static Tables _tb;
        public static Tables tb => _tb ??= InitializeTables();

        // 在运行时自动初始化
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            // 确保在游戏启动时立即初始化
            if (_tb == null)
            {
                InitializeTables();
            }
        }
        private static Tables InitializeTables()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
// #if UNITY_EDITOR
    // Android平台（非编辑器）使用Resources.Load
     return new Tables(file =>
    {
        try
        {
            // 修正路径：去掉文件扩展名，确保路径正确
            string resourcePath = $"Luban/Output/Json/{file}";
            TextAsset textAsset = UnityEngine.Resources.Load<TextAsset>(resourcePath);
            
            if (textAsset == null)
            {
                Debug.LogError($"无法加载配置文件: {resourcePath}");
                throw new System.Exception($"Load config file failed: {file}");
            }
            
            return SimpleJSON.JSON.Parse(textAsset.text);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"加载配置表 {file} 时出错: {ex.Message}");
            throw;
        }
    }
    );
#else
            // 其他平台（包括编辑器）使用原来的文件读取方式
            return new Tables(file =>
            {
                string filePath = UnityEngine.Application.dataPath + $"/Resources/Luban/Output/Json/{file}.json";
                if (!System.IO.File.Exists(filePath))
                {
                    Debug.LogError($"配置文件不存在: {filePath}");
                    throw new System.Exception($"Config file not found: {file}");
                }
                return SimpleJSON.JSON.Parse(System.IO.File.ReadAllText(filePath));
            });
#endif
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


    /// <summary>
    /// 返回品质色
    /// </summary>
    /// <param name="_quality"></param>
    /// <param name="_isLight"></param>
    /// <returns></returns>
    public static Color SetQualityColor(cfg.Enums.Com.Quality _quality, bool _isLight)
    {
        int qualityid = 1;
        Color qualityColor;

        var config = cfg.Tables.tb.Color;

        switch (_quality)
        {
            case cfg.Enums.Com.Quality.GREEN:
                qualityid = 101;
                break;
            case cfg.Enums.Com.Quality.BLUE:
                qualityid = 102;
                break;
            case cfg.Enums.Com.Quality.PURPLE:
                qualityid = 103;
                break;
            case cfg.Enums.Com.Quality.ORANGE:
                qualityid = 104;
                break;
            case cfg.Enums.Com.Quality.RED:
                qualityid = 105;
                break;
        }
        ColorUtility.TryParseHtmlString(
            _isLight ?
            config.Get(qualityid).ColorLightbg : config.Get(qualityid).ColorDarkbg
            , out qualityColor);

        return qualityColor;

    }

    /// <summary>
    /// 大数字转换
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static string BigNumber(int number)
    {
        string str = number.ToString();
        if (number >= 10000) str = string.Format("{0:0.00}", number / 10000f) + "万";

        return str;
    }



    private static bool Compare(int a, int b, string op)
    {
        switch (op)
        {
            case ">=": return a >= b;
            case "<=": return a <= b;
            case "==": return a == b;
            case "<": return a < b;
            case ">": return a > b;
            case "!=": return a != b;
            default:
                Debug.LogError("不支持的判断条件: " + op);
                return false;
        }
    }

    /// <summary>
    /// 通用条件判断
    /// </summary>
    /// <returns></returns>
    public static bool CondCheck(cfg.Enums.Com.CondType condType, List<string> _stringParams, List<int> _intParams)
    {
        switch (condType)
        {
            case cfg.Enums.Com.CondType.NULL:
                return true;
            case cfg.Enums.Com.CondType.WEAPONUNLOCK:
                if (GameManager.Instance.gameState != GameManager.GameState.BATTLE)
                {
                    Debug.LogError("非战斗中无法使用WEAPONUNLOCK的解锁条件！");
                    return false;
                }
                // return DataManager.Instance.IsWeaponPreequipped(_intParams[0]) >= 0;
                if (_stringParams.Count > 0)
                {
                    foreach (var weapon in Player.instance.battleEquipedWeapon)
                    {
                        if (weapon.Key.weaponId == _intParams[0]) return _stringParams[0] != "!=";
                    }

                }
                else
                {
                    foreach (var weapon in Player.instance.battleEquipedWeapon)
                    {
                        if (weapon.Key.weaponId == _intParams[0]) return true;
                    }

                }
                return false;
            // return Player.instance.battleEquipedWeapon.ContainsKey(cfg.Tables.tb.Weapon.Get(_intParams[0]));

            case cfg.Enums.Com.CondType.WEAPONLEVEL:
                return Compare(DataManager.Instance.GetWeaponLevel(_intParams[0]), _intParams[1], _stringParams[0]);

            case cfg.Enums.Com.CondType.DUNGEON_PASS:
                return Compare(DataManager.Instance.dungeonPassedLevel, _intParams[0], _stringParams[0]);

            case cfg.Enums.Com.CondType.TOTALBATTLEDAMAGE:
                return Compare(BattleManager.Instance.totalDamage, _intParams[0], _stringParams[0]);

            case cfg.Enums.Com.CondType.BATTLE_STATE:
                return Compare((int)BattleManager.Instance.battleState, _intParams[0], _stringParams[0]);
            // switch (_intParams[0])
            // {
            //     case 0: return BattleManager.Instance.battleState == BattleState.BATTLEFAIL;
            //     case 1: return BattleManager.Instance.battleState == BattleState.BATTLESUCCESS;
            //     default:
            //         Debug.LogError("这是什么解锁条件？");
            //         return false;
            // }

            case cfg.Enums.Com.CondType.BATTLE_LOSE_REASON:
                return Compare((int)BattleManager.Instance.battleLoseReason, _intParams[0], _stringParams[0]);
        }
        Debug.LogError("这是什么解锁条件？");
        return false;
    }
    public static bool CondCheck(cfg.Enums.Com.CondType condType, List<string> _stringParams, List<int> _intParams, out string _lockStr)
    {
        _lockStr = "";
        switch (condType)
        {
            case cfg.Enums.Com.CondType.NULL:
                return true;
            case cfg.Enums.Com.CondType.WEAPONUNLOCK:
                _lockStr = $"需要穿戴武器{cfg.Tables.tb.Weapon.Get(_intParams[0]).TextName}！";
                return DataManager.Instance.IsWeaponPreequipped(_intParams[0]) >= 0;

            case cfg.Enums.Com.CondType.WEAPONLEVEL:
                _lockStr = $"{cfg.Tables.tb.Weapon.Get(_intParams[0]).TextName} 等级需要达到 {_intParams[1]}！";
                return Compare(DataManager.Instance.GetWeaponLevel(_intParams[0]), _intParams[1], _stringParams[0]);

            case cfg.Enums.Com.CondType.DUNGEON_PASS:
                _lockStr = $"请先通过第 {_intParams[0]} 关！";
                return Compare(DataManager.Instance.dungeonPassedLevel, _intParams[0], _stringParams[0]);

            case cfg.Enums.Com.CondType.TOTALBATTLEDAMAGE:
                return Compare(BattleManager.Instance.totalDamage, _intParams[0], _stringParams[0]);

            case cfg.Enums.Com.CondType.BATTLE_STATE:
                return Compare((int)BattleManager.Instance.battleState, _intParams[0], _stringParams[0]);

            case cfg.Enums.Com.CondType.BATTLE_LOSE_REASON:
                return Compare((int)BattleManager.Instance.battleLoseReason, _intParams[0], _stringParams[0]);
        }
        Debug.LogError("这是什么解锁条件？");
        return false;
    }

    public static bool CondListCheck(List<cfg.Beans.Com_UnlockConds> _conds)
    {
        if (_conds.Count == 0) return true;

        foreach (var cond in _conds)
        {
            if (!CondCheck(cond.CondType, cond.StringParams, cond.IntParams)) return false;
        }
        return true;
    }

    /// <summary>
    /// 从列表中随机一个
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static T GetRandomByList<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("列表为空或未初始化");
            return default;
        }

        return list[UnityEngine.Random.Range(0, list.Count)];
    }


    /// <summary>
    /// 根据数量设置颜色
    /// </summary>
    /// <param name="_count"></param>
    /// <param name="_countRed"></param>
    /// <param name="_countYellow"></param>
    /// <returns></returns>
    public static Color SetColorByCount(int _count, int _countRed, int _countYellow)
    {

        // 根据数量设置不同颜色
        if (_count <= _countRed)
        {
            return Color.red;
        }
        else if (_countRed != _countYellow && _count < _countYellow)
        {
            return Color.yellow;
        }
        else
        {
            return Color.white;
        }
    }

    public static string GetQualityName(cfg.Enums.Com.Quality _quality)
    {
        switch (_quality)
        {
            case cfg.Enums.Com.Quality.NULL: return "无";
            case cfg.Enums.Com.Quality.GREEN: return "普通";
            case cfg.Enums.Com.Quality.BLUE: return "民用";
            case cfg.Enums.Com.Quality.PURPLE: return "军用";
            case cfg.Enums.Com.Quality.ORANGE: return "机密";
            case cfg.Enums.Com.Quality.RED: return "绝密";
        }
        return "未知";
    }

}
