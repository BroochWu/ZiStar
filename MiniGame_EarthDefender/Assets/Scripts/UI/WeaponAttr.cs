using UnityEngine;
using UnityEngine.UI;

public class WeaponAttr : MonoBehaviour
{
    // public enum WeaponAttrType
    // { 

    // }


    public Text textAttrName;
    public Text textAttrNum;



    public void Initialize(string _attrName, string _attrNum)
    {
        textAttrName.text = _attrName;
        //武器基础伤害倍率是百分数
        textAttrNum.text = _attrNum;
    }

    public void RefreshNum(string _newNum)
    {
        textAttrNum.text = _newNum;
    }


}