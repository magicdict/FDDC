using System.Collections.Generic;
using System.Linq;
using JiebaNet.Segmenter.PosSeg;

/// <summary>
/// 可信度
/// </summary>
public class CIBase
{
    //1.从长度上考量
    //2.从首词词性上考量


    /// <summary>
    /// 实体长度权重
    /// </summary>
    private int WordLengthWeight = 25;
    /// <summary>
    /// 长度和分数字典
    /// </summary>
    public Dictionary<int, int> WordLengthScore;


    /// <summary>
    /// 首词词性权重
    /// </summary>
    private int FirstWordPosWeight = 50;

    /// <summary>
    /// 首词词性和分数字典
    /// </summary>
    public Dictionary<string, int> FirstWordPosScore;


    /// <summary>
    /// 词数权重
    /// </summary>
    private int WordCountWeight = 25;

    /// <summary>
    /// 词数和分数字典
    /// </summary>
    public Dictionary<int, int> WordCountScore;

    public struct Factors
    {
        /// <summary>
        /// 实体长度
        /// </summary>
        public int WordLength;
        /// <summary>
        /// 首词词性
        /// </summary>
        public string FirstWordPos;
        /// <summary>
        /// 词数
        /// </summary>
        public int WordCount;

    }


    /// <summary>
    /// 评价
    /// </summary>
    /// <param name="factors"></param>
    /// <returns></returns>
    public int Predict(string candidate)
    {
        Factors factors = GetFactors(candidate);
        int Score = 0;
        if (WordLengthScore != null)
        {
            if (WordLengthScore.ContainsKey(factors.WordLength))
            {
                Score += WordLengthScore[factors.WordLength] * WordLengthWeight;
            }
        }
        if (FirstWordPosScore != null)
        {
            if (FirstWordPosScore.ContainsKey(factors.FirstWordPos))
            {
                Score += FirstWordPosScore[factors.FirstWordPos] * FirstWordPosWeight;
            }
        }
        if (WordCountScore != null)
        {
            if (WordCountScore.ContainsKey(factors.WordCount))
            {
                Score += WordCountScore[factors.WordCount] * WordCountWeight;
            }
        }
        return Score / 100;
    }
    PosSegmenter posSeg = new PosSegmenter();
    Factors GetFactors(string candidate)
    {
        var words = posSeg.Cut(candidate);

        var factors = new Factors();
        factors.WordLength = candidate.Length;
        factors.FirstWordPos = words.First().Flag;
        if (PosNS.NsDict.Contains(words.First().Word))
        {
            //地名修正，由于比例不高，暂时在训练的时候不修正
            factors.FirstWordPos = LTP.地名;
        }
        factors.WordCount = words.Count();
        return factors;
    }


}