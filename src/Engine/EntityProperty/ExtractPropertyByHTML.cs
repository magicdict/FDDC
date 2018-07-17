using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static HTMLEngine;
using static LocateProperty;

public class ExtractPropertyByHTML : ExtractProperyBase
{
    public void Extract(MyRootHtmlNode root)
    {
        CandidateWord.Clear();
        //先导词列表
        if (LeadingColonKeyWordList.Length > 0) ExtractByColonKeyWord(root);
        //结尾词列表
        if (TrailingWordList.Length > 0) ExtractByTrailingKeyWord(root);
        //是否有符号包裹特征
        if (MarkFeature.Length > 0) ExtractByMarkFeature(root);
        //开始字符结束字符
        if (StartEndFeature.Length > 0) ExtractByStartEndStringFeature(root);
        //正则表达式检索
        if (RegularExpressFeature.Length > 0) ExtractByRegularExpressFeature(root);
    }

    public static bool HasWord(string KeyWord, MyRootHtmlNode root)
    {
        var paragrahIdList = new List<int>();
        foreach (var paragrah in root.Children)
        {
            //从各个段落的内容中取得：内容包含了内置列表，所以，这里不再重复
            foreach (var contentNode in paragrah.Children)
            {
                if (contentNode.TableId == -1)
                {
                    if (contentNode.Content.IndexOf(KeyWord) != -1) return true;
                }
            }
        }
        return false;
    }


    /// <summary>
    /// 指定词语出现的次数
    /// /// </summary>
    /// <param name="KeyWord"></param>
    /// <param name="root"></param>
    /// <returns></returns>
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
                    if (contentNode.Content.IndexOf(KeyWord) != -1) paragrahIdList.Add(contentNode.PositionId);
                }
            }
        }
        return paragrahIdList;
    }

    /// <summary>
    /// 寻找字符的位置信息
    /// </summary>
    /// <param name="KeyWord"></param>
    /// <param name="root"></param>
    /// <returns></returns>
    public static List<LocAndValue<String>> FindWordLoc(string KeyWord, MyRootHtmlNode root)
    {
        var paragrahIdList = new List<LocAndValue<String>>();
        foreach (var paragrah in root.Children)
        {
            //从各个段落的内容中取得：内容包含了内置列表，所以，这里不再重复
            foreach (var contentNode in paragrah.Children)
            {
                if (contentNode.TableId == -1)
                {
                    var Idx = contentNode.Content.IndexOf(KeyWord);
                    if (Idx != -1)
                    {
                        var Loc = new LocAndValue<String>()
                        {
                            Value = KeyWord,
                            Loc = contentNode.PositionId,
                            StartIdx = Idx,
                        };
                        paragrahIdList.Add(Loc);
                    }
                }
            }
        }
        return paragrahIdList;
    }


    /// <summary>
    /// 先导词
    /// </summary>
    /// <param name="root"></param>
    void ExtractByColonKeyWord(MyRootHtmlNode root)
    {
        foreach (var word in LeadingColonKeyWordList)
        {
            Func<String, List<String>> ExtractMethod = (x) =>
            {
                var strlist = new List<String>();
                if (Utility.GetStringAfter(x, word) != String.Empty) strlist.Add(Utility.GetStringAfter(x, word));
                return strlist;
            };
            SearchNormalContent(root, ExtractMethod);
        }
    }


    /// <summary>
    /// 结尾词
    /// </summary>
    /// <param name="root"></param>
    void ExtractByTrailingKeyWord(MyRootHtmlNode root)
    {
        foreach (var word in TrailingWordList)
        {
            Func<String, List<String>> ExtractMethod = (x) =>
            {
                var strlist = new List<String>();
                if (Utility.GetStringBefore(x, word) != String.Empty) strlist.Add(Utility.GetStringBefore(x, word));
                return strlist;
            };
            SearchNormalContent(root, ExtractMethod);
        }
    }



    /// <summary>
    /// 检索流程方法
    /// </summary>
    /// <param name="root">HTML根</param>
    /// <param name="ExtractMethod">特定检索方法(HTML内容，候补词列表)</param>
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
                    foreach (var item in candidate)
                    {
                        CandidateWord.Add(new LocAndValue<String>()
                        {
                            Loc = contentNode.PositionId,
                            Value = item
                        });
                    }
                }
            }
        }
    }




    /// <summary>
    /// 符号包裹
    /// </summary>
    /// <param name="root"></param>
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

    /// <summary>
    /// 开始结尾特征词
    /// </summary>
    /// <param name="root"></param>
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
                StartEndResultList.Add(detail);
                return list;
            };
            SearchNormalContent(root, ExtractMethod);
        }
    }

    /// <summary>
    /// 正则表达式抽取
    /// </summary>
    /// <param name="root"></param>
    void ExtractByRegularExpressFeature(MyRootHtmlNode root)
    {
        foreach (var regularfeature in RegularExpressFeature)
        {
            //特定检索方法(HTML内容，候补词列表)
            Func<String, List<String>> ExtractMethod = (x) =>
            {
                return RegularExFinder(0, x, regularfeature).Select(y => y.Value).ToList();
            };
            SearchNormalContent(root, ExtractMethod);
        }
    }


    public static List<LocAndValue<String>> FindRegularExpressLoc(struRegularExpressFeature KeyWord, MyRootHtmlNode root)
    {
        var list = new List<LocAndValue<String>>();
        foreach (var paragrah in root.Children)
        {
            //从各个段落的内容中取得：内容包含了内置列表，所以，这里不再重复
            foreach (var contentNode in paragrah.Children)
            {
                list.AddRange(RegularExFinder(contentNode.PositionId, contentNode.Content, KeyWord));
            }
        }
        return list;
    }


    public static List<LocAndValue<String>> RegularExFinder(int loc, string x, struRegularExpressFeature regularfeature)
    {
        var list = new List<LocAndValue<String>>();
        var reglist = RegularTool.GetRegular(x, regularfeature.RegularExpress);
        foreach (var reg in reglist)
        {
            //根据前后词语进行过滤
            if (regularfeature.LeadingWordList != null)
            {
                //前置词语
                foreach (var leading in regularfeature.LeadingWordList)
                {
                    if (reg.Index - leading.Length >= 0)
                    {
                        var word = x.Substring(reg.Index - leading.Length, leading.Length);
                        if (word.Equals(leading))
                        {
                            var value = x.Substring(reg.Index - leading.Length, leading.Length + reg.Length);
                            var Loc = new LocAndValue<String>()
                            {
                                Value = value,
                                StartIdx = reg.Index - leading.Length,
                                Loc = loc
                            };
                            list.Add(Loc);
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            if (regularfeature.TrailingWordList != null)
            {
                //后置词语
                foreach (var trailing in regularfeature.TrailingWordList)
                {
                    if (reg.Index + reg.Length + trailing.Length < x.Length)
                    {
                        var word = x.Substring(reg.Index + reg.Length, trailing.Length);
                        if (word.Equals(trailing))
                        {
                            var value = x.Substring(reg.Index, trailing.Length + reg.Length);
                            var Loc = new LocAndValue<String>()
                            {
                                Value = value,
                                StartIdx = reg.Index,
                                Loc = loc
                            };
                            list.Add(Loc);
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
        }
        return list;
    }
}