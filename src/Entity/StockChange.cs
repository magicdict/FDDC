using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FDDC;
using static CompanyNameLogic;
using static HTMLEngine;
using static HTMLTable;
using static LocateProperty;

public class StockChange : AnnouceDocument
{
    public static Dictionary<String, String> PublishTime = new Dictionary<String, String>();
    public static void ImportPublishTime()
    {
        if (!System.IO.Directory.Exists(Program.DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_public_time_20180629")) return;
        foreach (var csvfilename in System.IO.Directory.GetFiles(Program.DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_public_time_20180629"))
        {
            if (csvfilename.EndsWith(".csv"))
            {
                var sr = new StreamReader(csvfilename);
                sr.ReadLine();  //Skip Header
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(",");
                    PublishTime.Add(line[1], line[0]);
                }
            }
        }
    }

    public override List<RecordBase> Extract()
    {
        SpecialFixTable();
        var DateRange = LocateDateRange(root);
        var list = new List<RecordBase>();
        var Name = GetHolderName();
        if (!String.IsNullOrEmpty(Name.FullName) && !String.IsNullOrEmpty(Name.ShortName))
        {
            companynamelist.Add(new struCompanyName()
            {
                secFullName = Name.FullName,
                secShortName = Name.ShortName
            });
        }
        list = ExtractFromTable();
        //list = ExtractFromTableByContent();
        if (list.Count > 0) return list;    //如果这里直接返回，由于召回率等因素，可以细微提高成绩

        var stockchange = new StockChangeRec();
        //公告ID
        stockchange.Id = Id;
        //if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("公告ID:" + stockchange.id);
        stockchange.HolderFullName = Name.FullName.NormalizeTextResult();
        if (Utility.TrimEnglish(stockchange.HolderFullName).Length > ContractTraning.JiaFangES.MaxLength)
        {
            stockchange.HolderFullName = String.Empty;
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
                stockchange.ChangeEndDate = String.Empty;
            }
        }

        if (!string.IsNullOrEmpty(stockchange.HolderFullName) && !string.IsNullOrEmpty(stockchange.ChangeEndDate))
        {
            if (!stockchange.HolderFullName.Contains("增持") && !stockchange.HolderFullName.Contains("减持")) list.Add(stockchange);
        }

        return list;
    }

    /// <summary>
    /// 本业务逻辑专用修补表
    /// </summary>
    void SpecialFixTable()
    {
        //如果表里面含有这些关键字，同时表格的列数都一致的话，进行合并
        var KeyWord = new string[] { "集中竞价交易", "竞价交易", "大宗交易", "约定式购回" };
        for (int TableNo = 1; TableNo <= root.TableList.Count - 1; TableNo++)
        {
            var Table = new HTMLTable(root.TableList[TableNo]);
            var Col = Table.ColumnCount;
            var Nexttable = new HTMLTable(root.TableList[TableNo + 1]);
            var NextCol = Nexttable.ColumnCount;
            var Contains = false;
            var NextContains = false;
            foreach (var item in root.TableList[TableNo])
            {
                foreach (var key in KeyWord)
                {
                    if (item.Contains(key))
                    {
                        Contains = true;
                        break;
                    }
                }
                if (Contains) break;
            }

            foreach (var item in root.TableList[TableNo + 1])
            {
                foreach (var key in KeyWord)
                {
                    if (item.Contains(key))
                    {
                        NextContains = true;
                        break;
                    }
                }
                if (NextContains) break;
            }

            var ThirdCol = -1;
            var ThirdContais = false;
            if (root.TableList.ContainsKey(TableNo + 2))
            {
                var ThirdTable = new HTMLTable(root.TableList[TableNo + 2]);
                ThirdCol = ThirdTable.ColumnCount;
                foreach (var item in root.TableList[TableNo + 2])
                {
                    foreach (var key in KeyWord)
                    {
                        if (item.Contains(key))
                        {
                            ThirdContais = true;
                            break;
                        }
                    }
                    if (ThirdContais) break;
                }
            }

            if (ThirdCol == NextCol && ThirdContais && NextContains)
            {
                HTMLTable.MergeTable(this, TableNo + 2);
            }

            if (Col == NextCol && Contains && NextContains)
            {
                HTMLTable.MergeTable(this, TableNo + 1);
            }

        }
    }

    /// <summary>
    /// 根据表头标题抽取
    /// </summary>
    /// <param name="root"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    List<RecordBase> ExtractFromTable()
    {
        var StockHolderRule = new TableSearchTitleRule();
        StockHolderRule.Name = "股东全称";
        StockHolderRule.Title = new string[] { "股东名称", "名称", "增持主体", "增持人", "减持主体", "减持人", "姓名" }.ToList();
        StockHolderRule.IsTitleEq = true;
        StockHolderRule.IsRequire = true;

        var ChangeDateRule = new TableSearchTitleRule();
        ChangeDateRule.Name = "变动截止日期";
        ChangeDateRule.Title = new string[] { "买卖时间","日期","减持期间", "增持期间", "减持股份期间", "增持股份期间",
                                             "减持时间", "增持时间", "减持股份时间", "增持股份时间","买入时间","卖出时间" }.ToList();
        ChangeDateRule.IsTitleEq = false;
        ChangeDateRule.Normalize = NormailizeEndChangeDate;


        var ChangePriceRule = new TableSearchTitleRule();
        ChangePriceRule.Name = "变动价格";
        ChangePriceRule.Title = new string[] { "成交均价", "减持价格", "增持价格", "减持均", "增持均" }.ToList();
        ChangePriceRule.IsTitleEq = false;
        ChangePriceRule.Normalize = (x, y) =>
        {

            var prices = RegularTool.GetRegular(x, RegularTool.MoneyExpress);
            if (prices.Count == 0)
            {
                if (x.Contains("元"))
                {
                    return Utility.GetStringBefore(x, "元");
                }
            }
            else
            {
                //增减持，区间的情况，取最高价,假设最后一个数字是最大的
                return prices.Last().RawData;
            }
            return x;
        };

        var ChangeNumberRule = new TableSearchTitleRule();
        ChangeNumberRule.Name = "变动数量";
        ChangeNumberRule.Title = new string[] { "成交数量", "减持股数", "增持股数", "减持数量", "增持数量", "买入股份数", "卖出股份数" }.ToList();
        ChangeNumberRule.IsTitleEq = false;
        ChangeNumberRule.Normalize = NumberUtility.NormalizerStockNumber;


        var Rules = new List<TableSearchTitleRule>();
        Rules.Add(StockHolderRule);
        Rules.Add(ChangeDateRule);
        Rules.Add(ChangePriceRule);
        Rules.Add(ChangeNumberRule);

        var result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, false);

        if (result.Count == 0)
        {
            //没有抽取到任何数据
            Rules.Clear();
            ChangeDateRule.IsRequire = true;
            Rules.Add(ChangeDateRule);
            Rules.Add(ChangePriceRule);
            Rules.Add(ChangeNumberRule);
            result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, false);
            if (result.Count == 0)
            {
                return new List<RecordBase>();
            }
            var NewResult = new List<CellInfo[]>();
            var Name = GetHolderName();
            if (String.IsNullOrEmpty(Name.FullName) && String.IsNullOrEmpty(Name.ShortName))
            {
                return new List<RecordBase>();
            }
            foreach (var item in result)
            {
                NewResult.Add(new CellInfo[]
                {new CellInfo() {RawData = String.IsNullOrEmpty(Name.FullName)?Name.ShortName:Name.FullName} , item[0], item[1], item[2] });
            }
            result = NewResult;
        }

        var holderafterlist = GetHolderAfter();

        var stockchangelist = new List<RecordBase>();
        foreach (var rec in result)
        {
            var stockchange = new StockChangeRec();
            stockchange.Id = Id;

            var ModifyName = rec[0].RawData;
            //表格里面长的名字可能被分页切割掉
            //这里使用合计表进行验证
            if (!holderafterlist.Select((z) => { return z.Name; }).ToList().Contains(ModifyName))
            {
                foreach (var item in holderafterlist)
                {
                    if (item.Name.EndsWith("先生")) break;  //特殊处理，没有逻辑可言
                    if (item.Name.StartsWith(ModifyName) && !item.Name.Equals(ModifyName))
                    {
                        ModifyName = item.Name;
                        break;
                    }
                    if (item.Name.EndsWith(ModifyName) && !item.Name.Equals(ModifyName))
                    {
                        ModifyName = item.Name;
                        break;
                    }
                }
            }


            var Name = CompanyNameLogic.NormalizeCompanyName(this, ModifyName);
            stockchange.HolderFullName = Name.FullName.NormalizeTextResult();
            stockchange.HolderShortName = Name.ShortName;

            if (stockchange.HolderFullName.Contains("简称"))
            {
                stockchange.HolderShortName = Utility.GetStringAfter(stockchange.HolderFullName, "简称");
                stockchange.HolderShortName = stockchange.HolderShortName.Replace(")", String.Empty).Replace("“", String.Empty).Replace("”", String.Empty);
                stockchange.HolderFullName = Utility.GetStringBefore(stockchange.HolderFullName, "(");
            }

            stockchange.ChangeEndDate = rec[1].RawData;

            DateTime x;
            if (!DateTime.TryParse(stockchange.ChangeEndDate, out x))
            {
                //无法处理的情况
                if (!Program.IsDebugMode)
                {
                    //非调试模式
                    stockchange.ChangeEndDate = String.Empty;
                }
            }

            if (!String.IsNullOrEmpty(rec[2].RawData))
            {
                //股价区间化的去除
                if (!(rec[2].RawData.Contains("-") || rec[2].RawData.Contains("~") || rec[2].RawData.Contains("至")))
                {
                    stockchange.ChangePrice = rec[2].RawData.Replace(" ", String.Empty);
                    stockchange.ChangePrice = stockchange.ChangePrice.Replace("*", "");
                    stockchange.ChangePrice = stockchange.ChangePrice.NormalizeNumberResult();
                }
            }
            if (!RegularTool.IsUnsign(stockchange.ChangePrice))
            {
                if (!String.IsNullOrEmpty(stockchange.ChangePrice)) Console.WriteLine("Error ChangePrice:[" + stockchange.ChangePrice + "]");
                stockchange.ChangePrice = String.Empty;
            }


            if (!String.IsNullOrEmpty(rec[3].RawData))
            {
                stockchange.ChangeNumber = rec[3].RawData.Replace(" ", String.Empty);
                stockchange.ChangeNumber = stockchange.ChangeNumber.NormalizeNumberResult();
                if (!RegularTool.IsUnsign(stockchange.ChangeNumber))
                {
                    if (!String.IsNullOrEmpty(stockchange.ChangeNumber)) Console.WriteLine("Error ChangeNumber:[" + stockchange.ChangeNumber + "]");
                    stockchange.ChangeNumber = String.Empty;
                }
            }

            //基本上所有的有效记录都有股东名和截至日期，所以，这里这么做，可能对于极少数没有截至日期的数据有伤害，但是对于整体指标来说是好的
            if (string.IsNullOrEmpty(stockchange.HolderFullName) || string.IsNullOrEmpty(stockchange.ChangeEndDate)) continue;
            if (stockchange.ChangeNumber == "0" || stockchange.ChangePrice == "0") continue;
            stockchangelist.Add(stockchange);
        }


        //寻找所有的股东全称
        var namelist = stockchangelist.Select(x => ((StockChangeRec)x).HolderFullName).Distinct().ToList();
        var newRec = new List<StockChangeRec>();
        foreach (var name in namelist)
        {
            var stocklist = stockchangelist.Where((x) => { return ((StockChangeRec)x).HolderFullName == name; }).ToList();
            stocklist.Sort((x, y) => { return ((StockChangeRec)x).ChangeEndDate.CompareTo(((StockChangeRec)x).ChangeEndDate); });
            var last = (StockChangeRec)stocklist.Last();
            for (int i = 0; i < holderafterlist.Count; i++)
            {
                var after = holderafterlist[i];
                after.Name = after.Name.Replace(" ", "");
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
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("增持者数量确认！");
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
    List<struHoldAfter> GetHolderAfter()
    {
        var HoldList = new List<struHoldAfter>();
        foreach (var table in root.TableList)
        {
            var mt = new HTMLTable(table.Value);
            for (int RowIdx = 0; RowIdx < mt.RowCount; RowIdx++)
            {
                for (int ColIdx = 0; ColIdx < mt.ColumnCount; ColIdx++)
                {
                    if (mt.CellValue(RowIdx + 1, ColIdx + 1) == "合计持有股份" || mt.CellValue(RowIdx + 1, ColIdx + 1) == "合计持股")
                    {
                        var HolderName = mt.CellValue(RowIdx + 1, 1);
                        var strHolderCnt = mt.CellValue(RowIdx + 1, mt.ColumnCount - 1);
                        strHolderCnt = Normalizer.NormalizeNumberResult(strHolderCnt);
                        var title = mt.CellValue(2, 5);
                        string HolderCnt = getAfterstock(title, strHolderCnt);

                        var StrPercent = mt.CellValue(RowIdx + 1, mt.ColumnCount);
                        var HodlerPercent = getAfterpercent(StrPercent);
                        HoldList.Add(new struHoldAfter() { Name = HolderName, Count = HolderCnt, Percent = HodlerPercent, Used = false });
                    }
                }
            }
        }
        if (HoldList.Count == 0)
        {
            HoldList = GetHolderAfter2ndStep();
        }
        if (HoldList.Count == 0)
        {
            HoldList = GetHolderAfter3rdStep();
        }
        return HoldList;
    }

    private static string getAfterpercent(string StrPercent)
    {
        if (string.IsNullOrEmpty(StrPercent)) return "";
        Regex r = new Regex(@"\d+\.?\d*");
        if (!String.IsNullOrEmpty(r.Match(StrPercent).Value))
        {
            var pecent = Math.Round((double.Parse(r.Match(StrPercent).Value) * 0.01), 4);
            return pecent.ToString();
        }
        return "";
    }

    private static string getAfterstock(string Title, string strHolderCnt)
    {
        if (string.IsNullOrEmpty(strHolderCnt)) return "";
        Regex r = new Regex(@"\d+\.?\d*");
        var HolderCnt = String.Empty;
        if (!String.IsNullOrEmpty(r.Match(strHolderCnt).Value))
        {
            if (Title.Contains("万"))
            {
                //是否要*10000
                HolderCnt = (double.Parse(r.Match(strHolderCnt).Value) * 10_000).ToString();
            }
            else
            {
                HolderCnt = r.Match(strHolderCnt).Value;
            }
        }

        return HolderCnt;
    }

    List<struHoldAfter> GetHolderAfter2ndStep()
    {
        var HoldList = new List<struHoldAfter>();
        var keyword = new string[] { "增持后持股", "减持后持股" };
        foreach (var table in root.TableList)
        {
            var HeaderRowNo = -1;
            var mt = new HTMLTable(table.Value);
            for (int RowCount = 1; RowCount <= mt.RowCount; RowCount++)
            {
                for (int ColumnCount = 1; ColumnCount < mt.ColumnCount; ColumnCount++)
                {
                    var value = mt.CellValue(RowCount, ColumnCount);
                    foreach (var key in keyword)
                    {
                        if (value.Contains(key))
                        {
                            HeaderRowNo = RowCount;
                            break;
                        }
                    }
                    if (HeaderRowNo != -1) break;
                }
                if (HeaderRowNo != -1) break;
            }
            if (HeaderRowNo != -1)
            {
                //如果有5格
                if (mt.ColumnCount != 5) continue;
                int PercentCol = -1;
                for (int rowno = HeaderRowNo + 1; rowno <= mt.RowCount; rowno++)
                {
                    var value1 = mt.CellValue(rowno, 1);

                    var Title4 = mt.CellValue(HeaderRowNo, 4);
                    var value4 = mt.CellValue(rowno, 4);
                    value4 = value4.Trim().Replace(",", String.Empty);
                    value4 = value4.Trim().Replace("，", String.Empty);

                    var Title5 = mt.CellValue(HeaderRowNo, 5).Replace(" ", "");
                    var value5 = mt.CellValue(rowno, 5);
                    value5 = value5.Trim().Replace(",", String.Empty);
                    value5 = value5.Trim().Replace("，", String.Empty);
                    if (Title5.Contains("增持后持股比例（%）") || Title5.Contains("减持后持股比例（%）"))
                    {
                        PercentCol = 5;
                        //Console.WriteLine(Title5);
                    }
                    if (PercentCol == 5 && !value5.Contains("%")) value5 += "%";
                    if (RegularTool.IsNumeric(value4) && RegularTool.IsPercent(value5))
                    {
                        //Console.WriteLine("GetHolderAfter2ndStep:" + value1);
                        HoldList.Add(new struHoldAfter()
                        {
                            Name = value1,
                            Count = getAfterstock(Title4, value4),
                            Percent = getAfterpercent(value5),
                            Used = false
                        });
                        continue;
                    }
                }
            }
        }
        return HoldList;
    }

    List<struHoldAfter> GetHolderAfter3rdStep()
    {
        var HoldList = new List<struHoldAfter>();
        var StockHolderRule = new TableSearchTitleRule();
        StockHolderRule.Name = "股东全称";
        StockHolderRule.Title = new string[] { "股东名称", "名称", "增持主体", "增持人", "减持主体", "减持人" }.ToList();
        StockHolderRule.IsTitleEq = true;
        StockHolderRule.IsRequire = true;

        var HoldNumberAfterChangeRule = new TableSearchTitleRule();
        HoldNumberAfterChangeRule.Name = "变动后持股数";
        HoldNumberAfterChangeRule.IsRequire = true;
        HoldNumberAfterChangeRule.SuperTitle = new string[] { "减持后", "增持后" }.ToList();
        HoldNumberAfterChangeRule.IsSuperTitleEq = false;
        HoldNumberAfterChangeRule.Title = new string[] {
             "持股股数","持股股数",
             "持股数量","持股数量",
             "持股总数","持股总数","股数"
        }.ToList();
        HoldNumberAfterChangeRule.IsTitleEq = false;

        var HoldPercentAfterChangeRule = new TableSearchTitleRule();
        HoldPercentAfterChangeRule.Name = "变动后持股数比例";
        HoldPercentAfterChangeRule.IsRequire = true;
        HoldPercentAfterChangeRule.SuperTitle = HoldNumberAfterChangeRule.SuperTitle;
        HoldPercentAfterChangeRule.IsSuperTitleEq = false;
        HoldPercentAfterChangeRule.Title = new string[] { "比例" }.ToList();
        HoldPercentAfterChangeRule.IsTitleEq = false;

        var Rules = new List<TableSearchTitleRule>();
        Rules.Add(StockHolderRule);
        Rules.Add(HoldNumberAfterChangeRule);
        Rules.Add(HoldPercentAfterChangeRule);
        var result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, false);
        if (result.Count != 0)
        {
            foreach (var item in result)
            {
                var HolderName = item[0].RawData;
                var strHolderCnt = item[1].RawData;
                strHolderCnt = Normalizer.NormalizeNumberResult(strHolderCnt);
                string HolderCnt = getAfterstock(item[1].Title, strHolderCnt);
                var StrPercent = item[2].RawData;
                var HodlerPercent = getAfterpercent(StrPercent);
                //Console.WriteLine("GetHolderAfter3rdStep:" + HolderName);
                HoldList.Add(new struHoldAfter()
                {
                    Name = HolderName,
                    Count = HolderCnt,
                    Percent = HodlerPercent,
                    Used = false
                });
            }
        }
        else
        {

            StockHolderRule.SuperTitle = StockHolderRule.Title;
            StockHolderRule.IsSuperTitleEq = true;
            StockHolderRule.Title.Clear();
            StockHolderRule.IsTitleEq = false;

            Rules = new List<TableSearchTitleRule>();
            Rules.Add(HoldNumberAfterChangeRule);
            Rules.Add(HoldPercentAfterChangeRule);
            Rules.Add(StockHolderRule);
            result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, false);
            if (result.Count != 0)
            {
                foreach (var item in result)
                {
                    var HolderName = item[2].RawData;
                    var strHolderCnt = item[0].RawData;
                    strHolderCnt = Normalizer.NormalizeNumberResult(strHolderCnt);
                    string HolderCnt = getAfterstock(item[1].Title, strHolderCnt);
                    var StrPercent = item[1].RawData;
                    var HodlerPercent = getAfterpercent(StrPercent);
                    //Console.WriteLine("GetHolderAfter4thStep:" + HolderName);
                    HoldList.Add(new struHoldAfter()
                    {
                        Name = HolderName,
                        Count = HolderCnt,
                        Percent = HodlerPercent,
                        Used = false
                    });
                }
            }
        }

        return HoldList;
    }

    /// <summary>
    /// 获得股东
    /// </summary>
    /// <returns></returns>
    (String FullName, String ShortName) GetHolderName()
    {
        //接到 XXXXXX 的增持通知   XXXX应该在NER列表里面的人名
        //公司的简称或者全称
        //最后要求结果不应该包含 增持，减持等字样
        var ForbitWords = new string[] { "增持", "减持" };
        var Extractor = new ExtractPropertyByHTML();
        var StartArray = new string[] { "接到", "收到"};
        var EndArray = new string[] { "通知", "告知函", "减持", "增持" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(root);
        foreach (var word in Extractor.CandidateWord)
        {
            var HasForbitWord = false;
            foreach (var fw in ForbitWords)
            {
                if (word.Value.Contains(fw))
                {
                    HasForbitWord = true;
                    break;
                }
            }
            if (HasForbitWord) continue;
            var HolderName = "";
            //实际控制人
            if (word.Value.Contains("实际控制人"))
            {
                HolderName = Utility.GetStringAfter(word.Value, "实际控制人");
                if (HolderName.Contains("先生")) HolderName = Utility.GetStringAfter(HolderName, "先生");
                if (HolderName.Contains("女士")) HolderName = Utility.GetStringAfter(HolderName, "女士");
                return (HolderName, string.Empty);
            }
            var FullName = CompanyNameLogic.AfterProcessFullName(word.Value);
            if (FullName.Score == 80) return (FullName.secFullName, FullName.secShortName);
            var name = CompanyNameLogic.NormalizeCompanyName(this, FullName.secFullName);
            if (!String.IsNullOrEmpty(name.FullName) && !String.IsNullOrEmpty(name.ShortName))
            {
                return name;
            }
        }
        return (String.Empty, String.Empty);
    }


    //变动截止日期
    string GetChangeEndDate(HTMLEngine.MyRootHtmlNode root)
    {
        var Extractor = new ExtractPropertyByHTML();
        var StartArray = new string[] { "截止", "截至" };
        var EndArray = new string[] { "日" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            if (item.Value.Length > 20) continue;
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("候补变动截止日期：[" + item.Value + "]");
            return NormailizeEndChangeDate(item.Value + "日");
        }
        return String.Empty;
    }

    public string NormailizeEndChangeDate(string orgString, string keyword = "")
    {
        orgString = orgString.Replace(" ", "");
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
        return DateUtility.GetRangeDateEndDate(orgString, this.AnnouceDate, format);
    }
}