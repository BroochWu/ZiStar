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
        var _useNum = Mathf.Clamp(DataManager.Instance.GetResourceCount(itemId), 1, 20);
        if (_useNum == 0)
        {
            UIManager.Instance.CommonToast("？这不是啥也没有吗");
            return;
        }
        StartCoroutine(UseChest(_useNum));
    }
    IEnumerator UseChest(int _useNum)
    {

        DataManager.Instance.rewardList.Clear();
        var wait = new WaitForSecondsRealtime(0.02f);
        //使用道具
        for (int i = 1; i <= _useNum; i++)
        {
            if (DataManager.Instance.UseItemInItemStruct(item, 1))
            {
                Debug.Log($"使用了{itemId}");
            }
            yield return wait;
        }

        ChestsRewardSystem.PlusChestScore(score * _useNum);

        var sortedDict =
        DataManager.Instance.rewardList
        .OrderByDescending(key => key.Key.Quality)
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Dictionary<cfg.item.Item, int> tempDict = new();
        // foreach (var i in sortedList)
        // {
        //     tempDict.Add(i.Key, i.Value);
        // }
        UIManager.Instance.CommonCongra(sortedDict);
        GetComponentInParent<TreasureDetailUI>().RefreshAll();

        // Dictionary<int, string> dict = new Dictionary<int, string>
        // {
        //     { 3, "C" }, // 假设这里的值（"C"）可以被某种方式转换为可比较的顺序类型，比如长度或字典序等。这里仅为示例。
        //     { 1, "A" }, // 例如，我们可以基于字符串长度来排序。
        //     { 2, "BB" } // 较长的字符串排在前面。
        // };

        // // 按Value排序（例如，按字符串长度）
        // var sortedDict = dict.OrderBy(kvp => kvp.Value.Length).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }



}