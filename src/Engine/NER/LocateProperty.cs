using System;
using System.Collections.Generic;

public static class LocateProperty
{

    public struct LocAndValue
    {
        public int Loc;

        public string Value;
    }

    public static List<LocAndValue> LocateDate(HTMLEngine.MyRootHtmlNode root)
    {
        var list = new List<LocAndValue>();
        foreach (var paragrah in root.Children)
        {
            foreach (var sentence in paragrah.Children)
            {
                var OrgString = sentence.Content;
                OrgString = Utility.ConvertUpperDateToLittle(OrgString).Replace(" ", "");
                if (!String.IsNullOrEmpty(RegularTool.GetDate(OrgString)))
                {
                    list.Add(new LocAndValue() { Loc = sentence.PositionId, Value = RegularTool.GetDate(OrgString) });
                }
            }
        }
        return list;
    }

    public static List<LocAndValue> LocateMoney(HTMLEngine.MyRootHtmlNode root)
    {
        var list = new List<LocAndValue>();
        foreach (var paragrah in root.Children)
        {
            foreach (var sentence in paragrah.Children)
            {
                var OrgString = sentence.Content;
                OrgString = Utility.ConvertUpperDateToLittle(OrgString).Replace(" ", "");
                var Money = Utility.SeekMoney(OrgString);
                if (!String.IsNullOrEmpty(Money))
                {
                    list.Add(new LocAndValue() { Loc = sentence.PositionId, Value = Money });
                }
            }
        }
        return list;
    }
}