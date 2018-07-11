using System;
using System.Collections.Generic;

public class FactorItem<T>
{
    /// <summary>
    /// 指标权重
    /// </summary>
    public int weight = 5;
    /// <summary>
    /// 使用最上位的多少数据
    /// </summary>
    public int UseTopRank = 10;

    /// <summary>
    /// 评分字典
    /// </summary>
    public Dictionary<T, int> ScoreDict;
    /// <summary>
    /// 候选词变换为指标
    /// </summary>
    public Func<string, T> Transform;
    /// <summary>
    /// 训练用内部字典
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="int"></typeparam>
    /// <returns></returns>
    Dictionary<T, int> TraningCountDict = new Dictionary<T, int>();
    /// <summary>
    /// 复位
    /// /// </summary>
    public void Reset()
    {
        TraningCountDict.Clear();
        ScoreDict = null;
    }
    /// <summary>
    /// 重新计算
    /// </summary>
    public void ReComputeScoreDict()
    {
        ScoreDict = Utility.ConvertRankToCIDict(Utility.FindTop(UseTopRank, TraningCountDict));
    }
    /// <summary>
    /// 加入训练内容
    /// </summary>
    /// <param name="TraningValue"></param>
    public void AddTraining(string TraningValue)
    {
        var wl = Transform(TraningValue);
        if (TraningCountDict.ContainsKey(wl))
        {
            TraningCountDict[wl] = TraningCountDict[wl] + 1;
        }
        else
        {
            TraningCountDict.Add(wl, 1);
        }
    }
    /// <summary>
    /// 评价
    /// </summary>
    /// <param name="Candidate"></param>
    /// <returns></returns>
    public int Evaluate(String Candidate)
    {
        if (Transform == null) return 0;
        var value = Transform(Candidate);
        if (ScoreDict == null) ReComputeScoreDict();
        if (ScoreDict.ContainsKey(value))
        {
            return ScoreDict[value] * weight;
        }
        return 0;
    }

    public override string ToString()
    {
        var rtn = "";
        foreach (var item in ScoreDict)
        {
            rtn += item.Key + "\t" + item.Value + "%" + System.Environment.NewLine;
        }
        return rtn;

    }

}