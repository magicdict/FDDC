using System;
using FDDC;
using System.Linq;

public class EvaluateItem
{
    /// <summary>
    /// 项目名称
    /// </summary>
    string ItemName = "";
    /// <summary>
    /// 列表形式
    /// </summary>
    public bool IsList;
    /// <summary>
    /// 是否多选一
    /// </summary>
    public bool IsOptional;

    public int POS = 0;
    public int ACT = 0;
    int COR = 0;

    int CorrentCnt = 0;
    int WrongCnt = 0;
    int NotPickCnt = 0;
    int MistakePickCnt = 0;

    public EvaluateItem(string name)
    {
        ItemName = name;
    }

    /// <summary>
    /// F1用COR数据
    /// </summary>
    /// <param name="StardardValue"></param>
    /// <param name="EvaluateValue"></param>
    public void PutCORData(string StardardValue, string EvaluateValue)
    {
        if (!String.IsNullOrEmpty(StardardValue))
        {
            //存在标准值
            if (!String.IsNullOrEmpty(EvaluateValue))
            {
                //存在标准值 存在测评值
                if (IsOptional)
                {
                    if (IsList)
                    {
                        //列表且可选的情况，暂时这里要么是列表，要么是可选
                        if (EvaluateValue.Contains("|"))
                        {
                            //可选
                            //多个可选项用 | 分开，只要一个正确即可
                            var EvaluateValueList = EvaluateValue.Split("|");
                            foreach (var ev in EvaluateValueList)
                            {
                                if (StardardValue.Equals(ev))
                                {
                                    //COR:主键匹配 且 提交字段值=正确字段值 且 均不为空
                                    COR++;
                                    break;
                                }
                            }
                        }
                        if (EvaluateValue.Contains(Utility.SplitChar))
                        {
                            //多项的情况
                            var StardardList = StardardValue.Split(Utility.SplitChar).ToList();
                            var EvaluateList = EvaluateValue.Split(Utility.SplitChar).ToList();
                            var IsOk = false;
                            if (StardardList.Count == EvaluateList.Count)
                            {
                                IsOk = true;
                                for (int i = 0; i < StardardList.Count; i++)
                                {
                                    if (StardardList[i].Equals(EvaluateList[i])) continue;
                                    IsOk = false;
                                    break;
                                }
                            }
                            if (IsOk)
                            {
                                COR++;
                            }
                        }
                    }
                    else
                    {
                        //多个可选项用 | 分开，只要一个正确即可
                        var EvaluateValueList = EvaluateValue.Split("|");
                        foreach (var ev in EvaluateValueList)
                        {
                            if (StardardValue.Equals(ev))
                            {
                                //COR:主键匹配 且 提交字段值=正确字段值 且 均不为空
                                COR++;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (StardardValue.Equals(EvaluateValue))
                    {
                        //COR:主键匹配 且 提交字段值=正确字段值 且 均不为空
                        COR++;
                    }
                }
            }
        }
    }


    public void PutItemData(string StardardValue, string EvaluateValue, String Id = "")
    {
        if (!String.IsNullOrEmpty(StardardValue))
        {
            //存在标准值
            if (!String.IsNullOrEmpty(EvaluateValue))
            {
                if (IsOptional)
                {
                    //单项的情况(暂时不考虑多项)
                    if (StardardValue.NormalizeTextResult().Equals(EvaluateValue.NormalizeTextResult()))
                    {
                        CorrentCnt++;
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(Id))
                        {
                            Program.Evaluator.WriteLine(ItemName + " " + Id + ":【标准】" + StardardValue);
                            Program.Evaluator.WriteLine(ItemName + " " + Id + ":【评估】" + EvaluateValue);
                        }
                        WrongCnt++;
                    }
                }
                else
                {
                    //存在标准值 存在测评值
                    if (IsList)
                    {
                        //多项的情况
                        var StardardList = StardardValue.Split(Utility.SplitChar).ToList();
                        var EvaluateList = EvaluateValue.Split(Utility.SplitChar).ToList();
                        var IsOk = false;
                        if (StardardList.Count == EvaluateList.Count)
                        {
                            IsOk = true;
                            for (int i = 0; i < StardardList.Count; i++)
                            {
                                if (StardardList[i].Equals(EvaluateList[i])) continue;
                                IsOk = false;
                                break;
                            }
                        }
                        if (IsOk)
                        {
                            CorrentCnt++;
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(Id))
                            {
                                Program.Evaluator.WriteLine(ItemName + " " + Id + ":【标准】" + StardardValue);
                                Program.Evaluator.WriteLine(ItemName + " " + Id + ":【评估】" + EvaluateValue);
                            }
                            WrongCnt++;
                        }
                    }
                    else
                    {
                        //单项的情况
                        if (StardardValue.NormalizeTextResult().Equals(EvaluateValue.NormalizeTextResult()))
                        {
                            CorrentCnt++;
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(Id))
                            {
                                Program.Evaluator.WriteLine(ItemName + " " + Id + ":【标准】" + StardardValue);
                                Program.Evaluator.WriteLine(ItemName + " " + Id + ":【评估】" + EvaluateValue);
                            }
                            WrongCnt++;
                        }
                    }

                }
            }
            else
            {
                //存在标准值 不存在测评值
                if (!String.IsNullOrEmpty(Id))
                {
                    Program.Evaluator.WriteLine(ItemName + " " + Id + ":【未检出】" + StardardValue);
                }
                NotPickCnt++;
            }
        }
        else
        {
            //不存在标准值
            if (!String.IsNullOrEmpty(EvaluateValue))
            {
                if (!String.IsNullOrEmpty(Id))
                {
                    Program.Evaluator.WriteLine(ItemName + " " + Id + ":[错误检出]");
                }
                MistakePickCnt++;
            }
        }
    }

    /// <summary>
    /// 输出结果
    /// </summary>
    public void WriteScore()
    {
        Program.Evaluator.WriteLine("标准项：" + ItemName);
        Program.Evaluator.WriteLine("标准数据集数：" + POS);
        Program.Evaluator.WriteLine("测评数据集数：" + ACT);
        Program.Evaluator.WriteLine("主键匹配据集数：" + COR);
        Program.Evaluator.WriteLine("正确：" + CorrentCnt);
        Program.Evaluator.WriteLine("错误：" + WrongCnt);
        Program.Evaluator.WriteLine("未检出：" + NotPickCnt);
        Program.Evaluator.WriteLine("错检出：" + MistakePickCnt);
    }

    public double F1
    {
        get
        {
            WriteScore();
            return GetF1(ItemName, POS, ACT, COR);
        }
    }

    public static double GetF1(String ItemName, double POS, double ACT, double COR)
    {
        //Recall = COR / POS
        //Precision = COR/ ACT
        //F1 = 2 * Recall * Precision / (Recall + Precision)
        double Recall = COR / POS;
        double Precision = COR / ACT;
        double F1 = 2 * Recall * Precision / (Recall + Precision);
        if ((Recall + Precision) == 0) F1 = 0;
        Program.Score.WriteLine("Item:" + ItemName);
        Program.Score.WriteLine("POS:" + POS.ToString());
        Program.Score.WriteLine("ACT:" + ACT.ToString());
        Program.Score.WriteLine("COR:" + COR.ToString());
        Program.Score.WriteLine("F1:" + F1.ToString());
        return F1;
    }
}