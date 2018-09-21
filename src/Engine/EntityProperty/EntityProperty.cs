using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FDDC;
using static ExtractProperyBase;

public class EntityProperty
{
    public static StreamWriter Logger;

    /// <summary>
    /// 属性类型
    /// </summary>
    public enmType PropertyType = enmType.NER;
    /// <summary>
    /// 属性名称
    /// </summary>
    public string PropertyName = "属性名称";
    /// <summary>
    /// 属性类型枚举
    /// </summary>
    public enum enmType
    {
        Percent,    //百分比
        Money,      //货币
        Number,     //数字
        Date,       //日期
        Time,       //时间
        NER,        //人名、机构名、地名
        Customer    //自定义
    }

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
    /// 候选词预处理
    /// </summary>
    public Func<String, String> CandidatePreprocess;

    /// <summary>
    /// 最高置信度候选词专用于处理器
    /// </summary>
    public Func<String, String> LeadingColonKeyWordCandidatePreprocess;


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
    /// 从引号或者书名号里提取某个关键字结尾的词语,不提取括号里面的词语
    /// </summary>
    public bool QuotationTrailingWordList_IsSkipBracket = false;

    public Func<String, string> QuotationTrailingPreprocess;

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

    /// <summary>
    /// 候选词预处理
    /// </summary>
    public Func<String, String> ExternalStartEndStringFeatureCandidatePreprocess;
    /// <summary>
    /// 外部特征
    /// </summary>
    public struStartEndStringFeature[] ExternalStartEndStringFeature;
    /// <summary>
    /// 外部特征候选词列表
    /// </summary>
    /// <typeparam name="string"></typeparam>
    /// <returns></returns>
    public List<string> ExternalStartEndStringFeatureCandidate = new List<string>();

    /// <summary>
    /// 正则表达式检索
    /// </summary>
    public struRegularExpressFeature[] RegularExpressFeature;
    /// <summary>
    /// 正则表达式检索候选词
    /// </summary>
    /// <typeparam name="string"></typeparam>
    /// <returns></returns>
    public List<string> RegularExpressFeatureCandidate = new List<string>();

    /// <summary>
    /// 不能包含的词语列表
    /// </summary>
    public string[] ExcludeContainsWordList;
    /// <summary>
    /// 不能等于的词语列表
    /// </summary>
    public string[] ExcludeEqualsWordList;


