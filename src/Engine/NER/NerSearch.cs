using System;
using System.Collections.Generic;
using System.Linq;
using static LocateProperty;
using static NerMap;


/// <summary>
/// 根据周围词语来定位实体
/// </summary>
public class NerSearch
{
    public struct WordRule
    {
        /// <summary>
        /// 基准词描述
        /// </summary>
        public List<String> Description;
        /// <summary>
        /// 具体的词
        /// </summary>
        public List<String> Word;
    }


    public struct SearchRule
    {
        /// <summary>
        /// 基础实体
        /// </summary>
        public WordRule BaseWord;
        /// <summary>
        /// 是否向前寻找
        /// </summary>
        public bool SearchForward;
        /// <summary>
        /// 目标实体
        /// </summary>
        public WordRule Target;

        public Func<LocAndValue<string>, bool> Validator;
    }

    /// <summary>
    /// 检索
    /// </summary>
    /// <param name="paragragh"></param>
    /// <returns></returns>
    public static List<LocAndValue<String>> Search(AnnouceDocument doc, SearchRule rule)
    {
        var rtn = new List<LocAndValue<String>>();
        if (rule.BaseWord.Description.Count == 0)
        {
            //关键字，没有任何描述
            doc.CustomerList = LocateCustomerWord(doc.root, rule.BaseWord.Word, "关键字");
            doc.nermap.Anlayze(doc);
        }
        foreach (var paragragh in doc.nermap.ParagraghlocateDict.Values)
        {
            for (int baseIdx = 0; baseIdx < paragragh.NerList.Count; baseIdx++)
            {
                var evaluate = paragragh.NerList[baseIdx];
                if (!IsMatch(rule.BaseWord, evaluate)) continue;

                if (rule.SearchForward)
                {
                    //向前
                    for (int ScanIdx = baseIdx + 1; ScanIdx < paragragh.NerList.Count; ScanIdx++)
                    {
                        evaluate = paragragh.NerList[ScanIdx];
                        if (IsMatch(rule.Target, evaluate))
                        {
                            if (rule.Validator == null)
                            {
                                rtn.Add(evaluate);
                                break;
                            }
                            else
                            {
                                if (rule.Validator(evaluate))
                                {
                                    rtn.Add(evaluate);
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //向后
                    for (int ScanIdx = baseIdx - 1; ScanIdx > -1; ScanIdx--)
                    {
                        evaluate = paragragh.NerList[ScanIdx];
                        if (IsMatch(rule.Target, evaluate))
                        {
                            if (rule.Validator == null)
                            {
                                rtn.Add(evaluate);
                                break;
                            }
                            else
                            {
                                if (rule.Validator(evaluate))
                                {
                                    rtn.Add(evaluate);
                                    break;
                                }
                            }
                        }
                    }
                }

            }
        }
        return rtn;
    }

    static bool IsMatch<T>(WordRule rule, LocAndValue<T> evaluate)
    {
        if (rule.Description != null && rule.Description.Count != 0)
        {
            if (!rule.Description.Contains(evaluate.Description)) return false;
        }
        if (rule.Word != null && rule.Word.Count != 0)
        {
            if (!rule.Word.Contains(evaluate.Value.ToString())) return false;
        }
        return true;
    }

}