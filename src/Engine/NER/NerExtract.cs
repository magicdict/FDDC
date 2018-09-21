using System;
using System.Collections.Generic;
using System.Linq;
using static LTPTrainingNER;

/// <summary>
/// 根据实体抽取规则获得实体
/// </summary>
public class NerExtract
{
    //根据实体的自身特性来抽取实体
    //例如：公司的时候，规则就是：
    //1. 地名 + XXXX + [有限][公司]
    //2. 地名 + XXXX + [有限][合伙]
    //不同的分词系统，可能会有不同的分词结果

    /// <summary>
    /// 抽取规则
    /// </summary>
    public struct NerExtractRule
    {
        /// <summary>
        /// 首词NER属性序列
        /// </summary>
        public List<struWordNER> StartWord;
        /// <summary>
        /// 结束词NER属性序列
        /// </summary>
        public List<struWordNER> EndWord;

        /// <summary>
        /// 最大长度
        /// </summary>
        public int MaxWordLength;
    }

    /// <summary>
    /// 抽取
    /// </summary>
    /// <param name="rule"></param>
    /// <returns></returns>
    public static List<String> Extract(NerExtractRule rule, List<List<struWordNER>> content)
    {
        var Rtn = new List<String>();
        //从头开始寻找符合开始匹配模式的地方放入数组
        //从头开始寻找符合结尾匹配模式的地方放入数组
        foreach (var paragragh in content)
        {
            var StartIdx = SearchMatchIndex(rule.StartWord, paragragh);
            var EndIdx = SearchMatchIndex(rule.EndWord, paragragh);
            if (StartIdx.Count == 0 || EndIdx.Count == 0) continue;
            int PreviewEndIdx = -1;
            foreach (var eIdx in EndIdx)
            {
                //以结束字符为依据
                foreach (var sIdx in StartIdx)
                {
                    if (sIdx < eIdx)
                    {
                        //开始位于结束之前，但是，也必须在上一个结束位置之后
                        if (PreviewEndIdx == -1 || sIdx > PreviewEndIdx)
                        {
                            var ner = string.Empty;
                            for (int WordIdx = sIdx; WordIdx < eIdx + rule.EndWord.Count; WordIdx++)
                            {
                                ner += paragragh[WordIdx].cont;
                            }
                            if (rule.MaxWordLength != 0)
                            {
                                if ((eIdx + rule.EndWord.Count - sIdx) > rule.MaxWordLength) continue;
                            }
                            Rtn.Add(ner);
                            PreviewEndIdx = eIdx;
                        }
                    }
                }
            }
        }
        return Rtn.Distinct().ToList();
    }
    /// <summary>
    /// 根据规则查找开始索引
    /// </summary>
    /// <param name="rule"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    static List<int> SearchMatchIndex(List<struWordNER> rule, List<struWordNER> content)
    {
        var rtn = new List<int>();
        if (content.Count < rule.Count) return rtn;
        for (int ScanStartIdx = 0; ScanStartIdx < content.Count; ScanStartIdx++)
        {
            if (ScanStartIdx + rule.Count > content.Count) break;
            var IsMatch = true;
            for (int ruleIdx = 0; ruleIdx < rule.Count; ruleIdx++)
            {
                var TestIdx = ScanStartIdx + ruleIdx;
                if (!string.IsNullOrEmpty(rule[ruleIdx].pos))
                {
                    //"S-Ns","B-Ns"
                    if (!content[TestIdx].pos.Equals(rule[ruleIdx].pos))
                    {
                        IsMatch = false;
                        break;
                    }
                }
                if (!String.IsNullOrEmpty(rule[ruleIdx].cont))
                {
                    if (!content[TestIdx].cont.Equals(rule[ruleIdx].cont))
                    {
                        IsMatch = false;
                        break;
                    }
                }

            }
            if (IsMatch) rtn.Add(ScanStartIdx);
        }
        return rtn;
    }
}