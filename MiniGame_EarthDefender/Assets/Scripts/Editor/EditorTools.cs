using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrefabMaterialFixer : EditorWindow
{
    [MenuItem("Tools/修复预制件材质")]
    static void FixPrefabMaterials()
    {
        int fixedCount = 0;
        
        // 查找所有预制件
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            // 检查预制件中的渲染器组件
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            
            foreach (Renderer renderer in renderers)
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    Material mat = renderer.sharedMaterials[i];
                    if (mat != null && mat.shader.name.Contains("Error"))
                    {
                        // 替换为默认材质或标准着色器
                        Material newMat = new Material(Shader.Find("Standard"));
                        newMat.name = mat.name + "_Fixed";
                        
                        // 保存新材质
                        string newPath = System.IO.Path.GetDirectoryName(path) + "/" + newMat.name + ".mat";
                        AssetDatabase.CreateAsset(newMat, newPath);
                        
                        // 更新预制件引用
                        Material[] materials = renderer.sharedMaterials;
                        materials[i] = newMat;
                        renderer.sharedMaterials = materials;
                        
                        fixedCount++;
                    }
                }
            }
            
            if (fixedCount > 0)
            {
                PrefabUtility.SavePrefabAsset(prefab);
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"修复了 {fixedCount} 个预制件材质引用");
    }
}