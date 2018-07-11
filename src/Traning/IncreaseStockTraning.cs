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
            if (!PreviewId.Equals(increase.id))
            {
                var htmlfile = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\定增\html\" + increase.id + ".html";
                PreviewRoot = new HTMLEngine().Anlayze(htmlfile, "");
                PreviewId = increase.id;
                Cnt++; if (Cnt == TraningCnt) break;
            }
            TargetTool.PutTitleTrainingItem(PreviewRoot, increase.PublishTarget);
            IncreaseNumberTool.PutTitleTrainingItem(PreviewRoot, increase.IncreaseNumber);
            IncreaseMoneyTool.PutTitleTrainingItem(PreviewRoot, increase.IncreaseMoney);
        }
        Program.Training.WriteLine("增发对象");
        TargetTool.WriteTop(5);
        Program.Training.WriteLine("增发数量");
        IncreaseNumberTool.WriteTop(5);
        Program.Training.WriteLine("增发金额");
        IncreaseMoneyTool.WriteTop(5);
    }
}