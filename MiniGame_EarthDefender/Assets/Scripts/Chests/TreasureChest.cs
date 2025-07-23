using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TreasureChest : MonoBehaviour
{
    public int itemId;
    public cfg.item.Item item => cfg.Tables.tb.Item.Get(itemId);
    public int score;

    private Toggle _toggle;

    void Awake()
    {
        //正在看的可能没有
        TryGetComponent(out _toggle);
        if (TryGetComponent<PersistentSelectedToggle>(out var persist))
        {
            var sprite = Resources.Load<Sprite>($"Images/{GetComponent<TreasureChest>().item.ImagePath}");
            persist.normalSprite = sprite;
            if (persist.isON)
            {
                GetComponentInParent<TreasureDetailUI>().SetNowLookChest(this);
            }
            else
            {
                GetComponent<Image>().sprite = sprite;
            }

        }
    }
    void OnEnable()
    {
        // 订阅Toggle值变化事件
        _toggle?.onValueChanged.AddListener(OnToggleValueChanged);
    }
    void OnDisable()
    {
        // 订阅Toggle值变化事件
        _toggle?.onValueChanged.RemoveListener(OnToggleValueChanged);
    }
    void OnToggleValueChanged(bool _isOn)
    {
        if (_isOn)
        {
            GetComponentInParent<TreasureDetailUI>().SetNowLookChest(this);
        }
    }

    public void RefreshUI()
    {
        GetComponentInChildren<Text>().text = 'x' + DataManager.Instance.GetItemCount(item).ToString();
    }

    public void RefreshImage()
    {
        GetComponent<Image>().sprite = Resources.Load<Sprite>($"Images/{GetComponent<TreasureChest>().item.ImagePath}");
    }



    public void UseChest()
    {
        var count = Mathf.Clamp(DataManager.Instance.GetResourceCount(itemId), 1, 20);
        if (count == 0)
        {
            UIManager.Instance.CommonToast("？这不是啥也没有吗");
            return;
        }
        StartCoroutine(UseChest(count));
    }
    IEnumerator UseChest(int count)
    {

        DataManager.Instance.rewardList.Clear();
        var wait = new WaitForSeconds(0.05f);
        //使用道具
        for (int i = 1; i <= count; i++)
        {
            if (DataManager.Instance.UseItemInItemStruct(item, 1))
            {
                Debug.Log($"使用了{itemId}");
            }
            yield return wait;
        }
        UIManager.Instance.CommonCongra(DataManager.Instance.rewardList);
        GetComponentInParent<TreasureDetailUI>().RefreshAllChests();

    }



}