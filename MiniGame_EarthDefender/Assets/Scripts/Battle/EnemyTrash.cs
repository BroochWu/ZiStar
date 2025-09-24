// EnemyNormal.cs - 普通敌人
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class EnemyTrash : EnemyBase
{
    private enum EnemyState { MOVE, ATTACK }
    private EnemyState _state;
    private static WaitForSeconds _attackSep = new WaitForSeconds(0.5f);

    public override void Initialize(cfg.enemy.Enemy enemy, int enemyLevel, Quaternion initDir, Portal parent)
    {
        IsReleased = false;
        BattleManager.Instance.RegisterEnemy(this);
        // sprite.sortingOrder = _initOrder;
        transform.SetPositionAndRotation(parent.transform.position, initDir);
        _enemyLevel = enemyLevel;



        if (enemyId != enemy.Id)
        {
            SetBasicEssentials(enemy);
        }

        ResetAttributes();
    }

    protected override void UpdateBehavior()
    {
        if (_state == EnemyState.MOVE)
        {
            UpdateMoveState();
        }
        // 攻击状态由协程处理
    }

    private void UpdateMoveState()
    {
        // 移动逻辑
        transform.position += transform.up * _moveSpeed;

        // 旋转朝向
        Utility.LookTarget2D(transform, _earth, _rotationSpeed, true);

        // 优化距离检查
        if (Time.frameCount % 5 == 0)
        {
            var dis = ((Vector2)(transform.position - _earth.position)).sqrMagnitude;
            if (dis <= _enemyStopDisSqr)
            {
                _state = EnemyState.ATTACK;
                StartCoroutine(Attack());
            }

            // sprite.sortingOrder = Mathf.Max(sprite.sortingOrder - 1, _initOrder);
        }
    }

    private IEnumerator Attack()
    {
        while (_state == EnemyState.ATTACK)
        {
            var obj = ObjectPoolManager.Instance.GetBullet(bulletId);
            obj.GetOrAddComponent<Bullet>().Initialize(this);
            yield return _attackSep;
        }
    }

    protected override void ResetAttributes()
    {
        base.ResetAttributes();

        _state = EnemyState.MOVE;

    }
}