using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ImageSizeFitter : MonoBehaviour
{
    [Header("References")]
    public RectTransform background; // 背景对象
    public RectTransform image;     // 图像对象

    [Header("Settings")]
    public bool matchWidth = true;
    public bool matchHeight = true;
    public Vector2 sizeOffset = Vector2.zero; // 尺寸偏移

    [Header("Position Settings")]
    public bool matchPosition = true;
    public Vector2 positionOffset = Vector2.zero;

    // 缓存组件
    private ContentSizeFitter _contentSizeFitter;
    private LayoutGroup _layoutGroup;
    private bool _isDirty = true;

    private void OnEnable()
    {
        if (background != null)
        {
            // 获取背景上的布局组件
            _contentSizeFitter = background.GetComponent<ContentSizeFitter>();
            _layoutGroup = background.GetComponent<LayoutGroup>();
        }

        // 如果图像未指定，使用当前对象
        if (image == null)
        {
            image = GetComponent<RectTransform>();
        }

        // 添加布局监听
        LayoutRebuilder.ForceRebuildLayoutImmediate(background);
    }

    private void Update()
    {
        // 在编辑模式下实时更新
        if (!Application.isPlaying || _isDirty)
        {
            UpdateImageSize();
            _isDirty = false;
        }
    }

    // 当布局改变时标记需要更新
    private void OnRectTransformDimensionsChange()
    {
        _isDirty = true;
    }

    public void UpdateImageSize()
    {
        if (background == null || image == null) return;

        // 更新位置
        if (matchPosition)
        {
            image.position = background.position + (Vector3)positionOffset;
        }

        // 获取背景的实际尺寸（考虑ContentSizeFitter）
        Vector2 bgSize = GetBackgroundSize();

        // 应用尺寸
        Vector2 targetSize = new Vector2(
            matchWidth ? bgSize.x + sizeOffset.x : image.sizeDelta.x,
            matchHeight ? bgSize.y + sizeOffset.y : image.sizeDelta.y
        );

        image.sizeDelta = targetSize;
    }

    // 获取背景的实际尺寸（处理ContentSizeFitter）
    private Vector2 GetBackgroundSize()
    {
        // 如果没有ContentSizeFitter，直接返回尺寸
        if (_contentSizeFitter == null)
        {
            return background.rect.size;
        }

        // 临时禁用ContentSizeFitter以获取计算值
        bool wasEnabled = _contentSizeFitter.enabled;
        _contentSizeFitter.enabled = false;

        // 强制布局重新计算
        LayoutRebuilder.ForceRebuildLayoutImmediate(background);

        // 获取实际尺寸
        Vector2 calculatedSize = background.rect.size;

        // 恢复原始状态
        _contentSizeFitter.enabled = wasEnabled;
        LayoutRebuilder.ForceRebuildLayoutImmediate(background);

        return calculatedSize;
    }

#if UNITY_EDITOR
    // 编辑器按钮
    [ContextMenu("立即同步尺寸")]
    private void OnDrawGizmos()
    {
        UpdateImageSize();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}