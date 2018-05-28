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