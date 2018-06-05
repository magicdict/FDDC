using HtmlAgilityPack;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static ExtractProperty;

public static class Utility
{
    //获得开始字符结束字符的排列组合
    public static struStartEndStringFeature[] GetStartEndStringArray(string[] StartStringList, string[] EndStringList)
    {
        var KeyWordListArray = new struStartEndStringFeature[StartStringList.Length * EndStringList.Length];
        int cnt = 0;
        foreach (var StartString in StartStringList)
        {
            foreach (var EndString in EndStringList)
            {
                KeyWordListArray[cnt] = new struStartEndStringFeature { StartWith = StartString, EndWith = EndString };
                cnt++;
            }
        }
        return KeyWordListArray;
    }

    //提取某个关键字后的信息
    public static string GetStringAfter(String SearchLine, String KeyWord, String Exclude = "")
    {

        if (Exclude != "")
        {
            if (SearchLine.IndexOf(Exclude) != -1) return "";
        }

        int index = SearchLine.IndexOf(KeyWord);
        if (index != -1)
        {
            index = index + KeyWord.Length;
            return SearchLine.Substring(index);
        }
        return "";
    }

    //提取某个关键字前的信息
    public static string GetStringBefore(String SearchLine, String KeyWord, String Exclude = "")
    {
        if (Exclude != "")
        {
            if (SearchLine.IndexOf(Exclude) != -1) return "";
        }
        int index = SearchLine.IndexOf(KeyWord);
        if (index != -1)
        {
            return SearchLine.Substring(0, index);
        }
        return "";
    }

    //将大写数字转小写（非金额数字）
    public static string ConvertUpperDateToLittle(string OrgString)
    {
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

    //在某关键字之后寻找表示金额的阿拉伯数字
    public static string SeekMoney(string OrgString, string KeyWord)
    {
        string RemainString = GetStringAfter(OrgString, KeyWord);
        if (RemainString == "") return "";
        //寻找第一个阿拉伯数字，
        var NumberIndex = -1;
        Regex rex = new Regex(@"^\d+$");

        for (int i = 0; i < RemainString.Length; i++)
        {
            var s = RemainString.Substring(i, 1);
            if (NumberIndex != -1)
            {
                //数字模式下
                if (s == "元" || s == "美元" || s == "欧元")
                {
                    return RemainString.Substring(NumberIndex, i - NumberIndex) + s;
                }
                if (s == "," || s == "万" || rex.IsMatch(s))
                {
                    continue;
                }
            }
            else
            {
                if (rex.IsMatch(s)) NumberIndex = i;
            }
        }
        return "";
    }
}