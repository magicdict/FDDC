using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
public static class LocateProperty
{
    /// <summary>
    /// 位置和值
    /// </summary>
    public struct LocAndValue<T>
    {
        /// <summary>
        /// HTML整体位置
        /// </summary>
        public int Loc;
        /// <summary>
        /// 开始位置
        /// </summary>
        public int StartIdx;
        /// <summary>
        /// 值
        /// </summary>
        public T Value;
        /// <summary>
        /// 描述
        /// </summary>
        public string Description;

        /// <summary>
        /// 距离（别的词语在后面，则为正数）
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int Distance(LocAndValue<T> other)
        {
            int mypos = Loc * 1000 + StartIdx;
            int otherpos = other.Loc * 1000 + other.StartIdx;
            if (Value is string)
            {
                //别的词语在后面，则为正数
                if (other.StartIdx > this.StartIdx)
                {
                    //其他
                    return otherpos - mypos - Value.ToString().Length;
                }
                else
                {
                    return otherpos + other.Value.ToString().Length - mypos;
                }
            }
            else
            {
                //别的词语在后面，则为正数
                return otherpos - mypos;
            }
        }
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
                        Description = "书名号",
                        Value = item.Value.Substring(1, item.Value.Length - 2),
                        StartIdx = item.Index
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
                        Description = "引号",
                        Value = item.Value.Substring(1, item.Value.Length - 2),
                        StartIdx = item.Index
                    });
                }

                r = new Regex(@"\（.*?\）");
                foreach (var item in r.Matches(OrgString).ToList())
                {
                    list.Add(new LocAndValue<String>()
                    {
                        Loc = sentence.PositionId,
                        Description = "中文小括号",
                        Value = item.Value.Substring(1, item.Value.Length - 2),
                        StartIdx = item.Index
                    });
                }

            }
        }
        return list;
    }


    /// <summary>
    /// 获得日期范围
    /// </summary>
    /// <param name="StartDate"></param>
    /// <param name="EndDate"></param>
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
                            Description = "日期范围",
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
                            Description = "日期范围",
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
                            Description = "日期范围",
                            Value = (ST, ED)
                        });
                    }
                }
            }
        }
        return list;
    }

    /// <summary>
    /// 获得日期
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
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
                            Description = "日期",
                            Value = DateUtility.GetWorkDay(year, month, day)
                        });
                    }
                }
            }
        }
        return list;
    }

    /// <summary>
    /// 获得金额
    /// </summary>
    /// <param name="MoneyAmount"></param>
    /// <param name="MoneyCurrency"></param>
    public static List<LocAndValue<(String MoneyAmount, String MoneyCurrency)>> LocateMoney(HTMLEngine.MyRootHtmlNode root)
    {
        var list = new List<LocAndValue<(String MoneyAmount, String MoneyCurrency)>>();
        foreach (var paragrah in root.Children)
        {
            foreach (var sentence in paragrah.Children)
            {
                var OrgString = sentence.Content;
                OrgString = NumberUtility.ConvertUpperToLower(OrgString).Replace(" ", String.Empty);
                var Money = MoneyUtility.SeekMoney(OrgString);
                foreach (var money in Money)
                {
                    list.Add(new LocAndValue<(String MoneyAmount, String MoneyCurrency)>
                    {
                        Loc = sentence.PositionId,
                        Description = "金额",
                        Value = money,
                        StartIdx = OrgString.IndexOf(money.MoneyAmount)
                    });
                }
            }
        }
        return list;
    }

    /// <summary>
    /// 自定义字符列表
    /// </summary>
    /// <param name="root"></param>
    /// <param name="CustomerWord"></param>
    /// <returns></returns>
    public static List<LocAndValue<String>> LocateCustomerWord(HTMLEngine.MyRootHtmlNode root,
                                        List<String> CustomerWord, string description = "字符")
    {
        var list = new List<LocAndValue<String>>();
        foreach (var paragrah in root.Children)
        {
            foreach (var sentence in paragrah.Children)
            {
                var OrgString = sentence.Content;
                foreach (var word in CustomerWord)
                {
                    int ScanStartIdx = 0;
                    while (OrgString.IndexOf(word, ScanStartIdx) != -1)
                    {
                        list.Add(new LocAndValue<String>()
                        {
                            Loc = sentence.PositionId,
                            Description = description,
                            Value = word,
                            StartIdx = OrgString.IndexOf(word, ScanStartIdx)
                        });
                        ScanStartIdx = OrgString.IndexOf(word, ScanStartIdx) + word.Length;
                    }
                }
            }
        }
        return list;
    }
}