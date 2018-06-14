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
            return id + ":" + HolderFullName.NormalizeKey() + ":" + ChangeEndDate;
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
        var root = HTMLEngine.Anlayze(htmlFileName);
        companynamelist = BussinessLogic.GetCompanyNameByCutWord(root);
        foreach (var cn in companynamelist)
        {
            Program.Logger.WriteLine("公司名称：" + cn.secFullName);
            Program.Logger.WriteLine("公司简称：" + cn.secShortName);
        }
        var Name = GetHolderName(root);
        if (!String.IsNullOrEmpty(Name.Item1) && !String.IsNullOrEmpty(Name.Item2))
        {
            companynamelist.Add(new struCompanyName()
            {
                secFullName = Name.Item1,
                secShortName = Name.Item2
            });
        }
        list = ExtractFromTable(root, fi.Name.Replace(".html", ""));
        if (list.Count > 0) return list;

        var stockchange = new struStockChange();
        //公告ID
        stockchange.id = fi.Name.Replace(".html", "");
        Program.Logger.WriteLine("公告ID:" + stockchange.id);

        stockchange.HolderFullName = Name.Item1.TrimStart("—".ToCharArray()).TrimStart("-".ToCharArray());
        stockchange.HolderShortName = Name.Item2;
        stockchange.ChangeEndDate = GetChangeEndDate(root);
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
        ChangeDateRule.Rule = new string[] { "减持期间", "增持期间", "减持时间", "增持时间", "减持期间", "增持期间" }.ToList();
        ChangeDateRule.IsEq = false;
        ChangeDateRule.Normalize = Normalizer.NormailizeDate;


        var ChangePriceRule = new TableSearchRule();
        ChangePriceRule.Name = "变动价格";
        ChangePriceRule.Rule = new string[] { "减持均价", "增持均价", "减持价格", "增持价格" }.ToList();
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
        ChangeNumberRule.Rule = new string[] { "减持股数", "增持股数", "减持数量", "增持数量" }.ToList();
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
            stockchange.HolderFullName = Name.Item1.TrimStart("—".ToCharArray()).TrimStart("-".ToCharArray());
            stockchange.HolderShortName = Name.Item2;
            stockchange.ChangeEndDate = rec[1].RawData;

            if (!String.IsNullOrEmpty(rec[2].RawData) &&
                !(rec[2].RawData.Contains("-") || rec[2].RawData.Contains("至")))
            {
                //股价区间化的去除
                stockchange.ChangePrice = rec[2].RawData;
                stockchange.ChangePrice = stockchange.ChangePrice.NormalizeNumberResult();
            }

            stockchange.ChangeNumber = rec[3].RawData;
            stockchange.ChangeNumber = stockchange.ChangeNumber.NormalizeNumberResult();
            //基本上所有的有效记录都有股东名和截至日期，所以，这里这么做，可能对于极少数没有截至日期的数据有伤害，但是对于整体指标来说是好的
            if (string.IsNullOrEmpty(stockchange.HolderFullName) || string.IsNullOrEmpty(stockchange.ChangeEndDate)) continue;
            stockchangelist.Add(stockchange);
        }

        var holderafterlist = GetHolderAfter(root);

        //寻找所有的股东全称
        var namelist = stockchangelist.Select(x => x.HolderFullName).Distinct().ToList();
        var newRec = new List<struStockChange>();
        foreach (var name in namelist)
        {
            var sl = stockchangelist.Where((x) => { return x.HolderFullName == name; }).ToList();
            sl.Sort((x, y) => { return x.ChangeEndDate.CompareTo(y.ChangeEndDate); });
            var last = sl.Last();
            for (int i = 0; i < holderafterlist.Count; i++)
            {
                var after = holderafterlist[i];
                if (after.Name == last.HolderFullName || after.Name == last.HolderShortName)
                {
                    stockchangelist.Remove(last);   //结构体，无法直接修改！！使用删除，增加的方法
                    last.HoldNumberAfterChange = after.Count;
                    last.HoldPercentAfterChange = after.Percent;
                    newRec.Add(last);
                }
            }
        }
        stockchangelist.AddRange(newRec);
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
            for (int RowIdx = 0; RowIdx < mt.RowCount; RowIdx++)
            {
                for (int ColIdx = 0; ColIdx < mt.ColumnCount; ColIdx++)
                {
                    if (mt.CellValue(RowIdx + 1, ColIdx + 1) == "合计持有股份")
                    {
                        var HolderName = mt.CellValue(RowIdx + 1, 1);
                        Regex r = new Regex(@"\d+\.?\d*");

                        var strHolderCnt = mt.CellValue(RowIdx + 1, 5);
                        strHolderCnt = Normalizer.NormalizeNumberResult(strHolderCnt);
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

    static Tuple<String, String> GetHolderName(HTMLEngine.MyRootHtmlNode root)
    {
        var Extractor = new EntityProperty();
        var StartArray = new string[] { "接到", "收到", "股东" };
        var EndArray = new string[] { "的", "通知", "告知函", "减持", "增持", "《" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(root);
        foreach (var word in Extractor.CandidateWord)
        {
            var name = NormalizeCompanyName(word);
            if (!String.IsNullOrEmpty(name.Item1) && !String.IsNullOrEmpty(name.Item2))
            {
                return name;
            }
        }
        foreach (var word in Extractor.CandidateWord)
        {
            var name = NormalizeCompanyName(word);
            if (!String.IsNullOrEmpty(name.Item1))
            {
                return name;
            }
        }
        return Tuple.Create("", "");
    }

    public static string[] CompanyNameTrailingwords = new string[] { "（以下简称", "（下称", "（以下称", "（简称", "(以下简称", "(下称", "(以下称", "(简称" };


    private static Tuple<String, String> NormalizeCompanyName(string word)
    {
        if (!String.IsNullOrEmpty(word))
        {
            var fullname = word.Replace(" ", "");
            var shortname = "";
            var StdIdx = word.IndexOf("“");
            var EndIdx = word.IndexOf("”");
            if (EndIdx < StdIdx) return Tuple.Create("", "");
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
                //如果进来的是简称，而提取的公司信息里面，只有全称，这里简单推断一下
                //简称和全称的关系
                if (companyname.secFullName.Contains(fullname) &&
                    companyname.secFullName.Length > fullname.Length)
                {
                    fullname = companyname.secFullName;
                    shortname = word;
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
        var Extractor = new EntityProperty();
        var StartArray = new string[] { "截止", "截至" };
        var EndArray = new string[] { "日" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            if (item.Length >20) continue;
            Program.Logger.WriteLine("候补变动截止日期：[" + item + "]");
            return Normalizer.NormailizeDate(item + "日");
        }
        return "";
    }
}