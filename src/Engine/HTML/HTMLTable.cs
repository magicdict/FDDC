using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using static HTMLEngine;

public partial class HTMLTable
{
    public const string strNullValue = "<null>";
    public const string strRowSpan = "rowspan";
    public const string strRowSpanValue = "<rowspan>";
    public const string strColSpan = "colspan";
    public const string strColSpanValue = "<colspan>";
    public int RowCount = 0;
    public int ColumnCount = 0;

    Dictionary<String, string> dict = new Dictionary<String, string>();

    public static List<String> GetTable(HtmlNode table, int TableId)
    {
        var tablecontentlist = new List<String>();
        var dict = new Dictionary<String, String>();

        //表格处理：
        foreach (var tablebody in table.ChildNodes)
        {
            //整理出最大行列数
            int MaxRow = 0;
            int MaxColumn = 0;

            foreach (var tableRows in tablebody.ChildNodes)
            {
                if (tableRows.ChildNodes.Count != 0)
                {
                    int xc = 0;
                    foreach (var tableData in tableRows.ChildNodes)
                    {
                        if (tableData.Name == "td")
                        {
                            if (tableData.Attributes[strColSpan] != null)
                            {
                                xc += int.Parse(tableData.Attributes[strColSpan].Value);
                            }
                            else
                            {
                                xc++;
                            }
                        }
                    }
                    if (xc > MaxColumn) MaxColumn = xc;
                    MaxRow++;
                }
            }


            //准备Cell内容字典
            for (int Row = 1; Row < MaxRow + 1; Row++)
            {
                for (int Col = 1; Col < MaxColumn + 1; Col++)
                {
                    dict.Add(Row + "," + Col, String.Empty);
                }
            }

            int CurrentRow = 1;
            int NextNeedToFillColumn = 1;

            foreach (var tableRows in tablebody.ChildNodes)
            {
                if (tableRows.ChildNodes.Count != 0)
                {
                    foreach (var tableData in tableRows.ChildNodes)
                    {
                        //对于#text的过滤
                        if (tableData.Name == "td")
                        {

                            //寻找该行下一个需要填充的格子的列号
                            for (int Col = 1; Col < MaxColumn + 1; Col++)
                            {
                                if (dict[CurrentRow + "," + Col] == String.Empty)
                                {
                                    NextNeedToFillColumn = Col;
                                    break;
                                }
                            }

                            var cellvalue = HTMLEngine.CorrectHTML(Normalizer.Normalize(tableData.InnerText));
                            var cellpos = CurrentRow + "," + NextNeedToFillColumn;
                            if (cellvalue == String.Empty)
                            {
                                cellvalue = strNullValue;
                            }
                            dict[CurrentRow + "," + NextNeedToFillColumn] = cellvalue;
                            if (tableData.Attributes[strRowSpan] != null)
                            {
                                //具有RowSpan特性的情况
                                for (int i = 1; i < int.Parse(tableData.Attributes[HTMLTable.strRowSpan].Value); i++)
                                {
                                    dict[(CurrentRow + i) + "," + NextNeedToFillColumn] = strRowSpanValue;
                                }
                            }
                            if (tableData.Attributes[strColSpan] != null)
                            {
                                //具有ColSpan特性的情况
                                for (int i = 1; i < int.Parse(tableData.Attributes[strColSpan].Value); i++)
                                {
                                    dict[CurrentRow + "," + (NextNeedToFillColumn + i)] = strColSpanValue;
                                }
                            }
                        }
                    }
                    CurrentRow++;
                }
            }
        }

        //表格分页的修正
        var NeedToModify = String.Empty;
        foreach (var item in dict)
        {
            if (item.Value == strNullValue)
            {
                var Row = int.Parse(item.Key.Split(",")[0]) - 1;
                var Column = item.Key.Split(",")[1];
                if (Row == 0) continue;
                if (dict[Row + "," + Column] == strRowSpanValue)
                {
                    NeedToModify = item.Key;
                }
            }
        }

        if (NeedToModify != String.Empty) dict[NeedToModify] = strRowSpanValue;

        foreach (var item in dict)
        {
            tablecontentlist.Add(TableId + "," + item.Key + "|" + item.Value);
        }

        return tablecontentlist;
    }


