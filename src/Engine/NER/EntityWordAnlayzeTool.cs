using FDDC;
using JiebaNet.Analyser;
using JiebaNet.Segmenter;
using JiebaNet.Segmenter.PosSeg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
public static class EntityWordAnlayzeTool
{

    public const string 地名 = "ns";
    public const string 副词 = "d";
    public const string 助词 = "ul";
    public const string 英语 = "eng";

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
        FindTop(5, FirstWordPosDict);
        Program.Training.WriteLine("词长统计：");
        FindTop(5, WordLengthDict);
        Program.Training.WriteLine("词尾统计：");
        FindTop(5, LastWordDict);
        Program.Training.WriteLine("分词数统计：");
        FindTop(5, WordCountDict);
        Program.Training.WriteLine("词性统计：");
        FindTop(5, WordFlgsDict);
    }

    //寻找Top5
    public static void FindTop<T>(int n, Dictionary<T, int> dict)
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

    public static void AnlayzeEntitySurroundWords(HTMLEngine.MyRootHtmlNode root, string KeyWord)
    {
        Program.Training.WriteLine("关键字：[" + KeyWord + "]");
        JiebaSegmenter segmenter = new JiebaSegmenter();
        segmenter.AddWord(KeyWord);
        foreach (var paragrah in root.Children)
        {
            var segments = segmenter.Cut(paragrah.FullText.NormalizeKey()).ToList();  // 默认为精确模式
            //Console.WriteLine("【精确模式】：{0}", string.Join("/ ", segments));
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
                        Program.Training.WriteLine("前导关键字：[" + segments[s] + "]");
                        if (segments[s] == "：")
                        {
                            var leading = "";
                            for (int l = startInx; l < s; l++)
                            {
                                leading += segments[l];
                            }
                            Console.WriteLine("冒号前导词：" + leading);
                        }
                    }
                    Program.Training.WriteLine("关键字：[" + KeyWord + "]");
                    for (int s = i + 1; s < EndInx; s++)
                    {
                        Program.Training.WriteLine("后续关键字：[" + segments[s] + "]");
                    }

                }
            }
        }
    }

    public static string GetMainWordSentence(string OrgString)
    {
        var MainWordSentence = "";
        var pos = new JiebaNet.Segmenter.PosSeg.PosSegmenter();
        var list = pos.Cut(OrgString);
        foreach (var word in list)
        {
            //去除“副词”和“了”之后的句子
            if (word.Flag != EntityWordAnlayzeTool.助词 &&
                word.Flag != EntityWordAnlayzeTool.副词)
            {
                MainWordSentence += word.Word;
            }
        }
        return MainWordSentence;
    }

    public static string TrimEnglish(string OrgString)
    {
        var MainWordSentence = "";
        var pos = new JiebaNet.Segmenter.PosSeg.PosSegmenter();
        var list = pos.Cut(OrgString);
        foreach (var word in list)
        {
            //去除“副词”和“了”之后的句子
            if (word.Flag != 英语)
            {
                MainWordSentence += word.Word;
            }
        }
        return MainWordSentence;
    }

    public static string TrimLeadingUL(string OrgString)
    {
        var MainWordSentence = "";
        var pos = new JiebaNet.Segmenter.PosSeg.PosSegmenter();
        var list = pos.Cut(OrgString);
        var HasStart = false;
        foreach (var word in list)
        {
            if (HasStart || (word.Flag != EntityWordAnlayzeTool.助词))
            {
                HasStart = true;
                MainWordSentence += word.Word;
            }
        }
        return MainWordSentence;
    }


    public static void ConsoleWritePos(string OrgString)
    {
        var pos = new JiebaNet.Segmenter.PosSeg.PosSegmenter();
        var list = pos.Cut(OrgString);
        foreach (var item in list)
        {
            Console.WriteLine(item.Word + ":" + item.Flag);
        }
    }

}