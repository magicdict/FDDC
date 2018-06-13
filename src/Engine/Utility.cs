using HtmlAgilityPack;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static EntityProperty;

public static class Utility
{
    //获得开始字符结束字符的排列组合
    public static struStartEndStringFeature[] GetStartEndStringArray(string[] StartStringList, string[] EndStringList)
    {
        var KeyWordListArray = new struStartEndStringFeature[StartStringList.Length * EndStringList.Length];
        int cnt = 0;
        foreach (var StartString in StartStringList)
        {
            foreach (var EndString in EndStringList)
            {
                KeyWordListArray[cnt] = new struStartEndStringFeature { StartWith = StartString, EndWith = EndString };
                cnt++;
            }
        }
        return KeyWordListArray;
    }

    //提取某个关键字后的信息
    public static string GetStringAfter(String SearchLine, String KeyWord, String Exclude = "")
    {

        if (Exclude != "")
        {
            if (SearchLine.IndexOf(Exclude) != -1) return "";
        }

        int index = SearchLine.IndexOf(KeyWord);
        if (index != -1)
        {
            index = index + KeyWord.Length;
            return SearchLine.Substring(index);
        }
        return "";
    }

    //提取某个关键字前的信息
    public static string GetStringBefore(String SearchLine, String KeyWord, String Exclude = "")
    {
        if (Exclude != "")
        {
            if (SearchLine.IndexOf(Exclude) != -1) return "";
        }
        int index = SearchLine.IndexOf(KeyWord);
        if (index != -1)
        {
            return SearchLine.Substring(0, index);
        }
        return "";
    }

    //将大写数字转小写（非金额数字）
    public static string ConvertUpperDateToLittle(string OrgString)
    {
        if (String.IsNullOrEmpty(OrgString)) return "";
        //二○一二年十一月三十日

        OrgString = OrgString.Replace("二十一", "21");
        OrgString = OrgString.Replace("二十二", "22");
        OrgString = OrgString.Replace("二十三", "23");
        OrgString = OrgString.Replace("二十四", "24");
        OrgString = OrgString.Replace("二十五", "25");
        OrgString = OrgString.Replace("二十六", "26");
        OrgString = OrgString.Replace("二十七", "27");
        OrgString = OrgString.Replace("二十八", "28");
        OrgString = OrgString.Replace("二十九", "29");
        OrgString = OrgString.Replace("三十一", "31");

        OrgString = OrgString.Replace("三十", "30");
        OrgString = OrgString.Replace("十一", "11");
        OrgString = OrgString.Replace("十二", "12");
        OrgString = OrgString.Replace("十三", "13");
        OrgString = OrgString.Replace("十四", "14");
        OrgString = OrgString.Replace("十五", "15");
        OrgString = OrgString.Replace("十六", "16");
        OrgString = OrgString.Replace("十七", "17");
        OrgString = OrgString.Replace("十八", "18");
        OrgString = OrgString.Replace("十九", "19");
        OrgString = OrgString.Replace("二十", "20");

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

        return OrgString;
    }

   
    public static List<Tuple<String, String>> SeekMoney(string OrgString)
    {
        OrgString = OrgString.Replace(" ", "");
        var MoneyList = new List<Tuple<String, String>>();
        var LastIndex = 0;
        var detectString = OrgString;
        while (true)
        {
            detectString = detectString.Substring(LastIndex);
            var MoneyCurrency = "";
            //可能同时存在多个关键字，这里选择最前面一个关键字
            var MinIdx = -1;
            foreach (var Currency in Normalizer.CurrencyList)
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
                if (SingleChar == "." || SingleChar == "," || SingleChar == "，" || SingleChar == "万" || SingleChar == "惩" || SingleChar == "亿" || rex.IsMatch(SingleChar))
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
                    MoneyList.Add(Tuple.Create(MoneyAmount, MoneyCurrency));
                    MoneyAmount = "";
                    break;
                }
            }
            if (MoneyAmount != "") MoneyList.Add(Tuple.Create(MoneyAmount, MoneyCurrency));
            LastIndex += MoneyCurrency.Length;
        }


        return MoneyList;
    }
}