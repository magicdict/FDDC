using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;

//关于表的统计
public class TableAnlayzeTool
{

    /// <summary>
    /// 统计用
    /// </summary>
    /// <typeparam name="string"></typeparam>
    /// <typeparam name="int"></typeparam>
    /// <returns></returns>
    public Dictionary<string, int> TrainingTitleResult = new Dictionary<string, int>();

    /// <summary>
    /// 变换规则
    /// 输入：表格内容，表格标题
    /// 输出：变换后词语
    /// </summary>
    public Func<String, String, String> Transform;


    //寻找同时含有关键字的列的表头
    public void PutTrainingItem(HTMLEngine.MyRootHtmlNode root, string KeyWord)
    {
        foreach (var Table in root.TableList)
        {
            var t = new HTMLTable(Table.Value);
            for (int RowNo = 2; RowNo < t.RowCount; RowNo++)
            {
                //从第二行开始
                for (int ColNo = 1; ColNo < t.ColumnCount; ColNo++)
                {
                    var title = t.CellValue(1, ColNo);
                    var value = t.CellValue(RowNo, ColNo);
                    if (Transform != null) value = Transform(value,title);
                    if (value.NormalizeTextResult().Equals(KeyWord.NormalizeTextResult()))
                    {
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
    /// <summary>
    /// 输出TOP
    /// </summary>
    /// <param name="top"></param>
    public void WriteTop(int top = 10)
    {
        Utility.FindTop(top, TrainingTitleResult);
    }
}