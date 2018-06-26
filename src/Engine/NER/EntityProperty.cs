using System.Collections.Generic;

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

    public Dictionary<string, string> KeyWordMap = new Dictionary<string, string>();

    public List<string> ExtractByKeyWordMap(HTMLEngine.MyRootHtmlNode root)
    {
        var result = new List<string>();
        foreach (var item in KeyWordMap)
        {
            var cnt = ExtractPropertyByHTML.FindWordCnt(item.Key, root).Count;
            if (cnt > 0){
                if (!result.Contains(item.Value)) result.Add(item.Value);
            }
        }
        return result;
    }

}