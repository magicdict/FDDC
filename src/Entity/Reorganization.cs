using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;
using static CompanyNameLogic;
using static HTMLTable;

public class Reorganization : AnnouceDocument
{
    public override List<RecordBase> Extract()
    {
        var list = new List<RecordBase>();
        var targets = getTargetListFromReplaceTable();
        if (targets.Count == 0) return list;
        var TradeCompany = getTradeCompany(targets);
        foreach (var item in targets)
        {
            var reorgRec = new ReorganizationRec();
            reorgRec.Id = this.Id;
            reorgRec.Target = item.Target;
            reorgRec.TargetCompany = item.Comany;
            foreach (var tc in TradeCompany)
            {
                if (tc.TargetCompany == item.Comany)
                {
                    reorgRec.TradeCompany = tc.TradeCompany;
                    break;
                }
            }
            var Price = GetPrice(reorgRec);
            reorgRec.Price = MoneyUtility.Format(Price.MoneyAmount, String.Empty);
            reorgRec.EvaluateMethod = getEvaluateMethod(reorgRec);

            //最后才能进行 多选配置！！！
            foreach (var dict in ReplacementDict)
            {
                if (dict.Key.Equals(reorgRec.TargetCompany) || dict.Value.Equals(reorgRec.TargetCompany))
                {
                    reorgRec.TargetCompany = dict.Key + "|" + dict.Value;
                }
            }

            list.Add(reorgRec);
        }
        return list;
    }