    public HTMLTable(List<String> TableContent)
    {
        for (int i = 0; i < TableContent.Count; i++)
        {
            //Table,Row,Column|Keyword
            var pos = TableContent[i].Split("|")[0];
            var value = TableContent[i].Split("|")[1];

            var RowPos = int.Parse(pos.Split(",")[1]);
            var ColumnPos = int.Parse(pos.Split(",")[2]);

            if (RowCount < RowPos) RowCount = RowPos;
            if (ColumnCount < ColumnPos) ColumnCount = ColumnPos;
            dict.Add(RowPos + "," + ColumnPos, value);
        }
    }

    public string CellValue(int RowPos, int ColPos)
    {
        var pos = RowPos + "," + ColPos;
        if (dict.ContainsKey(pos))
        {
            var content = dict[pos];
            if (content == strRowSpanValue)
            {
                //向上寻找非<rowspan>的内容
                for (int i = RowPos - 1; i >= 0; i--)
                {
                    pos = i + "," + ColPos;
                    if (dict.ContainsKey(pos))
                    {
                        content = dict[pos];
                        if (content != strRowSpanValue) return content;
                    }
                }
            }

            if (content == strColSpanValue)
            {
                //向上寻找非<colspan>的内容
                for (int i = ColPos - 1; i >= 0; i--)
                {
                    pos = RowPos + "," + i;
                    if (dict.ContainsKey(pos))
                    {
                        content = dict[pos];
                        if (content != strColSpanValue) return content;
                    }
                }
            }
            if (!dict.ContainsKey(pos))
            {
                //Console.WriteLine("Error!!!Position Not Found:" + pos);
            }
            else
            {
                return dict[pos];
            }
        }
        return String.Empty;
    }

    public String[] GetRow(int RowNo = 1)
    {
        var Header = new String[ColumnCount];
        for (int i = 1; i < ColumnCount + 1; i++)
        {
            Header[i - 1] = CellValue(RowNo, i);
        }
        return Header;
    }

    public bool IsTotalRow(int RowNo)
    {
        bool IsTotalRow = false;
        for (int i = 1; i <= ColumnCount; i++)
        {
            var x = CellValue(RowNo, i).Replace(" ", String.Empty);
            if (x.Contains("合计") || x.Contains("小计") || x.Contains("总计") ||
                x == "—" || x == "－" || x == "-" || x == "/" ||
                x == "--" || x == "——")
            {
                IsTotalRow = true;
                break;
            }
        }

        if (!IsTotalRow)
        {
            int RowSpanCnt = 0;
            for (int ColNo = 1; ColNo <= ColumnCount; ColNo++)
            {
                var pos = RowNo + "," + ColNo;
                if (!dict.ContainsKey(pos)) return false;
                if (dict[pos] == strRowSpanValue || dict[pos] == strColSpanValue)
                {
                    RowSpanCnt++;
                }
            }
            if (RowSpanCnt == ColumnCount - 1) IsTotalRow = true;
        }
        return IsTotalRow;
    }


