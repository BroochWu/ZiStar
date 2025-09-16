// EnemyRewardWeapon.cs - 奖励武器敌人
using UnityEngine;
using System.Collections;

public class EnemyRewardWeapon : EnemyBase
{
    private enum EnemyState { WANDER, STOP }
    private EnemyState _state;

    // 游荡相关变量
    private Vector2 _wanderTarget;
    private float _wanderChangeTime;
    private float _wanderChangeInterval = 2f;
    private System.Random _wanderRandom;
    private Vector2 _moveDir;

    public override void Initialize(cfg.enemy.Enemy enemy, int enemyLevel, Quaternion initDir, Portal parent)
    {
        //奖励类型的怪物，其层级+10，在普通的怪物之上
        enemyUI._initOrder += 10;
        _wanderRandom = BattleManager.Instance.dungeonSeed;

        _isReleased = false;
        BattleManager.Instance.RegisterEnemy(this);

        //宝箱怪无偏移
        transform.SetPositionAndRotation(parent.transform.position, Quaternion.Euler(new Vector3(0,0,180)));

        _enemyLevel = enemyLevel;

        if (enemyId != enemy.Id)
        {
            SetBasicEssentials(enemy);
        }

        StartWander();

        ResetAttributes();
    }


    protected override void UpdateBehavior()
    {
        switch (_state)
        {
            case EnemyState.WANDER:
                UpdateWanderState();
                break;
            case EnemyState.STOP:
                UpdateStopState();
                break;
        }
    }

    private void UpdateWanderState()
    {
        // 向目标移动
        _moveDir = _wanderTarget - (Vector2)transform.position;
        if (_moveDir.magnitude > 0.1f)
        {
            // 移动
            transform.position += (Vector3)_moveDir.normalized * _moveSpeed * 0.5f;

            // // 旋转朝向
            // float angle = Mathf.Atan2(_moveDir.y, _moveDir.x) * Mathf.Rad2Deg - 90;
            // Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
            // transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed * 0.5f);
        }
        else
        {
            //靠近了就停止
            _state = EnemyState.STOP;
            _wanderChangeTime = Time.time + _wanderChangeInterval;
        }
    }

    private void UpdateStopState()
    {
        //达到停止时间时，更新目标
        if (Time.time > _wanderChangeTime)
        {
            StartWander();
        }

    }

    void StartWander()
    {
        SetNewWanderTarget();
        _state = EnemyState.WANDER;
    }

    /// <summary>
    /// 设置随机移动的目标位置
    /// </summary>
    private void SetNewWanderTarget()
    {
        // 获取屏幕边界
        Vector2 screenBounds = CameraController.Instance.GetScreenBounds();

        // 在屏幕内随机选择一个目标点
        float x = (float)(_wanderRandom.NextDouble() * screenBounds.x * 1.8f - screenBounds.x);
        float y = (float)(_wanderRandom.NextDouble() * screenBounds.y * 1.5f - screenBounds.y);

        _wanderTarget = new Vector2(x, y);
    }

    protected override void OnDie()
    {
        base.OnDie(); // 调用基类的死亡逻辑

        // 启动抽卡流程(临时的，后面会改成武器)
        BattleManager.Instance.StartTri(cfg.Enums.Card.Type.WEAPONUNLOCK);
    }



    protected override void ResetAttributes()
    {
        base.ResetAttributes();

        _state = EnemyState.WANDER;

        if (_dynamicConfig == null)
        {
            _dynamicConfig = cfg.Tables.tb.Enemy.Get(enemyId);
        }

        // 计算属性
        _levelData = cfg.Tables.tb.EnemyLevel.Get(_dynamicConfig.LevelId, _enemyLevel);

        Damage = _levelData.Damage;
        InitHp = _levelData.Hp;
        _currentHp = InitHp;

        // 设置初始游荡目标
        SetNewWanderTarget();
    }
}