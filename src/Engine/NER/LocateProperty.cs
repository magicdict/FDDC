using System;
using System.Collections.Generic;

public static class LocateProperty
{

    public struct LocAndValue<T>
    {
        //位置
        public int Loc;
        //值
        public T Value;
    }

    //获得日期
    public static List<LocAndValue<String>> LocateDate(HTMLEngine.MyRootHtmlNode root)
    {
        var list = new List<LocAndValue<String>>();
        foreach (var paragrah in root.Children)
        {
            foreach (var sentence in paragrah.Children)
            {
                var OrgString = sentence.Content;
                OrgString = Utility.ConvertUpperDateToLittle(OrgString).Replace(" ", "");
                if (!String.IsNullOrEmpty(RegularTool.GetDate(OrgString)))
                {
                    list.Add(new LocAndValue<String>() { Loc = sentence.PositionId, Value = RegularTool.GetDate(OrgString) });
                }
            }
        }
        return list;
    }

    //获得金额
    public static List<LocAndValue<Tuple<String,String>>> LocateMoney(HTMLEngine.MyRootHtmlNode root)
    {
        var list = new List<LocAndValue<Tuple<String,String>>>();
        foreach (var paragrah in root.Children)
        {
            foreach (var sentence in paragrah.Children)
            {
                var OrgString = sentence.Content;
                OrgString = Utility.ConvertUpperDateToLittle(OrgString).Replace(" ", "");
                var Money = Utility.SeekMoney(OrgString);
            }
        }
        return list;
    }
}