using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;

public class StockChangeTraning
{

    /// <summary>
    /// 增减持训练
    /// </summary>
    /// <param name="TraningCnt">训练条数</param>
    public static void Traning(int TraningCnt = int.MaxValue)
    {
        var ChangeMethodTool = new TableAnlayzeTool();
        var PreviewId = String.Empty;
        var PreviewRoot = new HTMLEngine.MyRootHtmlNode();
        int Cnt = 0;
        foreach (var stockchange in TraningDataset.StockChangeList)
        {
            if (!PreviewId.Equals(stockchange.Id))
            {
                var htmlfile = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\增减持\html\" + stockchange.Id + ".html";
                PreviewRoot = new HTMLEngine().Anlayze(htmlfile, "");
                PreviewId = stockchange.Id;
                Cnt++; if (Cnt == TraningCnt) break;
            }
            ChangeMethodTool.PutValueTrainingItem(PreviewRoot, new string[] { "减持方式", "增持方式" }.ToList());
        }

        var rank = Utility.FindTop(10, ChangeMethodTool.TrainingValueResult);
        Program.Training.WriteLine("增减持方式");
        foreach (var rec in rank)
        {
            Program.Training.WriteLine(rec.ToString());
        }
    }
}