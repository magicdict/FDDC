using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JiebaNet.Segmenter.PosSeg;
using static CIBase;

public class EntitySelf
{
    PosSegmenter posSeg = new PosSegmenter();
    public FactorItem<String> FirstWordPosFactorItem;
    public FactorItem<int> WordLengtFactorItem;
    public FactorItem<int> WordCountFactorItem;
    public FactorItem<String> LastWordFactorItem;

    public int MaxLength = 999;

    public string MaxLengthWord = "";

    public int MinLength = -1;

    public string MinLengthWord = "";


    public void InitFactorItem()
    {
        FirstWordPosFactorItem = new FactorItem<String>();
        FirstWordPosFactorItem.Transform = (x) => posSeg.Cut(x).First().Flag;
        WordLengtFactorItem = new FactorItem<int>();
        WordLengtFactorItem.Transform = (x) => x.Length;
        WordCountFactorItem = new FactorItem<int>();
        WordCountFactorItem.Transform = (x) => posSeg.Cut(x).Count();
        LastWordFactorItem = new FactorItem<String>();
        LastWordFactorItem.Transform = (x) => posSeg.Cut(x).Last().Word;
        MaxLength = -1;
        MinLength = 999;
    }

    public void PutEntityWordPerperty(string Word)
    {
        if (String.IsNullOrEmpty(Word)) return;
        FirstWordPosFactorItem.AddTraining(Word);
        WordLengtFactorItem.AddTraining(Word);
        WordCountFactorItem.AddTraining(Word);
        LastWordFactorItem.AddTraining(Word);
        Word = Utility.TrimEnglish(Word);
        if (Word.Length > MaxLength)
        {
            MaxLength = Word.Length;
            MaxLengthWord = Word;
        }
        if (Word.Length < MinLength)
        {
            MinLength = Word.Length;
            MinLengthWord = Word;
        }
    }

    public void Commit()
    {
        FirstWordPosFactorItem.ReComputeScoreDict();
        WordLengtFactorItem.ReComputeScoreDict();
        WordCountFactorItem.ReComputeScoreDict();
        LastWordFactorItem.ReComputeScoreDict();
    }

    public CIBase GetStardardCI()
    {
        var ci = new CIBase();
        ci.IntFactors.Add(WordCountFactorItem);
        ci.IntFactors.Add(WordLengtFactorItem);
        ci.StringFactors.Add(FirstWordPosFactorItem);
        ci.StringFactors.Add(LastWordFactorItem);
        return ci;
    }

    /// <summary>
    /// 日志输出
    /// </summary>
    /// <param name="logger"></param>
    public void WriteToLog(StreamWriter logger)
    {
        logger.WriteLine("最大长度：" + MaxLength);
        logger.WriteLine("最大长度单词：[" + MaxLengthWord + "]");
        logger.WriteLine("最小长度：" + MinLength);
        logger.WriteLine("最小长度单词：[" + MinLengthWord + "]");
        logger.WriteLine("首词词性：");
        logger.WriteLine(FirstWordPosFactorItem.ToString());
        logger.WriteLine("词长：");
        logger.WriteLine(WordLengtFactorItem.ToString());
        logger.WriteLine("词数：");
        logger.WriteLine(WordCountFactorItem.ToString());
        logger.WriteLine("最后单词：");
        logger.WriteLine(LastWordFactorItem.ToString());

    }

}