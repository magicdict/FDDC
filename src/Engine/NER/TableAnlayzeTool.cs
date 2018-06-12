using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;

//关于表的统计
public static class TableAnlayzeTool
{

    public static Dictionary<string, int> TrainingTitleResult = new Dictionary<string, int>();

    //寻找同时含有关键字的列的表头
    public static void PutTrainingItem(HTMLEngine.MyRootHtmlNode root, string KeyWord)
    {
        foreach (var Table in root.TableList)
        {
            var t = new HTMLTable(Table.Value);
            for (int RowNo = 2; RowNo < t.RowCount; RowNo++)
            {
                //从第二行开始
                for (int ColNo = 1; ColNo < t.ColumnCount; ColNo++)
                {
                    if (t.CellValue(RowNo, ColNo).NormalizeKey().Equals(KeyWord.NormalizeKey()))
                    {
                        var title = t.CellValue(1, ColNo);
                        if (!TrainingTitleResult.ContainsKey(title))
                        {
                            TrainingTitleResult.Add(title, 1);
                        }
                        else
                        {
                            TrainingTitleResult[title]++;
                        }
                    }

                }
            }

        }
    }
}