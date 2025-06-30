using System.Collections.Generic;
using UnityEngine;

public class QuadTree
{
    private const int MAX_OBJECTS = 8;//每个分区最多接受几个物体
    private const int MAX_LEVELS = 5;//树最多多深

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

        nodes[0] = new QuadTree(level + 1, new Rect(x + subWidth, y, subWidth, subHeight));
        nodes[1] = new QuadTree(level + 1, new Rect(x, y, subWidth, subHeight));
        nodes[2] = new QuadTree(level + 1, new Rect(x, y + subHeight, subWidth, subHeight));
        nodes[3] = new QuadTree(level + 1, new Rect(x + subWidth, y + subHeight, subWidth, subHeight));
    }

    public int GetIndex(Rect rect)
    {
        int index = -1;
        float verticalMidpoint = bounds.x + (bounds.width / 2f);
        float horizontalMidpoint = bounds.y + (bounds.height / 2f);

        bool topQuadrant = rect.y > horizontalMidpoint;
        bool bottomQuadrant = rect.y < horizontalMidpoint && (rect.y + rect.height) < horizontalMidpoint;
        bool leftQuadrant = rect.x < verticalMidpoint && (rect.x + rect.width) < verticalMidpoint;
        bool rightQuadrant = rect.x > verticalMidpoint;

        if (leftQuadrant)
        {
            if (topQuadrant) index = 2;
            else if (bottomQuadrant) index = 1;
        }
        else if (rightQuadrant)
        {
            if (topQuadrant) index = 3;
            else if (bottomQuadrant) index = 0;
        }

        return index;
    }

    public void Insert(GameObject obj)
    {
        if (nodes[0] != null)
        {
            int index = GetIndex(GetRectForObject(obj));
            if (index != -1)
            {
                nodes[index].Insert(obj);
                return;
            }
        }

        objects.Add(obj);

        if (objects.Count > MAX_OBJECTS && level < MAX_LEVELS)
        {
            if (nodes[0] == null) Split();

            int i = 0;
            while (i < objects.Count)
            {
                int index = GetIndex(GetRectForObject(objects[i]));
                if (index != -1)
                {
                    nodes[index].Insert(objects[i]);
                    objects.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
    }

    public List<GameObject> Retrieve(List<GameObject> returnObjects, GameObject obj)
    {
        int index = GetIndex(GetRectForObject(obj));
        if (index != -1 && nodes[0] != null)
        {
            nodes[index].Retrieve(returnObjects, obj);
        }
        returnObjects.AddRange(objects);
        return returnObjects;
    }

    private Rect GetRectForObject(GameObject obj)
    {
        // 简化的边界框计算
        float size = obj.GetComponent<SimpleCollider>().Size;
        Vector3 pos = obj.transform.position;
        return new Rect(pos.x - size/2, pos.y - size/2, size, size);
    }
}