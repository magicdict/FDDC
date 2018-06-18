using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;
using JiebaNet.Segmenter.PosSeg;

public class EntitySelf{
        static PosSegmenter posSeg = new PosSegmenter();
    //甲方乙方首单词统计
    static Dictionary<String, int> FirstWordPosDict = new Dictionary<String, int>();
    static Dictionary<int, int> WordLengthDict = new Dictionary<int, int>();
    //词数
    static Dictionary<int, int> WordCountDict = new Dictionary<int, int>();
    static Dictionary<String, int> LastWordDict = new Dictionary<String, int>();
    static Dictionary<String, int> WordFlgsDict = new Dictionary<String, int>();

    public static void Init()
    {
        FirstWordPosDict.Clear();
        WordLengthDict.Clear();
        LastWordDict.Clear();
        WordCountDict.Clear();
        WordFlgsDict.Clear();
    }

    public static void PutEntityWordPerperty(string Word)
    {
        if (String.IsNullOrEmpty(Word)) return;
        var words = posSeg.Cut(Word);
        if (words.Count() > 0)
        {
            var pos = words.First().Flag;
            if (FirstWordPosDict.ContainsKey(pos))
            {
                FirstWordPosDict[pos] = FirstWordPosDict[pos] + 1;
            }
            else
            {
                FirstWordPosDict.Add(pos, 1);
            }

            var wl = Word.Length;
            if (WordLengthDict.ContainsKey(wl))
            {
                WordLengthDict[wl] = WordLengthDict[wl] + 1;
            }
            else
            {
                WordLengthDict.Add(wl, 1);
            }

            var wc = words.Count();
            if (WordCountDict.ContainsKey(wc))
            {
                WordCountDict[wc] = WordCountDict[wc] + 1;
            }
            else
            {
                WordCountDict.Add(wc, 1);
            }

            var lastword = words.Last().Word;
            if (LastWordDict.ContainsKey(lastword))
            {
                LastWordDict[lastword] = LastWordDict[lastword] + 1;
            }
            else
            {
                LastWordDict.Add(lastword, 1);
            }

            var wordflgs = "";
            foreach (var item in words)
            {
                wordflgs += item.Flag + "/";
            }
            if (WordFlgsDict.ContainsKey(wordflgs))
            {
                WordFlgsDict[wordflgs] = WordFlgsDict[wordflgs] + 1;
            }
            else
            {
                WordFlgsDict.Add(wordflgs, 1);
            }

        }
    }

    public static void WriteFirstAndLengthWordToLog()
    {
        Program.Training.WriteLine("首词词性统计：");
        Utility.FindTop(5, FirstWordPosDict);
        Program.Training.WriteLine("词长统计：");
        Utility.FindTop(5, WordLengthDict);
        Program.Training.WriteLine("词尾统计：");
        Utility.FindTop(5, LastWordDict);
        Program.Training.WriteLine("分词数统计：");
        Utility.FindTop(5, WordCountDict);
        Program.Training.WriteLine("词性统计：");
        Utility.FindTop(5, WordFlgsDict);
    }
}