    public void Extract(AnnouceDocument doc)
    {
        //纯关键字类型
        if (KeyWordMap.Count != 0)
        {
            WordMapResult = ExtractByKeyWordMap(doc.root);
            return;
        }

        if (LeadingColonKeyWordList != null)
        {
            //按照规则，由固定先导词的，例如  [项目名：]  
            //这里的词语不受任何其他因素制约，例如最大最小长度，有专用的预处理器
            var ExtractorText = new ExtractPropertyByText();
            //这些关键字后面:注意：TEXT版本可能存在空格，所以HTML版本也检查一遍
            ExtractorText.LeadingColonKeyWordList = LeadingColonKeyWordList;
            ExtractorText.ExtractFromTextFile(doc.TextFileName);
            foreach (var item in ExtractorText.CandidateWord)
            {
                var PropertyValue = item.Value;
                if (LeadingColonKeyWordCandidatePreprocess != null) PropertyValue = LeadingColonKeyWordCandidatePreprocess(PropertyValue);
                if (String.IsNullOrEmpty(PropertyValue)) continue;
                if (!Program.IsMultiThreadMode) Logger.WriteLine(this.PropertyName + "：[" + PropertyValue + "]");
                LeadingColonKeyWordCandidate.Add(PropertyValue);
            }

            var Extractor = new ExtractPropertyByHTML();
            Extractor.LeadingColonKeyWordList = ExtractorText.LeadingColonKeyWordList;
            Extractor.Extract(doc.root);
            foreach (var item in ExtractorText.CandidateWord)
            {
                var PropertyValue = item.Value;
                if (LeadingColonKeyWordCandidatePreprocess != null) PropertyValue = LeadingColonKeyWordCandidatePreprocess(PropertyValue);
                if (String.IsNullOrEmpty(PropertyValue)) continue;
                if (!Program.IsMultiThreadMode) Logger.WriteLine(this.PropertyName + "：[" + PropertyValue + "]");
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
                if (QuotationTrailingWordList_IsSkipBracket)
                {
                    if (bracket.Description == "中文小括号") continue;
                }
                foreach (var trailingword in QuotationTrailingWordList)
                {
                    var EvaluateWord = bracket.Value;
                    if (QuotationTrailingPreprocess != null)
                    {
                        EvaluateWord = QuotationTrailingPreprocess(EvaluateWord);
                    }
                    if (EvaluateWord.EndsWith(trailingword))
                    {
                        var PropertyValue = CheckCandidate(EvaluateWord);
                        if (String.IsNullOrEmpty(PropertyValue)) continue;
                        if (!Program.IsMultiThreadMode) Logger.WriteLine(this.PropertyName + "：[" + PropertyValue + "]");
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
                if (!Program.IsMultiThreadMode) Logger.WriteLine(this.PropertyName + "：[" + PropertyValue + "]");
                DpKeyWordCandidate.Add(PropertyValue);
            }
        }

        if (ExternalStartEndStringFeature != null)
        {
            var ExtractorTEXT = new ExtractPropertyByText();
            ExtractorTEXT.StartEndFeature = ExternalStartEndStringFeature;
            ExtractorTEXT.ExtractFromTextFile(doc.TextFileName);
            foreach (var item in ExtractorTEXT.CandidateWord)
            {
                var PropertyValue = item.Value;
                if (ExternalStartEndStringFeatureCandidatePreprocess != null)
                {
                    PropertyValue = ExternalStartEndStringFeatureCandidatePreprocess(PropertyValue);
                }
                PropertyValue = CheckCandidate(PropertyValue);
                if (String.IsNullOrEmpty(PropertyValue)) continue;
                if (!Program.IsMultiThreadMode) Logger.WriteLine(this.PropertyName + "：[" + PropertyValue + "]");
                ExternalStartEndStringFeatureCandidate.Add(PropertyValue);
            }

            //一部分无法提取TEXT的情况
            var ExtractorHTML = new ExtractPropertyByHTML();
            ExtractorHTML.StartEndFeature = ExternalStartEndStringFeature;
            ExtractorHTML.Extract(doc.root);
            foreach (var item in ExtractorHTML.CandidateWord)
            {
                var PropertyValue = item.Value;
                if (ExternalStartEndStringFeatureCandidatePreprocess != null)
                {
                    PropertyValue = ExternalStartEndStringFeatureCandidatePreprocess(PropertyValue);
                }
                PropertyValue = CheckCandidate(PropertyValue);
                if (String.IsNullOrEmpty(PropertyValue)) continue;
                if (!Program.IsMultiThreadMode) Logger.WriteLine(this.PropertyName + "：[" + PropertyValue + "]");
                if (!ExternalStartEndStringFeatureCandidate.Contains(PropertyValue)) ExternalStartEndStringFeatureCandidate.Add(PropertyValue);
            }
        }

        if (RegularExpressFeature != null)
        {
            var Extractor = new ExtractPropertyByHTML();
            Extractor.RegularExpressFeature = RegularExpressFeature;
            Extractor.Extract(doc.root);
            foreach (var item in Extractor.CandidateWord)
            {
                var PropertyValue = item.Value;
                if (String.IsNullOrEmpty(PropertyValue)) continue;
                if (!Program.IsMultiThreadMode) Logger.WriteLine(this.PropertyName + "：[" + PropertyValue + "]");
                if (!RegularExpressFeatureCandidate.Contains(PropertyValue)) RegularExpressFeatureCandidate.Add(PropertyValue);
            }
        }

    }

    public string CheckCandidate(string PropertyValue)
    {
        PropertyValue = PropertyValue.Trim();
        if (CandidatePreprocess != null) PropertyValue = CandidatePreprocess(PropertyValue);
        if (ExcludeContainsWordList != null && ExcludeContainsWordList.Length != 0)
        {
            foreach (var excude in ExcludeContainsWordList)
            {
                if (PropertyValue.Contains(excude))
                {
                    return String.Empty;
                }
            }
        }
        if (ExcludeEqualsWordList != null && ExcludeEqualsWordList.Length != 0)
        {
            foreach (var excude in ExcludeEqualsWordList)
            {
                if (PropertyValue.Equals(excude))
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


    #region 置信度
    /// <summary>
    /// 可信度
    /// </summary>
    public CIBase Confidence;
    public void CheckIsCandidateContainsTarget(string StartandValue)
    {
        bool IsFound = false;
        if (!Program.IsMultiThreadMode) Logger.WriteLine("标准" + PropertyName + ":" + StartandValue);
        if (LeadingColonKeyWordCandidate.Select((x) => { return x.NormalizeTextResult(); }).Contains(StartandValue.NormalizeTextResult()))
        {
            if (!Program.IsMultiThreadMode) Logger.WriteLine("存在于 冒号关键字 候补词语");
            IsFound = true;
        }
        if (QuotationTrailingCandidate.Select((x) => { return x.NormalizeTextResult(); }).Contains(StartandValue.NormalizeTextResult()))
        {
            if (!Program.IsMultiThreadMode) Logger.WriteLine("存在于 书名号和引号 候补词语");
            IsFound = true;
        }
        if (DpKeyWordCandidate.Select((x) => { return x.NormalizeTextResult(); }).Contains(StartandValue.NormalizeTextResult()))
        {
            if (!Program.IsMultiThreadMode) Logger.WriteLine("存在于 句法依赖 候补词语");
            IsFound = true;
        }
        if (ExternalStartEndStringFeatureCandidate.Select((x) => { return x.NormalizeTextResult(); }).Contains(StartandValue.NormalizeTextResult()))
        {
            if (!Program.IsMultiThreadMode) Logger.WriteLine("存在于 前后关键字 候补词语");
            IsFound = true;
        }
        if (!IsFound && !Program.IsMultiThreadMode) Logger.WriteLine("候补词语未抽取信息！");
    }

    /// <summary>
    /// 为所有词语进行置信度评价
    /// </summary>
    public string EvaluateCI()
    {
        var Result = "";
        var MaxScore = -1;
        if (Confidence != null)
        {
            foreach (var candidate in LeadingColonKeyWordCandidate)
            {
                //项目名称：这样的候选词置信度最高,趋向于无条件置信
                var score = 1000;
                if (!Program.IsMultiThreadMode) Logger.WriteLine(candidate + ":" + score);
                if (score > MaxScore)
                {
                    Result = candidate;
                    MaxScore = score;
                }
            }
            foreach (var candidate in QuotationTrailingCandidate)
            {
                var score = Confidence.Predict(candidate) * 2;
                if (!Program.IsMultiThreadMode) Logger.WriteLine(candidate + ":" + score);
                if (score > MaxScore)
                {
                    Result = candidate;
                    MaxScore = score;
                }

            }
            foreach (var candidate in DpKeyWordCandidate)
            {
                var score = Confidence.Predict(candidate);
                if (!Program.IsMultiThreadMode) Logger.WriteLine(candidate + ":" + score);
                if (score > MaxScore)
                {
                    Result = candidate;
                    MaxScore = score;
                }

            }
            foreach (var candidate in ExternalStartEndStringFeatureCandidate)
            {
                var score = Confidence.Predict(candidate);
                if (!Program.IsMultiThreadMode) Logger.WriteLine(candidate + ":" + score);
                if (score > MaxScore)
                {
                    Result = candidate;
                    MaxScore = score;
                }
            }
        }
        else
        {
            if (LeadingColonKeyWordCandidate != null && LeadingColonKeyWordCandidate.Count > 0) return LeadingColonKeyWordCandidate.First();
            if (QuotationTrailingCandidate != null && QuotationTrailingCandidate.Count > 0) return QuotationTrailingCandidate.First();
            if (DpKeyWordCandidate != null && DpKeyWordCandidate.Count > 0) return DpKeyWordCandidate.First();
            if (ExternalStartEndStringFeatureCandidate != null && ExternalStartEndStringFeatureCandidate.Count > 0)
                return ExternalStartEndStringFeatureCandidate.First();

        }
        return Result;
    }


    #endregion


    #region 纯关键字类型

    //纯关键字类型
    public Dictionary<string, string> KeyWordMap = new Dictionary<string, string>();

    public List<string> WordMapResult;

    List<string> ExtractByKeyWordMap(HTMLEngine.MyRootHtmlNode root)
    {
        var result = new List<string>();
        foreach (var item in KeyWordMap)
        {
            var HasKey = ExtractPropertyByHTML.HasWord(item.Key, root);
            if (HasKey)
            {
                if (!result.Contains(item.Value)) result.Add(item.Value);
            }
        }
        return result;
    }
    #endregion
}