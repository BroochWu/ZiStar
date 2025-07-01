using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Portal : MonoBehaviour
{
    float timer;
    List<KeyValuePair<float, cfg.Beans.Enemy_Init>> waitCreateEnemys = new List<KeyValuePair<float, cfg.Beans.Enemy_Init>>();
    Quaternion portalBaseRotation;//传送门的初始旋转朝向
    int nowCreate;
    int enemyLevel;
    bool isActive;//是否还在生成怪物

    public void Initialize(cfg.dungeon.DungeonWave wave)
    {
        timer = 0;
        nowCreate = 0;
        isActive = true;
        enemyLevel = BattleManager.dungeonLevel;


        Utility.LookTarget2D(transform, Player.instance.rotationTarget.transform, 1, true);
        portalBaseRotation = transform.rotation;

        foreach (var i in wave.EnemyCreate)
        {
            // StartCoroutine(EnemyCreator(i.InitTime, i.EnemyInit, dungeonLevel));
            waitCreateEnemys.Add(new KeyValuePair<float, cfg.Beans.Enemy_Init>(i.InitTime, i.EnemyInit));
        }

        //按生成时间排序
        waitCreateEnemys.Sort((a, b) => a.Key.CompareTo(b.Key));
    }

    void Update()
    {
        if (!isActive) return;


        if (nowCreate >= waitCreateEnemys.Count)
        {
            isActive = false;
            BattleManager.Instance.UnregisterPortal(this);
            return;
        }

        timer += Time.deltaTime;

        if (timer >= waitCreateEnemys[nowCreate].Key)
        {
            CreateEnemy(waitCreateEnemys[nowCreate].Value);
            nowCreate += 1;
        }
    }

    /// <summary>
    /// 创造怪物（非协程）
    /// </summary>
    /// <param name="enemy_Init"></param>
    void CreateEnemy(cfg.Beans.Enemy_Init enemy_Init)
    {

        //根据所填的角度的数量，有几个角度就生成几个怪物
        foreach (var i in enemy_Init.Angles)
        {
            Quaternion moveDirection = portalBaseRotation * Quaternion.Euler(0, 0, i);

            var obj = ObjectPoolManager.Instance.GetEnemy(enemy_Init.EnemyId_Ref.Id).GetOrAddComponent<Enemy>();
            obj.Initialize(enemy_Init.EnemyId_Ref, enemyLevel, moveDirection, this);
            BattleManager.Instance.RegisterEnemy(obj);

        }
    }



    // /// <summary>
    // /// 创造怪物
    // /// </summary>
    // IEnumerator EnemyCreator(float time, cfg.Beans.Enemy_Init enemy_Init)
    // {
    //     //我想想，我计划这样：首先将预生成的怪物列表按照时间生成顺序升序排序
    //     //之后游戏时间正常跑，并用游戏时间去判断【第0个】怪物的诞生时间，一旦>=，就生成、移除
    //     //或者就用协程试试吧
    //     yield return new WaitForSeconds(time);

    //     // var prefab = Resources.Load<GameObject>($"Prefabs/Enemys/{enemy_Init.EnemyId_Ref.Prefab}");

    //     var enemyLevel = BattleManager.dungeonLevel;

    //     // 获取生成点的当前旋转
    //     Quaternion portalBaseRotation = transform.rotation;

    //     //根据所填的角度的数量，有几个角度就生成几个怪物
    //     foreach (var i in enemy_Init.Angles)
    //     {
    //         Quaternion moveDirection = portalBaseRotation * Quaternion.Euler(0, 0, i);

    //         var obj = ObjectPoolManager.Instance.GetEnemy(enemy_Init.EnemyId_Ref.Id);
    //         obj.GetOrAddComponent<Enemy>()?.Initialize(enemy_Init.EnemyId_Ref, enemyLevel, moveDirection, this);

    //     }


    // }
}
