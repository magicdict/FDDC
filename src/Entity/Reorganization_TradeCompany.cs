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

    /// <summary>
    /// 人名列表
    /// </summary>
    /// <typeparam name="String"></typeparam>
    /// <returns></returns>
    List<String> PersonList = new List<String>();

    void GetPersonList()
    {
        //交易对象
        var rtn = new List<(string TargetCompany, string TradeCompany)>();
        TradeCompany.IsRequire = true;
        var Rules = new List<TableSearchTitleRule>();
        Rules.Add(TradeCompany);
        var opt = new HTMLTable.SearchOption();
        opt.IsMeger = true;
        opt.IsContainTotalRow = true;
        var result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, opt);
        if (result.Count != 0)
        {
            //首页表格提取出交易者列表
            var tableid = result[0][0].TableId;
            //注意：由于表格检索的问题，这里只将第一个表格的内容作为依据
            //交易对方是释义表的一个项目，这里被错误识别为表头
            //TODO:这里交易对方应该只选取文章前部的表格！！
            var TableTrades = result.Where(z => !ExplainTableId.Contains(z[0].TableId))
                               .Select(x => x[0].RawData)
                               .Where(y => !y.Contains("不超过")).ToList();
            PersonList.AddRange(TableTrades);
        }

        foreach (var e in ExplainDict)
        {
            if (e.Value.Contains("自然人"))
            {
                var PersonArray = e.Value.Split(Utility.SplitChar);
                foreach (var person in PersonArray)
                {
                    if (person.Contains("等") || person.Contains("自然人"))
                    {
                        var trimPerson = person;
                        if (trimPerson.Contains("等"))
                        {
                            trimPerson = Utility.GetStringBefore(trimPerson, "等");
                        }
                        if (trimPerson.Contains("自然人"))
                        {
                            trimPerson = Utility.GetStringBefore(trimPerson, "自然人");
                        }
                        PersonList.Add(trimPerson);
                    }
                    else
                    {
                        PersonList.Add(person);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 交易对方
    /// </summary>
    /// <returns></returns>
    public List<string> getTradeCompany(ReorganizationRec target)
    {
        var rtn = new List<string>();
        TradeCompany.IsRequire = true;
        var Rules = new List<TableSearchTitleRule>();
        Rules.Add(TradeCompany);
        var opt = new HTMLTable.SearchOption();
        opt.IsMeger = true;
        var result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, opt);
        if (result.Count == 0) return rtn;
        //首页表格提取出交易者列表
        var tableid = result[0][0].TableId;
        //注意：由于表格检索的问题，这里只将第一个表格的内容作为依据
        //交易对方是释义表的一个项目，这里被错误识别为表头
        //TODO:这里交易对方应该只选取文章前部的表格！！
        var TableTrades = result.Where(z => !ExplainTableId.Contains(z[0].TableId))
                           .Select(x => x[0].RawData)
                           .Where(y => !y.Contains("不超过")).ToList();
        var TargetLoc = LocateProperty.LocateCustomerWord(root, new string[] { target.TargetCompanyFullName, target.TargetCompanyShortName }.ToList(), "标的");
        var HolderLoc = LocateProperty.LocateCustomerWord(root, new string[] { "持有", "所持" }.ToList(), "持有");
        var OwnerLoc = LocateProperty.LocateCustomerWord(root, TableTrades.ToList(), "交易对手");
        CustomerList.AddRange(TargetLoc);
        CustomerList.AddRange(HolderLoc);
        CustomerList.AddRange(OwnerLoc);
        nermap.Anlayze(this);
        foreach (var nerlist in nermap.ParagraghlocateDict.Values)
        {
            //交易对手 持有 标的 这样的文字检索
            int OwnerIdx = -1;
            int HolderIdx = -1;
            int TargetIdx = -1;
            nerlist.CustomerList.Sort((x, y) => { return x.StartIdx.CompareTo(y.StartIdx); });
            var OwnerName = string.Empty;
            foreach (var ner in nerlist.CustomerList)
            {
                if (ner.Description == "交易对手")
                {
                    OwnerIdx = ner.StartIdx;
                    OwnerName = ner.Value;
                }
                if (ner.Description == "持有" && OwnerIdx != -1)
                {
                    HolderIdx = ner.StartIdx;
                }
                if (ner.Description == "标的" && OwnerIdx != -1 && HolderIdx != -1)
                {
                    TargetIdx = ner.StartIdx;
                }
                if (OwnerIdx != -1 && HolderIdx != -1 && TargetIdx != -1)
                {
                    if (TargetIdx - OwnerIdx < 20)
                    {
                        rtn.Add(OwnerName);
                    }
                    OwnerIdx = -1;
                    HolderIdx = -1;
                    TargetIdx = -1;
                }

            }
        }
        return rtn.Distinct().ToList();
    }
    /// <summary>
    /// 通过释义表里的关键字获得交易对手情况
    /// </summary>
    /// <param name="rec"></param>
    /// <returns></returns>
    public Dictionary<string, string> getTradeCompanyByExplain(ReorganizationRec rec)
    {
        //释义表
        var ExplainTrades = new Dictionary<string, string>();
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
                if (!String.IsNullOrEmpty(rec.TargetCompanyFullName) &&
                    key.Equals(rec.TargetCompanyFullName + "交易对方"))
                {
                    if (!ExplainTrades.ContainsKey(rec.TargetCompanyFullName)) ExplainTrades.Add(rec.TargetCompanyFullName, item.Value);
                }
                if (!String.IsNullOrEmpty(rec.TargetCompanyShortName) &&
                    key.Equals(rec.TargetCompanyShortName + "交易对方"))
                {
                    if (!ExplainTrades.ContainsKey(rec.TargetCompanyShortName)) ExplainTrades.Add(rec.TargetCompanyShortName, item.Value);
                }
                if (key.Equals("交易对方"))
                {
                    if (!ExplainTrades.ContainsKey("交易对方")) ExplainTrades.Add("交易对方", item.Value);
                }
            }
        }
        return ExplainTrades;
    }


    public List<String> getTradeCompanyByKeyWord(ReorganizationRec rec)
    {

        var Rtn = new List<String>();

        //在释义表中寻找 持有的，持有者就是交易对手
        //广传媒持有的广报经营 100%股权、大洋传媒 100%股权及新媒体公司 100%股权
        //并向国信集团非公开发行股份购买其持有的江苏信托81.49%的股权
        //重庆百货向商社集团和新天域湖景发行股份购买其持有的新世纪百货61%和39%的股权
        //检查一下交易标的公司的位置，持有的位置，持有之前公司的位置
        foreach (var ExplainSentence in ExplainDict.Values)
        {
            var DetailList = ExplainSentence.Split("；");
            foreach (var DetailItem in DetailList)
            {
                var SingleSentenceList = new string[] { DetailItem };
                if (DetailItem.Contains(",")) SingleSentenceList = DetailItem.Split(",");
                if (DetailItem.Contains("，")) SingleSentenceList = DetailItem.Split("，");
                var PreviewSentence = "";
                foreach (var SingleSentenceItem in SingleSentenceList)
                {
                    //购买的 -> 购买
                    var SingleSentence = SingleSentenceItem;
                    SingleSentence = SingleSentence.Replace("购买的", "购买");
                    var HoldVerbIdxPlus = SingleSentence.IndexOf("所持有");
                    var HoldVerbIdx = SingleSentence.IndexOf("持有");
                    if (HoldVerbIdx == -1)
                    {
                        PreviewSentence = SingleSentence;
                        continue;
                    }
                    if (HoldVerbIdxPlus != -1) HoldVerbIdx = HoldVerbIdxPlus;
                    var targetIdx = -1;
                    if (!String.IsNullOrEmpty(rec.TargetCompanyShortName)) targetIdx = SingleSentence.IndexOf(rec.TargetCompanyFullName);
                    if (targetIdx == -1)
                    {
                        if (!String.IsNullOrEmpty(rec.TargetCompanyShortName)) targetIdx = SingleSentence.IndexOf(rec.TargetCompanyShortName);
                    }
                    if (targetIdx == -1)
                    {
                        PreviewSentence = SingleSentence;
                        continue;
                    }

                    var BuyIdx = SingleSentence.IndexOf("购买");

                    var BetweenBuyAndHoldString = "";
                    if (BuyIdx != -1 && BuyIdx < HoldVerbIdx)
                    {
                        BetweenBuyAndHoldString = SingleSentence.Substring(BuyIdx + 2, HoldVerbIdx - BuyIdx - 2);
                    }
                    //Console.WriteLine("公告ID：" + Id);
                    //Console.WriteLine("原始句子：" + SingleSentence);
                    //Console.WriteLine("持有的位置：" + HoldVerbIdx);
                    //Console.WriteLine("标的公司全称：" + rec.TargetCompanyFullName);
                    //Console.WriteLine("标的公司简称：" + rec.TargetCompanyShortName);
                    //Console.WriteLine("标的公司位置：" + targetIdx);
                    if (!String.IsNullOrEmpty(BetweenBuyAndHoldString))
                    {
                        //Console.WriteLine("购买的位置：" + BuyIdx);
                        //不为空
                        if (!BetweenBuyAndHoldString.Contains("交易"))
                        {
                            //Console.WriteLine("购买...持有之间的内容：" + BetweenBuyAndHoldString);
                            //不是交易对方,交易对手等字
                            if (BetweenBuyAndHoldString.EndsWith("全体股东")) BetweenBuyAndHoldString = BetweenBuyAndHoldString.Substring(0, BetweenBuyAndHoldString.Length - 4);
                            if (BetweenBuyAndHoldString.EndsWith("股东")) BetweenBuyAndHoldString = BetweenBuyAndHoldString.Substring(0, BetweenBuyAndHoldString.Length - 2);
                            Rtn = GetCompanys(BetweenBuyAndHoldString);
                            if (Rtn.Count != 0) return Rtn;
                        }
                    }


                    //向海纳川发行股份及支付现金
                    //上市公司因向众泰汽车股东购买其合计持有的众泰汽车100%股权而向其发行的股份
                    //三七互娱以发行股份及支付现金的方式向中汇影视全体股东购买其合计持有的中汇影视100％的股份、
                    //向杨东迈、谌维和网众投资购买其合计持有的墨鹍科技68.43％的股权

                    //注意字符串顺序！
                    //立思辰拟以向特定对象发行股份的方式
                    //向自然人张敏、陈勇、朱卫、潘凤岩、施劲松购买其所持有的友网科技合计100%股权
                    //这里必须要能够正确断句，且尽可能减少错误
                    var ToIdx = SingleSentence.IndexOf("向");   //这里拟向也是没有问题的
                    var ToIdx2nd = -1;
                    if (ToIdx != -1 && (ToIdx + 1) != SingleSentence.Length)
                    {
                        ToIdx2nd = SingleSentence.IndexOf("向", ToIdx + 1);
                    }
                    if (ToIdx2nd != -1)
                    {
                        //是否需要将ToIdx2nd变为ToIdx
                        var k = SingleSentence.IndexOf("发行");
                        if (k > ToIdx && k < ToIdx2nd) ToIdx = ToIdx2nd;
                    }

                    var BuyMethodList = new string[] { "发行股份及支付现金", "非公开发行股份", "定向发行股份", "发行股份", "支付现金", "发行A股股份" };
                    foreach (var BuyMethod in BuyMethodList)
                    {
                        var PublishStockAndPayCashIdx = SingleSentence.IndexOf(BuyMethod);
                        if (ToIdx != -1 && PublishStockAndPayCashIdx != -1 && PublishStockAndPayCashIdx > ToIdx)
                        {
                            var ToTarget = SingleSentence.Substring(ToIdx + 1, PublishStockAndPayCashIdx - ToIdx - 1);
                            if (ToTarget.EndsWith("全体股东")) ToTarget = ToTarget.Substring(0, ToTarget.Length - 4);
                            if (ToTarget.EndsWith("股东")) ToTarget = ToTarget.Substring(0, ToTarget.Length - 2);
                            //以...方式
                            if (ToTarget.EndsWith("以") && SingleSentence.Contains("方式")) ToTarget = ToTarget.Substring(0, ToTarget.Length - 1);
                            //Console.WriteLine("向...发行股份及支付现金:" + ToTarget);
                            Rtn = GetCompanys(ToTarget);
                            if (Rtn.Count != 0) return Rtn;
                        }
                    }
                    //没有支付手段，直接购买的情况
                    if (ToIdx != -1 && BuyIdx != -1 && BuyIdx > ToIdx)
                    {
                        var ToTarget = SingleSentence.Substring(ToIdx + 1, BuyIdx - ToIdx - 1);
                        if (ToTarget.EndsWith("全体股东")) ToTarget = ToTarget.Substring(0, ToTarget.Length - 4);
                        if (ToTarget.EndsWith("股东")) ToTarget = ToTarget.Substring(0, ToTarget.Length - 2);
                        //Console.WriteLine("向...购买:" + ToTarget);
                        Rtn = GetCompanys(ToTarget);
                        if (Rtn.Count != 0) return Rtn;
                    }

                    //与王悦等11名交易对方持有的恺英网络100%股权中的等值部分进行资产置换
                    //持有的句型可能嵌套在与...置换之间
                    var WithIdx = SingleSentence.IndexOf("与");
                    var ReplaceIdx = SingleSentence.IndexOf("置换");
                    if (WithIdx != -1 && ReplaceIdx != -1)
                    {
                        SingleSentence = SingleSentence.Substring(WithIdx + 1);
                    }

                    //合计持有，所持有，持有，这样的顺序去判定
                    var HoldWordList = new string[] { "共计持有", "合计持有", "以其持有", "所持有", "持有" };
                    foreach (var hw in HoldWordList)
                    {
                        if (SingleSentence.IndexOf(hw) != -1)
                        {
                            var HoldTarget = Utility.GetStringBefore(SingleSentence, hw);
                            //Console.WriteLine("....持有:" + HoldTarget);
                            Rtn = GetCompanys(HoldTarget);
                            if (Rtn.Count != 0) return Rtn;
                        }
                    }

                    if (!String.IsNullOrEmpty(BetweenBuyAndHoldString) && BetweenBuyAndHoldString.Equals("其"))
                    {
                        //Console.WriteLine("特殊指代：" + BetweenBuyAndHoldString);
                        //以.......为特定对象
                        var AdIdx = PreviewSentence.IndexOf("以");
                        var SpecalIdx = PreviewSentence.IndexOf("特定对象");
                        if (AdIdx != -1 && SpecalIdx != -1 && AdIdx < SpecalIdx)
                        {
                            var ToTarget = PreviewSentence.Substring(AdIdx + 1, SpecalIdx - AdIdx - 1);
                            if (ToTarget.EndsWith("为")) ToTarget = ToTarget.Substring(0, ToTarget.Length - 1);
                            Rtn = GetCompanys(ToTarget);
                        }
                    }
                    PreviewSentence = SingleSentence;
                }
            }
        }
        return Rtn;
    }

    public List<String> GetCompanys(string OrgString)
    {
        var Rtn = new List<String>();
        if (String.IsNullOrEmpty(OrgString)) return Rtn;
        OrgString = OrgString.Replace(" ", "");
        var Items = OrgString.Split(Utility.SplitChar);
        if (Items.Length > 3 && Items.Last().EndsWith("等"))
        {
            Items[Items.Length - 1] = Items[Items.Length - 1].Substring(0, Items[Items.Length - 1].Length - 1);
        }
        foreach (var SingleItem in Items)
        {
            var ExtractSingleItem = SingleItem;
            if (ExtractSingleItem.Equals("交易对方")) continue;
            var number = RegularTool.GetNumberList(ExtractSingleItem);
            if (number.Count == 1 && ExtractSingleItem.Contains("名"))
            {
                ExtractSingleItem = Utility.GetStringBefore(ExtractSingleItem, number[0]);
            }
            if (IsCompanyOrPerson(ExtractSingleItem))
            {
                Rtn.Add(ExtractSingleItem);
            }
            else
            {
                //这里可能出现一些 “和” ，“及” 这样的文字，需要区分
                var AndIdx = ExtractSingleItem.IndexOf("和");
                if (AndIdx == -1) AndIdx = ExtractSingleItem.IndexOf("及");
                if (AndIdx != -1 && AndIdx != 0 && AndIdx != (ExtractSingleItem.Length - 1))
                {
                    var FirstWord = ExtractSingleItem.Substring(0, AndIdx);
                    if (FirstWord.Contains("等"))
                    {
                        FirstWord = Utility.GetStringBefore(FirstWord, "等");
                    }
                    if (FirstWord.Contains("自然人"))
                    {
                        FirstWord = Utility.GetStringBefore(FirstWord, "自然人");
                    }
                    var Secondword = ExtractSingleItem.Substring(AndIdx + 1);
                    if (Secondword.Contains("等"))
                    {
                        Secondword = Utility.GetStringBefore(Secondword, "等");
                    }
                    if (Secondword.Contains("自然人"))
                    {
                        Secondword = Utility.GetStringBefore(Secondword, "自然人");
                    }
                    if (IsCompanyOrPerson(FirstWord) && IsCompanyOrPerson(Secondword))
                    {
                        Rtn.Add(FirstWord);
                        Rtn.Add(Secondword);
                    }
                    else
                    {
                        Console.WriteLine("无法匹配任何公司或者自然人：" + FirstWord + "|" + Secondword);
                        return new List<String>();
                    }
                }
                else
                {
                    Console.WriteLine("无法匹配任何公司或者自然人：" + ExtractSingleItem);
                    return new List<String>();
                }
            }
        }
        //Console.WriteLine("输入：" + OrgString);
        foreach (var item in Rtn)
        {
            //Console.WriteLine("输出：" + item);
        }
        return Rtn;
    }

    bool IsCompanyOrPerson(String SingleItem)
    {
        foreach (var cn in companynamelist)
        {
            if (SingleItem.Equals(cn.secFullName) || SingleItem.Equals(cn.secShortName))
            {
                return true;
            }
        }
        //人物
        if (PersonList.Contains(SingleItem))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 交易对手最后整型
    /// </summary>
    /// <param name="reorgRec"></param>
    public void NormalizeTradeCompany(ReorganizationRec reorgRec)
    {
        var TrimLeadingWords = new string[]{
            "交易对方包括","交易对方，具体指",
            "本次交易的对方，","交易对方","交易对方，即",
            "全体股东，包括","全体股东，即","全部股东，包括","全部股东，即",
            "自然人股东：","资金认购方"
        };
        foreach (var word in TrimLeadingWords)
        {
            if (reorgRec.TradeCompany.Contains(word))
            {
                reorgRec.TradeCompany = Utility.GetStringAfter(reorgRec.TradeCompany, word);
                return;
            }
        }
    }

}