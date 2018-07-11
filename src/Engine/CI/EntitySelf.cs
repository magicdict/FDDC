using System;
using System.Collections.Generic;
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

    public void InitFactorItem()
    {
        FirstWordPosFactorItem = new FactorItem<String>();
        FirstWordPosFactorItem.Transform = (x) => posSeg.Cut(x).First().Flag;
        WordLengtFactorItem = new  FactorItem<int>();
        WordLengtFactorItem.Transform = (x) => x.Length;
        WordCountFactorItem = new FactorItem<int> ();
        WordCountFactorItem.Transform = (x) => posSeg.Cut(x).Count();
        LastWordFactorItem = new FactorItem<String>();
        LastWordFactorItem.Transform = (x) => posSeg.Cut(x).Last().Word;
    }

    public void PutEntityWordPerperty(string Word)
    {
        if (String.IsNullOrEmpty(Word)) return;
        FirstWordPosFactorItem.AddTraining(Word);
        WordLengtFactorItem.AddTraining(Word);
        WordCountFactorItem.AddTraining(Word);
        LastWordFactorItem.AddTraining(Word);
    }

    public void Commit(){
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
}