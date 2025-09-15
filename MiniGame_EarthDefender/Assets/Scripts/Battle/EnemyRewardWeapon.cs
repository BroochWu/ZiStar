// EnemyRewardWeapon.cs - 奖励武器敌人
using UnityEngine;
using System.Collections;
using System;

public class EnemyRewardWeapon : EnemyBase
{
    private enum EnemyState { WANDER }
    private EnemyState _state;

    // 游荡相关变量
    private Vector2 _wanderTarget;
    private float _wanderChangeTime;
    private float _wanderChangeInterval = 2f;
    private System.Random _wanderRandom;

    public override void Initialize(cfg.enemy.Enemy enemy, int enemyLevel, Quaternion initDir, Portal parent)
    {
        _isReleased = false;
        BattleManager.Instance.RegisterEnemy(this);
        // sprite.sortingOrder = _initOrder;
        transform.SetPositionAndRotation(parent.transform.position, initDir);
        _enemyLevel = enemyLevel;

        if (enemyId != enemy.Id)
        {
            SetBasicEssentials(enemy);
        }

        // 初始化随机数生成器(种子)
        int seed = BattleManager.Instance.dungeonId + enemyId;
        _wanderRandom = new System.Random(seed);

        ResetAttributes();
    }

    protected override void UpdateBehavior()
    {
        if (_state == EnemyState.WANDER)
        {
            UpdateWanderState();
        }
    }

    private void UpdateWanderState()
    {
        // 定期更新游荡目标
        if (Time.time > _wanderChangeTime)
        {
            SetNewWanderTarget();
            _wanderChangeTime = Time.time + _wanderChangeInterval;
        }

        // 向目标移动
        Vector2 direction = _wanderTarget - (Vector2)transform.position;
        if (direction.magnitude > 0.1f)
        {
            // 移动
            transform.position += (Vector3)direction.normalized * _moveSpeed * 0.5f;

            // 旋转朝向
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed * 0.5f);
        }
    }

    private void SetNewWanderTarget()
    {
        // 获取屏幕边界
        Vector2 screenBounds = CameraController.Instance.GetScreenBounds();

        // 在屏幕内随机选择一个目标点
        float x = (float)(_wanderRandom.NextDouble() * screenBounds.x * 2 - screenBounds.x);
        float y = (float)(_wanderRandom.NextDouble() * screenBounds.y * 2 - screenBounds.y);

        _wanderTarget = new Vector2(x, y);
    }

    protected override void OnDie()
    {
        base.OnDie(); // 调用基类的死亡逻辑

        // 启动抽卡流程
        StartCoroutine(StartDrawProcess());
    }

    private IEnumerator StartDrawProcess()
    {
        yield return null;
        // ShopDrawManager.instance.StartDrawProcess(1);
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