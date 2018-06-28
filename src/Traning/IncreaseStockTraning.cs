using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;

public class IncreaseStockTraning
{
    
    public static void TrainingIncreaseTarget()
    {
        TraningDataset.InitIncreaseStock();
        var PreviewId = String.Empty;
        var PreviewRoot = new HTMLEngine.MyRootHtmlNode();
        foreach (var increase in TraningDataset.IncreaseStockList)
        {
            if (PreviewId.Equals(increase.id))
            {
                var htmlfile = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\定增\html\" + increase.id + ".html";
                PreviewRoot = HTMLEngine.Anlayze(htmlfile);
            }
            TableAnlayzeTool.PutTrainingItem(PreviewRoot, increase.PublishTarget);
        }

        var Rank = new List<int>();
        Rank = TableAnlayzeTool.TrainingTitleResult.Values.ToList();
        Rank.Sort();
        Rank.Reverse();
        var Top10 = Rank[9];
        foreach (var title in TableAnlayzeTool.TrainingTitleResult)
        {
            if (title.Value >= Top10)
            {
                Console.WriteLine(title.Key + ":" + title.Value);
            }
        }
    }
}