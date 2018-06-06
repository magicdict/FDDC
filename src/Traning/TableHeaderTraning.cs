using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;

public static class TableHeaderTraning
{

    public static Dictionary<string, int> TitleList = new Dictionary<string, int>();

    //定增对象
    public static void TrainingIncreaseTarget()
    {
        Traning.InitIncreaseStock();
        var PreviewId = "";
        var PreviewRoot = new HTMLEngine.MyRootHtmlNode();
        foreach (var increase in Traning.IncreaseStockList)
        {
            if (PreviewId.Equals(increase.id))
            {
                var htmlfile = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\定增\html\" + increase.id + ".html";
                PreviewRoot = HTMLEngine.Anlayze(htmlfile);
            }
            TableHeaderTraning.Training(PreviewRoot, increase.PublishTarget);
        }

        var Rank = new List<int>();
        Rank = TableHeaderTraning.TitleList.Values.ToList();
        Rank.Sort();
        Rank.Reverse();
        var Top10 = Rank[9];
        foreach (var title in TableHeaderTraning.TitleList)
        {
            if (title.Value >= Top10)
            {
                Console.WriteLine(title.Key + ":" + title.Value);
            }
        }
    }


    public static void Training(HTMLEngine.MyRootHtmlNode root, string KeyWord)
    {
        foreach (var Table in root.TableList)
        {
            var t = new HTMLTable(Table.Value);
            for (int RowNo = 2; RowNo < t.RowCount; RowNo++)
            {
                //从第二行开始
                for (int ColNo = 1; ColNo < t.ColumnCount; ColNo++)
                {
                    if (t.CellValue(RowNo, ColNo).NormalizeTextResult().Equals(KeyWord.NormalizeTextResult()))
                    {
                        var title = t.CellValue(1, ColNo);
                        if (!TitleList.ContainsKey(title))
                        {
                            TitleList.Add(title, 1);
                        }
                        else
                        {
                            TitleList[title]++;
                        }
                    }
                }
            }

        }
    }
}