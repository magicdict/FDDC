using System;
using System.Collections.Generic;
using System.Linq;
using static LocateProperty;
using static LTPTrainingNER;

public class NerMap
{
    /// <summary>
    /// 段落实体（基于HTML）
    /// </summary>
    /// <typeparam name="int">HTML段落号</typeparam>
    /// <typeparam name="ParagraghLoc">实体</typeparam>
    /// <returns></returns>
    Dictionary<int, ParagraghLoc> ParagraghlocateDict = new Dictionary<int, ParagraghLoc>();

    public void Anlayze(AnnouceDocument doc)
    {
        var nerlist = new List<LocAndValue<String>>();
        if (doc.Nerlist != null)
        {
            var nh = doc.Nerlist.Where(x => x.Type == enmNerType.Nh).Select(y => y.RawData).Distinct();
            nerlist.AddRange(LocateCustomerWord(doc.root, nh.ToList(), "人名"));

            var ni = doc.Nerlist.Where(x => x.Type == enmNerType.Ni).Select(y => y.RawData).Distinct();
            nerlist.AddRange(LocateCustomerWord(doc.root, ni.ToList(), "机构"));

            var ns = doc.Nerlist.Where(x => x.Type == enmNerType.Ns).Select(y => y.RawData).Distinct();
            nerlist.AddRange(LocateCustomerWord(doc.root, ns.ToList(), "地名"));

        }

        if (doc is Contract){
            
        }

        foreach (var paragragh in doc.root.Children)
        {
            foreach (var s in paragragh.Children)
            {
                var p = LocateParagraphInfo(doc, s.PositionId, nerlist);
                if (p.NerList.Count + p.moneylist.Count + p.datelist.Count != 0) ParagraghlocateDict.Add(s.PositionId, p);
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
        foreach (var item in doc.quotationList)
        {
            if (item.Loc == PosId) paragragh.NerList.Add(item);
        }
        foreach (var item in nerList)
        {
            if (item.Loc == PosId) paragragh.NerList.Add(item);
        }

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
        }
    }
}