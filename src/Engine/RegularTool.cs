using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class RegularTool
{

    public static List<string> GetDate(string str)
    {
        //中文数字转阿拉伯数字
        str = Utility.ConvertUpperDateToLittle(str);
        Regex r = new Regex(@"\d+年\d+月\d+日");
        var strList = new List<string>();
        foreach (var item in r.Matches(str).ToList())
        {
            if (!string.IsNullOrEmpty(item.Value)) strList.Add(item.Value);
        }
        return strList;
    }

    public static List<string> GetNumberList(string str)
    {
        var strList = new List<string>();
        Regex r = new Regex(@"\d+");
        foreach (var item in r.Matches(str).ToList())
        {
            if (!string.IsNullOrEmpty(item.Value)) strList.Add(item.Value);
        }
        return strList;
    }

    public static bool IsNumeric(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
    }
    public static bool IsInt(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        return Regex.IsMatch(value, @"^[+-]?\d*$");
    }
    public static bool IsUnsign(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        return Regex.IsMatch(value, @"^\d*[.]?\d*$");
    }

    public static List<string> GetMultiValueBetweenMark(string str, string s, string e)
    {
        var strList = new List<string>();
        Regex r = new Regex(@"(?<=\" + s + @")(\S+?)(?=\" + e + ")");
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