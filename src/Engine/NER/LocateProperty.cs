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
    public static List<LocAndValue<DateTime>> LocateDate(HTMLEngine.MyRootHtmlNode root)
    {
        var list = new List<LocAndValue<DateTime>>();
        foreach (var paragrah in root.Children)
        {
            foreach (var sentence in paragrah.Children)
            {
                var OrgString = sentence.Content;
                OrgString = Utility.ConvertUpperDateToLittle(OrgString).Replace(" ", "");
                var datelist = RegularTool.GetDate(OrgString);
                foreach (var strDate in datelist)
                {
                    var DateNumberList = RegularTool.GetNumberList(strDate);
                    String Year = DateNumberList[0];
                    String Month = DateNumberList[1];
                    String Day = DateNumberList[2];
                    int year; int month; int day;
                    if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
                    {
                        list.Add(new LocAndValue<DateTime>()
                        {
                            Loc = sentence.PositionId,
                            Value = Utility.GetWorkDay(year, month, day)
                        });
                    }
                }
            }
        }
        return list;
    }

    //获得金额
    public static List<LocAndValue<Tuple<String, String>>> LocateMoney(HTMLEngine.MyRootHtmlNode root)
    {
        var list = new List<LocAndValue<Tuple<String, String>>>();
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