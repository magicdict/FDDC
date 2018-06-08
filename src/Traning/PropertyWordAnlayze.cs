using FDDC;
using JiebaNet.Analyser;
using JiebaNet.Segmenter;
using JiebaNet.Segmenter.PosSeg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
public static class PropertyWordAnlayze
{
    static PosSegmenter posSeg = new PosSegmenter();
    //甲方乙方首单词统计
    static Dictionary<String, int> FirstWordPos = new Dictionary<String, int>();
    static Dictionary<int, int> WordLength = new Dictionary<int, int>();

    public static void Init()
    {
        FirstWordPos.Clear();
        WordLength.Clear();
    }

    public static void PutWord(string Word)
    {
        if (String.IsNullOrEmpty(Word)) return;
        var words = posSeg.Cut(Word);
        if (words.Count() > 0)
        {
            var pos = words.First().Flag;
            if (FirstWordPos.ContainsKey(pos))
            {
                FirstWordPos[pos] = FirstWordPos[pos] + 1;
            }
            else
            {
                FirstWordPos.Add(pos, 1);
            }
            var wl = Word.Length;
            if (WordLength.ContainsKey(wl))
            {
                WordLength[wl] = WordLength[wl] + 1;
            }
            else
            {
                WordLength.Add(wl, 1);
            }
        }
    }

    //寻找Top5
    static void FindTop<T>(int n, Dictionary<T, int> dict)
    {
        var Rank = dict.Values.ToList();
        Rank.Sort();
        Rank.Reverse();
        float Total = Rank.Sum();
        var pos = Math.Min(Rank.Count, n);
        int limit = Rank[n - 1];
        foreach (var key in dict.Keys)
        {
            if (dict[key] >= limit)
            {
                var percent = (dict[key] * 100 / Total) + "%";
                Program.Training.WriteLine(key + "(" + percent + ")");
            }
        }
    }


    public static void WriteToLog()
    {
        Program.Training.WriteLine("首词词性统计：");
        FindTop(5, FirstWordPos);
        Program.Training.WriteLine("词长统计：");
        FindTop(5, WordLength);
    }

}