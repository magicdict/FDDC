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
        if (String.IsNullOrEmpty(OrgString)) return "";
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
}