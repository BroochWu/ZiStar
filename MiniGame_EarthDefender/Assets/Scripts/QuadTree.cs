using System.Collections.Generic;
using UnityEngine;

public partial class QuadTree
{
    private const int MAX_OBJECTS = 8; // 每个分区最多接受几个物体
    private const int MAX_LEVELS = 5;  // 树最多多深

    private int level;
    private List<GameObject> objects;
    private Rect bounds;
    private QuadTree[] nodes;

    public QuadTree(int level, Rect bounds)
    {
        this.level = level;
        this.bounds = bounds;
        objects = new List<GameObject>();
        nodes = new QuadTree[4];
    }

    public void Clear()
    {
        objects.Clear();
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null)
            {
                nodes[i].Clear();
                nodes[i] = null;
            }
        }
    }

    private void Split()
    {
        float subWidth = bounds.width / 2f;
        float subHeight = bounds.height / 2f;
        float x = bounds.x;
        float y = bounds.y;

        // 修正象限顺序：左上、右上、左下、右下
        nodes[0] = new QuadTree(level + 1, new Rect(x, y + subHeight, subWidth, subHeight)); // 左上
        nodes[1] = new QuadTree(level + 1, new Rect(x + subWidth, y + subHeight, subWidth, subHeight)); // 右上
        nodes[2] = new QuadTree(level + 1, new Rect(x, y, subWidth, subHeight)); // 左下
        nodes[3] = new QuadTree(level + 1, new Rect(x + subWidth, y, subWidth, subHeight)); // 右下
    }

    public int GetIndex(Rect rect)
    {
        float verticalMidpoint = bounds.x + (bounds.width / 2f);
        float horizontalMidpoint = bounds.y + (bounds.height / 2f);

        // 检查物体是否完全在某个象限内
        bool topHalf = rect.y > horizontalMidpoint;
        bool bottomHalf = rect.y + rect.height < horizontalMidpoint;
        bool leftHalf = rect.x + rect.width < verticalMidpoint;
        bool rightHalf = rect.x > verticalMidpoint;

        if (topHalf)
        {
            if (leftHalf) return 0; // 左上
            if (rightHalf) return 1; // 右上
        }
        else if (bottomHalf)
        {
            if (leftHalf) return 2; // 左下
            if (rightHalf) return 3; // 右下
        }

        // 物体跨越多个象限，返回-1表示留在当前节点
        return -1;
    }

    public void Insert(GameObject obj)
    {
        // 只处理活动对象
        if (obj == null || !obj.activeSelf) return;

        if (nodes[0] != null)
        {
            // 获取对象的边界框
            Rect objBounds = GetRectForObject(obj);
            int index = GetIndex(objBounds);

            if (index != -1)
            {
                nodes[index].Insert(obj);
                return;
            }
        }

        objects.Add(obj);

        // 分裂条件：对象数量超过阈值且未达到最大深度
        if (objects.Count > MAX_OBJECTS && level < MAX_LEVELS)
        {
            if (nodes[0] == null) Split();

            int i = 0;
            while (i < objects.Count)
            {
                GameObject currentObj = objects[i];
                Rect objBounds = GetRectForObject(currentObj);
                int index = GetIndex(objBounds);

                if (index != -1 && currentObj != null && currentObj.activeSelf)
                {
                    nodes[index].Insert(currentObj);
                    objects.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
    }

    public List<GameObject> Retrieve(List<GameObject> returnObjects, Rect queryBounds)
    {
        // 检查查询矩形是否与当前节点相交
        if (!bounds.Overlaps(queryBounds))
            return returnObjects;

        // 如果当前节点有子节点，递归查询
        if (nodes[0] != null)
        {
            foreach (var node in nodes)
            {
                node.Retrieve(returnObjects, queryBounds);
            }
        }

        // 添加当前节点的对象
        foreach (var obj in objects)
        {
            if (obj != null && obj.activeSelf && !returnObjects.Contains(obj))
            {
                // 检查对象是否在查询矩形内
                var objCollider = obj.GetComponent<SimpleCollider>();
                if (objCollider != null && queryBounds.Overlaps(objCollider.GetBounds()))
                {
                    returnObjects.Add(obj);
                }
            }
        }

        return returnObjects;
    }

    private Rect GetRectForObject(GameObject obj)
    {
        if (obj == null) return new Rect();

        // 获取对象的 SimpleCollider 组件
        SimpleCollider collider = obj.GetComponent<SimpleCollider>();
        if (collider == null) return new Rect();

        // 使用碰撞体的实际边界框（考虑旋转）
        return collider.GetBounds();
    }
}