using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;

public class IncreaseStockTraning
{

    /// <summary>
    /// 增发对象训练
    /// </summary>
    /// <param name="TraningCnt">训练条数</param>
    public static void Training(int TraningCnt = int.MaxValue)
    {
        var TargetTool = new TableAnlayzeTool();
        var IncreaseNumberTool = new TableAnlayzeTool();
        IncreaseNumberTool.Transform = NumberUtility.NormalizerStockNumber;
        var IncreaseMoneyTool = new TableAnlayzeTool();
        IncreaseMoneyTool.Transform = MoneyUtility.Format;
        var PreviewId = String.Empty;
        var PreviewRoot = new HTMLEngine.MyRootHtmlNode();
        int Cnt = 0;
        foreach (var increase in TraningDataset.IncreaseStockList)
        {
            if (!PreviewId.Equals(increase.Id))
            {
                var htmlfile = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\定增\html\" + increase.Id + ".html";
                PreviewRoot = new HTMLEngine().Anlayze(htmlfile, "");
                PreviewId = increase.Id;
                Cnt++; if (Cnt == TraningCnt) break;
            }
            TargetTool.PutTitleTrainingItem(PreviewRoot, increase.PublishTarget);
            IncreaseNumberTool.PutTitleTrainingItem(PreviewRoot, increase.IncreaseNumber);
            IncreaseMoneyTool.PutTitleTrainingItem(PreviewRoot, increase.IncreaseMoney);
        }

        var rank = Utility.FindTop(10, TargetTool.TrainingTitleResult);
        Program.Training.WriteLine("增发对象");
        foreach (var rec in rank)
        {
            Program.Training.WriteLine(rec.ToString());
        }

        rank = Utility.FindTop(10, IncreaseNumberTool.TrainingTitleResult);
        Program.Training.WriteLine("增发数量");
        foreach (var rec in rank)
        {
            Program.Training.WriteLine(rec.ToString());
        }

        rank = Utility.FindTop(10, IncreaseMoneyTool.TrainingTitleResult);
        Program.Training.WriteLine("增发金额");
        foreach (var rec in rank)
        {
            Program.Training.WriteLine(rec.ToString());
        }
    }
}