using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FDDC;
using JiebaNet.Segmenter.PosSeg;
using static CompanyNameLogic;
using static HTMLTable;

public class Reorganization : AnnouceDocument
{
    public override List<RecordBase> Extract()
    {
        Program.Logger.WriteLine("ID:" + Id);
        Program.Logger.Flush();

        InitTableRules();
        var list = new List<RecordBase>();
        var targets = getTargetListFromReplaceTable();
        if (targets.Count == 0) return list;
        var TradeCompany = getTradeCompany(targets);

        var EvaluateMethodLoc = LocateProperty.LocateCustomerWord(root, ReOrganizationTraning.EvaluateMethodList, "评估法");
        this.CustomerList = EvaluateMethodLoc;
        nermap.Anlayze(this);
        foreach (var item in nermap.ParagraghlocateDict)
        {
            if (item.Value.CustomerList.Count != 0 && item.Value.moneylist.Count != 0)
            {
                Console.WriteLine("评估法出现次数" + item.Value.CustomerList.Count);
                Console.WriteLine("金额出现次数" + item.Value.moneylist.Count);
            }
        }

        foreach (var item in targets)
        {
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
            foreach (var dict in ExplainDict)
            {
                var keys = dict.Key.Split(Utility.SplitChar);
                var keys2 = dict.Key.Split("/");
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

                        reorgRec.TargetCompany = tempKey + "|" + tempvalue;
                        isHit = true;
                        break;
                    }
                }
                if (isHit) break;
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
    /// 获得别名
    /// </summary>
    /// <param name="CompanyName"></param>
    /// <returns></returns>
    public string GetAnOtherNameFromExplainTable(string CompanyName)
    {
        foreach (var dict in ExplainDict)
        {
            var keys = dict.Key.Split(Utility.SplitChar);
            var keys2 = dict.Key.Split("/");
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
                if (dict.Key.Equals(CompanyName) || dict.Value.Equals(CompanyName))
                {
                    return key.Equals(CompanyName) ? dict.Value : key;
                }
            }
        }
        return string.Empty;
    }


