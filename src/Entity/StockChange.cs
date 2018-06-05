using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;
using static BussinessLogic;
using static HTMLTable;

public class StockChange
{
    public struct struStockChange
    {
        //公告id
        public string id;

        //股东全称
        public string HolderFullName;

        //股东简称
        public string HolderName;

        //变动截止日期
        public string ChangeEndDate;

        //变动价格
        public string ChangePrice;

        //变动数量
        public string ChangeNumber;

        //变动后持股数
        public string HoldNumberAfterChange;

        //变动后持股比例
        public string HoldPercentAfterChange;

        public string GetKey()
        {
            return id + ":" + HolderFullName + ":" + ChangeEndDate;
        }

    }

    internal static struStockChange ConvertFromString(string str)
    {
        var Array = str.Split("\t");
        var c = new struStockChange();
        c.id = Array[0];
        c.HolderFullName = Array[1];
        c.HolderName = Array[2];
        if (Array.Length > 3)
        {
            c.ChangeEndDate = Array[3];
        }
        if (Array.Length > 4)
        {
            c.ChangePrice = Array[4];
        }
        if (Array.Length > 5)
        {
            c.ChangeNumber = Array[5];
        }
        if (Array.Length > 6)
        {
            c.HoldNumberAfterChange = Array[6];
        }
        if (Array.Length == 8)
        {
            c.HoldPercentAfterChange = Array[7];
        }
        return c;
    }



    internal static string ConvertToString(struStockChange increaseStock)
    {
        var record = increaseStock.id + "," +
        increaseStock.HolderFullName + "," +
        increaseStock.HolderName + "," +
        increaseStock.ChangeEndDate + ",";
        record += Normalizer.NormalizeNumberResult(increaseStock.ChangePrice) + ",";
        record += Normalizer.NormalizeNumberResult(increaseStock.ChangeNumber) + ",";
        record += Normalizer.NormalizeNumberResult(increaseStock.HoldNumberAfterChange) + ",";
        record += Normalizer.NormalizeNumberResult(increaseStock.HoldPercentAfterChange) + ",";
        return record;
    }


    static List<struCompanyName> companynamelist;

    public static List<struStockChange> Extract(string htmlFileName)
    {
        var list = new List<struStockChange>();
        var fi = new System.IO.FileInfo(htmlFileName);
        Program.Logger.WriteLine("Start FileName:[" + fi.Name + "]");
        var node = HTMLEngine.Anlayze(htmlFileName);
        companynamelist = BussinessLogic.GetCompanyNameByCutWord(node);

        list = ExtractFromTable(node, fi.Name.Replace(".html", ""));
        if (list.Count > 0) return list;

        var stockchange = new struStockChange();
        //公告ID
        stockchange.id = fi.Name.Replace(".html", "");
        Program.Logger.WriteLine("公告ID:" + stockchange.id);
        var Name = NormalizeCompanyName(GetHolderFullName(node));
        stockchange.HolderFullName = Name.Item1;
        stockchange.HolderName = Name.Item2;
        stockchange.ChangeEndDate = GetChangeEndDate(node);
        list.Add(stockchange);
        return list;
    }

    static List<struStockChange> ExtractFromTable(HTMLEngine.MyRootHtmlNode root, string id)
    {
        var StockHolderRule = new TableSearchRule();
        StockHolderRule.Name = "股东全称";
        StockHolderRule.Rule = new string[] { "股东名称" }.ToList();
        StockHolderRule.IsEq = true;

        var ChangeDateRule = new TableSearchRule();
        ChangeDateRule.Name = "变动截止日期";
        ChangeDateRule.Rule = new string[] { "减持期间", "增持期间" }.ToList();
        ChangeDateRule.IsEq = false;
        ChangeDateRule.Normalize = Normalizer.NormailizeDate;


        var ChangePriceRule = new TableSearchRule();
        ChangePriceRule.Name = "变动价格";
        ChangePriceRule.Rule = new string[] { "减持均价", "增持均价" }.ToList();
        ChangePriceRule.IsEq = false;
        ChangePriceRule.Normalize = (x, y) =>
        {
            if (x.Contains("元"))
            {
                return Utility.GetStringBefore(x, "元");
            }
            return x;
        };

        var ChangeNumberRule = new TableSearchRule();
        ChangeNumberRule.Name = "变动数量";
        ChangeNumberRule.Rule = new string[] { "减持股数", "增持股数" }.ToList();
        ChangeNumberRule.IsEq = false;
        ChangeNumberRule.Normalize = Normalizer.NormalizerStockNumber;

        var Rules = new List<TableSearchRule>();
        Rules.Add(StockHolderRule);
        Rules.Add(ChangeDateRule);
        Rules.Add(ChangePriceRule);
        Rules.Add(ChangeNumberRule);

        var result = HTMLTable.GetMultiInfo(root, Rules, false);
        var stockchangelist = new List<struStockChange>();
        foreach (var item in result)
        {
            var stockchange = new struStockChange();
            stockchange.id = id;
            var Name = NormalizeCompanyName(item[0].RawData);
            stockchange.HolderFullName = Name.Item1;
            stockchange.HolderName = Name.Item2;
            stockchange.ChangeEndDate = item[1].RawData;
            stockchange.ChangePrice = item[2].RawData;
            stockchange.ChangeNumber = item[3].RawData;
            stockchangelist.Add(stockchange);
        }
        return stockchangelist;

    }

