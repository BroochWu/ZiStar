using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public void Initialize(cfg.dungeon.DungeonWave wave, int dungeonLevel)
    {
        Utility.LookTarget2D(transform, Player.instance.rotationTarget.transform, 1, true);
        foreach (var i in wave.EnemyCreate)
        {
            StartCoroutine(EnemyCreator(i.InitTime, i.EnemyInit, dungeonLevel));
        }
    }



    /// <summary>
    /// 创造怪物
    /// </summary>
    IEnumerator EnemyCreator(float time, cfg.Beans.Enemy_Init enemy_Init, int enemyLevel)
    {
        //我想想，我计划这样：首先将预生成的怪物列表按照时间生成顺序升序排序
        //之后游戏时间正常跑，并用游戏时间去判断【第0个】怪物的诞生时间，一旦>=，就生成、移除
        //或者就用协程试试吧
        yield return new WaitForSeconds(time);

        var prefab = Resources.Load<GameObject>($"Prefabs/Enemys/{enemy_Init.EnemyId_Ref.Prefab}");

        // 获取生成点的当前旋转
        Quaternion portalBaseRotation = transform.rotation;

        //根据所填的角度的数量，有几个角度就生成几个怪物
        foreach (var i in enemy_Init.Angles)
        {
            Quaternion moveDirection = portalBaseRotation * Quaternion.Euler(0, 0, i);

            var obj = ObjectPoolManager.Instance.GetEnemy(enemy_Init.EnemyId_Ref.Id);
            obj.GetOrAddComponent<Enemy>()?.Initialize(enemy_Init.EnemyId_Ref, enemyLevel, moveDirection, this);

        }


    }
}
