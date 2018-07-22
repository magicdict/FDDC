using System;
using System.Collections.Generic;
using static LocateProperty;

public class ExtractProperyBase
{
    /// <summary>
    /// 候选词
    /// </summary>
    /// <returns></returns>
    public List<LocAndValue<String>> CandidateWord = new List<LocAndValue<String>>();


    /// <summary>
    /// 先导词（直接取先导词的后面的内容）
    /// </summary>
    /// <value></value>
    public string[] LeadingColonKeyWordList = new string[] { };

    /// <summary>
    /// //先导词（在中文括号之中）
    /// </summary>
    public string[] LeadingColonKeyWordListInChineseBrackets = new string[] { };


    /// <summary>
    /// 结尾词
    /// </summary>
    /// <value></value>
    public string[] TrailingWordList = new string[] { };

    public struStartEndStringFeature[] StartEndFeature = new struStartEndStringFeature[] { };

    public struct struStartEndStringFeature
    {
        //需要提取的内容，外部的开始符号
        public String StartWith;
        //需要提取的内容，外部的结束符号
        public String EndWith;
    }

    /// <summary>
    /// 开始结尾特征的检索结果结构体
    /// </summary>
    public struct struStartEndResultDetail
    {
        /// <summary>
        /// 特征
        /// </summary>
        public struStartEndStringFeature Feature;
        /// <summary>
        /// 候选词
        /// </summary>
        public List<String> CandidateWord;
    }

    /// <summary>
    /// 开始结尾特征的检索结果
    /// </summary>
    /// <typeparam name="struStartEndResultDetail"></typeparam>
    /// <returns></returns>
    public List<struStartEndResultDetail> StartEndResultList = new List<struStartEndResultDetail>();



    /// <summary>
    /// 符号包裹特征结构体
    /// </summary>
    public struct struMarkFeature
    {
        //需要提取的内容，外部的开始符号
        public String MarkStartWith;
        //需要提取的内容，外部的结束符号
        public String MarkEndWith;

        //内部2次鉴证用开始字符
        public String InnerStartWith;

        //内部2次鉴证用结束字符
        public List<String> InnerEndWith;
    }
    /// <summary>
    /// 符号包裹特征列表
    /// </summary>
    /// <value></value>
    public struMarkFeature[] MarkFeature = new struMarkFeature[] { };

    /// <summary>
    /// 正则表达式相关检索条件用结构体
    /// </summary>
    public struct struRegularExpressFeature
    {
        /// <summary>
        /// 正则表达式
        /// </summary>
        public string RegularExpress;
        /// <summary>
        /// 正则表达式前置词语
        /// </summary>
        public List<string> LeadingWordList;
        /// <summary>
        /// 正则表达式后置词语
        /// </summary>
        public List<string> TrailingWordList;
    }

    public struRegularExpressFeature[] RegularExpressFeature = new struRegularExpressFeature[] { };

}