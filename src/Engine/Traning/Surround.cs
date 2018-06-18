using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;
using JiebaNet.Segmenter;
using JiebaNet.Segmenter.PosSeg;

public class Surround
{

    Dictionary<string, int> LeadingWordDict = new Dictionary<String, int>();

    Dictionary<string, int> TrailingWordDict = new Dictionary<String, int>();

    public void Init()
    {
        LeadingWordDict.Clear();
        TrailingWordDict.Clear();
    }
    public void AnlayzeEntitySurroundWords(HTMLEngine.MyRootHtmlNode root, string KeyWord)
    {
        Program.Training.WriteLine("关键字：[" + KeyWord + "]");
        JiebaSegmenter segmenter = new JiebaSegmenter();
        segmenter.AddWord(KeyWord);
        PosSegmenter posSeg = new PosSegmenter(segmenter);
        foreach (var paragrah in root.Children)
        {
            var segments = posSeg.Cut(paragrah.FullText.NormalizeKey()).ToList();  // 默认为精确模式
            //寻找关键字的位置
            for (int i = 0; i < segments.Count; i++)
            {
                if (segments[i].Equals(KeyWord))
                {
                    //前5个词语和后五个词语
                    var startInx = Math.Max(0, i - 5);
                    var EndInx = Math.Min(i + 5, segments.Count);
                    for (int s = startInx; s < i; s++)
                    {
                        if (LeadingWordDict.ContainsKey(segments[s].Word))
                        {
                            LeadingWordDict[segments[s].Word]++;
                        }
                        else
                        {
                            LeadingWordDict.Add(segments[s].Word, 1);
                        }
                        Program.Training.WriteLine("前导关键字：[" + segments[s] + "]");

                        //特别关注动词和冒号的情况
                        if (segments[s].Flag == WordUtility.动词)
                        {
                            Program.Training.WriteLine("前导动词:" + segments[s].Word);
                        }
                        if (segments[s].Word == "：")
                        {
                            var leading = "";
                            for (int l = startInx; l < s; l++)
                            {
                                leading += segments[l];
                            }
                            Program.Training.WriteLine("冒号前导词：" + leading);
                        }
                    }
                    Program.Training.WriteLine("关键字：[" + KeyWord + "]");
                    for (int s = i + 1; s < EndInx; s++)
                    {
                        if (TrailingWordDict.ContainsKey(segments[s].Word))
                        {
                            TrailingWordDict[segments[s].Word]++;
                        }
                        else
                        {
                            TrailingWordDict.Add(segments[s].Word, 1);
                        }
                        Program.Training.WriteLine("后续关键字：[" + segments[s] + "]");
                    }

                }
            }
        }
    }

    public void WriteTop(int top)
    {
        Utility.FindTop(top, LeadingWordDict);
        Utility.FindTop(top, TrailingWordDict);
    }

}