using System;
using System.Collections.Generic;
using System.IO;
using FDDC;

public class ReOrganizationTraning
{
    public static void Train()
    {
        Console.WriteLine("开始分析 资产重组");
        GetEvaluateMethodEnum();
        GetEvaluateMethodTitle();
        //GetTradeCompanyTitle();
        //GetTradeCompanyFromReplaceTable();
        Console.WriteLine("结束分析 资产重组");
    }

    /// <summary>
    /// 寻找表中交易对手的标题
    /// </summary>
    /// <param name="TraningCnt"></param>
    public static void GetTradeCompanyTitle(int TraningCnt = int.MaxValue)
    {
        var TargetTool = new TableAnlayzeTool();
        var PreviewId = String.Empty;
        var PreviewRoot = new HTMLEngine.MyRootHtmlNode();
        int Cnt = 0;
        foreach (var ReOrg in TraningDataset.ReorganizationList)
        {
            if (!PreviewId.Equals(ReOrg.Id))
            {
                var htmlfile = Program.ReorganizationPath_TRAIN + @"\html\" + ReOrg.Id + ".html";
                if (!System.IO.File.Exists(htmlfile)) continue;
                PreviewRoot = new HTMLEngine().Anlayze(htmlfile, "");
                PreviewId = ReOrg.Id;
                Cnt++; if (Cnt == TraningCnt) break;
            }
            foreach (var item in ReOrg.TradeCompany.Split(Utility.SplitChar))
            {
                TargetTool.PutTitleTrainingItem(PreviewRoot, item);
            }
        }

        var rank = Utility.FindTop(10, TargetTool.TrainingTitleResult);
        Program.Training.WriteLine("交易对象");
        foreach (var rec in rank)
        {
            Program.Training.WriteLine(rec.ToString());
        }

        foreach (var item in TargetTool.WholeHeaderRow)
        {
            Program.Training.WriteLine(item);
        }
        Program.Training.Flush();
    }


    public static void GetEvaluateMethodTitle(int TraningCnt = int.MaxValue)
    {
        var TargetTool = new TableAnlayzeTool();
        var PreviewId = String.Empty;
        var PreviewRoot = new HTMLEngine.MyRootHtmlNode();
        int Cnt = 0;
        foreach (var ReOrg in TraningDataset.ReorganizationList)
        {
            if (!PreviewId.Equals(ReOrg.Id))
            {
                var htmlfile = Program.ReorganizationPath_TRAIN + Path.DirectorySeparatorChar + @"html" + Path.DirectorySeparatorChar + ReOrg.Id + ".html";
                if (!System.IO.File.Exists(htmlfile)) continue;
                PreviewRoot = new HTMLEngine().Anlayze(htmlfile, "");
                PreviewId = ReOrg.Id;
                Cnt++; if (Cnt == TraningCnt) break;
            }
            if (!String.IsNullOrEmpty(ReOrg.EvaluateMethod))
            {
                TargetTool.PutTitleTrainingItemWithCodition(PreviewRoot, ReOrg.EvaluateMethod, ReOrg.TargetCompany);
            }
        }

        var rank = Utility.FindTop(10, TargetTool.TrainingTitleResult);
        Program.Training.WriteLine("评估方法");
        foreach (var rec in rank)
        {
            Program.Training.WriteLine(rec.ToString());
        }
        Program.Training.WriteLine("标的");
        rank = Utility.FindTop(10, TargetTool.TrainingTitleCondition);
        foreach (var rec in rank)
        {
            Program.Training.WriteLine(rec.ToString());
        }
        Program.Training.Flush();
    }

    public static void GetTradeCompanyFromReplaceTable(int TraningCnt = int.MaxValue)
    {
        var TargetDict = new Dictionary<string, int>();
        var PreviewId = String.Empty;
        var PreviewRoot = new Reorganization();
        int Cnt = 0;
        foreach (var ReOrg in TraningDataset.ReorganizationList)
        {
            if (!PreviewId.Equals(ReOrg.Id))
            {
                var htmlfile = Program.ReorganizationPath_TRAIN + @"\html\" + ReOrg.Id + ".html";
                if (!System.IO.File.Exists(htmlfile)) continue;
                PreviewRoot = new Reorganization();
                PreviewRoot.Init(htmlfile);
                PreviewId = ReOrg.Id;
                Cnt++; if (Cnt == TraningCnt) break;
            }
            foreach (var item in PreviewRoot.ReplacementDict)
            {
                var values = item.Value;
                if (values.Contains(ReOrg.Target) && values.Contains(ReOrg.TargetCompany))
                {
                    var keys = item.Key.Split(Utility.SplitChar);
                    var keys2 = item.Key.Split("/");
                    if (keys.Length == 1 && keys2.Length > 1)
                    {
                        keys = keys2;
                    }
                    foreach (var key in keys)
                    {
                        if (!TargetDict.ContainsKey(key))
                        {
                            TargetDict.Add(key, 1);
                        }
                        else
                        {
                            TargetDict[key] += 1;
                        }
                    }
                }
            }
        }

        var rank = Utility.FindTop(10, TargetDict);
        Program.Training.WriteLine("交易对象");
        foreach (var rec in rank)
        {
            Program.Training.WriteLine(rec.ToString());
        }
    }


    /// <summary>
    /// /// 获得评估法枚举
    /// </summary>
    public static List<String> EvaluateMethodList = new List<String>();
    public static void GetEvaluateMethodEnum()
    {
        Program.Training.WriteLine("获得评估法枚举:");
        foreach (var ReOrg in TraningDataset.ReorganizationList)
        {
            foreach (var method in ReOrg.EvaluateMethod.Split(Utility.SplitChar))
            {
                if (String.IsNullOrEmpty(method)) continue;
                if (EvaluateMethodList.Contains(method)) continue;
                Program.Training.WriteLine(method);
                EvaluateMethodList.Add(method);
            }
        }
    }
}