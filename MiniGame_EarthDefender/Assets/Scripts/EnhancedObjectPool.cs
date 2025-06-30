using System.Collections.Generic;
using UnityEngine;

public class EnhancedObjectPool : MonoBehaviour
{
    public static EnhancedObjectPool Instance;
    public GameObject prefab;
    public int initialSize = 20;
    public int maxSize = 100;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private List<GameObject> activeObjects = new List<GameObject>();

    public static List<GameObject> ActiveObjects => Instance.activeObjects;

    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        InitializePool();
    }

    void InitializePool()
    {
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }

    GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }

    public GameObject GetObject()
    {
        if (pool.Count == 0)
        {
            if (activeObjects.Count + pool.Count < maxSize)
            {
                CreateNewObject();
            }
            else
            {
                // 达到最大数量限制
                return null;
            }
        }

        GameObject obj = pool.Dequeue();
        activeObjects.Add(obj);
        obj.SetActive(true);
        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        if (activeObjects.Contains(obj))
        {
            activeObjects.Remove(obj);
            pool.Enqueue(obj);
            obj.SetActive(false);
        }
    }

    public void Prewarm(int count)
    {
        int toCreate = Mathf.Min(count, maxSize - (pool.Count + activeObjects.Count));
        for (int i = 0; i < toCreate; i++)
        {
            CreateNewObject();
        }
    }
}