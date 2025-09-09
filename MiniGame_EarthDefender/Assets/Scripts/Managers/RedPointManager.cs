public class RedPointManager
{
    private static RedPointManager _instance;
    public static RedPointManager Instance => _instance ??= new RedPointManager();
}