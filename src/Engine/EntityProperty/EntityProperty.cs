using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;
using static ExtractProperyBase;

public class EntityProperty
{
    /// <summary>
    /// 属性类型
    /// </summary>
    public enmType PropertyType = enmType.Normal;
    /// <summary>
    /// 属性名称
    /// </summary>
    public string PropertyName = "属性名称";
    /// <summary>
    /// 属性类型枚举
    /// </summary>
    public enum enmType
    {
        Money,      //金钱
        Number,     //数字
        Date,       //日期
        Normal,     //普通文本
    }

    /// <summary>
    /// 可信度
    /// </summary>
    public CI Confidence;

    /// <summary>
    /// 最大长度
    /// </summary>
    public int MaxLength = -1;
    /// <summary>
    /// 最小长度
    /// </summary>
    public int MinLength = -1;
    /// <summary>
    /// 做最大长度时候用的预处理（不改变值）
    /// </summary>
    public Func<String, String> MaxLengthCheckPreprocess;

    /// <summary>
    /// 做最小长度时候用的预处理（不改变值）
    /// </summary>
    public Func<String, String> MinLengthCheckPreprocess;

    /// <summary>
    /// 冒号前导词
    /// </summary>
    public string[] LeadingColonKeyWordList;
    /// <summary>
    /// 冒号前导词候选结果
    /// </summary>
    public List<string> LeadingColonKeyWordCandidate = new List<string>();


    /// <summary>
    ///  书名号和引号尾部词
    /// </summary>
    public string[] QuotationTrailingWordList;
    /// <summary>
    /// 从引号或者书名号里提取某个关键字结尾的词语
    /// </summary>
    public bool QuotationTrailingWordList_IsSkipBracket = true;
    /// <summary>
    /// 书名号和引号候选词
    /// </summary>
    /// <typeparam name="string"></typeparam>
    /// <returns></returns>
    public List<string> QuotationTrailingCandidate = new List<string>();

    /// <summary>
    /// 句法依存关键字列表
    /// </summary>
    public List<ExtractPropertyByDP.DPKeyWord> DpKeyWordList;
    /// <summary>
    /// 句法依存候选词
    /// </summary>
    /// <typeparam name="string"></typeparam>
    /// <returns></returns>
    public List<string> DpKeyWordCandidate = new List<string>();


    public struStartEndStringFeature[] StartEndStringFeature;

    public List<string> StartEndStringFeatureCandidate = new List<string>();

    /// <summary>
    /// 不能包含的词语列表
    /// </summary>
    public string[] ExcludeWordList;
    /// <summary>
    /// 候选词预处理
    /// </summary>
    public Func<String, String> CandidatePreprocess;

