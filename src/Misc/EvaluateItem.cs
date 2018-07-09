using System;
using FDDC;

public class EvaluateItem
{
    String ItemName = "";
    int POS = 0;
    int ACT = 0;
    int COR = 0;

    int CorrectCnt = 0;
    int WrongCnt = 0;
    int NotPickCnt = 0;
    int MistakePickCnt = 0;

    public EvaluateItem(string name)
    {
        ItemName = name;
    }

    public void PutData(bool IsKeyMatch, string StardardValue, string EvaluateValue)
    {
        //POS:标准数据集中该字段不为空的记录数
        //ACT:选手提交结果中该字段不为空的记录数
        //COR:主键匹配 且 提交字段值=正确字段值 且 均不为空
        if (String.IsNullOrEmpty(StardardValue)) POS++;
        if (String.IsNullOrEmpty(EvaluateValue)) ACT++;
        if (!String.IsNullOrEmpty(StardardValue))
        {
            //存在标准值
            if (!String.IsNullOrEmpty(EvaluateValue))
            {
                //存在标准值 存在测评值
                if (StardardValue.Equals(EvaluateValue))
                {
                    CorrectCnt++;
                    if (IsKeyMatch) COR++;
                }
                else
                {
                    WrongCnt++;
                }
            }
            else
            {
                //存在标准值 不存在测评值
                NotPickCnt++;
            }
        }
        else
        {
            //不存在标准值
            if (!String.IsNullOrEmpty(EvaluateValue))
            {
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
        Program.Evaluator.WriteLine("正确：" + CorrectCnt);
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
        //POS:标准数据集中该字段不为空的记录数
        //ACT:选手提交结果中该字段不为空的记录数
        //COR:主键匹配 且 提交字段值=正确字段值 且 均不为空
        //Recall = COR / POS
        //Precision = COR/ ACT
        //F1 = 2 * Recall * Precision / (Recall + Precision)
        double Recall = COR / POS;
        double Precision = COR / ACT;
        double F1 = 2 * Recall * Precision / (Recall + Precision);
        if (POS == 0 || ACT == 0) F1 = 0;
        Program.Score.WriteLine("Item:" + ItemName);
        Program.Score.WriteLine("POS:" + POS.ToString());
        Program.Score.WriteLine("ACT:" + ACT.ToString());
        Program.Score.WriteLine("COR:" + COR.ToString());
        Program.Score.WriteLine("F1:" + F1.ToString());
        return F1;
    }
}