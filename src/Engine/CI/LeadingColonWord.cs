using System;
using System.Collections.Generic;
using System.IO;
using JiebaNet.Segmenter;
using static Utility;

public class LeadingColonWord
{
    public Dictionary<string, int> LeadingWordDict = new Dictionary<String, int>();
    JiebaNet.Segmenter.PosSeg.PosSegmenter pos = new JiebaNet.Segmenter.PosSeg.PosSegmenter();
    /// <summary>
    /// 所有可能出现的 XXX：形式的前导词列表
    /// </summary>
    public void AnlayzeLeadingWord(AnnouceDocument doc, String searchKey)
    {
        if (!File.Exists(doc.TextFileName)) return;
        var SR = new StreamReader(doc.TextFileName);
        while (!SR.EndOfStream)
        {
            var line = SR.ReadLine();
            var idx = line.IndexOf("：");
            if (idx != -1)
            {
                var LeadingWord = line.Substring(0, idx);
                var keyword = line.Substring(idx + 1);
                keyword = keyword.Trim();
                if (!keyword.NormalizeTextResult().Equals(searchKey.NormalizeTextResult())) continue;
                var leadwords = pos.Cut(LeadingWord);
                LeadingWord = "";
                //去除（一）合同名称 2、备查文件
                foreach (var word in leadwords)
                {
                    if (word.Flag == LTPTrainingNER.词性标点 || word.Flag == LTPTrainingNER.数词)
                    {
                        LeadingWord = "";
                    }
                    else
                    {
                        LeadingWord += word.Word;
                    }
                }
                LeadingWord = LeadingWord.Trim();
                if (String.IsNullOrEmpty(LeadingWord)) continue;
                if (LeadingWordDict.ContainsKey(LeadingWord))
                {
                    LeadingWordDict[LeadingWord] = LeadingWordDict[LeadingWord] + 1;
                }
                else
                {
                    LeadingWordDict.Add(LeadingWord, 1);
                }
            }

        }
    }

    public void WriteToLog(StreamWriter logger)
    {
        var ranks = Utility.FindTop(5, LeadingWordDict);
        foreach (var rank in ranks)
        {
            logger.WriteLine(rank.ToString());
        }
    }

}