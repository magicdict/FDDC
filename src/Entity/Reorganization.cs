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
        var targets = getTargetList();
        var EvaluateMethod = getEvaluateMethod();
        var TradeCompany = getTradeCompany();
        var Price = GetPrice();
        foreach (var item in targets)
        {
            var reorgRec = new ReorganizationRec();
            reorgRec.Id = this.Id;
            reorgRec.Target = item.Target;
            reorgRec.TargetCompany = item.Comany;
            reorgRec.EvaluateMethod = EvaluateMethod;
            reorgRec.TradeCompany = TradeCompany;
            reorgRec.Price = MoneyUtility.Format(Price.MoneyAmount, String.Empty);
            list.Add(reorgRec);
        }
        return list;
    }

    /// <summary>
    /// 获得标的
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
        TradeCompany.Title = new string[] {"交易对方" }.ToList();
        TradeCompany.IsTitleEq = true;
        TradeCompany.IsRequire = true;
        var Rules = new List<TableSearchTitleRule>();
        Rules.Add(TradeCompany);
        var result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, true);
        if (result.Count != 0)
        {
            return string.Join("、", result.Select(x => x[0].RawData));
        }
        return string.Empty;
    }
    (String MoneyAmount, String MoneyCurrency) GetPrice()
    {
        var Extractor = new ExtractPropertyByHTML();
        //这些关键字后面（暂时无法自动抽取）
        Extractor.LeadingColonKeyWordList = new string[] { "作价" };
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
        if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("评估方式:" + string.Join("、", p.WordMapResult));
        return string.Join("、", p.WordMapResult);
    }
}