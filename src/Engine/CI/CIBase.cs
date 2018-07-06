using System.Collections.Generic;
using System.Linq;
using JiebaNet.Segmenter.PosSeg;

/// <summary>
/// 可信度
/// </summary>
public class CIBase
{

    /// <summary>
    /// 整形指标
    /// </summary>
    /// <returns></returns>
    public List<FactorItem<int>> IntFactors = new List<FactorItem<int>>();

    /// <summary>
    /// 字符型指标
    /// </summary>
    /// <returns></returns>
    public List<FactorItem<string>> StringFactors = new List<FactorItem<string>>();

    /// <summary>
    /// 评价
    /// </summary>
    /// <param name="factors"></param>
    /// <returns></returns>
    public int Predict(string Candidate)
    {
        int score = 0;
        //整型因素
        foreach (var item in IntFactors)
        {
            score += item.Evaluate(Candidate);
        }
        //字符因素
        foreach (var item in StringFactors)
        {
            score += item.Evaluate(Candidate);
        }
        return score;
    }
}