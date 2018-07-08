using System;
using System.Collections.Generic;
using FDDC;
using static LTP;

/// <summary>
/// SRL统计
/// 关键字包含在那个SRL的WORD中，以及ARG的类型
/// </summary>
public class LTPTrainingSRL
{
    public Dictionary<struSrlTraning, int> WordArgDict = new Dictionary<struSrlTraning, int>();
    public struct struSrlTraning
    {
        public string word;

        public string relate;

        public string argtype;
    }

    public List<struSrlTraning> Training(List<List<struWordSRL>> srlList, string KeyWord)
    {
        List<struSrlTraning> list = new List<struSrlTraning>();
        foreach (var paragragh in srlList)
        {
            foreach (var word in paragragh)
            {
                if (word.args.Count != 0)
                {
                    foreach (var arg in word.args)
                    {
                        if (arg.cont.Contains(KeyWord))
                        {
                            var x = new struSrlTraning()
                            {
                                word = word.cont,
                                argtype = arg.type,
                                relate = word.relate
                            };
                            list.Add(x);
                            if (WordArgDict.ContainsKey(x))
                            {
                                WordArgDict[x]++;
                            }
                            else
                            {
                                WordArgDict.Add(x, 1);
                            }

                        }
                    }
                }
            }
        }
        return list;
    }
}