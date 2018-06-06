using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FDDC;
using static BussinessLogic;
using static HTMLEngine;
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
        public string HolderShortName;

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
            return id + ":" + HolderFullName.NormalizeTextResult() + ":" + ChangeEndDate;
        }

    }

    internal static struStockChange ConvertFromString(string str)
    {
        var Array = str.Split("\t");
        var c = new struStockChange();
        c.id = Array[0];
        c.HolderFullName = Array[1];
        c.HolderShortName = Array[2];
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
        increaseStock.HolderShortName + "," +
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
        stockchange.HolderShortName = Name.Item2;
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
        ChangeDateRule.Rule = new string[] { "减持期间", "增持期间", "减持时间", "增持时间" }.ToList();
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
        //只写在最后一条记录的地方,不过必须及时过滤掉不存在的记录
        result.Reverse();
        var stockchangelist = new List<struStockChange>();
        foreach (var rec in result)
        {
            var stockchange = new struStockChange();
            stockchange.id = id;
            var Name = NormalizeCompanyName(rec[0].RawData);
            stockchange.HolderFullName = Name.Item1;
            stockchange.HolderShortName = Name.Item2;
            stockchange.ChangeEndDate = rec[1].RawData;
            stockchange.ChangePrice = rec[2].RawData;
            stockchange.ChangeNumber = rec[3].RawData;
            var holderafterlist = GetHolderAfter(root);
            for (int i = 0; i < holderafterlist.Count; i++)
            {
                var after = holderafterlist[i];
                if (after.Used) continue;
                if (after.Name == stockchange.HolderFullName || after.Name == stockchange.HolderShortName)
                {
                    stockchange.HoldNumberAfterChange = after.Count;
                    stockchange.HoldPercentAfterChange = after.Percent;
                    after.Used = true;
                    break;
                }
            }
            stockchangelist.Add(stockchange);
        }
        return stockchangelist;

    }
    struct struHoldAfter
    {
        public String Name;

        public String Count;

        public string Percent;

        public Boolean Used;
    }
    static List<struHoldAfter> GetHolderAfter(MyRootHtmlNode root)
    {
        var HoldList = new List<struHoldAfter>();
        foreach (var table in root.TableList)
        {
            var mt = new HTMLTable(table.Value);
            for (int RowIdx = 0; RowIdx < mt.ColumnCount; RowIdx++)
            {
                for (int ColIdx = 0; ColIdx < mt.ColumnCount; ColIdx++)
                {
                    if (mt.CellValue(RowIdx + 1, ColIdx + 1) == "合计持有股份")
                    {
                        var HolderName = mt.CellValue(RowIdx + 1, 1);
                        Regex r = new Regex(@"\d+\.?\d*");

                        var strHolderCnt = mt.CellValue(RowIdx + 1, 5);
                        var HolderCnt = "";
                        if (!String.IsNullOrEmpty(r.Match(strHolderCnt).Value))
                        {
                            if (mt.CellValue(2, 5).Contains("万"))
                            {
                                //是否要*10000
                                HolderCnt = (double.Parse(r.Match(strHolderCnt).Value) * 10_000).ToString();
                            }
                            else
                            {
                                HolderCnt = r.Match(strHolderCnt).Value;
                            }
                        }

                        var StrPercent = mt.CellValue(RowIdx + 1, 6);
                        var HodlerPercent = "";
                        if (!String.IsNullOrEmpty(r.Match(StrPercent).Value))
                        {
                            HodlerPercent = (double.Parse(r.Match(StrPercent).Value) * 0.01).ToString();
                        }
                        HoldList.Add(new struHoldAfter() { Name = HolderName, Count = HolderCnt, Percent = HodlerPercent, Used = false });
                    }
                }
            }
        }
        return HoldList;
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

    public static string[] CompanyNameTrailingwords = new string[] { "（以下简称", "（下称", "（以下称", "（简称", "(以下简称", "(下称", "(以下称", "(简称" };


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

            //暂时不做括号的正规化
            foreach (var trailing in CompanyNameTrailingwords)
            {
                if (fullname.Contains(trailing))
                {
                    fullname = Utility.GetStringBefore(fullname, trailing);
                }
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