    TableSearchTitleRule TragetCompany = new TableSearchTitleRule();
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
    List<(string Target, string Comany)> getTargetListFromReplaceTable()
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
        var ReplaceIn = ExtractFromExplainTable(CompanyAtExplainTable, ExplainInKeys);
        var ReplaceOut = ExtractFromExplainTable(CompanyAtExplainTable, ExplainOutKeys);
        var Target = ExtractFromExplainTable(CompanyAtExplainTable, ExplainKeys);
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
        Console.WriteLine("Before TrimUJ:" + OrgString);
        Console.WriteLine("After TrimUJ:" + after);
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
            foreach (var ek in ExplainKeys)
            {
                if (keys.Contains(ek))
                {
                    foreach (var value in values2)
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
    private List<(string Target, string Company)> ExtractFromExplainTable(List<struCompanyName> CompanyAtExplainTable, string[] ExplainKeys)
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
            foreach (var item in ExplainDict)
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
                SearchKey = SearchKey.Select(x => x.Trim()).ToArray();
                if (SearchKey.Contains(Rplkey))
                {
                    foreach (var targetRecordItem in values)
                    {
                        //DEBUG:
                        var SingleItemList = Utility.CutByPOSConection(targetRecordItem);
                        if (SingleItemList.Count == 2)
                        {
                            //1.家和股份  和的问题
                            //2.空格问题
                            //3.置入和置出问题
                            //4.其他奇怪的问题
                            //5.资产和负债
                            //6.所拥有的，所持有的
                            //Console.WriteLine(Id + " 分割：");
                            //Console.WriteLine(Id + " 原词：" + targetRecordItem);
                            //Console.WriteLine(Id + " 分量1：" + SingleItemList[0]);
                            //Console.WriteLine(Id + " 分量2：" + SingleItemList[1]);
                        }
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
                                foreach (var rc in CompanyAtExplainTable)
                                {
                                    var IsFullNameHit = false;
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
    /// 交易对方
    /// </summary>
    /// <returns></returns>
    public List<(string TargetCompany, string TradeCompany)> getTradeCompany(List<(string Target, string TargetCompany)> targets)
    {
        var rtn = new List<(string TargetCompany, string TradeCompany)>();
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
        var TableTrades = result.Where(z => !ExplainTableId.Contains(z[0].TableId))
                           .Select(x => x[0].RawData)
                           .Where(y => !y.Contains("不超过")).ToList();

        TragetCompany.IsRequire = true;
        Rules.Add(TragetCompany);
        result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, true);


        //释义表
        var ReplaceTrades = new List<String>();
        foreach (var item in ExplainDict)
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
            //Console.WriteLine("TOP标的：" + targets[0].TargetCompany);
            //Console.WriteLine("TOP对手：" + string.Join(Utility.SplitChar, ReplaceTrades));
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
                        //Console.WriteLine("标的：" + targetcompany);
                        //Console.WriteLine("对手：" + ctrades);
                    }
                }
            }

            if (rankdict.Count > 1)
            {
                var top = Utility.FindTop<string>(1, rankdict).First().Value;
                //Console.WriteLine("TOP标的：" + targetcompany);
                //Console.WriteLine("TOP对手：" + top);
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
                    //Console.WriteLine(targets.First().Value + ":" + moneys.First().Value.MoneyAmount);
                    return moneys.First().Value;
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
             "标的资产作价",
             "评估结果","评估价值",
             "交易价格",
             "预估值","预估作价" }.ToList();
        FinallyPrice.IsTitleEq = false;
        FinallyPrice.IsRequire = true;

        var Rules = new List<TableSearchTitleRule>();
        Rules.Add(TragetCompany);
        Rules.Add(FinallyPrice);
        var result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, false);
        foreach (var item in result)
        {
            if (string.IsNullOrEmpty(rec.TargetCompany)) continue;
            if (item[0].RawData.Contains(rec.TargetCompany))
            {
                if (PriceTable.Count == 0) PriceTable = result;
                //Console.WriteLine(Id + ":" + item[1].RawData + " @ " + item[1].Title);
                return (item[1].RawData, string.Empty);
            }
            var AnOtherName = GetAnOtherNameFromExplainTable(rec.TargetCompany);
            if (!String.IsNullOrEmpty(AnOtherName))
            {
                if (item[0].RawData.Contains(AnOtherName))
                {
                    if (PriceTable.Count == 0) PriceTable = result;
                    //Console.WriteLine(Id + ":" + item[1].RawData + " @ " + item[1].Title);
                    return (item[1].RawData, string.Empty);
                }
            }
        }

        var Price = new TableSearchTitleRule();
        Price.Name = "作价";
        Price.Title = new string[] {
            "标的资产评估值",
            "标的资产初步作价" }.ToList();
        Price.IsTitleEq = false;
        Price.IsRequire = true;

        Rules.Clear();
        Rules.Add(TragetCompany);
        Rules.Add(Price);
        result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, false);
        foreach (var item in result)
        {
            if (string.IsNullOrEmpty(rec.TargetCompany)) continue;
            if (item[0].RawData.Contains(rec.TargetCompany))
            {
                if (PriceTable.Count == 0) PriceTable = result;
                //Console.WriteLine(Id + ":" + item[1].RawData + " @ " + item[1].Title);
                return (item[1].RawData, string.Empty);

            }
            var AnOtherName = GetAnOtherNameFromExplainTable(rec.TargetCompany);
            if (!String.IsNullOrEmpty(AnOtherName))
            {
                if (item[0].RawData.Contains(AnOtherName))
                {
                    if (PriceTable.Count == 0) PriceTable = result;
                    //Console.WriteLine(Id + ":" + item[1].RawData + " @ " + item[1].Title);
                    return (item[1].RawData, string.Empty);
                }
            }
        }
        return (string.Empty, string.Empty);
    }

    /// <summary>
    /// 评估方式
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    string getEvaluateMethod(ReorganizationRec rec)
    {
        //表格法优先
        var tableEvaluateMethod = getEvaluateMethodByTable(rec);
        if (!String.IsNullOrEmpty(tableEvaluateMethod))
        {
            return tableEvaluateMethod;
        }
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
                    //Console.WriteLine(targets.First().Value + ":" + ev);
                    return evs.First().Value;
                }
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
        var result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, false);
        foreach (var item in result)
        {
            if (string.IsNullOrEmpty(rec.TargetCompany)) continue;
            if (item[0].RawData.Contains(rec.TargetCompany))
            {
                if (EvaluateTable.Count == 0) EvaluateTable = result;
                //Console.WriteLine(Id + ":" + item[1].RawData + " @ " + item[1].Title);
                return item[1].RawData;
            }
            var AnOtherName = GetAnOtherNameFromExplainTable(rec.TargetCompany);
            if (!String.IsNullOrEmpty(AnOtherName))
            {
                if (item[0].RawData.Contains(AnOtherName))
                {
                    if (EvaluateTable.Count == 0) EvaluateTable = result;
                    //Console.WriteLine(Id + ":" + item[1].RawData + " @ " + item[1].Title);
                    return item[1].RawData;
                }
            }
        }

        Rules.Clear();
        Rules.Add(TragetCompany);
        Rules.Add(EvaluateMethod);
        result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, false);
        foreach (var item in result)
        {
            if (string.IsNullOrEmpty(rec.TargetCompany)) continue;
            if (item[0].RawData.Contains(rec.TargetCompany))
            {
                if (EvaluateTable.Count == 0) EvaluateTable = result;
                //Console.WriteLine(Id + ":" + item[1].RawData + " @ " + item[1].Title);
                return item[1].RawData;
            }
            var AnOtherName = GetAnOtherNameFromExplainTable(rec.TargetCompany);
            if (!String.IsNullOrEmpty(AnOtherName))
            {
                if (item[0].RawData.Contains(AnOtherName))
                {
                    if (EvaluateTable.Count == 0) EvaluateTable = result;
                    //Console.WriteLine(Id + ":" + item[1].RawData + " @ " + item[1].Title);
                    return item[1].RawData;
                }
            }
        }
        return string.Empty;
    }

}