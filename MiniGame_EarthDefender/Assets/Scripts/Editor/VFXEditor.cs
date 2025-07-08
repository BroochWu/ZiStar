using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VFX))]
public class VFXEditor : Editor
{
    private SerializedProperty vfxTypeProp;
    private SerializedProperty damageTextProp;
    private SerializedProperty bombEffectProp; // 如果需要的话

    void OnEnable()
    {
        vfxTypeProp = serializedObject.FindProperty("vFXType");
        damageTextProp = serializedObject.FindProperty("damageText");
        bombEffectProp = serializedObject.FindProperty("bombEffect"); // 可选
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // 始终显示 VFXType 枚举
        EditorGUILayout.PropertyField(vfxTypeProp);
        
        // 获取当前选择的类型
        VFXType currentType = (VFXType)vfxTypeProp.enumValueIndex;
        
        // 根据类型显示相关字段
        switch (currentType)
        {
            case VFXType.DAMAGETEXT:
                EditorGUILayout.PropertyField(damageTextProp);
                break;
                
            case VFXType.BOMB:
                // 如果添加了爆炸特效字段，可以在这里显示
                if (bombEffectProp != null)
                    EditorGUILayout.PropertyField(bombEffectProp);
                break;
        }
        
        // 显示其他公共字段
        DrawPropertiesExcluding(serializedObject, "vFXType", "damageText", "bombEffect");
        
        serializedObject.ApplyModifiedProperties();
    }
}