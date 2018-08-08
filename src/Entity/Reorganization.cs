using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FDDC;
using JiebaNet.Segmenter.PosSeg;
using static CompanyNameLogic;
using static HTMLTable;

public partial class Reorganization : AnnouceDocument
{
    public override List<RecordBase> Extract()
    {
        InitTableRules();
        GetPersonList();
        //是否在释义表中存在交易对手信息
        foreach (var item in ExplainDict)
        {
            var keys = item.Key.Split(Utility.SplitChar);
            var keys2 = item.Key.Split(new char[] { '／', '/' });
            if (keys.Length == 1 && keys2.Length > 1)
            {
                keys = keys2;
            }
            foreach (var k in keys)
            {
                if (k.Contains("交易对方")) Console.WriteLine("交易对方条目：" + k);
            }
        }
        //HTML结构
        foreach (var item in root.Children)
        {
            //var title = item.Content.Normalize().NormalizeTextResult();
            //Console.WriteLine(item.PositionId + ":" + title.Substring(0, Math.Min(20, title.Length)));
        }

        var list = new List<RecordBase>();
        var targets = getTargetListFromExplainTable().Distinct().ToList();
        if (targets.Count == 0) return list;
        var EvaluateMethodLoc = LocateProperty.LocateCustomerWord(root, ReOrganizationTraning.EvaluateMethodList, "评估法");
        this.CustomerList = EvaluateMethodLoc;
        nermap.Anlayze(this);
        foreach (var item in targets)
        {
            if (item.Target.Contains("发行")) continue;
            if (item.Target.Contains("置换")) continue;
            if (item.Target.Contains("置出")) continue;
            if (item.Target.Contains("置入")) continue;
            if (item.Target.Contains("本次")) continue;
            if (item.Target.Contains("出售")) continue;
            if (item.Target.Contains("购买")) continue;

            var reorgRec = new ReorganizationRec();
            reorgRec.Id = this.Id;
            reorgRec.Target = item.Target;
            reorgRec.TargetCompany = item.Comany.TrimEnd("合计".ToArray());
            //<1>XXXX公司的的对应
            Regex r = new Regex(@"\<(\d+)\>");
            if (r.IsMatch(reorgRec.TargetCompany))
            {
                Console.WriteLine("Before Trim:" + reorgRec.TargetCompany);
                reorgRec.TargetCompany = r.Replace(reorgRec.TargetCompany, "");
                Console.WriteLine("After  Trim:" + reorgRec.TargetCompany);
            }
            if (reorgRec.TargetCompany.Equals("本公司")) continue;
            if (reorgRec.TargetCompany.Equals("标的公司")) continue;
            //标的公司的简称填补
            foreach (var dict in ExplainDict)
            {
                var keys = dict.Key.Split(Utility.SplitChar);
                var keys2 = dict.Key.Split(new char[] { '／', '/' });
                var isHit = false;
                if (keys.Length == 1 && keys2.Length > 1)
                {
                    keys = keys2;
                }
                foreach (var key in keys)
                {
                    if (key.Contains("标的")) continue;
                    if (key.Contains("目标")) continue;
                    if (key.Equals("上市公司")) continue;
                    if (key.Equals("本公司")) continue;
                    if (key.Equals(reorgRec.TargetCompany) || dict.Value.Equals(reorgRec.TargetCompany))
                    {
                        var tempKey = key;
                        if (tempKey.Contains("，")) tempKey = Utility.GetStringBefore(tempKey, "，");

                        var tempvalue = dict.Value;
                        if (tempvalue.Contains("，")) tempvalue = Utility.GetStringBefore(tempvalue, "，");
                        reorgRec.TargetCompanyFullName = tempvalue;
                        reorgRec.TargetCompanyShortName = tempKey;
                        isHit = true;
                        break;
                    }
                }
                if (isHit) break;
            }

            var TradeCompany = getTradeCompany(reorgRec);
            reorgRec.TradeCompany = String.Join(Utility.SplitChar, TradeCompany);
            //根据各种模板规则获得的交易对手
            var xTradeList = getTradeCompanyByKeyWord(reorgRec);
            if (xTradeList.Count == 1)
            {
                var xTrade = "";
                xTrade = xTradeList.First();
                //单个结果的情况下
                if (!String.IsNullOrEmpty(xTrade))
                {
                    foreach (var dict in ExplainDict)
                    {
                        var keys = dict.Key.Split(Utility.SplitChar);
                        var keys2 = dict.Key.Split(new char[] { '／', '/' });
                        var isHit = false;
                        if (keys.Length == 1 && keys2.Length > 1)
                        {
                            keys = keys2;
                        }
                        foreach (var key in keys)
                        {
                            if (key.Contains("标的")) continue;
                            if (key.Contains("目标")) continue;
                            if (key.Equals("上市公司")) continue;
                            if (key.Equals("本公司")) continue;
                            if (key.Equals(xTrade) || dict.Value.Equals(xTrade))
                            {
                                var tempKey = key;
                                if (tempKey.Contains("，")) tempKey = Utility.GetStringBefore(tempKey, "，");

                                var tempvalue = dict.Value;
                                if (tempvalue.Contains("，")) tempvalue = Utility.GetStringBefore(tempvalue, "，");
                                reorgRec.TradeCompanyFullName = tempvalue;
                                reorgRec.TradeCompanyShortName = tempKey;
                                isHit = true;
                                break;
                            }
                        }
                        if (isHit) break;
                    }
                    reorgRec.TradeCompany = xTrade;
                    if (!String.IsNullOrEmpty(reorgRec.TradeCompanyFullName) &&
                        !String.IsNullOrEmpty(reorgRec.TradeCompanyShortName))
                    {
                        reorgRec.TradeCompany = reorgRec.TradeCompanyFullName + "|" + reorgRec.TradeCompanyShortName;
                    }
                }

            }

            if (xTradeList.Count > 1)
            {
                reorgRec.TradeCompany = String.Join(Utility.SplitChar, xTradeList);
            }

            //释义表中获得的交易对手，进行必要的订正
            if (string.IsNullOrEmpty(reorgRec.TradeCompany))
            {
                var xTradeListExplain = getTradeCompanyByExplain(reorgRec);
                foreach (var tradeItem in xTradeListExplain)
                {
                    //交易公司的简称填补
                    foreach (var dict in ExplainDict)
                    {
                        var keys = dict.Key.Split(Utility.SplitChar);
                        var keys2 = dict.Key.Split(new char[] { '／', '/' });
                        var isHit = false;
                        if (keys.Length == 1 && keys2.Length > 1)
                        {
                            keys = keys2;
                        }
                        foreach (var key in keys)
                        {
                            if (key.Contains("标的")) continue;
                            if (key.Contains("目标")) continue;
                            if (key.Equals("上市公司")) continue;
                            if (key.Equals("本公司")) continue;
                            if (key.Equals(tradeItem.Value) || dict.Value.Equals(tradeItem.Value))
                            {
                                var tempKey = key;
                                if (tempKey.Contains("，")) tempKey = Utility.GetStringBefore(tempKey, "，");

                                var tempvalue = dict.Value;
                                if (tempvalue.Contains("，")) tempvalue = Utility.GetStringBefore(tempvalue, "，");
                                reorgRec.TradeCompanyFullName = tempvalue;
                                if (!tempKey.Equals("交易对方") && !tempKey.Equals("发行对象"))
                                {
                                    reorgRec.TradeCompanyShortName = tempKey;
                                }
                                isHit = true;
                                break;
                            }
                        }
                        if (isHit) break;
                    }
                    reorgRec.TradeCompany = tradeItem.Value;
                    if (!String.IsNullOrEmpty(reorgRec.TradeCompanyFullName) &&
                        !String.IsNullOrEmpty(reorgRec.TradeCompanyShortName))
                    {
                        reorgRec.TradeCompany = reorgRec.TradeCompanyFullName + "|" + reorgRec.TradeCompanyShortName;
                    }
                    else
                    {
                        //中建六局及中建八局
                        var tradeArray = tradeItem.Value.Split(Utility.SplitChar).ToList();
                        var last = tradeArray.Last();
                        if (last.Contains("以及"))
                        {
                            tradeArray.RemoveAt(tradeArray.Count - 1);
                            tradeArray.Add(Utility.GetStringBefore(last, "以及"));
                            tradeArray.Add(Utility.GetStringAfter(last, "以及"));
                            reorgRec.TradeCompany = String.Join(Utility.SplitChar, tradeArray);
                        }
                        else
                        {
                            if (last.Contains("及"))
                            {
                                tradeArray.RemoveAt(tradeArray.Count - 1);
                                tradeArray.Add(Utility.GetStringBefore(last, "及"));
                                tradeArray.Add(Utility.GetStringAfter(last, "及"));
                                reorgRec.TradeCompany = String.Join(Utility.SplitChar, tradeArray);
                            }
                            if (last.Contains("和"))
                            {
                                tradeArray.RemoveAt(tradeArray.Count - 1);
                                tradeArray.Add(Utility.GetStringBefore(last, "和"));
                                tradeArray.Add(Utility.GetStringAfter(last, "和"));
                                reorgRec.TradeCompany = String.Join(Utility.SplitChar, tradeArray);
                            }
                        }

                    }
                    Console.WriteLine("使用释义表的交易对手：" + tradeItem.Key + ":" + reorgRec.TradeCompany);
                    break;
                }
            }

            //交易对手最后整型
            NormalizeTradeCompany(reorgRec);

            var Price = GetPrice(reorgRec, targets.Count == 1);
            reorgRec.Price = MoneyUtility.Format(Price.MoneyAmount, String.Empty);
            reorgRec.EvaluateMethod = getEvaluateMethod(reorgRec, targets.Count == 1);

            if (!String.IsNullOrEmpty(reorgRec.TargetCompanyFullName) &&
                !String.IsNullOrEmpty(reorgRec.TargetCompanyShortName))
            {
                reorgRec.TargetCompany = reorgRec.TargetCompanyFullName + "|" + reorgRec.TargetCompanyShortName;
            }

            if (String.IsNullOrEmpty(reorgRec.TargetCompany) || String.IsNullOrEmpty(reorgRec.Target)) continue;
            //相同记录合并
            var UnionKey = reorgRec.TargetCompany + reorgRec.Target;
            bool IsKeyExist = false;
            foreach (ReorganizationRec exist in list)
            {
                var existKey = exist.TargetCompany + exist.Target;
                if (UnionKey.Equals(existKey))
                {
                    IsKeyExist = true;
                    break;
                }
            }
            if (!IsKeyExist) list.Add(reorgRec);
        }

        //价格或者评估表中出现过的（以下代码这里只是检证）
        if (PriceTable.Count != 0 && EvaluateTable.Count != 0 && PriceTable.Count == EvaluateTable.Count)
        {
            if (PriceTable.Count != list.Count)
            {
                Console.WriteLine(Id);
                foreach (var item in EvaluateTable)
                {
                    Console.WriteLine("评估表：" + item[0].RawData.Replace(" ", "") + " Value:" + item[1].RawData);
                }
                foreach (var item in PriceTable)
                {
                    Console.WriteLine("价格表：" + item[0].RawData.Replace(" ", "") + " Value:" + item[1].RawData);
                }

                foreach (ReorganizationRec item in list)
                {
                    Console.WriteLine("抽出：" + item.TargetCompany + item.Target);
                }

            }
        }

        return list;
    }



