using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MoneyUtility
{
    public static string[] CurrencyList = { "人民币", "澳门元", "肯先令", "港币", "美元", "欧元", "元" };

    /// <summary>
    /// 寻找金额
    /// </summary>
    /// <param name="MoneyAmount"></param>
    /// <param name="MoneyCurrency"></param>
    public static List<(String MoneyAmount, String MoneyCurrency)> SeekMoney(string OrgString)
    {
        OrgString = OrgString.Replace(" ", String.Empty);
        OrgString = NumberUtility.ConvertUpperToLower(OrgString);    

        var MoneyList = new List<(String MoneyAmount, String MoneyCurrency)>();
        var LastIndex = 0;
        var detectString = OrgString;
        while (true)
        {
            bool IsCurrencyMark = false;
            detectString = detectString.Substring(LastIndex);
            var MoneyCurrency = String.Empty;
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
            if (MoneyCurrency == String.Empty)
            {
                if (detectString.Contains("￥"))
                {
                    IsCurrencyMark = true;
                    MoneyCurrency = "人民币";
                    int currencyMarkIdx = detectString.IndexOf("￥");
                    for (int k = currencyMarkIdx + 1; k < detectString.Length; k++)
                    {
                        var s = detectString.Substring(k, 1);
                        if (RegularTool.IsNumeric(s) || s == ",")
                        {
                            if (k == detectString.Length - 1)
                            {
                                LastIndex = k;
                                break;
                            }
                            continue;
                        }
                        LastIndex = k;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            else
            {
                LastIndex = detectString.IndexOf(MoneyCurrency);
            }
            Regex rex = new Regex(@"^\d+");
            var MoneyAmount = String.Empty;
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
                    MoneyAmount = String.Empty;
                    if (LastIndex == i + 1) break;
                    MoneyAmount = detectString.Substring(i + 1, LastIndex - i - 1);
                    MoneyAmount = Normalizer.NormalizeNumberResult(MoneyAmount);
                    if (!rex.IsMatch(MoneyAmount))
                    {
                        MoneyAmount = String.Empty;
                        break;  //暂时认为一定要有阿拉伯数字
                    }
                    MoneyList.Add((MoneyAmount, MoneyCurrency));
                    MoneyAmount = String.Empty;
                    break;
                }
            }
            if (MoneyAmount != String.Empty) MoneyList.Add((MoneyAmount, MoneyCurrency));
            if (!IsCurrencyMark)
            {
                LastIndex += MoneyCurrency.Length;
            }
            else
            {
                //"￥"在字符串末尾的时候，如果不及时退出，会发生死循环！
                if (MoneyAmount == String.Empty) break;
            }
        }
        return MoneyList;
    }

    //金额的处理
    public static string Format(string orgAmountString, string TitleWord)
    {
        if (!String.IsNullOrEmpty(orgAmountString))
        {
            orgAmountString = orgAmountString.Trim().Replace(",", String.Empty);
            orgAmountString = orgAmountString.Trim().Replace("，", String.Empty);
        }
        orgAmountString = orgAmountString.Replace("不超过", String.Empty);
        orgAmountString = orgAmountString.Replace("不低于", String.Empty);
        orgAmountString = orgAmountString.Replace("不多于", String.Empty);
        orgAmountString = orgAmountString.Replace("不少于", String.Empty);

        foreach (var Currency in CurrencyList)
        {
            if (orgAmountString.EndsWith(Currency))
            {
                orgAmountString = orgAmountString.Replace(Currency, String.Empty);
                orgAmountString = orgAmountString.Trim();
                break;
            }
        }

        //对于【亿，万】的处理
        if (TitleWord.Contains("万元"))
        {
            double x;
            if (double.TryParse(orgAmountString, out x))
            {
                orgAmountString = (x * 10_000).ToString();
            }
        }
        else
        {
            if (orgAmountString.EndsWith("万"))
            {
                orgAmountString = orgAmountString.Replace("万", String.Empty);
                double x;
                if (double.TryParse(orgAmountString, out x))
                {
                    orgAmountString = (x * 10_000).ToString();
                }
            }
        }

        //惩 本次HTML特殊处理
        if (orgAmountString.EndsWith("亿") || orgAmountString.EndsWith("惩"))
        {
            orgAmountString = orgAmountString.Replace("亿", String.Empty);
            orgAmountString = orgAmountString.Replace("惩", String.Empty);
            double x;
            if (double.TryParse(orgAmountString, out x))
            {
                orgAmountString = (x * 100_000_000).ToString();
            }
        }
        else
        {
            //XXX亿美元
            foreach (var Currency in CurrencyList)
            {
                //惩 本次HTML特殊处理
                if (orgAmountString.EndsWith("亿") || orgAmountString.EndsWith("惩"))
                {
                    orgAmountString = orgAmountString.Replace("亿", String.Empty);
                    orgAmountString = orgAmountString.Replace("惩", String.Empty);
                    double x;
                    if (double.TryParse(orgAmountString, out x))
                    {
                        orgAmountString = (x * 10_000).ToString();
                    }
                }
            }
        }

        if (orgAmountString.EndsWith(".00")) orgAmountString = orgAmountString.Substring(0, orgAmountString.Length - 3);
        orgAmountString = orgAmountString.Trim();
        return orgAmountString;
    }

}