using System.Collections.Generic;
using System.Linq;
using FDDC;

public class EntityProperty
{
    //属性类型
    public enmType PropertyType = enmType.Normal;

    public enum enmType
    {
        Money,      //金钱
        Number,     //数字
        Date,       //日期
        Normal,     //普通文本
    }

    //最大长度
    public int MaxLength = -1;
    //最小长度
    public int MinLength = -1;

    public string Extract(AnnouceDocument doc)
    {
        if (KeyWordMap.Count != 0)
        {
            //纯关键字类型
            var candidate = ExtractByKeyWordMap(doc.root);
            if (candidate.Count == 0) return "";
            if (candidate.Count > 1)
            {
                if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("找到纯关键字类型两个关键字");
            }
            return candidate.First();
        }

        return "";
    }

    //纯关键字类型
    public Dictionary<string, string> KeyWordMap = new Dictionary<string, string>();

    public List<string> ExtractByKeyWordMap(HTMLEngine.MyRootHtmlNode root)
    {
        var result = new List<string>();
        foreach (var item in KeyWordMap)
        {
            var cnt = ExtractPropertyByHTML.FindWordCnt(item.Key, root).Count;
            if (cnt > 0)
            {
                if (!result.Contains(item.Value)) result.Add(item.Value);
            }
        }
        return result;
    }

}