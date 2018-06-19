using HtmlAgilityPack;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static ExtractProperty;
using System.Linq;
using FDDC;

public static class Utility
{

    //寻找Top5
    public static void FindTop<T>(int n, Dictionary<T, int> dict)
    {
        var Rank = dict.Values.ToList();
        Rank.Sort();
        Rank.Reverse();
        float Total = Rank.Sum();
        var pos = Math.Min(Rank.Count, n);
        int limit = Rank[n - 1];
        foreach (var key in dict.Keys)
        {
            if (dict[key] >= limit)
            {
                var percent = (dict[key] * 100 / Total) + "%";
                Program.Training.WriteLine(key + "(" + percent + ")");
            }
        }
    }

    public static T FindTopOne<T>(Dictionary<T, int> dict)
    {
        var Rank = dict.Values.ToList();
        Rank.Sort();
        Rank.Reverse();
        float Total = Rank.Sum();
        int limit = Rank[0];
        foreach (var key in dict.Keys)
        {
            if (dict[key] == limit)
            {
                return key;
            }
        }
        return dict.Keys.First();
    }

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
}