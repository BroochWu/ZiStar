using System.Collections;
using UnityEngine;

public class EnemyUI : MonoBehaviour
{
    // 公共字段和属性
    [Header("组件引用")]
    public GameObject hpBar;
    public GameObject hpLight;
    public SpriteRenderer sprite;
    public Material _spriteMaterial;
    protected int _initOrder = 20;


    void Awake()
    {

        // 缓存组件引用
        if (hpBar == null) hpBar = transform.Find("root/HpBar").gameObject;
        if (hpLight == null) hpLight = transform.Find("root/HpBar/Hp").gameObject;
        if (sprite == null) sprite = transform.Find("root/Sprite").GetComponent<SpriteRenderer>();

        hpBar.SetActive(false);
        _spriteMaterial = sprite.material;
    }

    void Update()
    {
        // 更新排序层级，不打会降低层级,以保证受击的永远在最前面
        if (Time.frameCount % 5 == 0)
        {
            sprite.sortingOrder = Mathf.Max(sprite.sortingOrder - 1, _initOrder);
        }
    }

    public IEnumerator OnHitEffect(float _hitDuration)
    {
        sprite.sortingOrder = 50;
        _spriteMaterial.color = Color.red;

        yield return new WaitForSeconds(_hitDuration);

        sprite.sortingOrder = _initOrder;
        _spriteMaterial.color = Color.white;
    }


    public void ResetAttributes()
    {

        _spriteMaterial.color = Color.white;
        hpBar.SetActive(false);
    }

}