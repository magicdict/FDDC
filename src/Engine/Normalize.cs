using System;
using System.Text.RegularExpressions;

public static class Normalizer
{
    public static string Normalize(string orgString)
    {
        //去除空白,换行    
        var stringArray = orgString.Trim().Split("\n");
        string rtn = "";
        foreach (var item in stringArray)
        {
            rtn += item.Trim();
        }
        //表示项目编号的数字归一化  => []
        rtn = NormalizeItemListNumber(rtn);
        return rtn;
    }

    public static string NormalizeTextResult(this string orgString)
    {
        //HTML符号的过滤
        if (orgString.Contains("&amp;"))
        {
            orgString = orgString.Replace("&amp;", "&");
        }
        if (orgString.Contains("&nbsp;"))
        {
            orgString = orgString.Replace("&nbsp;", " ");
        }
        if (orgString.Contains("&lt;"))
        {
            orgString = orgString.Replace("&lt;", "<");
        }
        if (orgString.Contains("&gt;"))
        {
            orgString = orgString.Replace("&gt;", ">");
        }
        orgString = orgString.TrimEnd("。".ToCharArray());
        orgString = orgString.TrimEnd("；".ToCharArray());
        return orgString;
    }

    //主键的处理
    public static string NormalizeKey(this string orgString)
    {
        if (!String.IsNullOrEmpty(orgString))
        {
            orgString = orgString.Trim().Replace(" ", "").ToLower();
        }
        return orgString;
    }

    //数字的处理
    public static string NormalizeNumberResult(this string orgString)
    {
        if (!String.IsNullOrEmpty(orgString))
        {
            orgString = orgString.Trim().Replace(",", "");
            orgString = orgString.Trim().Replace(" ", "");
        }
        return orgString;
    }

    public static string NormalizeItemListNumber(string orgString)
    {
        //（1）  => [1]
        RegexOptions ops = RegexOptions.Multiline;
        Regex r = new Regex(@"\（(\d+)\）", ops);
        if (r.IsMatch(orgString))
        {
            orgString = r.Replace(orgString, "<$1>");
            return orgString;
        }

        //4 、   => [4]
        r = new Regex(@"(\d+)\ \、", ops);
        if (r.IsMatch(orgString))
        {
            orgString = r.Replace(orgString, "<$1>");
            return orgString;
        }

        //（1）、 => [4]
        new Regex(@"\（(\d+)\）、", ops);
        if (r.IsMatch(orgString))
        {
            orgString = r.Replace(orgString, "<$1>");
            return orgString;
        }

        //4、    => [4]
        r = new Regex(@"(\d+)\、", ops);
        if (r.IsMatch(orgString))
        {
            orgString = r.Replace(orgString, "<$1>");
            return orgString;
        }

        return orgString;
    }

}