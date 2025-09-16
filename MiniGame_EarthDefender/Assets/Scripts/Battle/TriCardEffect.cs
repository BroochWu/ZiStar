using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriCardEffect
{
    public static bool TakeEffect(cfg.card.Card card)
    {
        try
        {

            foreach (var effect in card.CardEffects)
            {
                CheckEffect(effect.EffectType, effect.IntParams);
            }
            return true;
        }
        catch
        {
            UIManager.Instance.CommonToast("未知错误！选择失败！");
            return false;
        }
    }

    static void CheckEffect(cfg.Enums.Card.EffectsType _effectsType, List<int> _params)
    {
        switch (_effectsType)
        {
            case cfg.Enums.Card.EffectsType.WEAPONCOLUMN:
                Player.instance.battleEquipedWeapon.Keys.FirstOrDefault(weapon => weapon.weaponId == _params[0])
                .PlusColumnCount(_params[1]);
                break;
            case cfg.Enums.Card.EffectsType.WEAPONROW:
                Player.instance.battleEquipedWeapon.Keys.FirstOrDefault(weapon => weapon.weaponId == _params[0])
                .PlusRowCount(_params[1]);
                break;
            case cfg.Enums.Card.EffectsType.WEAPONDAMAGE:
                if (_params[0] == -1)
                {
                    BattleManager.Instance.PlusGlobalDamageMultiInOneBattle(_params[1]);
                }
                else
                {
                    Player.instance.battleEquipedWeapon.Keys.FirstOrDefault(weapon => weapon.weaponId == _params[0])
                    .PlusLocalDamageMultiInOneBattle(_params[1]);
                }
                break;
            case cfg.Enums.Card.EffectsType.WEAPONCD:
                UIManager.Instance.CommonToast("假装-CD了吧");
                break;
            case cfg.Enums.Card.EffectsType.HEAL:
                BattleManager.Instance.currentEarthHp *= (int)(1 + _params[0] / 10000f);
                UIManager.Instance.battleLayer.RefreshEarthHp();
                break;
            case cfg.Enums.Card.EffectsType.WEAPONUNLOCK:
                Player.instance.AddWeaponInBattle(_params[0]);
                break;
            default:
                Debug.LogError("你这传了啥进来啊");
                return;
        }
    }

}