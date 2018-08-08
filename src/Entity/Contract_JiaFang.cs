using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FDDC;
using static CompanyNameLogic;
using static HTMLEngine;
using static HTMLTable;
using static LocateProperty;

public partial class Contract : AnnouceDocument
{
    /// <summary>
    /// 获得甲方
    /// </summary>
    /// <returns></returns>
    string GetJiaFang(String YiFang)
    {
        //最高置信度的抽取
        EntityProperty e = new EntityProperty();
        e.ExcludeContainsWordList = new string[] { "招标代理" };
        e.LeadingColonKeyWordList = new string[] {
            "甲方：","合同买方：",
            "发包人：","发包单位：","发包方：","发包机构：","发包人名称：",
            "招标人：","招标单位：","招标方：","招标机构：","招标人名称：","项目招标人：",
            "业主："  ,"业主单位：" ,"业主方：", "业主机构：","业主名称：",
            "采购单位：","采购单位名称：","采购人：", "采购人名称：","采购方：","采购方名称："
        };
        e.CandidatePreprocess = (x =>
        {
            x = Normalizer.ClearTrailing(x);
            return CompanyNameLogic.AfterProcessFullName(x).secFullName;
        });
        e.MaxLength = 32;
        e.MaxLengthCheckPreprocess = Utility.TrimEnglish;
        e.MinLength = 3;
        e.Extract(this);
        //这里不直接做Distinct，出现频次越高，则可信度越高
        //多个甲方的时候，可能意味着没有甲方！
        if (e.LeadingColonKeyWordCandidate.Distinct().Count() > 1)
        {
            foreach (var candidate in e.LeadingColonKeyWordCandidate)
            {
                Program.Logger.WriteLine("发现多个甲方：" + candidate);
            }
        }
        if (e.LeadingColonKeyWordCandidate.Count > 0) return e.LeadingColonKeyWordCandidate[0];

        var ner = SearchJiaFang();
        var NerJia = String.Empty;
        if (!String.IsNullOrEmpty(ner))
        {
            foreach (var cn in companynamelist)
            {
                if (cn.secShortName == ner) ner = cn.secFullName;
            }
            if (String.IsNullOrEmpty(YiFang)) NerJia = ner;
            if (!YiFang.Equals(ner)) NerJia = ner;
        }

        //招标
        var Extractor = new ExtractPropertyByHTML();
        var CandidateWord = new List<String>();
        var StartArray = new string[] { "招标单位", "业主", "收到", "接到" };
        var EndArray = new string[] { "发来", "发出", "的中标" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            var JiaFang = CompanyNameLogic.AfterProcessFullName(item.Value.Trim());
            if (JiaFang.secFullName.Contains("招标代理")) continue; //特殊业务规则
            JiaFang.secFullName = JiaFang.secFullName.Replace("业主", String.Empty).Trim();
            JiaFang.secFullName = JiaFang.secFullName.Replace("招标单位", String.Empty).Trim();
            if (Utility.TrimEnglish(JiaFang.secFullName).Length > 32) continue;
            if (JiaFang.secFullName.Length < 3) continue;     //使用实际长度排除全英文的情况
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("甲方候补词(招标)：[" + JiaFang.secFullName + "]");
            CandidateWord.Add(JiaFang.secFullName);
        }

        //合同
        Extractor = new ExtractPropertyByHTML();
        StartArray = new string[] { "与", "与业主" };
        EndArray = new string[] { "签署", "签订" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            var JiaFang = CompanyNameLogic.AfterProcessFullName(item.Value.Trim());
            JiaFang.secFullName = JiaFang.secFullName.Replace("业主", String.Empty).Trim();
            if (JiaFang.secFullName.Contains("招标代理")) continue; //特殊业务规则
            if (Utility.TrimEnglish(JiaFang.secFullName).Length > 32) continue;
            if (JiaFang.secFullName.Length < 3) continue;     //使用实际长度排除全英文的情况
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("甲方候补词(合同)：[" + JiaFang.secFullName + "]");
            CandidateWord.Add(JiaFang.secFullName);
        }
        if (!String.IsNullOrEmpty(NerJia))
        {
            //原则上，有NER中提取的甲方，则使用甲方
            foreach (var c in CandidateWord)
            {
                //但是，这里有可能是正确的解答，例如
                //NER：(集团)有限公司 实际上应该是 XXXX(集团)有限公司
                if (c.EndsWith(NerJia)) return c;
            }
            return NerJia;
        }
        else
        {
            return CompanyNameLogic.MostLikeCompanyName(CandidateWord);
        }
    }

    /// <summary>
    /// NER检索
    /// </summary>
    /// <returns></returns>
    string SearchJiaFang()
    {
        var BaseWord = new NerSearch.WordRule();
        BaseWord.Word = new string[] { "中标通知书" }.ToList();
        BaseWord.Description = new string[] { "书名号" }.ToList();

        var TargetWord = new NerSearch.WordRule();
        TargetWord.Description = new string[] { "公司名", "机构" }.ToList();

        var SearchRule = new NerSearch.SearchRule();
        SearchRule.BaseWord = BaseWord;
        SearchRule.Target = TargetWord;
        SearchRule.SearchForward = false;   //向前检索
        SearchRule.Validator = JiaFangValidator;

        var SuccessResult = NerSearch.Search(this, SearchRule);
        if (SuccessResult.Count == 1) return SuccessResult.First().Value;
        BaseWord = new NerSearch.WordRule();
        BaseWord.Word = new string[] { "甲方","合同买方",
            "发包人","发包单位","发包方","发包机构","发包人名称",
            "招标人","招标单位","招标方","招标机构","项目招标人",
            "业主"  ,"业主单位" ,"业主方", "业主机构",
            "采购单位","采购人","采购方",
            "收到", "接到" }.ToList();
        BaseWord.Description = new List<String>();

        TargetWord = new NerSearch.WordRule();
        TargetWord.Description = new string[] { "公司名", "机构" }.ToList();

        SearchRule = new NerSearch.SearchRule();
        SearchRule.BaseWord = BaseWord;
        SearchRule.Target = TargetWord;
        SearchRule.SearchForward = true;   //向后检索
        SearchRule.Validator = JiaFangValidator;
        var ReceiverResult = NerSearch.Search(this, SearchRule);

        if (SuccessResult.Count > 1 && ReceiverResult.Count > 0)
        {
            foreach (var SuccessItem in SuccessResult)
            {
                foreach (var ReceiveItem in ReceiverResult)
                {
                    if (SuccessItem.Value == ReceiveItem.Value)
                    {
                        return ReceiveItem.Value;
                    }
                }
            }
        }

        if (ReceiverResult.Count > 0) return ReceiverResult.First().Value;
        return string.Empty;
    }

    bool JiaFangValidator(LocAndValue<string> x)
    {
        if (x.Value.Contains("招标")) return false;
        return true;
    }
}