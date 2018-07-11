using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;

public class IncreaseStockTraning
{

    /// <summary>
    /// 增发对象训练
    /// </summary>
    public static void TrainingIncreaseTarget()
    {
        var TargetTool = new TableAnlayzeTool();
        var IncreaseNumberTool = new TableAnlayzeTool();
        IncreaseNumberTool.Transform = NumberUtility.NormalizerStockNumber;
        var IncreaseMoneyTool = new TableAnlayzeTool();
        IncreaseMoneyTool.Transform =  MoneyUtility.Format;
        TraningDataset.InitIncreaseStock();
        var PreviewId = String.Empty;
        var PreviewRoot = new HTMLEngine.MyRootHtmlNode();
        foreach (var increase in TraningDataset.IncreaseStockList)
        {
            if (!PreviewId.Equals(increase.id))
            {
                var htmlfile = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\定增\html\" + increase.id + ".html";
                PreviewRoot = new HTMLEngine().Anlayze(htmlfile, "");
                PreviewId = increase.id;
            }
            TargetTool.PutTrainingItem(PreviewRoot, increase.PublishTarget);
            IncreaseNumberTool.PutTrainingItem(PreviewRoot, increase.IncreaseNumber);
            IncreaseMoneyTool.PutTrainingItem(PreviewRoot, increase.IncreaseMoney);
        }
        TargetTool.WriteTop(10);
    }
}