    /// <summary>
    /// 从释义表格中抽取
    /// </summary>
    /// <returns></returns>
    List<(string Target, string Comany)> getTargetListFromReplaceTable()
    {

        var ReplaceCompany = new List<struCompanyName>();
        foreach (var c in companynamelist)
        {
            if (c.positionId == -1)
            {
                //释义表
                ReplaceCompany.Add(c);
            }
        }
        ReplaceCompany = ReplaceCompany.Distinct().ToList();
        var ReplacementKeys = new string[]
        {
            "交易标的",        //09%	00303
            "标的资产",        //15%	00464
            "本次交易",        //12%	00369
            "本次重组",        //09%	00297
            "拟购买资产",      //07%	00221
            "本次重大资产重组", //07%	 00219
            "置入资产",        //03%	00107
            "本次发行",        //02%	00070
            "拟注入资产",      //02%	00068
            "目标资产"         //02%	00067
        };

        var ReplaceInKeys = new string[]{
            "置入资产",
            "置入股权",
        };

        var ReplaceOutKeys = new string[]{
            "置出资产",
            "置出股权",
        };

        var HasReplaceIn = false;
        var HasReplaceOut = false;

        foreach (var Rplkey in ReplacementKeys)
        {
            foreach (var item in ReplacementDict)
            {
                var keys = item.Key.Split(Utility.SplitChar);
                var keys2 = item.Key.Split("/");
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
        var ReplaceIn = ExtractFromExplainTable(ReplaceCompany, ReplaceInKeys);
        var ReplaceOut = ExtractFromExplainTable(ReplaceCompany, ReplaceOutKeys);
        var Target = ExtractFromExplainTable(ReplaceCompany, ReplacementKeys);
        if (HasReplaceIn) Target.AddRange(ReplaceIn);
        if (HasReplaceOut) Target.AddRange(ReplaceOut);
        return Target.Distinct().ToList();
    }

    private List<(string Target, string Comany)> ExtractFromExplainTable(List<struCompanyName> ReplaceCompany, string[] ReplacementKeys)
    {
        var AllCompanyName = new List<String>();

        foreach (var item in ReplaceCompany)
        {
            if (!String.IsNullOrEmpty(item.secShortName)) AllCompanyName.Add(item.secShortName);
            if (!String.IsNullOrEmpty(item.secFullName)) AllCompanyName.Add(item.secFullName);
        }

        //股份的抽取
        var targetRegular = new ExtractProperyBase.struRegularExpressFeature()
        {
            LeadingWordList = AllCompanyName,
            RegularExpress = RegularTool.PercentExpress,
            TrailingWordList = new string[] { "的股权", "股权", "的权益", "权益" }.ToList()
        };


        var OtherTargets = new string[] { "资产及负债" };

        var TargetAndCompanyList = new List<(string Target, string Comany)>();
        foreach (var Rplkey in ReplacementKeys)
        {
            //可能性最大的排在最前
            foreach (var item in ReplacementDict)
            {
                var keys = item.Key.Split(Utility.SplitChar);
                var keys2 = item.Key.Split("/");
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

                //keys里面可能包括【拟】字需要去除
                var SearchKey = keys.Select((x) => { return x.StartsWith("拟") ? x.Substring(1) : x; });

                if (SearchKey.Contains(Rplkey))
                {
                    foreach (var value in values)
                    {
                        var targetAndcompany = value.Trim().Replace(" ", "");
                        //将公司名称和交易标的划分开来
                        var ExpResult = ExtractPropertyByHTML.RegularExFinder(0, targetAndcompany, targetRegular, "|");
                        if (ExpResult.Count == 0)
                        {
                            //其他类型的标的
                            foreach (var rc in ReplaceCompany)
                            {
                                var IsFullNameHit = false;
                                if (!String.IsNullOrEmpty(rc.secFullName) && targetAndcompany.Contains(rc.secFullName))
                                {
                                    foreach (var ot in OtherTargets)
                                    {
                                        if (targetAndcompany.Contains(ot))
                                        {
                                            IsFullNameHit = true;
                                            TargetAndCompanyList.Add((rc.secFullName, ot));
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
                                                TargetAndCompanyList.Add((rc.secShortName, ot));
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (TargetAndCompanyList.Count == 0 && !String.IsNullOrEmpty(rc.secFullName) && targetAndcompany.StartsWith(rc.secFullName))
                                {
                                    var extra = (value.Substring(rc.secFullName.Length), rc.secFullName);
                                    if (!TargetAndCompanyList.Contains(extra))
                                    {
                                        TargetAndCompanyList.Add(extra);
                                    }
                                    break;
                                }
                                if (TargetAndCompanyList.Count == 0 && !String.IsNullOrEmpty(rc.secShortName) && targetAndcompany.StartsWith(rc.secShortName))
                                {
                                    var extra = (value.Substring(rc.secShortName.Length), rc.secShortName);
                                    if (!TargetAndCompanyList.Contains(extra))
                                    {
                                        TargetAndCompanyList.Add(extra);
                                    }
                                    break;
                                }

                            }
                        }
                        else
                        {
                            foreach (var r in ExpResult)
                            {
                                var arr = r.Value.Split("|");
                                var extra = (arr[1] + arr[2], arr[0]);
                                if (!TargetAndCompanyList.Contains(extra))
                                {
                                    TargetAndCompanyList.Add(extra);
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
    /// 交易对方
    /// </summary>
    /// <returns></returns>
    public List<(string TargetCompany, string TradeCompany)> getTradeCompany(List<(string Target, string TargetCompany)> targets)
    {
        var rtn = new List<(string TargetCompany, string TradeCompany)>();
        var TradeCompany = new TableSearchTitleRule();
        TradeCompany.Name = "交易对方";
        TradeCompany.Title = new string[] { "交易对方" }.ToList();
        TradeCompany.IsTitleEq = true;
        TradeCompany.IsRequire = true;
        var Rules = new List<TableSearchTitleRule>();
        Rules.Add(TradeCompany);
        var result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, true);
        if (result.Count == 0) return rtn;
        //首页表格提取出交易者列表
        var tableid = result[0][0].TableId;
        //注意：由于表格检索的问题，这里只将第一个表格的内容作为依据
        //交易对方是释义表的一个项目，这里被错误识别为表头
        //TODO:这里交易对方应该只选取文章前部的表格！！
        var TableTrades = result.Where(z => !ReplaceTableId.Contains(z[0].TableId))
                           .Select(x => x[0].RawData)
                           .Where(y => !y.Contains("不超过")).ToList();

        var TragetCompany = new TableSearchTitleRule();
        TragetCompany.Name = "标的公司";
        TragetCompany.Title = new string[] { "标的公司" }.ToList();
        TragetCompany.IsTitleEq = true;
        TragetCompany.IsRequire = true;
        Rules.Add(TragetCompany);
        result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, true);


        //释义表
        var ReplaceTrades = new List<String>();
        foreach (var item in ReplacementDict)
        {
            var keys = item.Key.Split(Utility.SplitChar);
            var keys2 = item.Key.Split("/");
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
            var ReplacementKeys = new string[]
            {
                "交易对方",
            };
            foreach (var key in keys)
            {
                if (ReplacementKeys.Contains(key))
                {
                    foreach (var value in values)
                    {
                        var trade = value.Replace("自然人", "");
                        if (!ReplaceTrades.Contains(trade))
                        {
                            ReplaceTrades.Add(trade);
                        }
                    }
                }
            }
        }

        if (targets.Count == 1 && ReplaceTrades.Count > 0)
        {
            //单标有交易对手的情况
            rtn.Add((targets[0].TargetCompany, string.Join(Utility.SplitChar, ReplaceTrades)));
            Console.WriteLine("TOP标的：" + targets[0].TargetCompany);
            Console.WriteLine("TOP对手：" + string.Join(Utility.SplitChar, ReplaceTrades));
            return rtn;
        }

        var trades = TableTrades;
        trades.AddRange(ReplaceTrades);

        //在全文中寻找交易对象出现的地方
        var traderLoc = LocateProperty.LocateCustomerWord(root, trades);
        var targetLoc = LocateProperty.LocateCustomerWord(root, targets.Select(x => x.TargetCompany).ToList());
        var TradeLocMap = new Dictionary<int, List<LocateProperty.LocAndValue<String>>>();
        var TargetLocMap = new Dictionary<int, List<LocateProperty.LocAndValue<String>>>();
        //按照段落为依据，进行分组
        foreach (var trloc in traderLoc)
        {
            if (!TradeLocMap.ContainsKey(trloc.Loc))
            {
                TradeLocMap.Add(trloc.Loc, new List<LocateProperty.LocAndValue<String>>());
            }
            TradeLocMap[trloc.Loc].Add(trloc);
        }
        foreach (var trloc in targetLoc)
        {
            if (!TargetLocMap.ContainsKey(trloc.Loc))
            {
                TargetLocMap.Add(trloc.Loc, new List<LocateProperty.LocAndValue<String>>());
            }
            TargetLocMap[trloc.Loc].Add(trloc);
        }

        var ctradesPlace = String.Join(Utility.SplitChar, ReplaceTrades);

        foreach (var t in targets)
        {
            var targetcompany = t.TargetCompany;
            var rankdict = new Dictionary<String, int>();
            foreach (var loc in TargetLocMap.Keys)
            {
                //寻找交集
                if (TradeLocMap.ContainsKey(loc))
                {
                    if (TargetLocMap[loc].Count == 1 && TradeLocMap[loc].Count > 1)
                    {
                        //单标的，多人物的
                        var comp = String.Join(Utility.SplitChar, TargetLocMap[loc].Select(x => x.Value));
                        if (!comp.Equals(targetcompany)) continue;
                        var ctrades = String.Join(Utility.SplitChar, TradeLocMap[loc].Select(x => x.Value).Distinct());
                        if (rankdict.ContainsKey(ctrades))
                        {
                            //如果是释义表中的，则+10
                            if (ctradesPlace.Equals(ctrades))
                            {
                                rankdict[ctrades] += 10;
                            }
                            else
                            {
                                rankdict[ctrades]++;
                            }
                        }
                        else
                        {
                            if (ctradesPlace.Equals(ctrades))
                            {
                                rankdict.Add(ctrades, 10);
                            }
                            else
                            {
                                rankdict.Add(ctrades, 1);
                            }

                        }
                        Console.WriteLine("标的：" + targetcompany);
                        Console.WriteLine("对手：" + ctrades);
                    }
                }
            }

            if (rankdict.Count > 1)
            {
                var top = Utility.FindTop<string>(1, rankdict).First().Value;
                Console.WriteLine("TOP标的：" + targetcompany);
                Console.WriteLine("TOP对手：" + top);
                rtn.Add((targetcompany, top));
            }
        }


        return rtn;
    }
    /// <summary>
    /// 标的作价
    /// </summary>
    /// <param name="rec"></param>
    /// <returns></returns>
    (String MoneyAmount, String MoneyCurrency) GetPrice(ReorganizationRec rec)
    {
        var targetLoc = LocateProperty.LocateCustomerWord(root, new string[] { rec.TargetCompany + rec.Target }.ToList());
        //按照段落为依据，进行分组
        var MoneyLocMap = new Dictionary<int, List<LocateProperty.LocAndValue<(String MoneyAmount, String MoneyCurrency)>>>();
        var TargetLocMap = new Dictionary<int, List<LocateProperty.LocAndValue<String>>>();
        foreach (var moneyloc in moneylist)
        {
            if (!MoneyLocMap.ContainsKey(moneyloc.Loc))
            {
                MoneyLocMap.Add(moneyloc.Loc, new List<LocateProperty.LocAndValue<(String MoneyAmount, String MoneyCurrency)>>());
            }
            MoneyLocMap[moneyloc.Loc].Add(moneyloc);
        }
        foreach (var trloc in targetLoc)
        {
            if (!TargetLocMap.ContainsKey(trloc.Loc))
            {
                TargetLocMap.Add(trloc.Loc, new List<LocateProperty.LocAndValue<String>>());
            }
            TargetLocMap[trloc.Loc].Add(trloc);
        }
        foreach (var targetloc in TargetLocMap)
        {
            if (MoneyLocMap.ContainsKey(targetloc.Key))
            {
                //寻找标的之后的金额：
                var targets = targetloc.Value;
                var moneys = MoneyLocMap[targetloc.Key];
                if (targets.Count == 1 && moneys.Count == 1)
                {
                    Console.WriteLine(targets.First().Value + ":" + moneys.First().Value.MoneyAmount);
                    return moneys.First().Value;
                }
            }
        }
        return ("", "");
    }
    /// <summary>
    /// 评估方式
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    string getEvaluateMethod(ReorganizationRec rec)
    {
        var evaluateLoc = LocateProperty.LocateCustomerWord(root, ReOrganizationTraning.EvaluateMethodList);
        if (evaluateLoc.Count == 0) return string.Empty;
        var targetLoc = LocateProperty.LocateCustomerWord(root, new string[] { rec.TargetCompany + rec.Target }.ToList());
        //按照段落为依据，进行分组
        var EvaluteLocMap = new Dictionary<int, List<LocateProperty.LocAndValue<String>>>();
        var TargetLocMap = new Dictionary<int, List<LocateProperty.LocAndValue<String>>>();
        foreach (var evaluateloc in evaluateLoc)
        {
            if (!EvaluteLocMap.ContainsKey(evaluateloc.Loc))
            {
                EvaluteLocMap.Add(evaluateloc.Loc, new List<LocateProperty.LocAndValue<String>>());
            }
            EvaluteLocMap[evaluateloc.Loc].Add(evaluateloc);
        }
        foreach (var trloc in targetLoc)
        {
            if (!TargetLocMap.ContainsKey(trloc.Loc))
            {
                TargetLocMap.Add(trloc.Loc, new List<LocateProperty.LocAndValue<String>>());
            }
            TargetLocMap[trloc.Loc].Add(trloc);
        }
        foreach (var targetloc in TargetLocMap)
        {
            if (EvaluteLocMap.ContainsKey(targetloc.Key))
            {
                //寻找标的之后的评估法：
                var targets = targetloc.Value;
                var evs = EvaluteLocMap[targetloc.Key];
                if (targets.Count == 1)
                {
                    var ev = string.Join(Utility.SplitChar, evs.Select(x => x.Value));
                    Console.WriteLine(targets.First().Value + ":" + ev);
                    return evs.First().Value;
                }
            }
        }
        return string.Empty;
    }
}