    /// <summary>
    /// 标的公司
    /// </summary>
    /// <returns></returns>
    TableSearchTitleRule TragetCompany = new TableSearchTitleRule();
    /// <summary>
    /// 交易公司
    /// </summary>
    /// <returns></returns>
    TableSearchTitleRule TradeCompany = new TableSearchTitleRule();

    /// <summary>
    /// 初始化表规则
    /// </summary>
    void InitTableRules()
    {
        TragetCompany.Name = "标的公司";
        TragetCompany.Title = new string[] {
        "标的公司", "标的资产",     //26%	00115	标的资产 16%	00069	标的公司
        "拟购买资产","拟出售标的资产",
        "被投资单位名称" ,"评估目的","预估标的","评估事由","交易标的","被评估企业","股权名称","公司名称","企业名称","项目","序号"}.ToList();
        TragetCompany.IsTitleEq = true;
        TragetCompany.IsRequire = true;

        TradeCompany.Name = "交易对方";
        TradeCompany.Title = new string[] { "交易对方" }.ToList();
        TradeCompany.IsTitleEq = true;
        TradeCompany.IsRequire = true;

    }


    /// <summary>
    /// 从释义表格中抽取
    /// </summary>
    /// <returns></returns>
    List<(string Target, string Comany)> getTargetListFromExplainTable()
    {

        var CompanyAtExplainTable = new List<struCompanyName>();
        foreach (var c in companynamelist)
        {
            if (c.positionId == -1)
            {
                //释义表
                CompanyAtExplainTable.Add(c);
            }
        }
        CompanyAtExplainTable = CompanyAtExplainTable.Distinct().ToList();

        var ExplainKeys = new string[]
        {
            "交易标的",        //09%	00303
            "标的资产",        //15%	00464
            "标的公司",        //15%	00464
            "本次交易",        //12%	00369
            "本次重组",        //09%	00297
            "拟购买资产",      //07%	00221
            "本次重大资产重组", //07%	 00219
            "置入资产",        //03%	00107
            "本次发行",        //02%	00070
            "拟注入资产",      //02%	00068
            "目标资产"         //02%	00067
        };

        var ExplainInKeys = new string[]{
            "置入资产",
            "置入股权",
        };

        var ExplainOutKeys = new string[]{
            "置出资产",
            "置出股权",
        };

        var HasReplaceIn = false;
        var HasReplaceOut = false;

        foreach (var Rplkey in ExplainKeys)
        {
            foreach (var item in ExplainDict)
            {
                var keys = item.Key.Split(Utility.SplitChar);
                var keys2 = item.Key.Split(new char[] { '／', '/' });
                if (keys.Length == 1 && keys2.Length > 1)
                {
                    keys = keys2;
                }
                var values = item.Value.Split(Utility.SplitChar);
                var values2 = item.Value.Split("；");
                if (values.Length == 1 && values2.Length > 1)
                {
                    values = values2;
                }
                keys = keys.Select(x => x.Trim()).ToArray();
                if (keys.Contains(Rplkey))
                {
                    foreach (var value in values)
                    {
                        if (value.Contains("置出")) { HasReplaceOut = true; };
                        if (value.Contains("置入")) { HasReplaceIn = true; };
                    }
                }
            }
        }
        var ReplaceIn = ExtractTargetFromExplainTable(CompanyAtExplainTable, ExplainInKeys);
        var ReplaceOut = ExtractTargetFromExplainTable(CompanyAtExplainTable, ExplainOutKeys);
        var Target = ExtractTargetFromExplainTable(CompanyAtExplainTable, ExplainKeys);
        var TargetWithOutCompanyName = ExtractExtend(ExplainKeys);
        if (HasReplaceIn) Target.AddRange(ReplaceIn);
        if (HasReplaceOut) Target.AddRange(ReplaceOut);
        Target.AddRange(TargetWithOutCompanyName);
        var Clear = new List<(string Target, string Company)>();
        foreach (var item in Target)
        {
            Clear.Add((item.Target, TrimUJWords(item.Company)));
        }
        Clear = Clear.Distinct().ToList();
        return Clear;
    }

