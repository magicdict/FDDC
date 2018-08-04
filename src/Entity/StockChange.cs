using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FDDC;
using static CompanyNameLogic;
using static ExtractProperyBase;
using static HTMLEngine;
using static HTMLTable;
using static LocateProperty;

public class StockChange : AnnouceDocument
{
    public static Dictionary<String, DateTime> PublishTime = new Dictionary<String, DateTime>();
    public static void ImportPublishTime()
    {
        if (!System.IO.Directory.Exists(Program.DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_public_time_20180629"))
        {
            Console.WriteLine("FDDC_announcements_round1_public_time_20180629 Not Exist");
            return;
        }
        foreach (var csvfilename in System.IO.Directory.GetFiles(Program.DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_public_time_20180629"))
        {
            if (csvfilename.EndsWith(".csv"))
            {
                var sr = new StreamReader(csvfilename);
                sr.ReadLine();  //Skip Header
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(",");
                    var numbers = RegularTool.GetNumberList(line[0]);
                    int year = int.Parse(numbers[0]);
                    int month = int.Parse(numbers[1]);
                    int day = int.Parse(numbers[2]);
                    var AnnouceDate = new DateTime(year, month, day);
                    PublishTime.Add(line[1], AnnouceDate);
                }
                sr.Close();
            }
        }
    }
    List<LocAndValue<(DateTime StartDate, DateTime EndDate)>> DateRange;

    public override List<RecordBase> Extract()
    {
        (DateTime StartDate, DateTime EndDate) ChangeDateRange;
        ChangeDateRange = (DateTime.MinValue, DateTime.MinValue);
        DateRange = LocateDateRange(root);
        var Reveiver = LocateCustomerWord(root, new string[] { "集中竞价交易", "竞价交易", "大宗交易", "约定式购回" }.ToList());
        foreach (var rec in Reveiver)
        {
            foreach (var range in DateRange)
            {
                if (rec.Loc == range.Loc)
                {
                    ChangeDateRange = range.Value;
                    Console.WriteLine(Id + ":" + range.Value.StartDate + " - " + range.Value.EndDate);
                    break;
                }
            }
            if (ChangeDateRange.StartDate != DateTime.MinValue) break;
        }

        MoneyManger();
        FivePercentStockHolder();
        SpecialFixTable();
        var list = new List<RecordBase>();
        var Name = GetHolderName();
        if (!String.IsNullOrEmpty(Name.FullName) && !String.IsNullOrEmpty(Name.ShortName))
        {
            companynamelist.Add(new struCompanyName()
            {
                secFullName = Name.FullName.Trim(),
                secShortName = Name.ShortName.Trim()
            });
        }
        list = ExtractFromTable();



        if (ChangeDateRange.StartDate != DateTime.MinValue)
        {
            foreach (StockChangeRec item in list)
            {
                if (!String.IsNullOrEmpty(item.ChangeEndDate))
                {
                    var cd = DateTime.Parse(item.ChangeEndDate);
                    if (cd < ChangeDateRange.StartDate || cd > ChangeDateRange.EndDate)
                    {
                        Console.WriteLine(Id + ":[ST]" + ChangeDateRange.StartDate.ToString("yyyy-MM-dd"));
                        Console.WriteLine(Id + ":[ED]" + ChangeDateRange.EndDate.ToString("yyyy-MM-dd"));
                        Console.WriteLine(Id + ":[CD]" + cd.ToString("yyyy-MM-dd"));
                    }
                }
            }
        }
        if (list.Count > 0) return list;    //如果这里直接返回，由于召回率等因素，可以细微提高成绩

        var stockchange = new StockChangeRec();
        //公告ID
        stockchange.Id = Id;
        //if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("公告ID:" + stockchange.id);
        stockchange.HolderFullName = Name.FullName.NormalizeTextResult();
        if (Utility.TrimEnglish(stockchange.HolderFullName).Length > 32)
        {
            stockchange.HolderFullName = String.Empty;
        }
        stockchange.HolderShortName = Name.ShortName;

        if (!String.IsNullOrEmpty(stockchange.HolderFullName))
        {
            stockchange.HolderFullName = stockchange.HolderFullName.NormalizeTextResult();
        }
        if (!String.IsNullOrEmpty(stockchange.HolderShortName))
        {
            stockchange.HolderShortName = stockchange.HolderShortName.NormalizeTextResult();
            stockchange.HolderShortName = stockchange.HolderShortName.TrimEnd(")".ToCharArray());
        }
        if (MoneyMangerList.Count == 1)
        {
            stockchange.HolderFullName = MoneyMangerList[0];
        }
        if (MoneyMangerList.Count == 2)
        {
            //考虑全称，简称可能性
            var MFull = "";
            var MShort = "";
            if (MoneyMangerList[0].Length > MoneyMangerList[1].Length)
            {
                MFull = MoneyMangerList[0];
                MShort = MoneyMangerList[1];
            }
            else
            {
                MFull = MoneyMangerList[1];
                MShort = MoneyMangerList[0];
            }
            //是否头尾一致？
            var IsMatch = false;
            if (MShort.Length >= 4)
            {
                var StartWord = MShort.Substring(0, 2);
                IsMatch = MFull.StartsWith(StartWord);
            }
            if (MoneyMangerList[1] == "信托计划")
            {
                stockchange.HolderFullName = MoneyMangerList[0];
            }
            if (IsMatch)
            {
                stockchange.HolderFullName = MFull;
                stockchange.HolderShortName = MShort;
            }
        }
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

        if (list.Count == 0)
        {
            stockchange = ExtractSingle();
            stockchange.Id = Id;
            if (!String.IsNullOrEmpty(stockchange.HolderFullName) && !String.IsNullOrEmpty(stockchange.ChangeEndDate)) list.Add(stockchange);
        }
        return list;
    }

    //资产管理计划的处理
    List<String> MoneyMangerList = new List<String>();
    public void MoneyManger()
    {
        foreach (var item in LocateProperty.LocateQuotation(root, false))
        {
            if (item.Description == "引号")
            {
                if (item.Value.EndsWith("信托计划"))
                {
                    MoneyMangerList.Add(item.Value);
                }
                var SplitCharArray = new Char[] { '－', '-' };
                if (item.Value.Split(SplitCharArray).Length > 1)
                {
                    if (item.Value.Contains("银行") || item.Value.Contains("证券"))
                    {
                        MoneyMangerList.Add(item.Value);
                    }
                }
            }
        }
        MoneyMangerList = MoneyMangerList.Distinct().ToList();
    }

    //5%以上股东
    public void FivePercentStockHolder()
    {
        var FivePercent = LocateCustomerWord(root, new string[] {
                "5%以上股东",
                "第一大股东",
                "第二大股东",
        }.ToList());
        foreach (var five in FivePercent)
        {
            if (nermap.ParagraghlocateDict.ContainsKey(five.Loc))
            {
                //寻找到5%之后的公司
                foreach (var ner in nermap.ParagraghlocateDict[five.Loc].NerList)
                {
                    if (ner.Description == "中文小括号" &&
                        ner.StartIdx > five.StartIdx &&
                        (ner.Value.Contains("简称") || ner.Value.Contains("下称")))
                    {
                        //注意这里需要过滤掉5%在书名号里面的情况
                        var isInBookMark = false;
                        foreach (var nerbookmark in nermap.ParagraghlocateDict[five.Loc].NerList)
                        {
                            if (nerbookmark.Description == "书名号")
                            {
                                if (nerbookmark.StartIdx <= five.StartIdx &&
                                    (nerbookmark.StartIdx + nerbookmark.Value.Length) >= five.StartIdx)
                                {
                                    isInBookMark = true;
                                    break;
                                }
                            }
                        }
                        if (isInBookMark) continue;
                        var Content = root.GetContentByPosId(five.Loc).Replace(" ", ""); ;
                        if (String.IsNullOrEmpty(Content)) continue;
                        var startIdx = five.StartIdx + five.Value.Length;
                        var length = ner.StartIdx - startIdx;
                        if (length <= 0) continue;
                        var CompanyFullName = Content.Substring(startIdx, length);
                        if (CompanyFullName.StartsWith("——") || CompanyFullName.StartsWith("-—"))
                        {
                            CompanyFullName = CompanyFullName.Substring(2);
                        }
                        var CompanyShortList = RegularTool.GetChineseQuotation(ner.Value);
                        if (CompanyShortList.Count > 0)
                        {
                            var CompanyShortName = CompanyShortList.First();
                            CompanyShortName = CompanyShortName.Substring(1, CompanyShortName.Length - 2);
                            //第一优先顺位使用
                            companynamelist.Insert(0, new struCompanyName()
                            {
                                secFullName = CompanyFullName,
                                secShortName = CompanyShortName
                            });
                            //Console.WriteLine("ID:" + Id);
                            //Console.WriteLine("CompanyFullName:" + CompanyFullName);
                            //Console.WriteLine("CompanyShortName:" + CompanyShortName);
                            return;
                        }
                        else
                        {
                            if (ner.Value.StartsWith("以下简称："))
                            {
                                var CompanyShortName = ner.Value.Substring(5);
                                companynamelist.Insert(0, new struCompanyName()
                                {
                                    secFullName = CompanyFullName,
                                    secShortName = CompanyShortName
                                });
                            }
                        }
                    }
                }
            }
        }
    }

    public StockChangeRec ExtractSingle()
    {
        var stockchange = new StockChangeRec();
        foreach (var nerItem in nermap.ParagraghlocateDict)
        {
            var ner = nerItem.Value;
            if (ner.datelist.Count == 1 &&
                ner.socketNumberList.Count >= 1 &&
                ner.percentList.Count >= 1)
            {
                stockchange.ChangeEndDate = ner.datelist.First().Value.ToString("yyyy-MM-dd");
                if (companynamelist.Count == 1)
                {
                    stockchange.HolderFullName = companynamelist[0].secFullName;
                    stockchange.HolderShortName = companynamelist[0].secShortName;
                }
                else
                {
                    var hn = GetHolderName();
                    stockchange.HolderFullName = hn.FullName;
                    stockchange.HolderShortName = hn.ShortName;
                }
                //寻找增持前，增持后这样的关键字
                var Keyword = LocateCustomerWord(root, new string[] { "增持后", "减持后 " }.ToList(), "关键字");
                foreach (var k in Keyword)
                {
                    if (k.Loc == nerItem.Key)
                    {
                        foreach (var p in ner.percentList)
                        {
                            if (p.StartIdx > k.StartIdx)
                            {
                                stockchange.HoldPercentAfterChange = getAfterpercent(p.Value);
                                break;
                            }
                        }
                        foreach (var p in ner.socketNumberList)
                        {
                            if (p.StartIdx > k.StartIdx)
                            {
                                stockchange.HoldNumberAfterChange = getAfterstock("", p.Value.NormalizeNumberResult());
                                break;
                            }
                        }
                    }
                }
            }
        }
        if (!String.IsNullOrEmpty(stockchange.HolderFullName))
        {
            stockchange.HolderFullName = stockchange.HolderFullName.NormalizeTextResult();
        }
        if (!String.IsNullOrEmpty(stockchange.HolderShortName))
        {
            stockchange.HolderShortName = stockchange.HolderShortName.NormalizeTextResult();
            stockchange.HolderShortName = stockchange.HolderShortName.TrimEnd(")".ToCharArray());
        }
        return stockchange;
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
                                              "减持时间", "增持时间", "减持股份时间", "增持股份时间","买入时间","卖出时间",
                                              "减持期间","增持期间" }.ToList();
        ChangeDateRule.IsTitleEq = false;
        ChangeDateRule.Normalize = NormailizeEndChangeDate;


        var ChangePriceRule = new TableSearchTitleRule();
        ChangePriceRule.Name = "变动价格";
        ChangePriceRule.Title = new string[] { "买入均价", "卖出均价", "成交均价", "成交价格", "减持价格", "增持价格", "减持股均价", "增持股均价", "减持均", "增持均", "价格区间" }.ToList();
        ChangePriceRule.IsTitleEq = false;
        ChangePriceRule.Normalize = (x, y) =>
        {
            if (x.Contains("*"))
            {
                Console.WriteLine(Id + ":* Before:" + x);
                x = x.Trim("*".ToCharArray());
                Console.WriteLine(Id + ":* After:" + x);
            }
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
        ChangeNumberRule.Title = new string[] { "成交量", "成交数量", "减持股数", "增持股数", "减持数量", "增持数量", "买入股份数", "卖出股份数", "股数" }.ToList();
        ChangeNumberRule.IsTitleEq = false;
        ChangeNumberRule.Normalize = NumberUtility.NormalizerStockNumber;

        var ChangeMoneyRule = new TableSearchTitleRule();
        ChangeMoneyRule.Name = "增持金额";
        ChangeMoneyRule.Title = new string[] { "增持金额", "减持金额" }.ToList();
        ChangeMoneyRule.IsTitleEq = false;
        ChangeMoneyRule.Normalize = (x, y) =>
        {
            if (x.Contains("*"))
            {
                Console.WriteLine(Id + ":* Before:" + x);
                x = x.Trim("*".ToCharArray());
                Console.WriteLine(Id + ":* After:" + x);
            }
            var prices = x.NormalizeNumberResult();
            return x;
        };



        var Rules = new List<TableSearchTitleRule>();
        Rules.Add(StockHolderRule);
        Rules.Add(ChangeDateRule);
        Rules.Add(ChangePriceRule);
        Rules.Add(ChangeNumberRule);
        Rules.Add(ChangeMoneyRule);
        var opt = new HTMLTable.SearchOption();
        opt.IsMeger = false;
        var result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, opt);

        if (result.Count == 0)
        {
            //没有抽取到任何数据
            Rules.Clear();
            ChangeDateRule.IsRequire = true;
            Rules.Add(ChangeDateRule);
            Rules.Add(ChangePriceRule);
            Rules.Add(ChangeNumberRule);
            result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, opt);
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
                {
                    new CellInfo() {
                        RawData = String.IsNullOrEmpty(Name.FullName)?Name.ShortName:Name.FullName
                    } , item[0], item[1], item[2] });
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
            if (!String.IsNullOrEmpty(rec[0].Title))
            {
                if (rec[0].Title.Equals("姓名"))
                {
                    if (ModifyName.EndsWith("女士") || ModifyName.EndsWith("先生"))
                    {
                        ModifyName = ModifyName.Substring(0, ModifyName.Length - 2);
                    }
                }
            }
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
            if (MoneyMangerList.Count == 1)
            {
                stockchange.HolderFullName = MoneyMangerList[0];
            }
            if (MoneyMangerList.Count == 2)
            {
                //考虑全称，简称可能性
                var MFull = "";
                var MShort = "";
                if (MoneyMangerList[0].Length > MoneyMangerList[1].Length)
                {
                    MFull = MoneyMangerList[0];
                    MShort = MoneyMangerList[1];
                }
                else
                {
                    MFull = MoneyMangerList[1];
                    MShort = MoneyMangerList[0];
                }
                //是否头尾一致？
                var IsMatch = false;
                if (MShort.Length >= 4)
                {
                    var StartWord = MShort.Substring(0, 2);
                    IsMatch = MFull.StartsWith(StartWord);
                }
                if (IsMatch)
                {
                    stockchange.HolderFullName = MFull;
                    stockchange.HolderShortName = MShort;
                }
            }
            if (MoneyMangerList.Count == 4)
            {
                //一般来说 全称 - 简称 
                if (ModifyName.Equals(MoneyMangerList[3]))
                {
                    stockchange.HolderFullName = MoneyMangerList[2];
                    stockchange.HolderShortName = MoneyMangerList[3];
                }
                if (ModifyName.Equals(MoneyMangerList[1]))
                {
                    stockchange.HolderFullName = MoneyMangerList[0];
                    stockchange.HolderShortName = MoneyMangerList[1];
                }

            }
            //资产管理特殊逻辑
            var SplitCharArray = new Char[] { '－', '-' };
            var m = stockchange.HolderFullName.Split(SplitCharArray);
            if (m.Length == 4 && m[0].Contains("管理的"))
            {
                stockchange.HolderFullName = m[1] + "-" + m[2] + "-" + m[3];
            }

            if (stockchange.HolderShortName == "公司") stockchange.HolderShortName = string.Empty;
            if (stockchange.HolderFullName.Contains("简称"))
            {
                stockchange.HolderShortName = Utility.GetStringAfter(stockchange.HolderFullName, "简称");
                stockchange.HolderShortName = stockchange.HolderShortName.Replace(")", String.Empty).Replace("“", String.Empty).Replace("”", String.Empty);
                stockchange.HolderFullName = Utility.GetStringBefore(stockchange.HolderFullName, "(");
            }
            if (stockchange.HolderFullName.Contains("下称"))
            {
                stockchange.HolderShortName = Utility.GetStringAfter(stockchange.HolderFullName, "下称");
                stockchange.HolderShortName = stockchange.HolderShortName.Replace(")", String.Empty).Replace("“", String.Empty).Replace("”", String.Empty);
                stockchange.HolderFullName = Utility.GetStringBefore(stockchange.HolderFullName, "(");
            }


            stockchange.ChangeEndDate = rec[1].RawData;
            if (stockchange.ChangeEndDate == null)
            {
                if (DateRange.Count == 1)
                {
                    //stockchange.ChangeEndDate = DateRange[0].Value.EndDate.ToString("yyyy-MM-dd");
                    //Console.WriteLine(Id + " 表格截止日确认：" + DateRange[0].Value.EndDate.ToString("yyyy-MM-dd"));
                }
                //stockchange.ChangeEndDate = GetChangeEndDate(this.root);
            }

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
            else
            {
                if (rec.Length == 5 && !String.IsNullOrEmpty(rec[4].RawData) &&
                    !String.IsNullOrEmpty(rec[3].RawData))
                {
                    double money;
                    if (double.TryParse(rec[4].RawData, out money))
                    {
                        if (rec[4].Title.Contains("元") && rec[4].Title.Contains("股"))
                        {
                            if (money < 100){
                                stockchange.ChangePrice = money.ToString();
                                Console.WriteLine(Id + "计算出来的价格:" + stockchange.ChangePrice);
                            }
                        }
                        else
                        {
                            double numer;
                            if (double.TryParse(rec[3].RawData.NormalizeNumberResult(), out numer))
                            {
                                if (numer != 0 && money != 0)
                                {
                                    double price = Math.Round(money / numer, 2);
                                    stockchange.ChangePrice = price.ToString();
                                    Console.WriteLine(Id + "计算出来的价格:" + stockchange.ChangePrice);
                                }
                            }
                        }
                    }
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
                    last.HoldNumberAfterChange = after.TotalCount;
                    last.HoldPercentAfterChange = after.TotalPercent;
                    //这里可能减持之后真的没有股票了，所以，不要和0进行比较
                    if (string.IsNullOrEmpty(last.HoldNumberAfterChange))
                    {
                        last.HoldNumberAfterChange = after.UnLimitCount;
                        last.HoldPercentAfterChange = after.UnLimitPercent;
                    }
                    if (string.IsNullOrEmpty(last.HoldNumberAfterChange))
                    {
                        last.HoldNumberAfterChange = after.LimitCount;
                        last.HoldPercentAfterChange = after.LimitPercent;
                    }
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
    class struHoldAfter
    {
        public string Name;

        public string TotalCount;

        public string TotalPercent;

        public string LimitCount;

        public string LimitPercent;

        public string UnLimitCount;

        public string UnLimitPercent;
    }
    List<struHoldAfter> GetHolderAfter()
    {
        var HoldDict = new Dictionary<String, struHoldAfter>();
        foreach (var table in root.TableList)
        {
            var mt = new HTMLTable(table.Value);
            for (int RowIdx = 0; RowIdx < mt.RowCount; RowIdx++)
            {
                for (int ColIdx = 0; ColIdx < mt.ColumnCount; ColIdx++)
                {
                    var Title = mt.CellValue(RowIdx + 1, ColIdx + 1).Replace(" ", "");
                    if (Title == "合计持有股份" || Title == "合计持股" ||
                        Title.Contains("无限售条件股份") || Title.Contains("有限售条件股份"))
                    {
                        var HolderName = mt.CellValue(RowIdx + 1, 1).NormalizeTextResult();
                        var strHolderCnt = mt.CellValue(RowIdx + 1, mt.ColumnCount - 1);
                        strHolderCnt = Normalizer.NormalizeNumberResult(strHolderCnt);
                        var title = mt.CellValue(2, 5);
                        string HolderCnt = getAfterstock(title, strHolderCnt);
                        var StrPercent = mt.CellValue(RowIdx + 1, mt.ColumnCount);
                        var HodlerPercent = getAfterpercent(StrPercent);
                        if (Title == "合计持有股份" ||
                            Title == "合计持股")
                        {
                            if (HoldDict.ContainsKey(HolderName))
                            {
                                HoldDict[HolderName].Name = HolderName;
                                HoldDict[HolderName].TotalCount = HolderCnt;
                                HoldDict[HolderName].TotalPercent = HodlerPercent;
                            }
                            else
                            {
                                HoldDict.Add(HolderName, new struHoldAfter()
                                {
                                    Name = HolderName,
                                    TotalCount = HolderCnt,
                                    TotalPercent = HodlerPercent
                                });
                            }
                        }
                        if (Title.Contains("无限售条件股份"))
                        {
                            if (HoldDict.ContainsKey(HolderName))
                            {
                                HoldDict[HolderName].Name = HolderName;
                                HoldDict[HolderName].UnLimitCount = HolderCnt;
                                HoldDict[HolderName].UnLimitPercent = HodlerPercent;
                            }
                            else
                            {
                                HoldDict.Add(HolderName, new struHoldAfter()
                                {
                                    Name = HolderName,
                                    UnLimitCount = HolderCnt,
                                    UnLimitPercent = HodlerPercent
                                });
                            }
                        }
                        if (Title.Contains("有限售条件股份"))
                        {
                            if (HoldDict.ContainsKey(HolderName))
                            {
                                HoldDict[HolderName].Name = HolderName;
                                HoldDict[HolderName].LimitCount = HolderCnt;
                                HoldDict[HolderName].LimitPercent = HodlerPercent;
                            }
                            else
                            {
                                HoldDict.Add(HolderName, new struHoldAfter()
                                {
                                    Name = HolderName,
                                    LimitCount = HolderCnt,
                                    LimitPercent = HodlerPercent
                                });
                            }
                        }
                    }
                }
            }
        }

        var HoldList = HoldDict.Values.ToList();
        if (HoldList.Count == 0)
        {
            HoldList = GetHolderAfter2ndStep();
        }
        if (HoldList.Count == 0)
        {
            HoldList = GetHolderAfter3rdStep();
        }
        if (HoldList.Count == 0)
        {
            HoldList = GetHolderAfter4thStep();
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
                            TotalCount = getAfterstock(Title4, value4),
                            TotalPercent = getAfterpercent(value5),
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

        var opt = new HTMLTable.SearchOption();
        opt.IsMeger = false;

        var result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, opt);
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
                    TotalCount = HolderCnt,
                    TotalPercent = HodlerPercent,
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
            result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, opt);
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
                        TotalCount = HolderCnt,
                        TotalPercent = HodlerPercent,
                    });
                }
            }
        }

        return HoldList;
    }


    /// <summary>
    /// 增持前之后一般为增持后，使用这个规则尝试抽取
    /// </summary>
    /// <returns></returns>
    List<struHoldAfter> GetHolderAfter4thStep()
    {
        var HoldList = new List<struHoldAfter>();
        var StockHolderRule = new TableSearchTitleRule();
        StockHolderRule.Name = "股东全称";
        StockHolderRule.Title = new string[] { "股东名称", "名称", "增持主体", "增持人", "减持主体", "减持人" }.ToList();
        StockHolderRule.IsTitleEq = true;
        StockHolderRule.IsRequire = true;

        var HoldNumberAfterChangeRule = new TableSearchTitleRule();
        HoldNumberAfterChangeRule.Name = "变动前钱持股数";
        HoldNumberAfterChangeRule.IsRequire = true;
        HoldNumberAfterChangeRule.SuperTitle = new string[] { "减持前持有股份", "增持前持有股份" }.ToList();
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

        var opt = new HTMLTable.SearchOption();
        opt.IsMeger = false;

        var result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, opt);
        if (result.Count != 0)
        {
            foreach (var item in result)
            {
                var table = new HTMLTable(root.TableList[item[0].TableId]);
                if (item[2].Column + 2 != table.ColumnCount) continue;
                var HolderName = item[0].RawData;
                var strHolderCnt = table.CellValue(item[0].Row, item[2].Column + 1);
                strHolderCnt = Normalizer.NormalizeNumberResult(strHolderCnt);
                string HolderCnt = getAfterstock(item[1].Title, strHolderCnt);
                var StrPercent = table.CellValue(item[0].Row, item[2].Column + 2);
                var HodlerPercent = getAfterpercent(StrPercent);
                HoldList.Add(new struHoldAfter()
                {
                    Name = HolderName,
                    TotalCount = HolderCnt,
                    TotalPercent = HodlerPercent,
                });
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
        var StartArray = new string[] { "接到", "收到" };
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
                if (HolderName.Contains("先生")) HolderName = Utility.GetStringBefore(HolderName, "先生");
                if (HolderName.Contains("女士")) HolderName = Utility.GetStringBefore(HolderName, "女士");
                if (HolderName.StartsWith("之一")) HolderName = Utility.GetStringAfter(HolderName, "之一");
                //防止这里出现的是一个集合
                var SplitArray = HolderName.Split(Utility.SplitChar);
                if (SplitArray.Length == 1) return (HolderName, string.Empty);
            }
            var ClearWord = word.Value;
            if (word.Value.Contains("发来的"))
            {
                ClearWord = Utility.GetStringBefore(ClearWord, "发来的");
            }
            var FullName = CompanyNameLogic.AfterProcessFullName(ClearWord);
            var name = CompanyNameLogic.NormalizeCompanyName(this, FullName.secFullName);
            if (!String.IsNullOrEmpty(name.FullName) && !String.IsNullOrEmpty(name.ShortName))
            {                //防止这里出现的是一个集合
                var SplitArray = name.FullName.Split(Utility.SplitChar);
                if (SplitArray.Length == 1) return name;
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