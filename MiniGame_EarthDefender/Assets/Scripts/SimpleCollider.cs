using UnityEngine;

public class SimpleCollider : MonoBehaviour
{
    public float Width = 1f;
    public float Height = 1f;
    public float offsetY = 0f;

    [Header("调试选项")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.green;

    void Start()
    {
#if !UNITY_EDITOR
        showGizmos = false;
#endif
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

    // 获取碰撞体边界框（正确跟随旋转）
    public Rect GetBounds()
    {
        // 获取世界空间中的四个角点
        Vector2[] worldCorners = GetWorldCornerPoints();

        // 计算包含所有点的边界框
        float minX = Mathf.Min(worldCorners[0].x, worldCorners[1].x, worldCorners[2].x, worldCorners[3].x);
        float maxX = Mathf.Max(worldCorners[0].x, worldCorners[1].x, worldCorners[2].x, worldCorners[3].x);
        float minY = Mathf.Min(worldCorners[0].y, worldCorners[1].y, worldCorners[2].y, worldCorners[3].y);
        float maxY = Mathf.Max(worldCorners[0].y, worldCorners[1].y, worldCorners[2].y, worldCorners[3].y);

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    // 获取世界空间中的四个角点
    public Vector2[] GetWorldCornerPoints()
    {
        // 计算本地空间的四个角点（不考虑旋转）
        Vector2[] localCorners = new Vector2[4];

        // 考虑缩放的实际尺寸
        float scaledWidth = Width * transform.lossyScale.x;
        float scaledHeight = Height * transform.lossyScale.y;
        float scaledOffsetY = offsetY * transform.lossyScale.y;

        // 本地空间的角点（以中心为原点）
        localCorners[0] = new Vector2(-scaledWidth / 2, -scaledHeight / 2); // 左下
        localCorners[1] = new Vector2(scaledWidth / 2, -scaledHeight / 2);  // 右下
        localCorners[2] = new Vector2(scaledWidth / 2, scaledHeight / 2);   // 右上
        localCorners[3] = new Vector2(-scaledWidth / 2, scaledHeight / 2);  // 左上

        // 应用Y轴偏移
        for (int i = 0; i < 4; i++)
        {
            localCorners[i] += new Vector2(0, scaledOffsetY);
        }

        // 转换到世界空间（应用旋转和平移）
        Vector2[] worldCorners = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            worldCorners[i] = TransformLocalToWorld(localCorners[i]);
        }

        return worldCorners;
    }

    // 将本地坐标转换为世界坐标（考虑旋转和位置）
    private Vector2 TransformLocalToWorld(Vector2 localPoint)
    {
        // 获取旋转矩阵
        float angle = transform.eulerAngles.z * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        // 应用旋转
        Vector2 rotatedPoint = new Vector2(
            localPoint.x * cos - localPoint.y * sin,
            localPoint.x * sin + localPoint.y * cos
        );

        // 应用位置偏移
        return rotatedPoint + (Vector2)transform.position;
    }

    // 检查两个碰撞体是否相交（使用分离轴定理，更精确）
    public bool CheckCollision(SimpleCollider other)
    {
        Vector2[] myCorners = this.GetWorldCornerPoints();
        Vector2[] otherCorners = other.GetWorldCornerPoints();

        // 分离轴定理检测
        return SATCollision(myCorners, otherCorners);
    }

    // 分离轴定理实现
    private bool SATCollision(Vector2[] polyA, Vector2[] polyB)
    {
        // 检查多边形A的边
        for (int i = 0; i < polyA.Length; i++)
        {
            Vector2 edge = polyA[(i + 1) % polyA.Length] - polyA[i];
            Vector2 normal = new Vector2(-edge.y, edge.x).normalized;

            if (!OverlapOnAxis(polyA, polyB, normal))
                return false;
        }

        // 检查多边形B的边
        for (int i = 0; i < polyB.Length; i++)
        {
            Vector2 edge = polyB[(i + 1) % polyB.Length] - polyB[i];
            Vector2 normal = new Vector2(-edge.y, edge.x).normalized;

            if (!OverlapOnAxis(polyA, polyB, normal))
                return false;
        }

        return true;
    }

    // 检查在两个多边形在给定轴上的投影是否重叠
    private bool OverlapOnAxis(Vector2[] polyA, Vector2[] polyB, Vector2 axis)
    {
        float minA = float.MaxValue;
        float maxA = float.MinValue;
        float minB = float.MaxValue;
        float maxB = float.MinValue;

        // 计算多边形A在轴上的投影
        for (int i = 0; i < polyA.Length; i++)
        {
            float projection = Vector2.Dot(polyA[i], axis);
            minA = Mathf.Min(minA, projection);
            maxA = Mathf.Max(maxA, projection);
        }

        // 计算多边形B在轴上的投影
        for (int i = 0; i < polyB.Length; i++)
        {
            float projection = Vector2.Dot(polyB[i], axis);
            minB = Mathf.Min(minB, projection);
            maxB = Mathf.Max(maxB, projection);
        }

        // 检查投影是否重叠
        return maxA >= minB && maxB >= minA;
    }

    // 获取碰撞体中心点（世界坐标）
    public Vector2 GetCenter()
    {
        // 本地中心点（考虑偏移）
        Vector2 localCenter = new Vector2(0, offsetY * transform.lossyScale.y);

        // 转换到世界空间
        return TransformLocalToWorld(localCenter);
    }

    // 可视化调试 - 正确绘制旋转后的碰撞体
    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;

        // 获取世界空间中的四个角点
        Vector2[] corners = GetWorldCornerPoints();

        // 绘制四条边
        for (int i = 0; i < 4; i++)
        {
            int nextIndex = (i + 1) % 4;
            Gizmos.DrawLine(
                new Vector3(corners[i].x, corners[i].y, 0),
                new Vector3(corners[nextIndex].x, corners[nextIndex].y, 0)
            );
        }

        // 绘制中心点
        Gizmos.color = Color.red;
        Vector2 center = GetCenter();
        Gizmos.DrawSphere(new Vector3(center.x, center.y, 0), 0.05f);

        // 绘制朝向指示器
        Gizmos.color = Color.blue;
        Vector3 directionEnd = new Vector3(center.x, center.y, 0) + transform.up * 0.3f;
        Gizmos.DrawLine(new Vector3(center.x, center.y, 0), directionEnd);

        // 绘制边界框（用于调试）
        Gizmos.color = new Color(1, 1, 0, 0.3f); // 半透明黄色
        Rect bounds = GetBounds();
        Vector3 boundsCenter = new Vector3(bounds.center.x, bounds.center.y, 0);
        Vector3 boundsSize = new Vector3(bounds.width, bounds.height, 0.1f);
        Gizmos.DrawWireCube(boundsCenter, boundsSize);
    }

    // 获取碰撞体在世界空间中的法线（用于碰撞响应）
    public Vector2 GetNormalAtPoint(Vector2 worldPoint)
    {
        Vector2[] corners = GetWorldCornerPoints();
        Vector2 closestPoint = GetClosestPointOnPerimeter(worldPoint);

        // 计算法线（从碰撞体中心指向碰撞点）
        Vector2 center = GetCenter();
        Vector2 normal = (closestPoint - center).normalized;

        return normal;
    }

    // 获取边界上离给定点最近的点
    public Vector2 GetClosestPointOnPerimeter(Vector2 worldPoint)
    {
        Vector2[] corners = GetWorldCornerPoints();
        Vector2 closestPoint = corners[0];
        float closestDistance = Vector2.Distance(worldPoint, corners[0]);

        // 检查每条边
        for (int i = 0; i < 4; i++)
        {
            int nextIndex = (i + 1) % 4;
            Vector2 pointOnEdge = GetClosestPointOnLineSegment(corners[i], corners[nextIndex], worldPoint);
            float distance = Vector2.Distance(worldPoint, pointOnEdge);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = pointOnEdge;
            }
        }

        return closestPoint;
    }

    // 获取线段上离给定点最近的点
    private Vector2 GetClosestPointOnLineSegment(Vector2 a, Vector2 b, Vector2 point)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(point - a, ab) / Vector2.Dot(ab, ab);
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }
}