    /// <summary>
    /// 去掉动词 + 组词结构
    /// </summary>
    /// <param name="OrgString"></param>
    /// <returns></returns>
    string TrimUJWords(string OrgString)
    {
        var pos = new PosSegmenter();
        var s1 = pos.Cut(OrgString).ToList();
        var ujidx = -1;
        for (int i = 0; i < s1.Count(); i++)
        {
            if (s1[i].Flag == "uj")
            {
                if (i - 1 >= 0 && s1[i - 1].Flag == "v")
                {
                    ujidx = i;
                    break;
                }
            }
            if (s1[i].Flag == "v" && s1[i].Word.Equals("购买"))
            {
                if (i + 1 < s1.Count && s1[i + 1].Flag != "uj")
                {
                    ujidx = i;
                    break;
                }
            }
        }
        var after = "";
        if (ujidx != -1)
        {
            for (int i = ujidx + 1; i < s1.Count(); i++)
            {
                after += s1[i].Word;
            }
        }
        else
        {
            return OrgString;
        }
        //Console.WriteLine("Before TrimUJ:" + OrgString);
        //Console.WriteLine("After TrimUJ:" + after);
        return after;
    }

    private List<(string Target, string Company)> ExtractExtend(string[] ExplainKeys)
    {
        var targetRegular = new ExtractProperyBase.struRegularExpressFeature()
        {
            RegularExpress = RegularTool.PercentExpress,
            TrailingWordList = new string[] { "的股权", "股权", "的权益", "权益", "的股份", "股份" }.ToList()
        };
        var Result = new List<(string Target, string Comany)>();
        //可能性最大的排在最前
        foreach (var item in ExplainDict)
        {
            var list = new List<(string Target, string Comany)>();
            var keys = item.Key.Split(Utility.SplitChar);
            var keys2 = item.Key.Split(new char[] { '／', '/' });
            if (keys.Length == 1 && keys2.Length > 1)
            {
                keys = keys2;
            }
            var values = item.Value.Split(Utility.SplitChar);
            var values2 = item.Value.Split("；");
            if (values.Length == 1 && values2.Length > 1)
            {
                values = values2;
            }
            foreach (var ek in ExplainKeys)
            {
                if (keys.Contains(ek))
                {
                    foreach (var value in values)
                    {
                        var serachWord = value.Replace(" ", string.Empty);
                        foreach (var words in serachWord.Split(Utility.SplitChar))
                        {
                            var SingleItemList = Utility.CutByPOSConection(words);
                            foreach (var SingleItem in SingleItemList)
                            {
                                var ExpResult = ExtractPropertyByHTML.RegularExFinder(0, SingleItem, targetRegular, "|");
                                foreach (var r in ExpResult)
                                {
                                    var arr = r.Value.Split("|");
                                    var target = arr[1] + arr[2];
                                    var targetCompany = SingleItem.Substring(0, r.StartIdx);
                                    if (targetCompany.Contains("持有的")) targetCompany = Utility.GetStringAfter(targetCompany, "持有的");
                                    if (targetCompany.Contains("持有")) targetCompany = Utility.GetStringAfter(targetCompany, "持有");
                                    if (targetCompany.Contains("所持")) targetCompany = Utility.GetStringAfter(targetCompany, "所持");
                                    var extra = (target, targetCompany);
                                    list.Add(extra);
                                }
                            }
                        }
                    }
                    if (list.Count != 0) return list.Distinct().ToList();
                }
            }
        }

        return Result;
    }


