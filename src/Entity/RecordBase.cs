public abstract class RecordBase
{
    /// <summary>
    /// 主键
    /// </summary>
    /// <returns></returns>
    public abstract string GetKey();

    /// <summary>
    /// 转换为字符
    /// </summary>
    /// <returns></returns>
    public abstract string ConvertToString();

    //公告id
    public string Id;

    public abstract string CSVTitle();
}