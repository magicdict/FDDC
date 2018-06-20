public static class WordUtility
{
    public const string 地名 = "ns";
    public const string 机构团体 = "nt";
    public const string 副词 = "d";
    public const string 助词 = "ul";
    public const string 英语 = "eng";
    public const string 标点 = "x";
    public const string 动词 = "v";

    //表示区间的文字和符号
    public static string[] RangeMarkAndChar = new string[] { "至", "-", "~", };

    //字典里面错误分类的地名
    public static string[] DictNSAdjust = new string[] { "大连", "霍尔果斯", "烟台" };
}