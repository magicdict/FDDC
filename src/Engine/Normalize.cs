using System;
using System.Globalization;
using System.Text.RegularExpressions;

public static class Normalizer
{
    public static string Normalize(string orgString)
    {
        //去除空白,换行    
        var stringArray = orgString.Trim().Split("\n");
        string rtn = String.Empty;
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
        orgString = orgString.Trim().Replace(" ", String.Empty);
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
        orgString = ClearTrailing(orgString);
        orgString = orgString.ToLower();
        //new CultureInfo("zh-cn") 也无法将中文括号变成普通括号
        orgString = orgString.Replace("（", "(");
        orgString = orgString.Replace("）", ")");
        orgString = orgString.Replace("～", "~");
        orgString = orgString.Trim(new Char[] { '—', '-', ']' });
        orgString = orgString.Replace("/", "");
        return orgString;
    }

    public static string ClearTrailing(string orgString)
    {
        orgString = orgString.Trim();
        orgString = orgString.TrimEnd("，".ToCharArray());
        orgString = orgString.TrimEnd("。".ToCharArray());
        orgString = orgString.TrimEnd("；".ToCharArray());
        if (orgString.Contains("。")) orgString = Utility.GetStringBefore(orgString, "。");
        return orgString;
    }

    //主键的处理
    public static string NormalizeKey(this string orgString)
    {
        if (!String.IsNullOrEmpty(orgString))
        {
            //输出数据已经去掉空格，不过，Traing没有去除空格
            orgString = orgString.Trim().Replace(" ", String.Empty).ToLower();
        }
        return orgString;
    }

    //数字的处理
    public static string NormalizeNumberResult(this string orgString)
    {
        if (!String.IsNullOrEmpty(orgString))
        {
            orgString = orgString.Trim().Replace(",", String.Empty);
            orgString = orgString.Trim().Replace(" ", String.Empty);
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