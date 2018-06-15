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

    public static string NormalizeKey(this string orgString)
    {
        if (!String.IsNullOrEmpty(orgString))
        {
            orgString = orgString.Trim().Replace(" ", "").ToLower();
        }
        return orgString;
    }

    public static string NormalizeNumberResult(this string orgString)
    {
        if (!String.IsNullOrEmpty(orgString))
        {
            orgString = orgString.Trim().Replace(",", "");
            orgString = orgString.Trim().Replace(" ", "");
        }
        return orgString;
    }

    public static string NormalizerStockNumber(string orgString, string TitleWord)
    {
        if (!String.IsNullOrEmpty(orgString))
        {
            orgString = orgString.Trim().Replace(",", "");
            orgString = orgString.Trim().Replace("，", "");
        }
        orgString = orgString.Replace("不超过", "");
        orgString = orgString.Replace("不低于", "");
        orgString = orgString.Replace("不多于", "");
        orgString = orgString.Replace("不少于", "");

        if (orgString.EndsWith("股"))
        {
            orgString = orgString.Replace("股", "");
        }
        //对于【亿，万】的处理
        if (orgString.EndsWith("万") || TitleWord.Contains("万股"))
        {
            orgString = orgString.Replace("万", "");
            double x;
            if (double.TryParse(orgString, out x))
            {
                orgString = (x * 10_000).ToString();
            }
        }
        if (orgString.EndsWith("亿") || orgString.EndsWith("惩"))  //惩 本次HTML特殊处理
        {
            orgString = orgString.Replace("亿", "");
            orgString = orgString.Replace("惩", "");
            double x;
            if (double.TryParse(orgString, out x))
            {
                orgString = (x * 100_000_000).ToString();
            }
        }
        return orgString;
    }


    public static string[] CurrencyList = { "人民币", "港币", "美元", "欧元", "元" };

    public static string NormalizerMoney(string orgString, string TitleWord)
    {
        if (!String.IsNullOrEmpty(orgString))
        {
            orgString = orgString.Trim().Replace(",", "");
            orgString = orgString.Trim().Replace("，", "");
        }
        orgString = orgString.Replace("不超过", "");
        orgString = orgString.Replace("不低于", "");
        orgString = orgString.Replace("不多于", "");
        orgString = orgString.Replace("不少于", "");

        foreach (var Currency in CurrencyList)
        {
            if (orgString.EndsWith(Currency))
            {
                orgString = orgString.Replace(Currency, "");
                orgString = orgString.Trim();
                break;
            }
        }
        //对于【亿，万】的处理
        if (orgString.EndsWith("万") || TitleWord.Contains("万元"))
        {
            orgString = orgString.Replace("万", "");
            double x;
            if (double.TryParse(orgString, out x))
            {
                orgString = (x * 10_000).ToString();
            }
        }
        if (orgString.EndsWith("亿") || orgString.EndsWith("惩"))  //惩 本次HTML特殊处理
        {
            orgString = orgString.Replace("亿", "");
            orgString = orgString.Replace("惩", "");
            double x;
            if (double.TryParse(orgString, out x))
            {
                orgString = (x * 100_000_000).ToString();
            }
        }
        if (orgString.EndsWith(".00")) orgString = orgString.Substring(0, orgString.Length - 3);
        orgString = orgString.Trim();
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