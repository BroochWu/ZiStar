using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            var sprite = GetComponent<TreasureChest>().item.Image;
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
        GetComponent<Image>().sprite = GetComponent<TreasureChest>().item.Image;
    }



    public void UseChest()
    {
        var _useNum = Mathf.Clamp(DataManager.Instance.GetResourceCount(itemId), 0, 20);
        // Debug.Log("useNum：" + _useNum);
        if (_useNum == 0)
        {
            UIManager.Instance.CommonToast("？这不是啥也没有吗");
            return;
        }
        UseChest(_useNum);
    }
    void UseChest(int _useNum)
    {

        DataManager.Instance.rewardList.Clear();
        // var wait = new WaitForSecondsRealtime(0.02f);

        //后面可能要改，反复跨脚本调用
        //使用道具
        // for (int i = 1; i <= _useNum; i++)
        // {
        //     if (DataManager.Instance.UseItemInItemStruct(item, 1))
        //     {
        //         Debug.Log($"使用了{itemId}");
        //     }
        //     // yield return wait;
        // }

        //使用宝箱
        DataManager.Instance.UseItemInItemStruct(item, _useNum);

        //增加宝箱积分
        ChestsRewardSystem.PlusChestScore(score * _useNum);

        var sortedDict =
        DataManager.Instance.rewardList
        .OrderByDescending(reward => reward.rewardItem.Quality)
        .ToList();

        UIManager.Instance.CommonCongra(sortedDict);

        GetComponentInParent<TreasureDetailUI>().RefreshAll();

    }



}