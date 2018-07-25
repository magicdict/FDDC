using HtmlAgilityPack;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using static ExtractProperyBase;
using JiebaNet.Segmenter.PosSeg;

public static class Utility
{
    /// <summary>
    /// 分割符号
    /// </summary>
    public const string SplitChar = "、";

    /// <summary>
    /// 排名结构体
    /// </summary>
    public struct struRankRecord<T>
    {
        public T Value;

        public int Percent;

        public int Count;

        public override string ToString()
        {
            return Percent.ToString("D2") + "%\t" + Count.ToString("D5") + "\t" + Value;
        }

    }
    /// <summary>
    /// 排名转字典
    /// </summary>
    /// <param name="ranks"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Dictionary<T, int> ConvertRankToCIDict<T>(List<Utility.struRankRecord<T>> ranks)
    {
        var rtn = new Dictionary<T, int>();
        foreach (var rank in ranks)
        {
            rtn.Add(rank.Value, rank.Percent);
        }
        return rtn;
    }

    /// <summary>
    /// 返回前N位的百分比字典
    /// </summary>
    /// <param name="n"></param>
    /// <param name="dict"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<struRankRecord<T>> FindTop<T>(int n, Dictionary<T, int> dict)
    {
        var result = new List<struRankRecord<T>>();
        if (dict.Count == 0) return result;
        var Rank = dict.Values.ToList();
        Rank.Sort();
        Rank.Reverse();
        float Total = Rank.Sum();
        var pos = Math.Min(Rank.Count, n);
        int limit = Rank[pos - 1];
        foreach (var key in dict.Keys)
        {
            if (dict[key] >= limit)
            {
                var percent = (dict[key] * 100 / Total);
                result.Add(new struRankRecord<T>() { Value = key, Percent = (int)percent, Count = dict[key] });
            }
        }
        result.Sort((x, y) => { return y.Count - x.Count; });
        return result;
    }

    /// <summary>
    /// 获得开始字符结束字符的排列组合
    /// </summary>
    /// <param name="StartStringList"></param>
    /// <param name="EndStringList"></param>
    /// <returns></returns>
    public static struStartEndStringFeature[] GetStartEndStringArray(string[] StartStringList, string[] EndStringList)
    {
        var KeyWordListArray = new struStartEndStringFeature[StartStringList.Length * EndStringList.Length];
        int cnt = 0;
        foreach (var StartString in StartStringList)
        {
            foreach (var EndString in EndStringList)
            {
                KeyWordListArray[cnt] = new struStartEndStringFeature { StartWith = StartString, EndWith = EndString };
                cnt++;
            }
        }
        return KeyWordListArray;
    }

    /// <summary>
    /// 提取某个关键字后的信息
    /// </summary>
    /// <param name="SearchLine"></param>
    /// <param name="KeyWord"></param>
    /// <param name="Exclude"></param>
    /// <returns></returns>
    public static string GetStringAfter(String SearchLine, String KeyWord, String Exclude = "")
    {

        if (Exclude != String.Empty)
        {
            if (SearchLine.IndexOf(Exclude) != -1) return String.Empty;
        }

        int index = SearchLine.IndexOf(KeyWord);
        if (index != -1)
        {
            index = index + KeyWord.Length;
            return SearchLine.Substring(index);
        }
        return String.Empty;
    }

    /// <summary>
    /// 提取某个关键字前的信息
    /// </summary>
    /// <param name="SearchLine"></param>
    /// <param name="KeyWord"></param>
    /// <param name="Exclude"></param>
    /// <returns></returns>
    public static string GetStringBefore(String SearchLine, String KeyWord, String Exclude = "")
    {
        if (Exclude != String.Empty)
        {
            if (SearchLine.IndexOf(Exclude) != -1) return String.Empty;
        }
        int index = SearchLine.IndexOf(KeyWord);
        if (index != -1)
        {
            return SearchLine.Substring(0, index);
        }
        return String.Empty;
    }

    /// <summary>
    /// 除去英语
    /// </summary>
    /// <param name="OrgString"></param>
    /// <returns></returns>
    public static string TrimEnglish(string OrgString)
    {
        var MainWordSentence = String.Empty;
        var pos = new JiebaNet.Segmenter.PosSeg.PosSegmenter();
        var list = pos.Cut(OrgString);
        foreach (var word in list)
        {
            if (word.Flag != LTPTrainingNER.英语)
            {
                MainWordSentence += word.Word;
            }
        }
        return MainWordSentence;
    }
    /// <summary>
    /// 除去词首助词
    /// </summary>
    /// <param name="OrgString"></param>
    /// <returns></returns>
    public static string TrimLeadingUL(string OrgString)
    {
        var MainWordSentence = String.Empty;
        var pos = new JiebaNet.Segmenter.PosSeg.PosSegmenter();
        var list = pos.Cut(OrgString);
        var HasStart = false;
        foreach (var word in list)
        {
            if (HasStart || (word.Flag != LTPTrainingNER.助词))
            {
                HasStart = true;
                MainWordSentence += word.Word;
            }
        }
        return MainWordSentence;
    }

    /// <summary>
    /// 将一个项目根据连词分割为两项
    /// </summary>
    /// <param name="OrgString"></param>
    /// <returns></returns>
    public static List<String> CutByPOSConection(string OrgString)
    {
        var pos = new PosSegmenter();
        var words = pos.Cut(OrgString);
        var rtn = new List<String>();
        var currentword = "";
        foreach (var item in words)
        {
            if (item.Flag == LTPTrainingNER.连词)
            {
                if (!String.IsNullOrEmpty(currentword))
                {
                    rtn.Add(currentword);
                    currentword = "";
                }
            }
            else
            {
                currentword += item.Word;
            }
        }
        if (!String.IsNullOrEmpty(currentword))
        {
            rtn.Add(currentword);
            currentword = "";
        }
        return rtn;
    }

}