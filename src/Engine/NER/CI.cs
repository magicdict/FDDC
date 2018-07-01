using System.Collections.Generic;
/// <summary>
/// 可信度
/// </summary>
public class CI
{
    //1.从长度上考量
    //2.从首词词性上考量

 
    /// <summary>
    /// 实体长度权重
    /// </summary>
    private int WordLengthWeight = 30;
    /// <summary>
    /// 长度和分数字典
    /// </summary>
    public Dictionary<int, int> WordLengthScore;


    /// <summary>
    /// 首词词性权重
    /// </summary>
    private int FirstWordPosWeight = 30;

    /// <summary>
    /// 长度和分数字典
    /// </summary>
    public Dictionary<string, int> FirstWordPosScore;

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
    }

    
    /// <summary>
    /// 评价
    /// </summary>
    /// <param name="factors"></param>
    /// <returns></returns>
    public int Predict(Factors factors)
    {
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
        return Score;
    }

}