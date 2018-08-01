using System;
using System.Collections.Generic;
using System.Linq;
using static LocateProperty;
using static LTPTrainingNER;

/// <summary>
/// NER地图
/// </summary>
public class NerMap
{
    /// <summary>
    /// 段落实体（基于HTML）
    /// </summary>
    /// <typeparam name="int">HTML段落号</typeparam>
    /// <typeparam name="ParagraghLoc">实体</typeparam>
    /// <returns></returns>
    public Dictionary<int, ParagraghLoc> ParagraghlocateDict = new Dictionary<int, ParagraghLoc>();
    /// <summary>
    /// 实体分析
    /// </summary>
    /// <param name="doc"></param>
    public void Anlayze(AnnouceDocument doc)
    {
        ParagraghlocateDict.Clear();
        var nerlist = new List<LocAndValue<String>>();
        if (doc.Nerlist != null)
        {

            var ni = doc.Nerlist.Where(x => x.Type == enmNerType.Ni).Select(y => y.RawData).Distinct();
            nerlist.AddRange(LocateCustomerWord(doc.root, ni.ToList(), "机构"));

            var ns = doc.Nerlist.Where(x => x.Type == enmNerType.Ns).Select(y => y.RawData).Distinct();
            nerlist.AddRange(LocateCustomerWord(doc.root, ns.ToList(), "地名"));

            var nh = doc.Nerlist.Where(x => x.Type == enmNerType.Nh).Select(y => y.RawData).Distinct();
            nerlist.AddRange(LocateCustomerWord(doc.root, nh.ToList(), "人名"));
        }

        var FullNameList = doc.companynamelist.Select((x) => x.secFullName).ToList();
        FullNameList = FullNameList.Where(x => !String.IsNullOrEmpty(x)).Distinct().ToList();
        //补充公司名称
        nerlist.AddRange(LocateCustomerWord(doc.root, FullNameList, "公司名"));

        foreach (var paragragh in doc.root.Children)
        {
            foreach (var s in paragragh.Children)
            {
                var p = LocateParagraphInfo(doc, s.PositionId, nerlist);
                if (p.NerList.Count + p.moneylist.Count + p.datelist.Count + p.percentList.Count + p.socketNumberList.Count != 0)
                {
                    if (!ParagraghlocateDict.ContainsKey(s.PositionId)) ParagraghlocateDict.Add(s.PositionId, p);
                }
            }
        }
    }
    /// <summary>
    /// 每句句子中，各种实体的聚合
    /// </summary>
    /// <param name="PosId"></param>
    /// <returns></returns>
    ParagraghLoc LocateParagraphInfo(AnnouceDocument doc, int PosId, List<LocAndValue<String>> nerList)
    {
        var paragragh = new ParagraghLoc();
        paragragh.Init();
        foreach (var item in doc.datelist)
        {
            if (item.Loc == PosId) paragragh.datelist.Add(item);
        }
        foreach (var item in doc.moneylist)
        {
            if (item.Loc == PosId) paragragh.moneylist.Add(item);
        }
        foreach (var item in doc.percentList)
        {
            if (item.Loc == PosId) paragragh.percentList.Add(item);
        }
        foreach (var item in doc.StockNumberList)
        {
            if (item.Loc == PosId) paragragh.socketNumberList.Add(item);
        }
        foreach (var item in doc.CustomerList)
        {
            if (item.Loc == PosId) paragragh.CustomerList.Add(item);    //加入CustomerList为了代码方便
            if (item.Loc == PosId) paragragh.NerList.Add(item);         //加入NerList为了查找方法
        }
        foreach (var item in doc.quotationList)
        {
            if (item.Loc == PosId) paragragh.NerList.Add(item);
        }
        foreach (var item in nerList)
        {
            if (item.Loc == PosId) paragragh.NerList.Add(item);
        }
        paragragh.NerList.Sort((x, y) => { return x.StartIdx.CompareTo(y.StartIdx); });
        return paragragh;
    }

    /// <summary>
    /// 段落实体获取器
    /// </summary>
    public struct ParagraghLoc
    {
        /// <summary>
        /// 日期
        /// </summary>
        public List<LocAndValue<DateTime>> datelist;
        /// <summary>
        /// 百分比
        /// </summary>
        public List<LocAndValue<String>> percentList;
        /// <summary>
        /// 股份数
        /// </summary>
        public List<LocAndValue<String>> socketNumberList;

        public List<LocAndValue<String>> CustomerList;

        /// <summary>
        /// 金额
        /// </summary>
        /// <param name="MoneyAmount"></param>
        /// <param name="MoneyCurrency"></param>
        public List<LocAndValue<(String MoneyAmount, String MoneyCurrency)>> moneylist;
        /// <summary>
        /// NER
        /// </summary>
        public List<LocAndValue<String>> NerList;
        public void Init()
        {
            datelist = new List<LocAndValue<DateTime>>();
            moneylist = new List<LocAndValue<(String MoneyAmount, String MoneyCurrency)>>();
            NerList = new List<LocAndValue<String>>();
            percentList = new List<LocAndValue<String>>();
            socketNumberList = new List<LocAndValue<String>>();
            CustomerList = new List<LocAndValue<String>>();
        }
    }
}