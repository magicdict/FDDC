using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;
using static HTMLTable;

public class Reorganization : AnnouceDocument
{
    public override List<RecordBase> Extract()
    {
        var list = new List<RecordBase>();
        var targets = getTargetListFromReplaceTable();      //优先使用指代表中的内容
        if (targets.Count == 0) return list;
        //if (targets.Count == 0) targets = getTargetList();   //文章中抽取
        var EvaluateMethod = getEvaluateMethod();
        var TradeCompany = getTradeCompany();
        foreach (var item in targets)
        {
            var reorgRec = new ReorganizationRec();
            reorgRec.Id = this.Id;
            reorgRec.Target = item.Target;
            reorgRec.TargetCompany = item.Comany;
            reorgRec.EvaluateMethod = EvaluateMethod;
            reorgRec.TradeCompany = TradeCompany;
            var Price = GetPrice(reorgRec);
            reorgRec.Price = MoneyUtility.Format(Price.MoneyAmount, String.Empty);
            list.Add(reorgRec);
        }
        return list;
    }

    /// <summary>
    /// 从指代表格中抽取
    /// </summary>
    /// <returns></returns>
    List<(string Target, string Comany)> getTargetListFromReplaceTable()
    {

        var ReplaceCompany = new List<String>();
        foreach (var c in companynamelist)
        {
            if (c.positionId == -1)
            {
                //释义表
                //if (!String.IsNullOrEmpty(c.secFullName)) ReplaceCompany.Add(c.secFullName);
                if (!String.IsNullOrEmpty(c.secShortName)) ReplaceCompany.Add(c.secShortName);
            }
        }

        var TargetAndCompanyList = new List<(string Target, string Comany)>();
        var targetRegular = new ExtractProperyBase.struRegularExpressFeature()
        {
            LeadingWordList = ReplaceCompany,
            RegularExpress = RegularTool.PercentExpress,
            TrailingWordList = new string[] { "的股权", "股权", "的权益", "权益" }.ToList()
        };
        /*         var targetLoc = ExtractPropertyByHTML.FindRegularExpressLoc(targetRegular, root);
                targetLoc.AddRange(ExtractPropertyByHTML.FindWordLoc("资产及负债", root));
                targetLoc.AddRange(ExtractPropertyByHTML.FindWordLoc("业务及相关资产负债", root));
         */
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
                "标的资产", //15%	00464
                "本次交易", //12%	00369
                "交易标的", //09%	00303
                "本次重组", //09%	00297
                "拟购买资产",      //07%	00221
                "本次重大资产重组", //07%	 00219
                "置入资产",        //03%	00107
                "本次发行",        //02%	00070
                "拟注入资产",      //02%	00068
                "目标资产"         //02%	00067
            };
            foreach (var key in keys)
            {
                if (ReplacementKeys.Contains(key))
                {
                    foreach (var value in values)
                    {
                        var targetAndcompany = value.Trim();
                        //将公司名称和交易标的划分开来
                        var ExpResult = ExtractPropertyByHTML.RegularExFinder(0, value, targetRegular, "|");
                        foreach (var r in ExpResult)
                        {
                            var arr = r.Value.Split("|");
                            var extra = (arr[0], arr[1] + arr[2]);
                            if (!TargetAndCompanyList.Contains(extra)) TargetAndCompanyList.Add(extra);
                        }
                    }
                }
            }
        }
        return TargetAndCompanyList;
    }

    /// <summary>
    /// 获得标的(不建议方法)
    /// </summary>
    /// <returns></returns>
    List<(string Target, string Comany)> getTargetList()
    {
        var rtn = new List<(string Target, string Comany)>();

        var targetRegular = new ExtractProperyBase.struRegularExpressFeature()
        {
            RegularExpress = RegularTool.PercentExpress,
            TrailingWordList = new string[] { "的股权", "股权", "的权益", "权益" }.ToList()
        };
        var targetLoc = ExtractPropertyByHTML.FindRegularExpressLoc(targetRegular, root);
        targetLoc.AddRange(ExtractPropertyByHTML.FindWordLoc("资产及负债", root));
        targetLoc.AddRange(ExtractPropertyByHTML.FindWordLoc("业务及相关资产负债", root));


        //所有公司名称
        var CompanyList = new List<string>();
        foreach (var companyname in companynamelist)
        {
            //注意，这里的companyname.WordIdx是分词之后的开始位置，不是位置信息！
            if (!CompanyList.Contains(companyname.secFullName))
            {
                if (!string.IsNullOrEmpty(companyname.secFullName)) CompanyList.Add(companyname.secFullName);
            }
            if (!CompanyList.Contains(companyname.secShortName))
            {
                if (!string.IsNullOrEmpty(companyname.secShortName)) CompanyList.Add(companyname.secShortName);
            }
        }

        var targetlist = new List<string>();

        foreach (var companyname in CompanyList)
        {
            var companyLoc = ExtractPropertyByHTML.FindWordLoc(companyname, root);
            foreach (var company in companyLoc)
            {
                foreach (var target in targetLoc)
                {
                    var EndIdx = company.StartIdx + company.Value.Length;
                    if (company.Loc == target.Loc && Math.Abs(target.StartIdx - EndIdx) < 2)
                    {
                        if (!targetlist.Contains(target.Value + ":" + company.Value))
                        {
                            rtn.Add((target.Value, company.Value));
                            targetlist.Add(target.Value + ":" + company.Value);
                        }
                    }
                }
            }
        }

        return rtn;
    }

    /// <summary>
    /// 交易对方
    /// </summary>
    /// <returns></returns>
    public string getTradeCompany()
    {
        var TradeCompany = new TableSearchTitleRule();
        TradeCompany.Name = "交易对方";
        //"投资者名称","股东名称"
        TradeCompany.Title = new string[] { "交易对方" }.ToList();
        TradeCompany.IsTitleEq = true;
        TradeCompany.IsRequire = true;
        var Rules = new List<TableSearchTitleRule>();
        Rules.Add(TradeCompany);
        var result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, true);
        if (result.Count != 0)
        {
            return string.Join(Utility.SplitChar, result.Select(x => x[0].RawData).Where(y => !y.Contains("不超过")));
        }
        return string.Empty;
    }
    (String MoneyAmount, String MoneyCurrency) GetPrice(ReorganizationRec rec)
    {
        var Extractor = new ExtractPropertyByHTML();
        //这些关键字后面（暂时无法自动抽取）
        var target = rec.TargetCompany + rec.Target;
        Extractor.LeadingColonKeyWordList = new string[] { target };
        Extractor.Extract(root);
        var AllMoneyList = new List<(String MoneyAmount, String MoneyCurrency)>();
        foreach (var item in Extractor.CandidateWord)
        {
            var moneylist = MoneyUtility.SeekMoney(item.Value);
            AllMoneyList.AddRange(moneylist);
        }
        if (AllMoneyList.Count == 0) return (String.Empty, String.Empty);
        foreach (var money in AllMoneyList)
        {
            if (money.MoneyCurrency == "人民币" ||
                money.MoneyCurrency == "元")
            {
                var amount = MoneyUtility.Format(money.MoneyAmount, String.Empty);
                var m = 0.0;
                if (double.TryParse(amount, out m))
                {
                    if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("标的资产的作价[" + money.MoneyAmount + ":" + money.MoneyCurrency + "]");
                    return money;
                }
            }
        }
        if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("标的资产的作价[" + AllMoneyList[0].MoneyAmount + ":" + AllMoneyList[0].MoneyCurrency + "]");
        return AllMoneyList[0];
    }
    /// <summary>
    /// 评估方式
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    string getEvaluateMethod()
    {
        var p = new EntityProperty();
        foreach (var method in ReOrganizationTraning.EvaluateMethodList)
        {
            p.KeyWordMap.Add(method, method);
        }
        p.Extract(this);
        if (p.WordMapResult == null) return string.Empty;
        if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("评估方式:" + string.Join(Utility.SplitChar, p.WordMapResult));
        return string.Join(Utility.SplitChar, p.WordMapResult);
    }
}