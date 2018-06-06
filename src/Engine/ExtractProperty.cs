using System;
using System.Collections.Generic;
using static HTMLEngine;

public class ExtractProperty
{
    public static List<int> FindWordCnt(string KeyWord, MyRootHtmlNode root)
    {
        var paragrahIdList = new List<int>();
        foreach (var paragrah in root.Children)
        {
            //从各个段落的内容中取得：内容包含了内置列表，所以，这里不再重复
            foreach (var contentNode in paragrah.Children)
            {
                if (contentNode.TableId == -1)
                {
                    if (contentNode.Content.IndexOf(KeyWord) != -1) paragrahIdList.Add(paragrah.ParagrahId);
                }
            }
        }
        return paragrahIdList;
    }
    
    //候选词
    public List<String> CandidateWord = new List<string>();

    public void Extract(MyRootHtmlNode root)
    {
        CandidateWord.Clear();
        //先导词列表
        if (LeadingWordList.Length > 0) ExtractByLeadingKeyWord(root);
        //结尾词列表
        if (TrailingWordList.Length > 0) ExtractByTrailingKeyWord(root);
        //是否有符号包裹特征
        if (MarkFeature.Length > 0) ExtractByMarkFeature(root);
        //开始字符结束字符
        if (StartEndFeature.Length > 0) ExtractByStartEndStringFeature(root);

    }

    //先导词（直接取先导词的后面的内容）
    public string[] LeadingWordList = new string[] { };
    //先导词
    void ExtractByLeadingKeyWord(MyRootHtmlNode root)
    {
        foreach (var word in LeadingWordList)
        {
            Func<String, List<String>> ExtractMethod = (x) =>
            {
                var strlist = new List<String>();
                if (Utility.GetStringAfter(x, word) != "") strlist.Add(Utility.GetStringAfter(x, word));
                return strlist;
            };
            SearchNormalContent(root, ExtractMethod);
        }
    }

    //先导词（直接取先导词的后面的内容）
    public string[] TrailingWordList = new string[] { };
    //先导词
    void ExtractByTrailingKeyWord(MyRootHtmlNode root)
    {
        foreach (var word in TrailingWordList)
        {
            Func<String, List<String>> ExtractMethod = (x) =>
            {
                var strlist = new List<String>();
                if (Utility.GetStringBefore(x, word) != "") strlist.Add(Utility.GetStringBefore(x, word));
                return strlist;
            };
            SearchNormalContent(root, ExtractMethod);
        }
    }



    //Search Normal Content
    void SearchNormalContent(MyRootHtmlNode root, Func<String, List<String>> ExtractMethod)
    {
        foreach (var paragrah in root.Children)
        {
            //从各个段落的内容中取得：内容包含了内置列表，所以，这里不再重复
            foreach (var contentNode in paragrah.Children)
            {
                if (contentNode.TableId == -1)
                {
                    //非表格
                    var candidate = ExtractMethod(contentNode.Content);
                    if (candidate.Count != 0) CandidateWord.AddRange(candidate);
                }
            }
        }
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

    //符号包裹
    void ExtractByMarkFeature(MyRootHtmlNode root)
    {
        foreach (var word in MarkFeature)
        {
            Func<String, List<String>> ExtractMethod = (x) =>
            {
                var strlist = new List<String>();
                foreach (var strContent in RegularTool.GetMultiValueBetweenMark(x, word.MarkStartWith, word.MarkEndWith))
                {
                    if (word.InnerStartWith != null)
                    {
                        if (!strContent.StartsWith(word.InnerStartWith)) continue;
                    }
                    if (word.InnerEndWith != null)
                    {
                        if (!strContent.EndsWith(word.InnerEndWith)) continue;
                    }
                    strlist.Add(strContent);
                }
                return strlist;
            };
            SearchNormalContent(root, ExtractMethod);
        }

    }


    public struct struStartEndStringFeature
    {
        //需要提取的内容，外部的开始符号
        public String StartWith;
        //需要提取的内容，外部的结束符号
        public String EndWith;
    }

    public struStartEndStringFeature[] StartEndFeature = new struStartEndStringFeature[] { };

    public List<struStartEndResultDetail> StartEndResultList = new List<struStartEndResultDetail>();

    public struct struStartEndResultDetail
    {
        public struStartEndStringFeature Feature;
        public List<String> CandidateWord;
    }
    //符号包裹
    void ExtractByStartEndStringFeature(MyRootHtmlNode root)
    {
        StartEndResultList.Clear();
        foreach (var word in StartEndFeature)
        {
            Func<String, List<String>> ExtractMethod = (x) =>
            {
                var list = RegularTool.GetMultiValueBetweenString(x, word.StartWith, word.EndWith);
                var detail = new struStartEndResultDetail();
                detail.Feature = word;
                detail.CandidateWord = list;
                return list;
            };
            SearchNormalContent(root, ExtractMethod);
        }
    }

}