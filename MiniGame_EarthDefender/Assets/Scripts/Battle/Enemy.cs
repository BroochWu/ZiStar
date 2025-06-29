using UnityEngine;

public class Enemy : MonoBehaviour
{
    int id;
    string TextName;
    GameObject prefab;

    public void Initialize(cfg.enemy.Enemy enemy)
    {
        this.id = enemy.Id;
        this.TextName = enemy.TextName;
        this.prefab = Resources.Load<GameObject>($"Prefabs/Enemys/{enemy.Prefab}");
    }



    void Update()
    {
        // transform.position += Time.deltaTime * Vector3.down;
        // transform.position += transform.position * Time.deltaTime;
        transform.position += transform.up * Time.deltaTime;
    }


    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("碰撞成功");
    }
}