    /// <summary>
    /// 从释义表抽取数据
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Comany"></param>
    /// <returns></returns>
    private List<(string Target, string Company)> ExtractTargetFromExplainTable(List<struCompanyName> CompanyAtExplainTable, string[] ExplainKeys)
    {
        var AllCompanyName = new List<String>();

        foreach (var item in CompanyAtExplainTable)
        {
            if (!String.IsNullOrEmpty(item.secShortName)) AllCompanyName.Add(item.secShortName);
            if (!String.IsNullOrEmpty(item.secFullName)) AllCompanyName.Add(item.secFullName);
        }

        //股份的抽取
        var targetRegular = new ExtractProperyBase.struRegularExpressFeature()
        {
            LeadingWordList = AllCompanyName,
            RegularExpress = RegularTool.PercentExpress,
            TrailingWordList = new string[] { "的股权", "股权", "的权益", "权益", "的股份", "股份" }.ToList()
        };


        //其他标的
        var OtherTargets = new string[] { "资产及负债", "资产和负债",
                                          "主要资产和部分负债","主要资产及部分负债",
                                          "经营性资产及负债","经营性资产和负债","应收账款和其他应收款",
                                          "负债", "债权", "全部权益","经营性资产","非股权类资产","资产、负债、业务",
                                          "直属资产", "普通股股份", "土地使用权","使用权","房产" };

        var TargetAndCompanyList = new List<(string Target, string Comany)>();
        foreach (var Rplkey in ExplainKeys)
        {
            //可能性最大的排在最前
            foreach (var ExplainDictItem in ExplainDict)
            {
                var keys = ExplainDictItem.Key.Split(Utility.SplitChar);
                var keys2 = ExplainDictItem.Key.Split(new char[] { '／', '/' });
                if (keys.Length == 1 && keys2.Length > 1)
                {
                    keys = keys2;
                }
                var values = ExplainDictItem.Value.Split(Utility.SplitChar);
                var values2 = ExplainDictItem.Value.Split("；");
                if (values.Length == 1 && values2.Length > 1)
                {
                    values = values2;
                }

                //keys里面可能包括【拟】字需要去除
                var SearchKey = keys.Select((x) => { return x.StartsWith("拟") ? x.Substring(1) : x; });
                SearchKey = SearchKey.Select(x => x.Trim()).ToArray();
                if (SearchKey.Contains(Rplkey))
                {
                    if (Rplkey.Equals("交易标的") || Rplkey.Equals("标的资产") || Rplkey.Equals("标的公司"))
                    {
                        foreach (var cn in companynamelist)
                        {
                            if (ExplainDictItem.Value.Equals(cn.secFullName) ||
                                ExplainDictItem.Value.Equals(cn.secShortName))
                            {
                                var extra = ("100%股权", ExplainDictItem.Value);
                                TargetAndCompanyList.Add(extra);
                                Console.WriteLine(Id + ":100%股权" + ExplainDictItem.Value);
                                return TargetAndCompanyList;
                            }
                        }
                    }
                    foreach (var targetRecordItem in values)
                    {
                        var SingleItemList = Utility.CutByPOSConection(targetRecordItem);
                        foreach (var SingleItem in SingleItemList)
                        {
                            var targetAndcompany = SingleItem.Trim().Replace(" ", "");
                            targetAndcompany = targetAndcompany.Trim().Replace("合计", "");
                            if (targetAndcompany.Contains("持有的")) targetAndcompany = Utility.GetStringAfter(targetAndcompany, "持有的");
                            if (targetAndcompany.Contains("持有")) targetAndcompany = Utility.GetStringAfter(targetAndcompany, "持有");
                            if (targetAndcompany.Contains("所持")) targetAndcompany = Utility.GetStringAfter(targetAndcompany, "所持");

                            //将公司名称和交易标的划分开来
                            var ExpResult = ExtractPropertyByHTML.RegularExFinder(0, targetAndcompany, targetRegular, "|");
                            if (ExpResult.Count == 0)
                            {
                                //其他类型的标的
                                if (!String.IsNullOrEmpty(GetOtherOwnerByExplainTable(targetAndcompany)))
                                {
                                    var extra = (targetAndcompany, GetOtherOwnerByExplainTable(targetAndcompany));
                                    if (!TargetAndCompanyList.Contains(extra))
                                    {
                                        TargetAndCompanyList.Add(extra);
                                    }
                                }
                                else
                                {
                                    foreach (var rc in CompanyAtExplainTable)
                                    {
                                        var IsFullNameHit = false;
                                        //资产里面可能是带有公司名字的情况
                                        if (!String.IsNullOrEmpty(rc.secFullName) && targetAndcompany.Contains(rc.secFullName))
                                        {
                                            foreach (var ot in OtherTargets)
                                            {
                                                if (targetAndcompany.Contains(ot))
                                                {
                                                    IsFullNameHit = true;
                                                    TargetAndCompanyList.Add((ot, rc.secFullName));
                                                    break;
                                                }
                                            }
                                        }
                                        if (!IsFullNameHit)
                                        {
                                            if (!String.IsNullOrEmpty(rc.secShortName) && targetAndcompany.Contains(rc.secShortName))
                                            {
                                                foreach (var ot in OtherTargets)
                                                {
                                                    if (targetAndcompany.Contains(ot))
                                                    {
                                                        IsFullNameHit = true;
                                                        TargetAndCompanyList.Add((ot, rc.secFullName));
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        //XXXX持有的XXXX的形式，不过现在可能已经不用了
                                        if (TargetAndCompanyList.Count == 0 && !String.IsNullOrEmpty(rc.secFullName) && targetAndcompany.StartsWith(rc.secFullName))
                                        {
                                            var extra = (targetAndcompany.Substring(rc.secFullName.Length), rc.secFullName);
                                            if (!TargetAndCompanyList.Contains(extra))
                                            {
                                                TargetAndCompanyList.Add(extra);
                                            }
                                            break;
                                        }
                                        if (TargetAndCompanyList.Count == 0 && !String.IsNullOrEmpty(rc.secShortName) && targetAndcompany.StartsWith(rc.secShortName))
                                        {
                                            var extra = (targetAndcompany.Substring(rc.secShortName.Length), rc.secShortName);
                                            if (!TargetAndCompanyList.Contains(extra))
                                            {
                                                TargetAndCompanyList.Add(extra);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (var r in ExpResult)
                                {
                                    var arr = r.Value.Split("|");
                                    var target = arr[1] + arr[2];
                                    var targetCompany = arr[0];
                                    if (targetCompany.Contains("持有的")) targetCompany = Utility.GetStringAfter(targetCompany, "持有的");
                                    if (targetCompany.Contains("持有")) targetCompany = Utility.GetStringAfter(targetCompany, "持有");
                                    if (targetCompany.Contains("所持")) targetCompany = Utility.GetStringAfter(targetCompany, "所持");
                                    var extra = (target.Replace(" ", ""), targetCompany.Replace(" ", ""));
                                    if (!TargetAndCompanyList.Contains(extra))
                                    {
                                        TargetAndCompanyList.Add(extra);
                                    }
                                }
                            }
                        }
                    }
                    if (TargetAndCompanyList.Count != 0) return TargetAndCompanyList;
                }
            }
        }
        return TargetAndCompanyList;
    }

    /// <summary>
    /// 其他资产
    /// </summary>
    /// <param name="targetAndcompany"></param>
    /// <returns></returns>
    public string GetOtherOwnerByExplainTable(string targetAndcompany)
    {
        foreach (var item in ExplainDict)
        {
            var keys = item.Key.Split(Utility.SplitChar);
            var keys2 = item.Key.Split(new char[] { '／', '/' });
            if (keys.Length == 1 && keys2.Length > 1)
            {
                keys = keys2;
            }
            var values = item.Value.Split(Utility.SplitChar);
            var values2 = item.Value.Split("；");
            if (values.Length == 1 && values2.Length > 1)
            {
                values = values2;
            }
            foreach (var key in keys)
            {
                if (key.Equals(targetAndcompany))
                {
                    foreach (var value in values)
                    {
                        if (value.Contains("持有的")) return Utility.GetStringBefore(value, "持有的");
                        if (value.Contains("持有")) return Utility.GetStringBefore(value, "持有");
                        if (value.Contains("所持")) return Utility.GetStringBefore(value, "所持");
                    }
                }
            }
        }
        return string.Empty;
    }


    /// <summary>
    /// 标的作价
    /// </summary>
    /// <param name="rec"></param>
    /// <returns></returns>
    (String MoneyAmount, String MoneyCurrency) GetPrice(ReorganizationRec rec, Boolean IsSingleTarget)
    {
        CustomerList = LocateProperty.LocateCustomerWord(root, new string[] { rec.TargetCompanyFullName, rec.TargetCompanyShortName }.ToList(), "标的");
        CustomerList.AddRange(LocateProperty.LocateCustomerWord(root, new string[] {
            "作价","最终评估价","交易价格"
        }.ToList(), "作价"));
        nermap.Anlayze(this);
        foreach (var nerlist in nermap.ParagraghlocateDict.Values)
        {
            //标的 作价 价格  这样的文字检索
            int TargetIdx = -1;
            int PriceIdx = -1;
            nerlist.CustomerList.Sort((x, y) => { return x.StartIdx.CompareTo(y.StartIdx); });
            foreach (var ner in nerlist.CustomerList)
            {
                if (ner.Description == "标的")
                {
                    TargetIdx = ner.StartIdx;
                }
                if (ner.Description == "作价" && TargetIdx != -1)
                {
                    PriceIdx = ner.StartIdx;
                    foreach (var item in nerlist.moneylist)
                    {
                        if (item.StartIdx > PriceIdx)
                        {
                            return item.Value;
                        }
                    }
                }
            }
        }

        //表格法优先
        var tablePrice = getPriceByTable(rec);
        if (!String.IsNullOrEmpty(tablePrice.MoneyAmount))
        {
            Double p;
            if (Double.TryParse(tablePrice.MoneyAmount, out p))
            {
                if (p < 10)
                {
                    tablePrice.MoneyAmount += "亿元";
                }
                else
                {
                    tablePrice.MoneyAmount += "万元";
                }
            }
            else
            {
                tablePrice.MoneyAmount += "万元";
            }
            return tablePrice;
        }

        if (!IsSingleTarget) return ("", "");
        //无标的关键字
        foreach (var nerlist in nermap.ParagraghlocateDict.Values)
        {
            //标的 作价 价格  这样的文字检索
            nerlist.CustomerList.Sort((x, y) => { return x.StartIdx.CompareTo(y.StartIdx); });
            foreach (var ner in nerlist.CustomerList)
            {
                if (ner.Description == "作价")
                {
                    int PriceIdx = ner.StartIdx;
                    foreach (var item in nerlist.moneylist)
                    {
                        if (item.StartIdx > PriceIdx)
                        {
                            Console.WriteLine(Id + ":作价" + item.Value.MoneyAmount);
                            return item.Value;
                        }
                    }
                }
            }
        }
        return ("", "");
    }

    List<CellInfo[]> PriceTable = new List<CellInfo[]>();

    List<CellInfo[]> EvaluateTable = new List<CellInfo[]>();

    /// <summary>
    /// 作价
    /// </summary>
    /// <param name="rec"></param>
    /// <returns></returns>
    (String MoneyAmount, String MoneyCurrency) getPriceByTable(ReorganizationRec rec)
    {
        var FinallyPrice = new TableSearchTitleRule();
        FinallyPrice.Name = "作价";
        FinallyPrice.Title = new string[] {
             "标的资产作价","评估值",
             "评估结果","评估价值",
             "交易价格" }.ToList();
        FinallyPrice.IsTitleEq = false;
        FinallyPrice.IsRequire = true;

        var Rules = new List<TableSearchTitleRule>();
        Rules.Add(TragetCompany);
        Rules.Add(FinallyPrice);
        var opt = new HTMLTable.SearchOption();
        opt.IsMeger = false;

        var result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, opt);
        foreach (var item in result)
        {
            if (string.IsNullOrEmpty(rec.TargetCompany)) continue;
            if (
                (!String.IsNullOrEmpty(rec.TargetCompanyFullName) && item[0].RawData.Contains(rec.TargetCompanyFullName)) ||
                (!String.IsNullOrEmpty(rec.TargetCompanyShortName) && item[0].RawData.Contains(rec.TargetCompanyShortName))
            )
            {
                if (PriceTable.Count == 0) PriceTable = result;
                //Console.WriteLine(Id + ":" + item[1].RawData + " @ " + item[1].Title);
                return (item[1].RawData, string.Empty);
            }
        }

        var Price = new TableSearchTitleRule();
        Price.Name = "作价";
        Price.Title = new string[] {
            "标的资产评估值",
            "标的资产初步作价",
            "预估值","预估作价" }.ToList();
        Price.IsTitleEq = false;
        Price.IsRequire = true;

        Rules.Clear();
        Rules.Add(TragetCompany);
        Rules.Add(Price);

        result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, opt);
        foreach (var item in result)
        {
            if (string.IsNullOrEmpty(rec.TargetCompany)) continue;
            if (
                (!String.IsNullOrEmpty(rec.TargetCompanyFullName) && item[0].RawData.Contains(rec.TargetCompanyFullName)) ||
                (!String.IsNullOrEmpty(rec.TargetCompanyShortName) && item[0].RawData.Contains(rec.TargetCompanyShortName))
            )
            {
                if (PriceTable.Count == 0) PriceTable = result;
                //Console.WriteLine(Id + ":" + item[1].RawData + " @ " + item[1].Title);
                return (item[1].RawData, string.Empty);

            }
        }
        return (string.Empty, string.Empty);
    }

    /// <summary>
    /// 评估方式
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    string getEvaluateMethod(ReorganizationRec rec, Boolean IsSingleTarget)
    {

        CustomerList = LocateProperty.LocateCustomerWord(root, new string[] { rec.TargetCompanyFullName, rec.TargetCompanyShortName }.ToList(), "标的");
        CustomerList.AddRange(LocateProperty.LocateCustomerWord(root, new string[] { "最终", "最后", "确定" }.ToList(), "评估"));
        CustomerList.AddRange(LocateProperty.LocateCustomerWord(root, ReOrganizationTraning.EvaluateMethodList, "评估方法"));
        nermap.Anlayze(this);
        foreach (var nerlist in nermap.ParagraghlocateDict.Values)
        {
            //标的 作价 价格  这样的文字检索
            int TargetIdx = -1;
            int EvaluateIdx = -1;
            int MethodIdx = -1;
            nerlist.CustomerList.Sort((x, y) => { return x.StartIdx.CompareTo(y.StartIdx); });
            if (IsSingleTarget) TargetIdx = 0;
            var Method = String.Empty;
            foreach (var ner in nerlist.CustomerList)
            {
                if (ner.Description == "标的")
                {
                    TargetIdx = ner.StartIdx;
                }
                if (ner.Description == "评估" && TargetIdx != -1)
                {
                    EvaluateIdx = ner.StartIdx;
                    if (MethodIdx != -1)
                    {
                        if (Math.Abs(EvaluateIdx - MethodIdx) <= 10) return Method;
                    }
                }
                if (ner.Description == "评估方法" && TargetIdx != -1)
                {
                    MethodIdx = ner.StartIdx;
                    Method = ner.Value;
                    if (EvaluateIdx != -1)
                    {
                        if (Math.Abs(EvaluateIdx - MethodIdx) <= 10) return Method;
                    }
                }
            }
        }





        //词频统计
        var MethodRank = new Dictionary<string, int>();
        foreach (var item in CustomerList)
        {
            if (item.Description == "评估方法")
            {
                if (MethodRank.ContainsKey(item.Value))
                {
                    MethodRank[item.Value]++;
                }
                else
                {
                    MethodRank.Add(item.Value, 1);
                }
            }
        }
        if (MethodRank.Count > 0 && IsSingleTarget)
        {
            //出现次数最多的胜出
            return Utility.FindTop(1, MethodRank).First().Value;
        }
        else
        {
            if (MethodRank.Count != 0 && MethodRank.Sum(x => x.Value) <= 5)
            {
                //少数派
                Console.WriteLine("少数派:" + Id);
                return Utility.FindTop(1, MethodRank).First().Value;
            }
        }

        //表格法
        var tableEvaluateMethod = getEvaluateMethodByTable(rec);
        if (!String.IsNullOrEmpty(tableEvaluateMethod))
        {
            return tableEvaluateMethod;
        }

        //无标的评估
        foreach (var nerlist in nermap.ParagraghlocateDict.Values)
        {
            //标的 作价 价格  这样的文字检索
            int TargetIdx = -1;
            int MethodIdx = -1;
            nerlist.CustomerList.Sort((x, y) => { return x.StartIdx.CompareTo(y.StartIdx); });
            if (IsSingleTarget) TargetIdx = 0;
            var Method = String.Empty;
            foreach (var ner in nerlist.CustomerList)
            {
                if (ner.Description == "标的")
                {
                    TargetIdx = ner.StartIdx;
                }
                if (ner.Description == "评估方法" && TargetIdx != -1)
                {
                    MethodIdx = ner.StartIdx;
                    Method = ner.Value;
                }
            }
            if (!String.IsNullOrEmpty(Method))
            {
                Console.WriteLine("评估方法：" + Method);
                return Method;
            }
        }

        return string.Empty;
    }



    /// <summary>
    /// 评估方法
    /// </summary>
    /// <param name="rec"></param>
    /// <returns></returns>
    string getEvaluateMethodByTable(ReorganizationRec rec)
    {
        var EvaluateMethod = new TableSearchTitleRule();
        EvaluateMethod.Name = "评估方法";
        EvaluateMethod.Title = new string[] {
            "预估方法","预估方式",
            "评估方法","评估方式",
            "定价方法","定价方式",
            "预估结论方法","预估结论方式",
            "预估值采用评估方法","预估值采用评估方式" }.ToList();
        EvaluateMethod.IsTitleEq = true;
        EvaluateMethod.IsRequire = true;

        var FinallyEvaluateMethod = new TableSearchTitleRule();
        FinallyEvaluateMethod.Name = "评估方法";
        FinallyEvaluateMethod.Title = new string[] {
             "作为评估结论","评估结论选取方法",
             "选定评估方法","选定评估方式",
             "最终选取的评估方法", "最终选取的评估方式",   //方式，方法
             "最终使用的评估方法",  "最终使用的评估方式",
             "最终评估结果使用方法", "最终评估结果使用方式" }.ToList();
        FinallyEvaluateMethod.IsTitleEq = true;
        FinallyEvaluateMethod.IsRequire = true;

        var Rules = new List<TableSearchTitleRule>();
        Rules.Add(TragetCompany);
        Rules.Add(FinallyEvaluateMethod);

        var opt = new HTMLTable.SearchOption();
        opt.IsMeger = false;

        var result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, opt);
        foreach (var item in result)
        {
            if (string.IsNullOrEmpty(rec.TargetCompany)) continue;
            if (
                (!String.IsNullOrEmpty(rec.TargetCompanyFullName) && item[0].RawData.Contains(rec.TargetCompanyFullName)) ||
                (!String.IsNullOrEmpty(rec.TargetCompanyShortName) && item[0].RawData.Contains(rec.TargetCompanyShortName))
            )
            {
                if (EvaluateTable.Count == 0) EvaluateTable = result;
                //Console.WriteLine(Id + ":" + item[1].RawData + " @ " + item[1].Title);
                return item[1].RawData;
            }
        }

        Rules.Clear();
        Rules.Add(TragetCompany);
        Rules.Add(EvaluateMethod);
        result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, opt);
        foreach (var item in result)
        {
            if (string.IsNullOrEmpty(rec.TargetCompany)) continue;
            if (
                (!String.IsNullOrEmpty(rec.TargetCompanyFullName) && item[0].RawData.Contains(rec.TargetCompanyFullName)) ||
                (!String.IsNullOrEmpty(rec.TargetCompanyShortName) && item[0].RawData.Contains(rec.TargetCompanyShortName))
            )
            {
                if (EvaluateTable.Count == 0) EvaluateTable = result;
                //Console.WriteLine(Id + ":" + item[1].RawData + " @ " + item[1].Title);
                return item[1].RawData;
            }
        }
        return string.Empty;
    }

}