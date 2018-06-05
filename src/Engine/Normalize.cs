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

    public static string NormalizeTextResult(string orgString)
    {
        if (!String.IsNullOrEmpty(orgString))
        {
            orgString = orgString.Trim();
            if (orgString.EndsWith("。")) orgString = orgString.TrimEnd("。".ToCharArray());
        }
        return orgString;
    }

    public static string NormalizeNumberResult(string orgString)
    {
        if (!String.IsNullOrEmpty(orgString))
        {
            orgString = orgString.Trim().Replace(",", "");
        }
        return orgString;
    }

    public static string NormailizeDate(string orgString, string keyword = "")
    {
        orgString = orgString.Trim().Replace(",", "");
        var NumberList = RegularTool.GetNumberList(orgString);
        if (NumberList.Count == 6)
        {
            String Year = NumberList[3];
            String Month = NumberList[4];
            String Day = NumberList[5];
            int year; int month; int day;
            if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
            {
                var d = new DateTime(year, month, day);
                return d.ToString("yyyy-MM-dd");
            }
        }
        if (NumberList.Count == 5)
        {
            if (orgString.IndexOf("年") != -1 && orgString.IndexOf("月") != -1 && orgString.IndexOf("日") != -1)
            {
                String Year = NumberList[0];
                String Month = NumberList[3];
                String Day = NumberList[4];
                int year; int month; int day;
                if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
                {
                    if (month <= 12 && day <= 31)
                    {
                        var d = new DateTime(year, month, day);
                        return d.ToString("yyyy-MM-dd");
                    }
                }
            }
        }

        if (orgString.Contains("年") && orgString.Contains("月") && orgString.Contains("月"))
        {
            String Year = Utility.GetStringBefore(orgString, "年");
            String Month = RegularTool.GetValueBetweenString(orgString, "年", "月");
            String Day = Utility.GetStringAfter(orgString, "月").Replace("日", "");
            int year; int month; int day;
            if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
            {
                var d = new DateTime(year, month, day);
                return d.ToString("yyyy-MM-dd");
            }
        }

        var SplitChar = new string[] { "/", ".", "-" };
        foreach (var sc in SplitChar)
        {
            var SplitArray = orgString.Split(sc);
            if (SplitArray.Length == 3)
            {
                String Year = SplitArray[0];
                String Month = SplitArray[1];
                String Day = SplitArray[2];
                int year; int month; int day;
                if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
                {
                    var d = new DateTime(year, month, day);
                    return d.ToString("yyyy-MM-dd");
                }
            }
        }

        return orgString;
    }

    public static string NormalizerStockNumber(string orgString, string TitleWord)
    {
        if (!String.IsNullOrEmpty(orgString))
        {
            orgString = orgString.Trim().Replace(",", "");
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
        if (orgString.EndsWith("亿"))
        {
            orgString = orgString.Replace("亿", "");
            double x;
            if (double.TryParse(orgString, out x))
            {
                orgString = (x * 100_000_000).ToString();
            }
        }
        return orgString;
    }

    public static string NormalizerMoney(string orgString, string TitleWord)
    {
        if (!String.IsNullOrEmpty(orgString))
        {
            orgString = orgString.Trim().Replace(",", "");
        }
        orgString = orgString.Replace("不超过", "");
        orgString = orgString.Replace("不低于", "");
        orgString = orgString.Replace("不多于", "");
        orgString = orgString.Replace("不少于", "");


        if (orgString.EndsWith("元"))
        {
            orgString = orgString.Replace("元", "");
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
        if (orgString.EndsWith("亿"))
        {
            orgString = orgString.Replace("亿", "");
            double x;
            if (double.TryParse(orgString, out x))
            {
                orgString = (x * 100_000_000).ToString();
            }
        }
        if (orgString.EndsWith(".00")) orgString = orgString.Substring(0,orgString.Length -3);
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