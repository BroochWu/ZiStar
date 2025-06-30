using UnityEngine;
using UnityEngine.Rendering;

public class InstancedRenderer : MonoBehaviour
{
    public Mesh mesh;
    public Material material;
    
    private Matrix4x4[] matrices;
    private MaterialPropertyBlock propertyBlock;
    private int count;
    
    public void Initialize(int maxCount)
    {
        matrices = new Matrix4x4[maxCount];
        propertyBlock = new MaterialPropertyBlock();
    }
    
    public void AddInstance(Matrix4x4 matrix)
    {
        if (count >= matrices.Length) return;
        matrices[count] = matrix;
        count++;
    }
    
    public void Render()
    {
        if (count == 0) return;
        
        Graphics.DrawMeshInstanced(
            mesh, 
            0, 
            material, 
            matrices, 
            count, 
            propertyBlock, 
            ShadowCastingMode.Off, 
            false
        );
        
        count = 0;
    }
}