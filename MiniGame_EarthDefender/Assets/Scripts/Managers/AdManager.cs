public static class AdManager
{
    // private static AdManager _Instance;
    // public static AdManager Instance => _Instance ??= new AdManager();

    public static bool PlayAd()
    {
        UIManager.Instance.CommonToast("看完广告了");
        //执行AVG事件
        AvgManager.Instance.CheckAndTriggerAvgs(cfg.Enums.Com.TriggerType.PLAYAD);
        return true;
    }
}