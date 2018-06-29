using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;

public class EntityProperty
{
    //属性类型
    public enmType PropertyType = enmType.Normal;

    public string PropertyName = "属性名称";

    public enum enmType
    {
        Money,      //金钱
        Number,     //数字
        Date,       //日期
        Normal,     //普通文本
    }

    //最大长度
    public int MaxLength = -1;
    //最小长度
    public int MinLength = -1;
    /// <summary>
    /// 冒号前导词
    /// </summary>
    public string[] LeadingColonKeyWordList;
    /// <summary>
    /// 括号尾部词
    /// </summary>
    public string[] QuotationTrailingWordList;
    /// <summary>
    /// 从引号或者书名号里提取某个关键字结尾的词语
    /// </summary>
    public bool QuotationTrailingWordList_IsSkipBracket = true;

    /// <summary>
    /// 做最大长度时候用的预处理（不改变值）
    /// </summary>
    public Func<String, String> MaxLengthCheckPreprocess;

    /// <summary>
    /// 做最小长度时候用的预处理（不改变值）
    /// </summary>
    public Func<String, String> MinLengthCheckPreprocess;

    public Func<String, String> CandidatePreprocess;

    public string Extract(AnnouceDocument doc)
    {
        //纯关键字类型
        if (KeyWordMap.Count != 0)
        {
            var candidate = ExtractByKeyWordMap(doc.root);
            if (candidate.Count == 0) return "";
            if (candidate.Count > 1)
            {
                if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("找到纯关键字类型两个关键字");
            }
            return candidate.First();
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
                var PropertyValue = item.Value.Trim();
                if (CandidatePreprocess != null) PropertyValue = CandidatePreprocess(PropertyValue);
                if (PropertyValue == string.Empty) continue;
                if (MaxLength != -1)
                {
                    if (MaxLengthCheckPreprocess == null)
                    {
                        if (PropertyValue.Length > MaxLength) continue;
                    }
                    else
                    {
                        if (MaxLengthCheckPreprocess(PropertyValue).Length > MaxLength) continue;
                    }
                }
                if (MinLength != -1)
                {
                    if (MinLengthCheckPreprocess == null)
                    {
                        if (PropertyValue.Length > MinLength) continue;
                    }
                    else
                    {
                        if (MinLengthCheckPreprocess(PropertyValue).Length > MinLength) continue;
                    }
                }
                if (!Program.IsMultiThreadMode) Program.Logger.WriteLine(PropertyName + " 候补词(前导关键字Text)：[" + PropertyValue + "]");
                return PropertyValue;
            }

            var Extractor = new ExtractPropertyByHTML();
            Extractor.LeadingColonKeyWordList = ExtractorText.LeadingColonKeyWordList;
            Extractor.Extract(doc.root);
            foreach (var item in ExtractorText.CandidateWord)
            {
                var PropertyValue = item.Value.Trim();
                if (CandidatePreprocess != null) PropertyValue = CandidatePreprocess(PropertyValue);
                if (PropertyValue == string.Empty) continue;
                if (MaxLength != -1)
                {
                    if (MaxLengthCheckPreprocess == null)
                    {
                        if (PropertyValue.Length > MaxLength) continue;
                    }
                    else
                    {
                        if (MaxLengthCheckPreprocess(PropertyValue).Length > MaxLength) continue;
                    }
                }
                if (MinLength != -1)
                {
                    if (MinLengthCheckPreprocess == null)
                    {
                        if (PropertyValue.Length > MinLength) continue;
                    }
                    else
                    {
                        if (MinLengthCheckPreprocess(PropertyValue).Length > MinLength) continue;
                    }
                }
                if (!Program.IsMultiThreadMode) Program.Logger.WriteLine(PropertyName + " 候补词(前导关键字Html)：[" + PropertyValue + "]");
                return PropertyValue;
            }
        }

        if (QuotationTrailingWordList != null)
        {
            //接下来《》，“” 优先
            foreach (var bracket in doc.quotationList)
            {
                if (!Program.IsMultiThreadMode) Program.Logger.WriteLine(PropertyName + " 候补词：[" + bracket.Value + "]");
            }
            foreach (var bracket in doc.quotationList)
            {
                foreach (var word in QuotationTrailingWordList)
                {
                    if (bracket.Value.EndsWith(word))
                    {
                        if (!Program.IsMultiThreadMode) Program.Logger.WriteLine(PropertyName + "：[" + bracket.Value + "]");
                        return bracket.Value;
                    }
                }
            }
        }
        return "";
    }

    #region 纯关键字类型

    //纯关键字类型
    public Dictionary<string, string> KeyWordMap = new Dictionary<string, string>();

    public List<string> ExtractByKeyWordMap(HTMLEngine.MyRootHtmlNode root)
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