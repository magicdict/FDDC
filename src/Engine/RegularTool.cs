using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class RegularTool
{

    public static List<string> GetNumberList(string str){
        var strList = new List<string>();
        Regex r = new Regex(@"\d+");
        foreach (var item in r.Matches(str).ToList())
        {
            if (!string.IsNullOrEmpty(item.Value)) strList.Add(item.Value);
        }
        return strList;
    }

   

    //提取《XXX》文字
    public static string GetValueBetweenMark(string str, string s, string e)
    {
        Regex r = new Regex(@"(?<=\" + s + @")(\S+)(?=\" + e + ")");
        if (r.IsMatch(str))
        {
            str = r.Match(str).Value;
            return str;
        }
        return "";
    }

    public static List<string> GetMultiValueBetweenMark(string str, string s, string e)
    {
        var strList = new List<string>();
        Regex r = new Regex(@"(?<=\" + s + @")(\S+)(?=\" + e + ")");
        foreach (var item in r.Matches(str).ToList())
        {
            if (!string.IsNullOrEmpty(item.Value)) strList.Add(item.Value);
        }
        return strList;
    }

    public static string GetValueBetweenString(string str, string s, string e)
    {
        Regex rg = new Regex("(?<=(" + s + "))[.\\s\\S]*?(?=(" + e + "))", RegexOptions.Multiline | RegexOptions.Singleline);
        return rg.Match(str).Value;
    }

    public static List<String> GetMultiValueBetweenString(string str, string s, string e)
    {
        var strList = new List<string>();
        Regex rg = new Regex("(?<=(" + s + "))[.\\s\\S]*?(?=(" + e + "))", RegexOptions.Multiline | RegexOptions.Singleline);
        foreach (var item in rg.Matches(str).ToList())
        {
            if (!string.IsNullOrEmpty(item.Value)) strList.Add(item.Value);
        }
        return strList;
    }
}