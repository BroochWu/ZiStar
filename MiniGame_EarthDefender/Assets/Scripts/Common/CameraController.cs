using UnityEngine;

public class CameraController
{
    private static CameraController _instance;
    public static CameraController Instance => _instance ??= new();
    private static Camera mainCamera = Camera.main;
    // 简化的屏幕边界获取方法
    public Vector2 GetScreenBounds()
    {
        float height = mainCamera.orthographicSize;
        float width = height * mainCamera.aspect;
        return new Vector2(width, height);
    }
}