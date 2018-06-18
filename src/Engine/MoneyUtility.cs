using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MoneyUtility
{
    public static string[] CurrencyList = { "人民币", "澳门元", "肯先令", "港币", "美元", "欧元", "元" };

    //将大写数字转小写
    public static string ConvertUpperToLower(string OrgString)
    {
        if (String.IsNullOrEmpty(OrgString)) return "";
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

    public static List<(String MoneyAmount, String MoneyCurrency)> SeekMoney(string OrgString)
    {
        OrgString = OrgString.Replace(" ", "");

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

        var MoneyList = new List<(String MoneyAmount, String MoneyCurrency)>();
        var LastIndex = 0;
        var detectString = OrgString;
        while (true)
        {
            detectString = detectString.Substring(LastIndex);
            var MoneyCurrency = "";
            //可能同时存在多个关键字，这里选择最前面一个关键字
            var MinIdx = -1;
            foreach (var Currency in CurrencyList)
            {
                if (detectString.IndexOf(Currency) != -1)
                {
                    if (MinIdx == -1)
                    {
                        MoneyCurrency = Currency;
                        MinIdx = detectString.IndexOf(Currency);
                    }
                    else
                    {
                        if (MinIdx > detectString.IndexOf(Currency))
                        {
                            MoneyCurrency = Currency;
                            MinIdx = detectString.IndexOf(Currency);
                        }
                    }
                }
            }
            if (MoneyCurrency == "") break;
            LastIndex = detectString.IndexOf(MoneyCurrency);
            Regex rex = new Regex(@"^\d+");
            var MoneyAmount = "";
            for (int i = LastIndex - 1; i >= 0; i--)
            {
                var SingleChar = detectString.Substring(i, 1);
                //惩 本次特殊处理
                if (SingleChar == "." || SingleChar == "," ||
                    SingleChar == "，" || SingleChar == "万" ||
                    SingleChar == "惩" || SingleChar == "亿" || rex.IsMatch(SingleChar))
                {
                    MoneyAmount = SingleChar + MoneyAmount;
                    continue;
                }
                else
                {
                    MoneyAmount = "";
                    if (LastIndex == i + 1) break;
                    MoneyAmount = detectString.Substring(i + 1, LastIndex - i - 1);
                    MoneyAmount = Normalizer.NormalizeNumberResult(MoneyAmount);
                    if (!rex.IsMatch(MoneyAmount))
                    {
                        MoneyAmount = "";
                        break;  //暂时认为一定要有阿拉伯数字
                    }
                    MoneyList.Add((MoneyAmount, MoneyCurrency));
                    MoneyAmount = "";
                    break;
                }
            }
            if (MoneyAmount != "") MoneyList.Add((MoneyAmount, MoneyCurrency));
            LastIndex += MoneyCurrency.Length;
        }
        return MoneyList;
    }

    //金额的处理
    public static string Format(string orgString, string TitleWord)
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
        if (TitleWord.Contains("万元"))
        {
            double x;
            if (double.TryParse(orgString, out x))
            {
                orgString = (x * 10_000).ToString();
            }
        }
        else
        {
            //XXX万美元
            foreach (var Currency in CurrencyList)
            {
                if (orgString.EndsWith("万" + Currency))
                {
                    orgString = orgString.Replace("万" + Currency, "");
                    double x;
                    if (double.TryParse(orgString, out x))
                    {
                        orgString = (x * 10_000).ToString();
                    }
                }
            }
        }

        //惩 本次HTML特殊处理
        if (orgString.EndsWith("亿") || orgString.EndsWith("惩"))
        {
            orgString = orgString.Replace("亿", "");
            orgString = orgString.Replace("惩", "");
            double x;
            if (double.TryParse(orgString, out x))
            {
                orgString = (x * 100_000_000).ToString();
            }
        }
        else
        {
            //XXX亿美元
            foreach (var Currency in CurrencyList)
            {
                //惩 本次HTML特殊处理
                if (orgString.EndsWith("亿" + Currency) || orgString.EndsWith("惩" + Currency))
                {
                    orgString = orgString.Replace("亿" + Currency, "");
                    orgString = orgString.Replace("惩" + Currency, "");
                    double x;
                    if (double.TryParse(orgString, out x))
                    {
                        orgString = (x * 10_000).ToString();
                    }
                }
            }
        }

        if (orgString.EndsWith(".00")) orgString = orgString.Substring(0, orgString.Length - 3);
        orgString = orgString.Trim();
        return orgString;
    }

}