using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
public static class LocateProperty
{

    public struct LocAndValue<T>
    {
        //位置
        public int Loc;
        //值
        public T Value;
        //类型
        public string Type;
    }

    /// <summary>
    /// 引号和书名号内容提取
    /// </summary>
    /// <param name="root">原始HTML</param>
    /// <param name="IsSkipBracket">是否忽略括号内部的内容</param>
    /// <returns></returns>
    public static List<LocAndValue<String>> LocateQuotation(HTMLEngine.MyRootHtmlNode root, bool IsSkipBracket = true)
    {
        var list = new List<LocAndValue<String>>();
        foreach (var paragrah in root.Children)
        {
            foreach (var sentence in paragrah.Children)
            {
                var OrgString = sentence.Content;
                var BracketList = RegularTool.GetChineseBrackets(OrgString);
                Regex r = new Regex(@"\《.*?\》");
                foreach (var item in r.Matches(OrgString).ToList())
                {
                    bool IsContentInBracket = false;
                    foreach (var bracketItem in BracketList)
                    {
                        if (bracketItem.Contains(item.Value))
                        {
                            IsContentInBracket = true;
                            break;
                        }
                    }
                    if (IsSkipBracket && IsContentInBracket) continue;
                    list.Add(new LocAndValue<String>()
                    {
                        Loc = sentence.PositionId,
                        Type = "字符",
                        Value = item.Value.Substring(1, item.Value.Length - 2)
                    });
                }
                r = new Regex(@"\“.*?\”");
                foreach (var item in r.Matches(OrgString).ToList())
                {
                    bool IsContentInBracket = false;
                    foreach (var bracketItem in BracketList)
                    {
                        if (bracketItem.Contains(item.Value))
                        {
                            IsContentInBracket = true;
                            break;
                        }
                    }
                    if (IsSkipBracket && IsContentInBracket) continue;
                    list.Add(new LocAndValue<String>()
                    {
                        Loc = sentence.PositionId,
                        Type = "字符",
                        Value = item.Value.Substring(1, item.Value.Length - 2)
                    });
                }
            }
        }
        return list;
    }


    //获得日期
    public static List<LocAndValue<(DateTime StartDate, DateTime EndDate)>> LocateDateRange(HTMLEngine.MyRootHtmlNode root)
    {
        var list = new List<LocAndValue<(DateTime StartDate, DateTime EndDate)>>();
        foreach (var paragrah in root.Children)
        {
            foreach (var sentence in paragrah.Children)
            {
                var OrgString = sentence.Content;
                OrgString = DateUtility.ConvertUpperToLower(OrgString).Replace(" ", String.Empty);
                var datelist = DateUtility.GetRangeDate(OrgString);
                foreach (var strDate in datelist)
                {
                    var DateNumberList = RegularTool.GetNumberList(strDate);
                    DateTime ST = new DateTime();
                    DateTime ED = new DateTime();
                    if (DateNumberList.Count == 6)
                    {
                        String Year = DateNumberList[0];
                        String Month = DateNumberList[1];
                        String Day = DateNumberList[2];
                        int year; int month; int day;
                        if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
                        {
                            ST = DateUtility.GetWorkDay(year, month, day);
                        }
                        Year = DateNumberList[3];
                        Month = DateNumberList[4];
                        Day = DateNumberList[5];
                        if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
                        {
                            ED = DateUtility.GetWorkDay(year, month, day);
                        }
                        list.Add(new LocAndValue<(DateTime StartDate, DateTime EndDate)>()
                        {
                            Loc = sentence.PositionId,
                            Type = "日期范围",
                            Value = (ST, ED)
                        });
                    }
                    if (DateNumberList.Count == 5)
                    {
                        String Year = DateNumberList[0];
                        String Month = DateNumberList[1];
                        String Day = DateNumberList[2];
                        int year; int month; int day;
                        if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
                        {
                            ST = DateUtility.GetWorkDay(year, month, day);
                        }
                        Month = DateNumberList[3];
                        Day = DateNumberList[4];
                        if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
                        {
                            ED = DateUtility.GetWorkDay(year, month, day);
                        }
                        list.Add(new LocAndValue<(DateTime StartDate, DateTime EndDate)>()
                        {
                            Loc = sentence.PositionId,
                            Type = "日期范围",
                            Value = (ST, ED)
                        });
                    }
                    if (DateNumberList.Count == 4)
                    {
                        String Year = DateNumberList[0];
                        String Month = DateNumberList[1];
                        String Day = DateNumberList[2];
                        int year; int month; int day;
                        if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
                        {
                            ST = DateUtility.GetWorkDay(year, month, day);
                        }
                        Day = DateNumberList[3];
                        if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
                        {
                            ED = DateUtility.GetWorkDay(year, month, day);
                        }
                        list.Add(new LocAndValue<(DateTime StartDate, DateTime EndDate)>()
                        {
                            Loc = sentence.PositionId,
                            Type = "日期范围",
                            Value = (ST, ED)
                        });
                    }
                }
            }
        }
        return list;
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
                OrgString = DateUtility.ConvertUpperToLower(OrgString).Replace(" ", String.Empty);
                var datelist = DateUtility.GetDate(OrgString);
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
                            Type = "日期",
                            Value = DateUtility.GetWorkDay(year, month, day)
                        });
                    }
                }
            }
        }
        return list;
    }

    //获得金额
    public static List<LocAndValue<(String MoneyAmount, String MoneyCurrency)>> LocateMoney(HTMLEngine.MyRootHtmlNode root)
    {
        var list = new List<LocAndValue<(String MoneyAmount, String MoneyCurrency)>>();
        foreach (var paragrah in root.Children)
        {
            foreach (var sentence in paragrah.Children)
            {
                var OrgString = sentence.Content;
                OrgString = MoneyUtility.ConvertUpperToLower(OrgString).Replace(" ", String.Empty);
                var Money = MoneyUtility.SeekMoney(OrgString);
                foreach (var money in Money)
                {
                    list.Add(new LocAndValue<(String MoneyAmount, String MoneyCurrency)>
                    {
                        Loc = sentence.PositionId,
                        Type = "金额",
                        Value = money
                    });
                }
            }
        }
        return list;
    }
}