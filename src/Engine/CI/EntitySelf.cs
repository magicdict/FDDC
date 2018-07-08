using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;
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
        FirstWordPosFactorItem.Transform = (x) => posSeg.Cut(x).First().Flag;
        WordLengtFactorItem.Transform = (x) => x.Length;
        WordCountFactorItem.Transform = (x) => posSeg.Cut(x).Count();
        LastWordFactorItem.Transform = (x) => posSeg.Cut(x).Last().Word;
    }

    public void PutEntityWordPerperty(string Word)
    {
        FirstWordPosFactorItem.AddTraining(Word);
        WordLengtFactorItem.AddTraining(Word);
        WordCountFactorItem.AddTraining(Word);
        LastWordFactorItem.AddTraining(Word);
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