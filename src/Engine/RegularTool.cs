using System;
using System.Text.RegularExpressions;

public static class RegularTool
{
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
    
    public static string GetValueBetweenString(string str, string s, string e)
    {
        Regex rg = new Regex("(?<=(" + s + "))[.\\s\\S]*?(?=(" + e + "))", RegexOptions.Multiline | RegexOptions.Singleline);
        return rg.Match(str).Value;
    }
}