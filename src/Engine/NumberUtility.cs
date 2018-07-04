using System;

public class NumberUtility
{

    //股数的处理
    public static string NormalizerStockNumber(string orgString, string TitleWord)
    {
        if (!String.IsNullOrEmpty(orgString))
        {
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