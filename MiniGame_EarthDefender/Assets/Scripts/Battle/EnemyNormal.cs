// EnemyNormal.cs - 普通敌人
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class EnemyNormal : EnemyBase
{
    private enum EnemyState { MOVE, ATTACK }
    private EnemyState _state;
    private string _bulletType;
    private static WaitForSeconds _attackSep = new WaitForSeconds(0.5f);
    
    public override void Initialize(cfg.enemy.Enemy enemy, int enemyLevel, Quaternion initDir, Portal parent)
    {
        _isReleased = false;
        BattleManager.Instance.RegisterEnemy(this);
        sprite.sortingOrder = _initOrder;
        transform.SetPositionAndRotation(parent.transform.position, initDir);
        _enemyLevel = enemyLevel;
        
        if (enemyId != enemy.Id)
        {
            SetBasicEssentials(enemy);
            _bulletType = _dynamicConfig.PrefabBullet;
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
            
            sprite.sortingOrder = Mathf.Max(sprite.sortingOrder - 1, _initOrder);
        }
    }
    
    private IEnumerator Attack()
    {
        while (_state == EnemyState.ATTACK)
        {
            var obj = ObjectPoolManager.Instance.GetBullet(_bulletType);
            obj.GetOrAddComponent<Bullet>().Initialize(this);
            yield return _attackSep;
        }
    }
    
    private void ResetAttributes()
    {
        _state = EnemyState.MOVE;
        _spriteMaterial.color = Color.white;
        hpBar.SetActive(false);
        
        if (_dynamicConfig == null)
        {
            _dynamicConfig = cfg.Tables.tb.Enemy.Get(enemyId);
        }
        
        // 计算属性
        float additionMulti = 0;
        _levelData = cfg.Tables.tb.EnemyLevel.Get(_dynamicConfig.LevelId, _enemyLevel);
        
        if (_enemyType == cfg.Enums.Enemy.Type.TRASH)
        {
            additionMulti = (int)(BattleManager.Instance.GameTime / 5) * 1f;
        }
        
        Damage = (int)(_levelData.Damage * (1 + additionMulti));
        InitHp = (int)(_levelData.Hp * (1 + additionMulti));
        _currentHp = InitHp;
    }
}