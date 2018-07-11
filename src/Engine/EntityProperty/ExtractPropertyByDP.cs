using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static LTPTrainingDP;
using static HTMLEngine;
using static LocateProperty;
using static LTPTrainingNER;

public class ExtractPropertyByDP : ExtractProperyBase
{
    public struct DPKeyWord
    {
        /// <summary>
        /// 开始关键字(不包含在结果中)
        /// </summary>
        public string[] StartWord;
        /// <summary>
        /// 开始词DP属性指定
        /// </summary>
        public string[] StartDPValue;
        /// <summary>
        /// 结束关键字(包含在结果中)
        /// </summary>
        public string[] EndWord;
        /// <summary>
        /// 结尾词DP属性指定
        /// </summary>
        public string[] EndDPValue;
    }


    /// <summary>
    /// 使用开始结束关键字进行抽取
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="dplist"></param>
    public void StartWithKey(List<DPKeyWord> keys, List<List<struWordDP>> dplist)
    {

        foreach (var key in keys)
        {
            foreach (var paragragh in dplist)
            {
                bool isStart = false;
                string x = String.Empty;
                foreach (var word in paragragh)
                {
                    if (word.cont == "。" || word.cont == "：" || word.cont == "，"|| word.cont == "；")
                    {
                        if (isStart)
                        {
                            x = String.Empty;
                            isStart = false;
                            continue;
                        }
                    }
                    if (isStart)
                    {
                        if (word.relate == LTPTrainingDP.右附加关系) continue;
                        x += word.cont;
                    }
                    if (key.StartWord.Contains(word.cont))
                    {
                        if (key.StartDPValue.Length == 0 || (key.StartDPValue.Length != 0 && key.StartDPValue.Contains(word.relate)))
                        {
                            isStart = true;
                        }
                    }
                    if (key.EndWord.Contains(word.cont))
                    {
                        if (key.EndDPValue.Length == 0 || (key.EndDPValue.Length != 0 && key.EndDPValue.Contains(word.relate)))
                        {
                            if (isStart) CandidateWord.Add(new LocAndValue<string>() { Value = x });
                            isStart = false;
                            x = String.Empty;
                        }
                    }
                }
            }
        }
    }
}