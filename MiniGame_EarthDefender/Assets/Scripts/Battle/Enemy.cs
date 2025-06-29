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
        //敌人最终目标是地球半径某处

        if (Vector3.Distance(transform.position, Player.instance.rotationTarget.transform.position) >= cfg.Tables.tb.GlobalParam.Get("enemy_stop_distance").IntValue)
        {
            //Debug.Log("当前距离：" + Vector3.Distance(transform.position, Player.instance.rotationTarget.transform.position));
            transform.position += transform.up * Time.deltaTime;

        }

    }


    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("碰撞成功");
    }
}