    static string GetHolderFullName(HTMLEngine.MyRootHtmlNode root)
    {
        var Extractor = new ExtractProperty();
        var StartArray = new string[] { "接到", "收到", "股东" };
        var EndArray = new string[] { "的", "通知", "告知函", "减持", "增持", "《" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(root);
        foreach (var word in Extractor.CandidateWord)
        {
            if (word.Contains("简称")) return word;
            Program.Logger.WriteLine("候补股东全称修正：[" + word + "]");
        }
        if (Extractor.CandidateWord.Count > 0) return Extractor.CandidateWord[0];
        return "";
    }

    private static Tuple<String, String> NormalizeCompanyName(string word)
    {
        if (!String.IsNullOrEmpty(word))
        {
            var fullname = word;
            var shortname = "";
            var StdIdx = word.IndexOf("“");
            var EndIdx = word.IndexOf("”");
            if (StdIdx != -1 && EndIdx != -1)
            {
                shortname = word.Substring(StdIdx + 1, EndIdx - StdIdx - 1);
            }

            if (fullname.Contains("（以下简称"))
            {
                fullname = Utility.GetStringBefore(fullname, "（以下简称");
            }
            if (fullname.Contains("（下称"))
            {
                fullname = Utility.GetStringBefore(fullname, "（下称");
            }
            if (fullname.Contains("（简称"))
            {
                fullname = Utility.GetStringBefore(fullname, "（简称");
            }

            //暂时不做括号的正规化
            if (fullname.Contains("(以下简称"))
            {
                fullname = Utility.GetStringBefore(fullname, "(以下简称");
            }
            if (fullname.Contains("(下称"))
            {
                fullname = Utility.GetStringBefore(fullname, "(下称");
            }
            if (fullname.Contains("(简称"))
            {
                fullname = Utility.GetStringBefore(fullname, "(简称");
            }

            if (fullname.Contains("股东"))
            {
                fullname = Utility.GetStringAfter(fullname, "股东");
            }
            if (!String.IsNullOrEmpty(BussinessLogic.GetCompanyNameByShortName(fullname).secFullName))
            {
                fullname = BussinessLogic.GetCompanyNameByShortName(fullname).secFullName;
            }


            foreach (var companyname in companynamelist)
            {
                if (companyname.secFullName == fullname)
                {
                    if (shortname == "")
                    {
                        shortname = companyname.secShortName;
                        break;
                    }
                }
                if (companyname.secShortName == fullname)
                {
                    fullname = companyname.secFullName;
                    shortname = companyname.secShortName;
                    break;
                }
            }

            if (shortname == "")
            {
                shortname = BussinessLogic.GetCompanyNameByFullName(fullname).secShortName;
            }
            return Tuple.Create(fullname, shortname);

        }
        return Tuple.Create("", "");
    }

    //变动截止日期
    static string GetChangeEndDate(HTMLEngine.MyRootHtmlNode root)
    {
        var Extractor = new ExtractProperty();
        var StartArray = new string[] { "截止", "截至" };
        var EndArray = new string[] { "日" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            Program.Logger.WriteLine("候补变动截止日期：[" + item + "]");
            return Normalizer.NormailizeDate(item + "日");
        }
        return "";
    }
}