using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class RegularTool
{
    public static List<string> GetChineseBrackets(string str)
    {
        Regex r = new Regex(@"\（.*?\）");
        return r.Matches(str).Select(x => { return x.Value; }).ToList();
    }
    public static List<string> GetBrackets(string str)
    {
        Regex r = new Regex(@"\（.*?\）");
        return r.Matches(str).Select(x => { return x.Value; }).ToList();
    }
    public static List<string> GetChineseQuotation(string str)
    {
        Regex r = new Regex(@"\“.*?\”");
        return r.Matches(str).Select(x => { return x.Value; }).ToList();
    }

    public static string TrimChineseBrackets(string str)
    {
        Regex r = new Regex(@"\（.*?\）");
        str = r.Replace(str, String.Empty);
        return str;
    }

    public static string TrimBrackets(string str)
    {
        Regex r = new Regex(@"\(.*?\)");
        str = r.Replace(str, String.Empty);
        return str;
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

    public static bool IsPercent(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        return Regex.IsMatch(value, @"^\d*[.]?\d*%$");
    }

    /// <summary>
    /// 百分比（非开始结尾类型）
    /// </summary>
    public const string PercentExpress = @"\d*[.]?\d*%";

    /// <summary>
    /// 数字（带小树）
    /// </summary>
    public const string MoneyExpress = @"\d*[.]?\d*";

    /// <summary>
    /// 正则表达式结果
    /// </summary>
    public struct RegularExResult
    {
        public int Index;

        public int Length;

        public string RawData;
    }

    /// <summary>
    /// 获得正则表达式结果
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static List<RegularExResult> GetRegular(string value, string exp)
    {
        var result = new List<RegularExResult>();
        var r = new Regex(exp);
        Match m = r.Match(value);
        while (m.Success)
        {
            if (!String.IsNullOrEmpty(m.Value))
            {
                result.Add(new RegularExResult()
                {
                    Index = m.Index,
                    Length = m.Length,
                    RawData = m.Value
                });
            }
            m = m.NextMatch();
        }
        return result;
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