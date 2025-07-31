using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DynamicMaskController : MonoBehaviour
{
    [Header("UI Elements")]
    public RawImage backgroundImage;  // 背景图片
    public RawImage maskImage;        // 遮罩图片
    
    [Header("Mask Settings")]
    [Range(0.1f, 5f)] public float minSize = 0.5f;
    [Range(0.1f, 5f)] public float maxSize = 2f;
    [Range(0f, 1f)] public float colorThreshold = 0.5f;
    [Range(0f, 0.5f)] public float edgeSoftness = 0.1f;
    public bool invertMask = false;
    
    [Header("Interaction Settings")]
    public float moveSensitivity = 1f;
    public float scaleSensitivity = 0.1f;
    public float rotationSensitivity = 5f;
    
    private Material maskMaterial;
    private RectTransform maskRect;
    
    void Start()
    {
        // 创建动态材质实例
        maskMaterial = new Material(Shader.Find("Custom/DynamicMaskEffect"));
        backgroundImage.material = maskMaterial;
        
        // 获取遮罩的RectTransform
        maskRect = maskImage.GetComponent<RectTransform>();
        
        // 初始化材质参数
        UpdateMaskProperties();
    }
    
    void Update()
    {
        HandleInput();
        UpdateMaskProperties();
    }
    
    void HandleInput()
    {
        // 移动遮罩
        if (Input.GetMouseButton(0))
        {
            Vector2 mouseDelta = new Vector2(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y")) * moveSensitivity;
            
            Vector2 newPos = maskRect.anchoredPosition + mouseDelta;
            maskRect.anchoredPosition = newPos;
        }
        
        // 缩放遮罩
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector2 newScale = maskRect.localScale + Vector3.one * scroll * scaleSensitivity;
            newScale.x = Mathf.Clamp(newScale.x, minSize, maxSize);
            newScale.y = Mathf.Clamp(newScale.y, minSize, maxSize);
            maskRect.localScale = newScale;
        }
        
        // 旋转遮罩
        if (Input.GetMouseButton(1))
        {
            float rotationDelta = Input.GetAxis("Mouse X") * rotationSensitivity;
            Vector3 newRotation = maskRect.localEulerAngles + new Vector3(0, 0, rotationDelta);
            maskRect.localEulerAngles = newRotation;
        }
        
        // 切换遮罩反转
        if (Input.GetKeyDown(KeyCode.Space))
        {
            invertMask = !invertMask;
        }
    }
    
    void UpdateMaskProperties()
    {
        if (maskMaterial == null) return;
        
        // 计算遮罩在背景中的位置（归一化）
        RectTransform bgRect = backgroundImage.rectTransform;
        Vector2 bgSize = bgRect.rect.size;
        Vector2 maskPos = maskRect.anchoredPosition;
        
        // 转换为0-1范围
        Vector2 normalizedPos = new Vector2(
            (maskPos.x + bgSize.x * 0.5f) / bgSize.x,
            1 - ((maskPos.y + bgSize.y * 0.5f) / bgSize.y) // 翻转Y轴
        );
        
        // 计算遮罩尺寸（归一化）
        Vector2 maskSize = maskRect.rect.size * maskRect.localScale;
        Vector2 normalizedSize = new Vector2(
            maskSize.x / bgSize.x,
            maskSize.y / bgSize.y
        );
        
        // 设置材质参数
        maskMaterial.SetTexture("_MaskTex", maskImage.texture);
        maskMaterial.SetVector("_MaskPosition", new Vector4(normalizedPos.x, normalizedPos.y, 0, 0));
        maskMaterial.SetVector("_MaskSize", new Vector4(1/normalizedSize.x, 1/normalizedSize.y, 0, 0));
        maskMaterial.SetFloat("_ColorThreshold", colorThreshold);
        maskMaterial.SetFloat("_EdgeSoftness", edgeSoftness);
        maskMaterial.SetFloat("_InvertMask", invertMask ? 1 : 0);
    }
    
    // 公开方法供UI控制
    public void SetColorThreshold(float value)
    {
        colorThreshold = value;
    }
    
    public void SetEdgeSoftness(float value)
    {
        edgeSoftness = value;
    }
    
    public void ToggleInvertMask()
    {
        invertMask = !invertMask;
    }
    
    public void ResetMask()
    {
        maskRect.anchoredPosition = Vector2.zero;
        maskRect.localScale = Vector3.one;
        maskRect.localEulerAngles = Vector3.zero;
    }
}