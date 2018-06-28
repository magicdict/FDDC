using System;
using System.Collections.Generic;
using FDDC;
using static LTP;

public class LTPTraining
{
    public Dictionary<string, int> LeadingWordDict = new Dictionary<string, int>();
    public void Training(List<List<struWordDP>> dplist, string KeyWord)
    {
        foreach (var paragragh in dplist)
        {
            for (int startIdx = 0; startIdx < paragragh.Count; startIdx++)
            {
                var CompWord = String.Empty;
                if (KeyWord.StartsWith(paragragh[startIdx].cont))
                {
                    //找到疑似开始位置
                    CompWord = paragragh[startIdx].cont;
                    for (int CompIdx = startIdx + 1; CompIdx < paragragh.Count; CompIdx++)
                    {
                        CompWord += paragragh[CompIdx].cont;
                        if (CompWord == KeyWord)
                        {
                            //找到整个词语
                            break;
                        }
                        else
                        {
                            if (KeyWord.StartsWith(CompWord))
                            {
                                //继续
                                continue;
                            }
                            else
                            {
                                //跳出整个循环，寻找两一个StartIdx
                                break;
                            }
                        }
                    }
                }
                //可能是找到整个词语，或者需要找寻下一个
                if (CompWord == KeyWord && startIdx != 0)
                {
                    var LW = paragragh[startIdx - 1];
                    if (LeadingWordDict.ContainsKey(LW.ToString()))
                    {
                        LeadingWordDict[LW.ToString()] += 1;
                    }
                    else
                    {
                        LeadingWordDict.Add(LW.ToString(), 1);
                    }
                    break;
                }
            }
        }
    }
    public void WriteTop(int top)
    {
        Program.Training.WriteLine("前导词语");
        Utility.FindTop(top, LeadingWordDict);
    }
}