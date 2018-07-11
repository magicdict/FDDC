using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class DateUtility
{
    public static List<string> GetDate(string str)
    {
        //中文数字转阿拉伯数字
        str = DateUtility.ConvertUpperToLower(str);
        Regex r = new Regex(@"\d+年\d+月\d+日");
        var strList = new List<string>();
        foreach (var item in r.Matches(str).ToList())
        {
            if (!string.IsNullOrEmpty(item.Value)) strList.Add(item.Value);
        }
        return strList;
    }

    //表示日期范围的字符串
    public static List<string> GetRangeDate(string str)
    {
        str = DateUtility.ConvertUpperToLower(str);
        var startReg = new string[] { @"\d+年\d+月\d+日" };
        var MidWordList = new string[] { "至" };
        var endReg = new string[] { @"\d+年\d+月\d+日", @"\d+月\d+日", @"\d+日" };
        var strList = new List<string>();
        foreach (var start in startReg)
        {
            foreach (var mid in MidWordList)
            {
                foreach (var end in endReg)
                {
                    Regex r = new Regex(start + mid + end);
                    foreach (var item in r.Matches(str).ToList())
                    {
                        if (!string.IsNullOrEmpty(item.Value)) strList.Add(item.Value);
                    }
                }
            }
        }

        return strList;
    }

    //将大写数字转小写
    public static string ConvertUpperToLower(string OrgString)
    {
        if (String.IsNullOrEmpty(OrgString)) return String.Empty;
        //二○一二年十一月三十日

        OrgString = OrgString.Replace("二十一", "21");
        OrgString = OrgString.Replace("二十二", "22");
        OrgString = OrgString.Replace("二十三", "23");
        OrgString = OrgString.Replace("二十四", "24");
        OrgString = OrgString.Replace("二十五", "25");
        OrgString = OrgString.Replace("二十六", "26");
        OrgString = OrgString.Replace("二十七", "27");
        OrgString = OrgString.Replace("二十八", "28");
        OrgString = OrgString.Replace("二十九", "29");
        OrgString = OrgString.Replace("三十一", "31");

        OrgString = OrgString.Replace("三十", "30");
        OrgString = OrgString.Replace("十一", "11");
        OrgString = OrgString.Replace("十二", "12");
        OrgString = OrgString.Replace("十三", "13");
        OrgString = OrgString.Replace("十四", "14");
        OrgString = OrgString.Replace("十五", "15");
        OrgString = OrgString.Replace("十六", "16");
        OrgString = OrgString.Replace("十七", "17");
        OrgString = OrgString.Replace("十八", "18");
        OrgString = OrgString.Replace("十九", "19");
        OrgString = OrgString.Replace("二十", "20");

        OrgString = OrgString.Replace("〇", "0");
        OrgString = OrgString.Replace("○", "0");    //本次HTML的特殊处理
        OrgString = OrgString.Replace("一", "1");
        OrgString = OrgString.Replace("二", "2");
        OrgString = OrgString.Replace("三", "3");
        OrgString = OrgString.Replace("四", "4");
        OrgString = OrgString.Replace("五", "5");
        OrgString = OrgString.Replace("六", "6");
        OrgString = OrgString.Replace("七", "7");
        OrgString = OrgString.Replace("八", "8");
        OrgString = OrgString.Replace("九", "9");
        OrgString = OrgString.Replace("十", "10");

        return OrgString;
    }

    public static DateTime GetWorkDay(int year, int month, int day)
    {
        DateTime x = new DateTime(1980, 11, 24);
        var IsWordDayMode = false;
        if (day == -1)
        {
            IsWordDayMode = true;
            day = 31;
        }
        for (int testDay = day; testDay > 0; testDay--)
        {
            if (DateTime.TryParse(year + "/" + month + "/" + testDay, out x))
            {
                if (!IsWordDayMode) break;
                if (x.DayOfWeek != DayOfWeek.Saturday && x.DayOfWeek != DayOfWeek.Sunday) break;
            }
        }
        return x;
    }

    public static string GetRangeDateEndDate(string orgString, DateTime BaseDate, string format = "yyyy-MM-dd")
    {
        orgString = orgString.Replace(" ", "");
        orgString = orgString.Trim().Replace(",", String.Empty);
        //XXXX年XX月XX日 - XXXX年XX月XX日
        var NumberList = RegularTool.GetNumberList(orgString);
        if (NumberList.Count == 6)
        {
            String Year = NumberList[3];
            String Month = NumberList[4];
            String Day = NumberList[5];
            int year; int month; int day;
            if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
            {
                var d = DateUtility.GetWorkDay(year, month, day);
                return d.ToString(format);
            }
        }

        //XXXX年XX月XX日 - XX月XX日
        if (NumberList.Count == 5)
        {
            if (orgString.IndexOf("年") != -1 && orgString.IndexOf("月") != -1 && orgString.IndexOf("日") != -1)
            {
                String Year = NumberList[0];
                String Month = NumberList[3];
                String Day = NumberList[4];
                int year; int month; int day;
                if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
                {
                    var d = DateUtility.GetWorkDay(year, month, day);
                    return d.ToString(format);
                }
            }
        }
        //XXXX年XX月XX日 - XX日 
        if (NumberList.Count == 4)
        {
            if (orgString.IndexOf("年") != -1 && orgString.IndexOf("月") != -1 && orgString.IndexOf("日") != -1)
            {
                String Year = NumberList[0];
                String Month = NumberList[1];
                String Day = NumberList[3];
                int year; int month; int day;
                if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
                {
                    var d = DateUtility.GetWorkDay(year, month, day);
                    return d.ToString(format);
                }
            }
        }
        //XX月XX日
        if (NumberList.Count == 2)
        {
            if (orgString.IndexOf("月") != -1 && orgString.IndexOf("日") != -1)
            {
                if (BaseDate.Year == 0) return orgString;
                String Month = NumberList[0];
                String Day = NumberList[1];
                int month; int day;
                if (int.TryParse(Month, out month) && int.TryParse(Day, out day))
                {
                    var d = DateUtility.GetWorkDay(BaseDate.Year, month, day);
                    return d.ToString(format);
                }
            }
            if (orgString.IndexOf("年") != -1 && orgString.IndexOf("月") != -1)
            {
                /*  
                    数据主要应用于“股东增减持”类型公告的抽取，对于“变动截止日期”字段，存在少量公告中只公布了月份，未公布具体的日期。对这种情况的处理标准为： 
                    如果该月份在公告发布月份的前面，变动截止日期为该月份最后1个交易日；
                    如果该月份是公告发布的月份，变动截止日期为公告发布日期（见本次更新表格）；
                */
                String Year = NumberList[0];
                String Month = NumberList[1];
                int year; int month;
                if (int.TryParse(Year, out year) && int.TryParse(Month, out month))
                {
                    //获得公告时间
                    if (year == BaseDate.Year && month == BaseDate.Month)
                    {
                        return BaseDate.ToString(format);
                    }
                    var d = DateUtility.GetWorkDay(year, month, -1);
                    return d.ToString(format);
                }
            }
            if (orgString.IndexOf("月") != -1)
            {
                String Year = NumberList[0];
                if (Year.Length != 4) return orgString;
                String Month = NumberList[1];
                int year; int month;
                if (int.TryParse(Year, out year) && int.TryParse(Month, out month))
                {
                    var d = DateUtility.GetWorkDay(year, month, -1);
                    return d.ToString(format);
                }
            }
        }
        //XXXX年XX月XX日
        if (orgString.Contains("年") && orgString.Contains("月") && orgString.Contains("月"))
        {
            String Year = Utility.GetStringBefore(orgString, "年");
            String Month = RegularTool.GetValueBetweenString(orgString, "年", "月");
            String Day = Utility.GetStringAfter(orgString, "月").Replace("日", String.Empty);
            int year; int month; int day;
            if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
            {
                var d = DateUtility.GetWorkDay(year, month, day);
                return d.ToString(format);
            }
        }

        if (RegularTool.IsInt(orgString))
        {
            if (orgString.Length == 8)
            {
                String Year = orgString.Substring(0, 4);
                String Month = orgString.Substring(4, 2);
                String Day = orgString.Substring(6, 2);
                int year; int month; int day;
                if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
                {
                    if (year < 1900 || year > 2100)
                    {
                        var d = DateUtility.GetWorkDay(year, month, day);
                        return d.ToString(format);
                    }
                }
            }
        }

        var SplitChar = new string[] { "/", ".", "-" };
        foreach (var sc in SplitChar)
        {
            var SplitArray = orgString.Split(sc);
            if (SplitArray.Length == 3)
            {
                String Year = SplitArray[0];
                String Month = SplitArray[1];
                String Day = SplitArray[2];
                int year; int month; int day;
                if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
                {
                    var d = DateUtility.GetWorkDay(year, month, day);
                    return d.ToString(format);
                }
            }
        }
        return orgString;
    }
}