    public void Extract(AnnouceDocument doc)
    {
        //纯关键字类型
        if (KeyWordMap.Count != 0)
        {
            var candidate = ExtractByKeyWordMap(doc.root);
            if (candidate.Count == 1) WordMapResult = candidate.First();
            if (candidate.Count > 1)
            {
                if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("找到纯关键字类型两个关键字");
            }
            return;
        }

        //按照规则，由固定先导词的，例如  [项目名：]  优先
        if (LeadingColonKeyWordList != null)
        {
            var ExtractorText = new ExtractPropertyByText();
            //这些关键字后面:注意：TEXT版本可能存在空格，所以HTML版本也检查一遍
            ExtractorText.LeadingColonKeyWordList = LeadingColonKeyWordList;
            ExtractorText.ExtractFromTextFile(doc.TextFileName);
            foreach (var item in ExtractorText.CandidateWord)
            {
                var PropertyValue = CheckCandidate(item.Value);
                if (String.IsNullOrEmpty(PropertyValue)) continue;
                if (!Program.IsMultiThreadMode) Program.Logger.WriteLine(this.PropertyName + "：[" + PropertyValue + "]");
                LeadingColonKeyWordCandidate.Add(PropertyValue);
            }

            var Extractor = new ExtractPropertyByHTML();
            Extractor.LeadingColonKeyWordList = ExtractorText.LeadingColonKeyWordList;
            Extractor.Extract(doc.root);
            foreach (var item in ExtractorText.CandidateWord)
            {
                var PropertyValue = CheckCandidate(item.Value);
                if (String.IsNullOrEmpty(PropertyValue)) continue;
                if (!Program.IsMultiThreadMode) Program.Logger.WriteLine(this.PropertyName + "：[" + PropertyValue + "]");
                //TEXT里面有的，这里不重复添加了
                if (!LeadingColonKeyWordCandidate.Contains(PropertyValue)) LeadingColonKeyWordCandidate.Add(PropertyValue);
            }
        }

        //书名号和引号
        if (QuotationTrailingWordList != null)
        {
            //接下来《》，“” 优先
            foreach (var bracket in doc.quotationList)
            {
                foreach (var word in QuotationTrailingWordList)
                {
                    if (bracket.Value.EndsWith(word))
                    {
                        var PropertyValue = CheckCandidate(bracket.Value);
                        if (String.IsNullOrEmpty(PropertyValue)) continue;
                        if (!Program.IsMultiThreadMode) Program.Logger.WriteLine(this.PropertyName + "：[" + PropertyValue + "]");
                        QuotationTrailingCandidate.Add(PropertyValue);
                    }
                }
            }
        }

        //句法依存
        if (DpKeyWordList != null)
        {
            var ExtractDP = new ExtractPropertyByDP();
            ExtractDP.StartWithKey(DpKeyWordList, doc.Dplist);
            foreach (var item in ExtractDP.CandidateWord)
            {
                var PropertyValue = CheckCandidate(item.Value);
                if (String.IsNullOrEmpty(PropertyValue)) continue;
                if (!Program.IsMultiThreadMode) Program.Logger.WriteLine(this.PropertyName + "：[" + PropertyValue + "]");
                DpKeyWordCandidate.Add(PropertyValue);
            }
        }

        if (StartEndStringFeature != null)
        {
            var ExtractorTEXT = new ExtractPropertyByText();
            ExtractorTEXT.StartEndFeature = StartEndStringFeature;
            ExtractorTEXT.ExtractFromTextFile(doc.TextFileName);
            foreach (var item in ExtractorTEXT.CandidateWord)
            {
                var PropertyValue = CheckCandidate(item.Value);
                if (String.IsNullOrEmpty(PropertyValue)) continue;
                if (!Program.IsMultiThreadMode) Program.Logger.WriteLine(this.PropertyName + "：[" + PropertyValue + "]");
                StartEndStringFeatureCandidate.Add(PropertyValue);
            }

            //一部分无法提取TEXT的情况
            var ExtractorHTML = new ExtractPropertyByHTML();
            ExtractorHTML.StartEndFeature = StartEndStringFeature;
            ExtractorHTML.Extract(doc.root);
            foreach (var item in ExtractorHTML.CandidateWord)
            {
                var PropertyValue = CheckCandidate(item.Value);
                if (String.IsNullOrEmpty(PropertyValue)) continue;
                if (!Program.IsMultiThreadMode) Program.Logger.WriteLine(this.PropertyName + "：[" + PropertyValue + "]");
                if (!StartEndStringFeatureCandidate.Contains(PropertyValue)) StartEndStringFeatureCandidate.Add(PropertyValue);
            }
        }
    }

    public string CheckCandidate(string PropertyValue)
    {
        PropertyValue = PropertyValue.Trim();
        if (CandidatePreprocess != null) PropertyValue = CandidatePreprocess(PropertyValue);
        if (ExcludeWordList != null && ExcludeWordList.Length != 0)
        {
            foreach (var excude in ExcludeWordList)
            {
                if (PropertyValue.Contains(excude))
                {
                    return String.Empty;
                }
            }
        }
        if (PropertyValue == string.Empty) return String.Empty;
        if (MaxLength != -1)
        {
            if (MaxLengthCheckPreprocess == null)
            {
                if (PropertyValue.Length > MaxLength) return String.Empty; ;
            }
            else
            {
                if (MaxLengthCheckPreprocess(PropertyValue).Length > MaxLength) return String.Empty; ;
            }
        }
        if (MinLength != -1)
        {
            if (MinLengthCheckPreprocess == null)
            {
                if (PropertyValue.Length < MinLength) return String.Empty; ;
            }
            else
            {
                if (MinLengthCheckPreprocess(PropertyValue).Length < MinLength) return String.Empty; ;
            }
        }
        return PropertyValue;
    }


    #region 纯关键字类型

    //纯关键字类型
    public Dictionary<string, string> KeyWordMap = new Dictionary<string, string>();

    public String WordMapResult = "";

    List<string> ExtractByKeyWordMap(HTMLEngine.MyRootHtmlNode root)
    {
        var result = new List<string>();
        foreach (var item in KeyWordMap)
        {
            var cnt = ExtractPropertyByHTML.FindWordCnt(item.Key, root).Count;
            if (cnt > 0)
            {
                if (!result.Contains(item.Value)) result.Add(item.Value);
            }
        }
        return result;
    }
    #endregion
}