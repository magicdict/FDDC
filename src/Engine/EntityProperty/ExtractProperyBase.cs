using System;
using System.Collections.Generic;
using static LocateProperty;

public class ExtractProperyBase
{

    public List<LocAndValue<String>> CandidateWord = new List<LocAndValue<String>>();


    //先导词（直接取先导词的后面的内容）
    public string[] LeadingColonKeyWordList = new string[] { };

    /// <summary>
    /// //先导词（在中文括号之中）
    /// </summary>
    public string[] LeadingColonKeyWordListInChineseBrackets = new string[] { };


    //先导词（直接取先导词的后面的内容）
    public string[] TrailingWordList = new string[] { };

    public struStartEndStringFeature[] StartEndFeature = new struStartEndStringFeature[] { };

    public struct struStartEndStringFeature
    {
        //需要提取的内容，外部的开始符号
        public String StartWith;
        //需要提取的内容，外部的结束符号
        public String EndWith;
    }
    public List<struStartEndResultDetail> StartEndResultList = new List<struStartEndResultDetail>();

    public struct struStartEndResultDetail
    {
        public struStartEndStringFeature Feature;
        public List<String> CandidateWord;
    }

    //符号包裹特征
    public struct struMarkFeature
    {
        //需要提取的内容，外部的开始符号
        public String MarkStartWith;
        //需要提取的内容，外部的结束符号
        public String MarkEndWith;

        //内部2次鉴证用开始字符
        public String InnerStartWith;

        //内部2次鉴证用结束字符
        public String InnerEndWith;

    }

    public struMarkFeature[] MarkFeature = new struMarkFeature[] { };

}