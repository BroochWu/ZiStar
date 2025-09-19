using UnityEngine;

public class SimpleCollider : MonoBehaviour
{
    public float Width = 1f;
    public float Height = 1f;
    public float offsetY = 0f;

    void Start()
    {
        // 注册到碰撞管理器
        if (CompareTag("Enemy"))
            CollisionManager.Instance.RegisterEnemy(gameObject);
        else if (CompareTag("PlayerBullet"))
            CollisionManager.Instance.RegisterBullet(gameObject);
    }

    void OnDestroy()
    {
        // 注销
        if (CompareTag("Enemy") && CollisionManager.Instance != null)
            CollisionManager.Instance.UnregisterEnemy(gameObject);
        else if (CompareTag("PlayerBullet") && CollisionManager.Instance != null)
            CollisionManager.Instance.UnregisterBullet(gameObject);
    }

    // 获取碰撞体边界框
    public Rect GetBounds()
    {
        Vector2 position = new Vector2(transform.position.x, transform.position.y);
        return new Rect(
            position.x - Width * transform.localScale.x / 2,
            position.y - Height * transform.localScale.y / 2 + offsetY,
            Width * transform.localScale.x,
            Height * transform.localScale.y + offsetY
        );
    }

    // 获取带旋转的边界框（如果需要支持旋转）
    public Rect GetRotatedBounds()
    {
        Vector2 position = new Vector2(transform.position.x, transform.position.y);
        float rotation = transform.eulerAngles.z;

        if (Mathf.Approximately(rotation, 0))
        {
            return new Rect(
                position.x - Width / 2,
                position.y - Height / 2,
                Width,
                Height
            );
        }

        // 计算旋转后的边界框
        float cos = Mathf.Cos(rotation * Mathf.Deg2Rad);
        float sin = Mathf.Sin(rotation * Mathf.Deg2Rad);

        // 计算四个角点
        Vector2[] corners = new Vector2[4];
        corners[0] = new Vector2(-Width / 2, -Height / 2);
        corners[1] = new Vector2(Width / 2, -Height / 2);
        corners[2] = new Vector2(Width / 2, Height / 2);
        corners[3] = new Vector2(-Width / 2, Height / 2);

        // 旋转角点
        for (int i = 0; i < 4; i++)
        {
            float x = corners[i].x * cos - corners[i].y * sin;
            float y = corners[i].x * sin + corners[i].y * cos;
            corners[i] = new Vector2(x, y) + position;
        }

        // 计算旋转后的边界框
        float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
        float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    // 可视化调试
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        // 绘制矩形碰撞体
        Rect bounds = GetBounds();
        Vector3 center = new Vector3(bounds.center.x, bounds.center.y, 0);
        Vector3 size = new Vector3(bounds.width, bounds.height, 0);

        Gizmos.DrawWireCube(center, size);
    }
}