    /// <summary>
    /// /// 分页表格的修复
    /// </summary>
    /// <param name="root"></param>
    public static void FixSpiltTable(AnnouceDocument doc)
    {
        //首行NULL的合并
        FirstRowNullFix(doc);

        OneRowFix(doc);

        for (int i = 0; i < doc.root.Children.Count; i++)
        {
            for (int j = 0; j < doc.root.Children[i].Children.Count; j++)
            {
                var node = doc.root.Children[i].Children[j];
                if (node.TableId != -1)
                {
                    if (node.NextBrother != null)
                    {
                        if (node.NextBrother.TableId != -1)
                        {
                            //1.是否存在连续表格 NextBrother
                            var nextnode = node.NextBrother;
                            var table = new HTMLTable(doc.root.TableList[node.TableId]);
                            var nexttable = new HTMLTable(doc.root.TableList[nextnode.TableId]);
                            //Console.WriteLine("First  Table:" + table.RowCount + "X" + table.ColumnCount);
                            //Console.WriteLine("Second Table:" + nexttable.RowCount + "X" + nexttable.ColumnCount);
                            if (table.ColumnCount != nexttable.ColumnCount) continue;
                            //Console.WriteLine("Two Tables Has Same Column Count!");
                            //2.连续表格的后一个，往往是有<NULL>的行
                            bool hasnull = false;
                            for (int nullcell = 1; nullcell <= table.ColumnCount; nullcell++)
                            {
                                if (nexttable.CellValue(1, nullcell) == HTMLTable.strNullValue)
                                {
                                    hasnull = true;
                                    break;
                                }
                            }

                            var ComboCompanyName = "";
                            var ComboCompanyNameColumnNo = -1;
                            var CompanyFullNameList = doc.companynamelist.Select((x) => { return x.secFullName; }).Distinct().ToList();
                            //两表同列的元素，是否有能够合并成为公司名称的？注意，需要去除空格！！
                            int MaxColumn = table.ColumnCount;
                            for (int col = 1; col <= MaxColumn; col++)
                            {
                                int TableAMaxRow = table.RowCount;
                                int TableBMaxRow = nexttable.RowCount;
                                for (int RowCntA = 1; RowCntA < TableAMaxRow; RowCntA++)
                                {
                                    for (int RowCntB = 1; RowCntB < TableBMaxRow; RowCntB++)
                                    {
                                        var valueA = table.CellValue(RowCntA, col).Replace(" ", "");
                                        var valueB = nexttable.CellValue(RowCntB, col).Replace(" ", "");
                                        if (valueA != "" && valueB != "")
                                        {
                                            var value = valueA + valueB;
                                            if (CompanyFullNameList.Contains(value))
                                            {
                                                ComboCompanyName = value;
                                                ComboCompanyNameColumnNo = col;
                                                //Console.WriteLine("Found FullName:" + value);
                                                break;
                                            }
                                        }
                                    }
                                    if (ComboCompanyNameColumnNo != -1) break;
                                }
                                if (ComboCompanyNameColumnNo != -1) break;
                            }
                            if (ComboCompanyNameColumnNo != -1)
                            {
                                //补完:注意，不能全部补！！A表以公司名开头，B表以公司名结尾
                                for (int k = 0; k < doc.root.TableList[node.TableId].Count; k++)
                                {
                                    var tablerec = doc.root.TableList[node.TableId][k].Split("|");
                                    var value = tablerec[1].Replace(" ", "");
                                    //A表以公司名开头
                                    if (ComboCompanyName.StartsWith(value))
                                    {
                                        doc.root.TableList[node.TableId][k] = tablerec[0] + "|" + ComboCompanyName;
                                    }
                                }
                                for (int k = 0; k < doc.root.TableList[nextnode.TableId].Count; k++)
                                {
                                    var tablerec = doc.root.TableList[nextnode.TableId][k].Split("|");
                                    var value = tablerec[1].Replace(" ", "");
                                    //A表以公司名开头
                                    if (ComboCompanyName.EndsWith(value))
                                    {
                                        doc.root.TableList[nextnode.TableId][k] = tablerec[0] + "|" + ComboCompanyName;
                                    }
                                }
                            }
                            if (hasnull || ComboCompanyNameColumnNo != -1)
                            {
                                MergeTable(doc, nextnode.TableId);
                            }
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// 单行合并
    /// </summary>
    /// <param name="doc"></param>
    private static void OneRowFix(AnnouceDocument doc)
    {
        for (int NextTableId = 2; NextTableId <= doc.root.TableList.Count; NextTableId++)
        {
            var table = new HTMLTable(doc.root.TableList[NextTableId - 1]);
            var nexttable = new HTMLTable(doc.root.TableList[NextTableId]);
            if (table.RowCount == 1 && table.ColumnCount == nexttable.ColumnCount)
            {
                MergeTable(doc, NextTableId);
            }
        }
    }

    /// <summary>
    /// 首行NULL的合并
    /// </summary>
    /// <param name="doc"></param>
    private static void FirstRowNullFix(AnnouceDocument doc)
    {
        for (int NextTableId = 2; NextTableId <= doc.root.TableList.Count; NextTableId++)
        {
            foreach (var item in doc.root.TableList[NextTableId])
            {
                var FirstTablePos = -1;
                var SecondTablePos = -1;
                foreach (var p in doc.root.Children)
                {
                    foreach (var s in p.Children)
                    {
                        if (s.TableId == NextTableId - 1) FirstTablePos = s.PositionId;
                        if (s.TableId == NextTableId) SecondTablePos = s.PositionId;
                    }
                }

                if (SecondTablePos - FirstTablePos > 200) continue;

                var tablerec = item.Split("|");
                var pos = tablerec[0].Split(",");
                var value = tablerec[1];
                var row = int.Parse(pos[1]);
                //第二张表，第一行存在NULL
                if (row == 1 && value == strNullValue)
                {
                    var table = new HTMLTable(doc.root.TableList[NextTableId - 1]);
                    var nexttable = new HTMLTable(doc.root.TableList[NextTableId]);
                    if (table.ColumnCount != nexttable.ColumnCount) continue;
                    MergeTable(doc, NextTableId);
                    //Console.WriteLine("FirstRowNullFix");
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 合并表
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="NextTableId"></param>
    public static void MergeTable(AnnouceDocument doc, int NextTableId)
    {
        var table = new HTMLTable(doc.root.TableList[NextTableId - 1]);
        string[] pos;
        string[] tablerec;
        string value;
        var offset = table.RowCount;
        //修改第二张表格的数据
        foreach (var Nextitem in doc.root.TableList[NextTableId])
        {
            tablerec = Nextitem.Split("|");
            pos = tablerec[0].Split(",");
            value = tablerec[1];
            var newtablerec = (NextTableId - 1) + "," + (offset + int.Parse(pos[1])) + "," + pos[2] + "|" + value;
            doc.root.TableList[NextTableId - 1].Add(newtablerec);
        }
        doc.root.TableList[NextTableId].Clear();
        for (int i = 0; i < doc.root.Children.Count; i++)
        {
            for (int j = 0; j < doc.root.Children[i].Children.Count; j++)
            {
                var node = doc.root.Children[i].Children[j];
                if (node.TableId == NextTableId) node.TableId = -1;
            }
        }
    }

    /// <summary>
    /// 使用公司名称填null值
    /// </summary>
    /// <param name="doc"></param>
    public static void FixNullValue(AnnouceDocument doc)
    {
        var CompanyFullNameList = doc.companynamelist.Select((x) => { return x.secFullName; }).Distinct().ToList();
        var CompanyShortNameList = doc.companynamelist.Select((x) => { return x.secShortName; }).Distinct().ToList();
        var CompanyPos = new List<String>();
        for (int tableId = 1; tableId <= doc.root.TableList.Count; tableId++)
        {
            var tableCells = doc.root.TableList[tableId];
            for (int checkItemIdx = 0; checkItemIdx < tableCells.Count; checkItemIdx++)
            {
                var tablerec = tableCells[checkItemIdx].Split("|");
                var pos = tablerec[0].Split(",");
                var value = tablerec[1].Replace(" ", "");
                var col = int.Parse(pos[2]);
                if (CompanyFullNameList.Contains(value) || CompanyShortNameList.Contains(value))
                {
                    CompanyPos.Add(tableCells[checkItemIdx]);
                }
            }
            CompanyPos.Reverse();
            for (int fixIdx = 0; fixIdx < tableCells.Count; fixIdx++)
            {
                var nullvalue = tableCells[fixIdx].Split("|")[1];
                var nullcol = int.Parse(tableCells[fixIdx].Split("|")[0].Split(",")[2]);
                var nullrow = int.Parse(tableCells[fixIdx].Split("|")[0].Split(",")[1]);
                if (nullvalue.Equals(strNullValue))
                {
                    foreach (var item in CompanyPos)
                    {
                        //向上寻找最近的
                        var tablerec = item.Split("|");
                        var pos = tablerec[0].Split(",");
                        var value = tablerec[1].Replace(" ", "");
                        var col = int.Parse(pos[2]);
                        var row = int.Parse(pos[1]);
                        if (nullcol == col && nullrow > row)
                        {
                            tableCells[fixIdx] = tableCells[fixIdx].Split("|")[0] + "|" + value;
                            break;
                        }
                    }
                }
            }
        }



        for (int tableId = 1; tableId <= doc.root.TableList.Count; tableId++)
        {
            var table = doc.root.TableList[tableId];
            for (int checkItemIdx = 0; checkItemIdx < table.Count; checkItemIdx++)
            {
                var tablerec = table[checkItemIdx].Split("|");
                var pos = tablerec[0].Split(",");
                var value = tablerec[1].Replace(" ", "");
                var row = int.Parse(pos[1]);
                var col = int.Parse(pos[2]);
                if (value == strNullValue && row != 1)
                {
                    //上一行是RowSpan，或者下一行是RowSpan，则这行也是RowSpan
                    var pre = tableId.ToString() + "," + (row - 1).ToString() + "," + col.ToString() + "|" + strRowSpanValue;
                    if (table.Contains(pre))
                    {
                        table[checkItemIdx] = tablerec[0] + "|" + strRowSpanValue;
                    }
                    else
                    {
                        var next = tableId.ToString() + "," + (row + 1).ToString() + "," + col.ToString() + "|" + strRowSpanValue;
                        if (table.Contains(next))
                        {
                            table[checkItemIdx] = tablerec[0] + "|" + strRowSpanValue;
                        }
                    }
                }
            }
        }

    }

}