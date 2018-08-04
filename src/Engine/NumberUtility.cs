using System;
using System.Text.RegularExpressions;

public class NumberUtility
{

    public static string ConvertChineseToNumber(string w)
    {
        if (w == "")
            return w;

        string e = "零一二三四五六七八九";
        string[] ew = new string[] { "十", "百", "千" };
        string ewJoin = "十百千";
        string[] ej = new string[] { "万", "亿" };

        string rss = "^([" + e + ewJoin + "]+" + ej[1] + ")?([" + e
            + ewJoin + "]+" + ej[0] + ")?([" + e + ewJoin + "]+)?$";
        string[] mcollect = Regex.Split(w, rss);
        if (mcollect.Length < 4)
            return w;
        return (
            Convert.ToInt64(foh(mcollect[1])) * 100000000 +
            Convert.ToInt64(foh(mcollect[2])) * 10000 +
            Convert.ToInt64(foh(mcollect[3]))
            ).ToString();
    }

    private static int foh(string str)
    {
        string e = "零一二三四五六七八九";
        string[] ew = new string[] { "十", "百", "千" };
        string[] ej = new string[] { "万", "亿" };

        int a = 0;
        if (str.IndexOf(ew[0]) == 0)
            a = 10;
        str = Regex.Replace(str, e[0].ToString(), "");

        if (Regex.IsMatch(str, "([" + e + "])$"))
        {
            a += e.IndexOf(Regex.Match(str, "([" + e + "])$").Value[0]);
        }

        if (Regex.IsMatch(str, "([" + e + "])" + ew[0]))
        {
            a += e.IndexOf(Regex.Match(str, "([" + e + "])" + ew[0]).Value[0]) * 10;
        }

        if (Regex.IsMatch(str, "([" + e + "])" + ew[1]))
        {
            a += e.IndexOf(Regex.Match(str, "([" + e + "])" + ew[1]).Value[0]) * 100;
        }

        if (Regex.IsMatch(str, "([" + e + "])" + ew[2]))
        {
            a += e.IndexOf(Regex.Match(str, "([" + e + "])" + ew[2]).Value[0]) * 1000;
        }
        return a;
    }



    /// <summary>
    /// 将大写数字转小写
    /// </summary>
    /// <param name="OrgString"></param>
    /// <returns></returns>
    public static string ConvertUpperToLower(string OrgString)
    {
        if (String.IsNullOrEmpty(OrgString)) return String.Empty;
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

        OrgString = OrgString.Replace("０", "0");    //本次HTML的特殊处理
        OrgString = OrgString.Replace("１", "1");
        OrgString = OrgString.Replace("２", "2");
        OrgString = OrgString.Replace("３", "3");
        OrgString = OrgString.Replace("４", "4");
        OrgString = OrgString.Replace("５", "5");
        OrgString = OrgString.Replace("６", "6");
        OrgString = OrgString.Replace("７", "7");
        OrgString = OrgString.Replace("８", "8");
        OrgString = OrgString.Replace("９", "9");

        return OrgString;
    }

    /// <summary>
    /// 股数的处理
    /// </summary>
    /// <param name="orgString"></param>
    /// <param name="TitleWord"></param>
    /// <returns></returns>
    public static string NormalizerStockNumber(string orgString, string TitleWord)
    {
        if (!String.IsNullOrEmpty(orgString))
        {
            if (orgString.Contains("*"))
            {
                Console.WriteLine("* Before:" + orgString);
                orgString = orgString.Trim("*".ToCharArray());
                Console.WriteLine("* After:" + orgString);
            }
            orgString = orgString.Trim().Replace(",", String.Empty);
            orgString = orgString.Trim().Replace("，", String.Empty);
        }
        orgString = orgString.Replace("不超过", String.Empty);
        orgString = orgString.Replace("不低于", String.Empty);
        orgString = orgString.Replace("不多于", String.Empty);
        orgString = orgString.Replace("不少于", String.Empty);

        if (orgString.EndsWith("股"))
        {
            orgString = orgString.Replace("股", String.Empty);
        }
        //对于【亿，万】的处理
        if (orgString.EndsWith("万") || TitleWord.Contains("万股"))
        {
            orgString = orgString.Replace("万", String.Empty);
            double x;
            if (double.TryParse(orgString, out x))
            {
                orgString = (x * 10_000).ToString();
            }
        }
        if (orgString.EndsWith("亿") || orgString.EndsWith("惩"))  //惩 本次HTML特殊处理
        {
            orgString = orgString.Replace("亿", String.Empty);
            orgString = orgString.Replace("惩", String.Empty);
            double x;
            if (double.TryParse(orgString, out x))
            {
                orgString = (x * 100_000_000).ToString();
            }
        }
        return orgString;
    }
}