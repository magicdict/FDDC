using System;
using System.Collections.Generic;
using System.IO;
using static LTPTrainingNER;

public class LTPTrainingDP
{


    #region  DP

    public const string 主谓关系 = "SBV";
    public const string 动宾关系 = "VOB";
    public const string 间宾关系 = "IOB";
    public const string 前置宾语 = "FOB";
    public const string 兼语 = "DBL";
    public const string 定中关系 = "ATT";
    public const string 状中结构 = "ADV";
    public const string 动补结构 = "CMP";
    public const string 并列关系 = "COO";
    public const string 介宾关系 = "POB";
    public const string 左附加关系 = "LAD";
    public const string 右附加关系 = "RAD";
    public const string 独立结构 = "IS";
    public const string 核心关系 = "HED";

    public const string 句型标点 = "WP";

    public struct struWordDP
    {
        public int id;

        public string cont;

        public string pos;

        public int parent;

        public string relate;

        public override string ToString()
        {
            return cont + "/" + relate;
        }

        public struWordDP(string element)
        {
            var x = RegularTool.GetMultiValueBetweenMark(element, "\"", "\"");
            if (x.Count != 5)
            {
                //Console.WriteLine(element);
                id = int.Parse(x[0]);
                cont = "\"";    //&quot;
                pos = x[1];
                parent = int.Parse(x[2]);
                relate = x[3];
            }
            else
            {
                id = int.Parse(x[0]);
                cont = x[1];
                pos = x[2];
                parent = int.Parse(x[3]);
                relate = x[4];
            }
        }
    }
    public static List<List<struWordDP>> AnlayzeDP(string xmlfilename)
    {
        //由于结果是多个XML构成的
        //1.掉所有的<?xml version="1.0" encoding="utf-8" ?>
        //2.加入<sentence></sentence> root节点    
        var DPList = new List<List<struWordDP>>();

        if (!File.Exists(xmlfilename)) return DPList;

        var sr = new StreamReader(xmlfilename);
        List<struWordDP> WordList = null;
        while (!sr.EndOfStream)
        {
            var line = sr.ReadLine().Trim();
            if (line.StartsWith("<sent"))
            {
                if (WordList != null) DPList.Add(WordList);
                //一个新的句子
                WordList = new List<struWordDP>();
            }
            if (line.StartsWith("<word"))
            {
                var word = new struWordDP(line);
                WordList.Add(word);
            }
        }
        if (WordList != null) DPList.Add(WordList);
        sr.Close();
        return DPList;
    }
    #endregion



    /// <summary>
    /// 前导词语
    /// </summary>
    /// <typeparam name="string"></typeparam>
    /// <typeparam name="int"></typeparam>
    /// <returns></returns>
    public Dictionary<string, int> LeadingWordDict = new Dictionary<string, int>();
    /// <summary>
    /// 前导动词
    /// </summary>
    /// <typeparam name="string"></typeparam>
    /// <typeparam name="int"></typeparam>
    /// <returns></returns>
    public Dictionary<string, int> LeadingVerbWordDict = new Dictionary<string, int>();
    /// <summary>
    /// 词尾
    /// </summary>
    /// <typeparam name="string"></typeparam>
    /// <typeparam name="int"></typeparam>
    /// <returns></returns>
    public Dictionary<string, int> LastWordDict = new Dictionary<string, int>();
    /// <summary>
    /// 训练
    /// </summary>
    /// <param name="dplist"></param>
    /// <param name="KeyWord"></param>
    public void Training(List<List<struWordDP>> dplist, string KeyWord)
    {
        foreach (var paragragh in dplist)
        {
            for (int startIdx = 0; startIdx < paragragh.Count; startIdx++)
            {
                var CompWord = String.Empty;
                if (KeyWord.StartsWith(paragragh[startIdx].cont))
                {
                    //找到疑似开始位置
                    CompWord = paragragh[startIdx].cont;
                    for (int CompIdx = startIdx + 1; CompIdx < paragragh.Count; CompIdx++)
                    {
                        CompWord += paragragh[CompIdx].cont;
                        if (CompWord == KeyWord)
                        {
                            if (LastWordDict.ContainsKey(paragragh[CompIdx].ToString()))
                            {
                                LastWordDict[paragragh[CompIdx].ToString()] += 1;
                            }
                            else
                            {
                                LastWordDict.Add(paragragh[CompIdx].ToString(), 1);
                            }
                            //找到整个词语
                            break;
                        }
                        else
                        {
                            if (KeyWord.StartsWith(CompWord))
                            {
                                //继续
                                continue;
                            }
                            else
                            {
                                //跳出整个循环，寻找两一个StartIdx
                                break;
                            }
                        }
                    }
                }
                //可能是找到整个词语，或者需要找寻下一个
                if (CompWord == KeyWord && startIdx != 0)
                {
                    //前导词
                    var LW = paragragh[startIdx - 1];
                    if (LeadingWordDict.ContainsKey(LW.ToString()))
                    {
                        LeadingWordDict[LW.ToString()] += 1;
                    }
                    else
                    {
                        LeadingWordDict.Add(LW.ToString(), 1);
                    }
                    //动词
                    for (int LeadingVerbIdx = startIdx - 1; LeadingVerbIdx >= 0; LeadingVerbIdx--)
                    {
                        if (paragragh[LeadingVerbIdx].pos == LTPTrainingNER.动词)
                        {
                            LW = paragragh[LeadingVerbIdx];
                            if (LeadingVerbWordDict.ContainsKey(LW.ToString()))
                            {
                                LeadingVerbWordDict[LW.ToString()] += 1;
                            }
                            else
                            {
                                LeadingVerbWordDict.Add(LW.ToString(), 1);
                            }
                            break;
                        }
                    }
                    break;
                }
            }
        }
    }
    public void WriteToLog(StreamWriter logger)
    {
        var ranks = Utility.FindTop(5, LeadingWordDict);
        logger.WriteLine("前导词语：");
        foreach (var rank in ranks)
        {
            logger.WriteLine(rank.ToString());
        }
        ranks = Utility.FindTop(5, LeadingVerbWordDict);
        logger.WriteLine("前导动词：");
        foreach (var rank in ranks)
        {
            logger.WriteLine(rank.ToString());
        }
        ranks = Utility.FindTop(5, LastWordDict);
        logger.WriteLine("后置词语：");
        foreach (var rank in ranks)
        {
            logger.WriteLine(rank.ToString());
        }
    }
}