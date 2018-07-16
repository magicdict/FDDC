using System;
using System.Collections.Generic;

public class ReOrganizationTraning
{
    public static void Train()
    {
        Console.WriteLine("开始分析");
        GetEvaluateMethodEnum();
        Console.WriteLine("结束分析");
    }

    /// <summary>
    /// /// 获得评估法枚举
    /// </summary>
    public static List<String> EvaluateMethodList = new List<String>();
    public static void GetEvaluateMethodEnum()
    {

        foreach (var ReOrg in TraningDataset.ReorganizationList)
        {
            foreach (var method in ReOrg.EvaluateMethod.Split("、"))
            {
                if (String.IsNullOrEmpty(method)) continue;
                if (!EvaluateMethodList.Contains(method)) EvaluateMethodList.Add(method);
            }
        }
    }
}