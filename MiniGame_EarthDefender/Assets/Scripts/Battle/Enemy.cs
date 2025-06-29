using UnityEngine;

public class Enemy : MonoBehaviour
{
    int id;
    string TextName;
    GameObject prefab;
    Transform Earth;

    float rotationSpeed = 0.2f;

    public void Initialize(cfg.enemy.Enemy enemy)
    {
        this.id = enemy.Id;
        this.TextName = enemy.TextName;
        this.prefab = Resources.Load<GameObject>($"Prefabs/Enemys/{enemy.Prefab}");
    }

    void Awake()
    {
        Earth = Player.instance.rotationTarget.transform;
    }

    void Update()
    {
        //朝向地球的方向
        Utility.LookTarget2D(transform, Earth, rotationSpeed);


        //敌人最终目标是地球半径某处，抵达即停止并攻击
        if (Vector3.Distance(transform.position, Earth.position) >= cfg.Tables.tb.GlobalParam.Get("enemy_stop_distance").IntValue)
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
