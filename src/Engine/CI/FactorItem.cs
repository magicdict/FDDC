using System;
using System.Collections.Generic;

public class FactorItem<T>
{
    /// <summary>
    /// 指标权重
    /// </summary>
    public int weight;
    /// <summary>
    /// 评分字典
    /// </summary>
    public Dictionary<T, int> ScoreDict;

    public Func<string, T> Transform;

    public int Evaluate(String Candidate)
    {
        if (Transform == null) return 0;
        var value = Transform(Candidate);
        if (ScoreDict != null)
        {
            if (ScoreDict.ContainsKey(value))
            {
                return ScoreDict[value] * weight;
            }
        }
        return 0;
    }
}