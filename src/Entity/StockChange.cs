using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FDDC;
using static BussinessLogic;
using static HTMLEngine;
using static HTMLTable;
using static LocateProperty;

public class StockChange : AnnouceDocument
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
        public static struStockChange ConvertFromString(string str)
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

        public string ConvertToString(struStockChange increaseStock)
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
    }


    static Char[] FullNameStartTrimChar = new Char[] { '—', '-', ']' };
    public static List<struStockChange> Extract(string htmlFileName)
    {
        Init(htmlFileName);
        var list = new List<struStockChange>();
        var Name = GetHolderName(root);
        if (!String.IsNullOrEmpty(Name.FullName) && !String.IsNullOrEmpty(Name.ShortName))
        {
            companynamelist.Add(new struCompanyName()
            {
                secFullName = Name.FullName,
                secShortName = Name.ShortName
            });
        }
        list = ExtractFromTable(root, Id);
        if (list.Count > 0) return list;    //如果这里直接返回，由于召回率等因素，可以细微提高成绩

        var stockchange = new struStockChange();
        //公告ID
        stockchange.id = Id;
        //Program.Logger.WriteLine("公告ID:" + stockchange.id);
        stockchange.HolderFullName = Name.FullName.TrimStart(FullNameStartTrimChar);
        if (EntityWordAnlayzeTool.TrimEnglish(stockchange.HolderFullName).Length > ContractTraning.MaxYiFangLength)
        {
            stockchange.HolderFullName = "";
        }
        stockchange.HolderShortName = Name.ShortName;
        stockchange.ChangeEndDate = GetChangeEndDate(root);

        DateTime x;
        if (!DateTime.TryParse(stockchange.ChangeEndDate, out x))
        {
            //无法处理的情况
            if (!Program.IsDebugMode)
            {
                //非调试模式
                stockchange.ChangeEndDate = "";
            }
        }

        if (!string.IsNullOrEmpty(stockchange.HolderFullName) && !string.IsNullOrEmpty(stockchange.ChangeEndDate))
        {
            list.Add(stockchange);
        }

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
        ChangeDateRule.Rule = new string[] { "减持期间", "增持期间", "减持股份期间", "增持股份期间",
                                             "减持时间", "增持时间", "减持股份时间", "增持股份时间" }.ToList();
        ChangeDateRule.IsEq = false;
        ChangeDateRule.Normalize = NormailizeEndChangeDate;


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
        ChangeNumberRule.Normalize = NumberUtility.NormalizerStockNumber;

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
            stockchange.HolderFullName = Name.FullName.TrimStart(FullNameStartTrimChar);
            stockchange.HolderShortName = Name.ShortName;
            stockchange.ChangeEndDate = rec[1].RawData;

            DateTime x;
            if (!DateTime.TryParse(stockchange.ChangeEndDate, out x))
            {
                //无法处理的情况
                if (!Program.IsDebugMode)
                {
                    //非调试模式
                    stockchange.ChangeEndDate = "";
                }
            }

            if (!String.IsNullOrEmpty(rec[2].RawData))
            {
                //股价区间化的去除
                if (!(rec[2].RawData.Contains("-") || rec[2].RawData.Contains("~") || rec[2].RawData.Contains("至")))
                {
                    stockchange.ChangePrice = rec[2].RawData.Replace(" ", "");
                    stockchange.ChangePrice = stockchange.ChangePrice.NormalizeNumberResult();
                }
            }
            if (!RegularTool.IsUnsign(stockchange.ChangePrice))
            {
                stockchange.ChangePrice = "";
            }

            if (!String.IsNullOrEmpty(rec[3].RawData))
            {
                stockchange.ChangeNumber = rec[3].RawData.Replace(" ", "");
                stockchange.ChangeNumber = stockchange.ChangeNumber.NormalizeNumberResult();
                if (!RegularTool.IsUnsign(stockchange.ChangeNumber))
                {
                    stockchange.ChangeNumber = "";
                }
            }

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

        if (holderafterlist.Count != namelist.Count)
        {
            Program.Logger.WriteLine("增持者数量确认！");
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

                        var strHolderCnt = mt.CellValue(RowIdx + 1, mt.ColumnCount - 1);
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

                        var StrPercent = mt.CellValue(RowIdx + 1, mt.ColumnCount);
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

    static (String FullName, String ShortName) GetHolderName(HTMLEngine.MyRootHtmlNode root)
    {
        var Extractor = new ExtractProperty();
        var StartArray = new string[] { "接到", "收到", "股东" };
        var EndArray = new string[] { "的", "通知", "告知函", "减持", "增持", "《" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(root);
        foreach (var word in Extractor.CandidateWord)
        {
            var name = NormalizeCompanyName(word.Value);
            if (!String.IsNullOrEmpty(name.FullName) && !String.IsNullOrEmpty(name.ShortName))
            {
                return name;
            }
        }
        foreach (var word in Extractor.CandidateWord)
        {
            var name = NormalizeCompanyName(word.Value);
            if (!String.IsNullOrEmpty(name.FullName))
            {
                return name;
            }
        }
        return ("", "");
    }

    public static string[] CompanyNameTrailingwords =
    new string[] { "（以下简称", "（下称", "（以下称", "（简称", "(以下简称", "(下称", "(以下称", "(简称" };


    private static (String FullName, String ShortName) NormalizeCompanyName(string word)
    {
        if (!String.IsNullOrEmpty(word))
        {
            var fullname = word.Replace(" ", "");
            var shortname = "";
            var StdIdx = word.IndexOf("“");
            var EndIdx = word.IndexOf("”");
            if (EndIdx < StdIdx) return ("", "");
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
            return (fullname, shortname);

        }
        return ("", "");
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
            if (item.Value.Length > 20) continue;
            Program.Logger.WriteLine("候补变动截止日期：[" + item.Value + "]");
            return NormailizeEndChangeDate(item.Value + "日");
        }
        return "";
    }


    public static string NormailizeEndChangeDate(string orgString, string keyword = "")
    {
        var format = "yyyy-MM-dd";
        if (orgString.StartsWith("到")) orgString = orgString.Substring(1);
        if (orgString.Contains("（")) orgString = Utility.GetStringBefore(orgString, "（");
        if (orgString.Contains("公告") || orgString.Contains("披露") || orgString.StartsWith("本"))
        {
            if (datelist.Count == 0) return orgString;
            if (datelist.Count > 1)
            {
                //这里有可能要使用第一次出现的日期
                //如果第一次出现的日期是公告发布日的前一天，则认为应该采用前一天
                var FirstAnnouceDate = datelist.First().Value;
                if (FirstAnnouceDate.Subtract(AnnouceDate).Days == -1) return FirstAnnouceDate.ToString(format);
                return AnnouceDate.ToString(format);
            }
        }

        orgString = orgString.Trim().Replace(",", "");

        //XXXX年XX月XX日 - XXXX年XX月XX日
        var NumberList = RegularTool.GetNumberList(orgString);
        if (NumberList.Count == 6)
        {
            String Year = NumberList[3];
            String Month = NumberList[4];
            String Day = NumberList[5];
            int year; int month; int day;
            if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
            {
                var d = DateUtility.GetWorkDay(year, month, day);
                return d.ToString(format);
            }
        }

        //XXXX年XX月XX日 - XX月XX日
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
                    var d = DateUtility.GetWorkDay(year, month, day);
                    return d.ToString(format);
                }
            }
        }
        //XXXX年XX月XX日 - XX日 
        if (NumberList.Count == 4)
        {
            if (orgString.IndexOf("年") != -1 && orgString.IndexOf("月") != -1 && orgString.IndexOf("日") != -1)
            {
                String Year = NumberList[0];
                String Month = NumberList[1];
                String Day = NumberList[3];
                int year; int month; int day;
                if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
                {
                    var d = DateUtility.GetWorkDay(year, month, day);
                    return d.ToString(format);
                }
            }
        }
        //XX月XX日
        if (NumberList.Count == 2)
        {
            if (orgString.IndexOf("月") != -1 && orgString.IndexOf("日") != -1)
            {
                if (datelist.Count == 0) return orgString;
                var AnnouceDate = datelist.Last();
                String Month = NumberList[0];
                String Day = NumberList[1];
                int month; int day;
                if (int.TryParse(Month, out month) && int.TryParse(Day, out day))
                {
                    var d = DateUtility.GetWorkDay(AnnouceDate.Value.Year, month, day);
                    return d.ToString(format);
                }
            }
            if (orgString.IndexOf("年") != -1 && orgString.IndexOf("月") != -1)
            {
                String Year = NumberList[0];
                String Month = NumberList[1];
                int year; int month;
                if (int.TryParse(Year, out year) && int.TryParse(Month, out month))
                {
                    var d = DateUtility.GetWorkDay(year, month, -1);
                    return d.ToString(format);
                }
            }
            if (orgString.IndexOf("月") != -1)
            {
                String Year = NumberList[0];
                if (Year.Length != 4) return orgString;
                String Month = NumberList[1];
                int year; int month;
                if (int.TryParse(Year, out year) && int.TryParse(Month, out month))
                {
                    var d = DateUtility.GetWorkDay(year, month, -1);
                    return d.ToString(format);
                }
            }
        }
        //XXXX年XX月XX日
        if (orgString.Contains("年") && orgString.Contains("月") && orgString.Contains("月"))
        {
            String Year = Utility.GetStringBefore(orgString, "年");
            String Month = RegularTool.GetValueBetweenString(orgString, "年", "月");
            String Day = Utility.GetStringAfter(orgString, "月").Replace("日", "");
            int year; int month; int day;
            if (int.TryParse(Year, out year) && int.TryParse(Month, out month) && int.TryParse(Day, out day))
            {
                var d = DateUtility.GetWorkDay(year, month, day);
                return d.ToString(format);
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
                    var d = DateUtility.GetWorkDay(year, month, day);
                    return d.ToString(format);
                }
            }
        }
        return orgString;
    }
}