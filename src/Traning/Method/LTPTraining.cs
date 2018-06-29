using System;
using System.Collections.Generic;
using FDDC;
using static LTP;

public class LTPTraining
{
    public Dictionary<string, int> LeadingWordDict = new Dictionary<string, int>();

    public Dictionary<string, int> LeadingVerbWordDict = new Dictionary<string, int>();

    public Dictionary<string, int> LastWordDict = new Dictionary<string, int>();

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
                            if (LastWordDict.ContainsKey(paragragh[CompIdx].ToString()))
                            {
                                LastWordDict[paragragh[CompIdx].ToString()] += 1;
                            }
                            else
                            {
                                LastWordDict.Add(paragragh[CompIdx].ToString(), 1);
                            }
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
                    //前导词
                    var LW = paragragh[startIdx - 1];
                    if (LeadingWordDict.ContainsKey(LW.ToString()))
                    {
                        LeadingWordDict[LW.ToString()] += 1;
                    }
                    else
                    {
                        LeadingWordDict.Add(LW.ToString(), 1);
                    }
                    //动词
                    for (int LeadingVerbIdx = startIdx - 1; LeadingVerbIdx >= 0; LeadingVerbIdx--)
                    {
                        if (paragragh[LeadingVerbIdx].pos == WordUtility.动词)
                        {
                            LW = paragragh[LeadingVerbIdx];
                            if (LeadingVerbWordDict.ContainsKey(LW.ToString()))
                            {
                                LeadingVerbWordDict[LW.ToString()] += 1;
                            }
                            else
                            {
                                LeadingVerbWordDict.Add(LW.ToString(), 1);
                            }
                            break;
                        }
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
        Program.Training.WriteLine("前导动词");
        Utility.FindTop(top, LeadingVerbWordDict);
        Program.Training.WriteLine("词尾");
        Utility.FindTop(top, LastWordDict);
    }
}