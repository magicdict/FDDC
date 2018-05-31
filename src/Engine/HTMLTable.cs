using System;
using System.Collections.Generic;
using System.Linq;
using static HTMLEngine;

public class HTMLTable
{
    public int RowCount = 0;
    public int ColumnCount = 0;

    Dictionary<String, string> dict = new Dictionary<String, string>();

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
            if (content == "<rowspan>")
            {
                //向上寻找非"<rowspan>"的内容
                for (int i = RowPos - 1; i >= 0; i--)
                {
                    pos = i + "," + ColPos;
                    if (dict.ContainsKey(pos))
                    {
                        content = dict[pos];
                        if (content != "<rowspan>") return content;
                    }
                }
            }

            if (content == "<colspan>")
            {
                //向上寻找非"<rowspan>"的内容
                for (int i = ColPos - 1; i >= 0; i--)
                {
                    pos = RowPos + "," + i;
                    if (dict.ContainsKey(pos))
                    {
                        content = dict[pos];
                        if (content != "<colspan>") return content;
                    }
                }
            }

            return dict[pos];
        }
        return "";
    }

    public String[] GetHeaderRow()
    {
        var Header = new String[ColumnCount];
        for (int i = 1; i < ColumnCount + 1; i++)
        {
            Header[i - 1] = CellValue(1, i);
        }

        return Header;
    }

    public bool IsTotalRow(int RowNo)
    {
        bool IsTotalRow = false;
        for (int i = 1; i <= ColumnCount; i++)
        {
            var x = CellValue(RowNo, i).Replace(" ", "");
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
                if (dict[pos] == "<rowspan>" || dict[pos] == "<colspan>")
                {
                    RowSpanCnt++;
                }
            }
            if (RowSpanCnt == ColumnCount - 1) IsTotalRow = true;
        }
        return IsTotalRow;
    }


    public struct CellInfo
    {
        public int TableId;

        public int Row;

        public int Column;

        public string RawData;
    }


    public List<CellInfo> CandidateCell = new List<CellInfo>();

    //在所有的表格中，寻找包含指定内容的单元格
    public void searchKeyWordAtTable(MyRootHtmlNode root, string keyword, string exclude = "")
    {
        foreach (var content in root.TableList)
        {
            var pos = "";
            var value = "";
            if (value.IndexOf(keyword) != -1)
            {
                if (exclude != "")
                {
                    if (value.IndexOf(exclude) != -1) continue;
                }
                var cellInfo = new CellInfo();
                cellInfo.RawData = value;
                cellInfo.Column = int.Parse(pos.Split(",")[0]);
                cellInfo.Row = int.Parse(pos.Split(",")[1]);
                cellInfo.Column = int.Parse(pos.Split(",")[2]);
                CandidateCell.Add(cellInfo);
            }
        }
    }


    public struct TableSearchRule
    {
        public string Name;

        public List<String> Rule;

        public bool IsEq;

        public Func<String, String, String> Normalize;

    }


    public static bool IsSameContent(CellInfo[] Row1, CellInfo[] Row2)
    {
        for (int i = 0; i < Row1.Length; i++)
        {
            if (String.IsNullOrEmpty(Row1[i].RawData) && String.IsNullOrEmpty(Row2[i].RawData)) continue;
            if (String.IsNullOrEmpty(Row1[i].RawData)) return false;
            if (String.IsNullOrEmpty(Row2[i].RawData)) return false;
            if (!Row1[i].RawData.Equals(Row2[i].RawData)) return false;
        }
        return true;
    }

    public static List<CellInfo[]> GetMultiInfo(HTMLEngine.MyRootHtmlNode root, List<TableSearchRule> Rules, bool IsMeger)
    {
        var Container = new List<CellInfo[]>();
        for (int tableIndex = 0; tableIndex < root.TableList.Count; tableIndex++)
        {
            var table = new HTMLTable(root.TableList[tableIndex + 1]);
            var HeaderRow = table.GetHeaderRow();

            var checkResult = new int[Rules.Count];
            for (int checkItemIdx = 0; checkItemIdx < Rules.Count; checkItemIdx++)
            {
                //在每个行首单元格检索
                for (int ColIndex = 0; ColIndex < HeaderRow.Length; ColIndex++)
                {
                    if (Rules[checkItemIdx].IsEq)
                    {
                        //相等模式：规则里面没有该词语
                        if (!Rules[checkItemIdx].Rule.Contains(HeaderRow[ColIndex])) continue;
                    }
                    else
                    {
                        bool IsMatch = false;
                        //包含模式
                        foreach (var r in Rules[checkItemIdx].Rule)
                        {
                            if (HeaderRow[ColIndex].Contains(r))
                            {
                                IsMatch = true;
                                break;
                            }
                        }
                        if (!IsMatch) continue;
                    }
                    //找到列位置
                    checkResult[checkItemIdx] = ColIndex + 1;
                    break;
                }
                //主字段没有找到，其他不用找了
                if (checkResult[0] == 0) break;
            }

            //主字段没有找到，下一张表
            if (checkResult[0] == 0) continue;

            for (int RowNo = 2; RowNo <= table.RowCount; RowNo++)
            {
                if (table.IsTotalRow(RowNo)) continue;          //非合计行
                var target = table.CellValue(RowNo, checkResult[0]);    //主字段非空
                if (target == "" || target == "<rowspan>" || target == "<colspan>" || target == "<null>") continue;
                if (Rules[0].Rule.Contains(target)) continue;

                var RowData = new CellInfo[Rules.Count];
                for (int checkItemIdx = 0; checkItemIdx < Rules.Count; checkItemIdx++)
                {
                    if (checkResult[checkItemIdx] == 0) continue;
                    var ColNo = checkResult[checkItemIdx];
                    RowData[checkItemIdx].TableId = tableIndex + 1;
                    RowData[checkItemIdx].Row = RowNo;
                    RowData[checkItemIdx].Column = ColNo;

                    if (table.CellValue(RowNo, ColNo).Equals("<null>")) continue;
                    RowData[checkItemIdx].RawData = table.CellValue(RowNo, ColNo);
                    if (Rules[checkItemIdx].Normalize != null)
                    {
                        RowData[checkItemIdx].RawData = Rules[checkItemIdx].Normalize(RowData[checkItemIdx].RawData, HeaderRow[ColNo - 1]);
                    }

                }

                var HasSame = false;
                foreach (var existRow in Container)
                {
                    if (IsSameContent(existRow, RowData))
                    {
                        HasSame = true;
                        break;
                    }
                }
                if (!HasSame) Container.Add(RowData);
            }
        }
        if (IsMeger) Container = MergerMultiInfo(Container);
        return Container;
    }

    static List<CellInfo[]> MergerMultiInfo(List<CellInfo[]> rows)
    {
        var dict = new Dictionary<String, CellInfo[]>();
        foreach (var Row in rows)
        {
            var key = Row[0].RawData;
            if (dict.ContainsKey(key))
            {
                //已经有了相同Key的记录
                var Rec = dict[key];
                for (int i = 1; i < Rec.Length; i++)
                {
                    if (!String.IsNullOrEmpty(Row[i].RawData))
                    {
                        if (String.IsNullOrEmpty(Rec[i].RawData) || Rec[i].RawData == "<null>")
                        {
                            Rec[i].RawData = Row[i].RawData;
                        }
                        else
                        {
                            if (!Rec[i].RawData.Equals(Row[i].RawData))
                            {
                                Rec[i].RawData += "|" + Row[i].RawData;
                            }
                        }
                    }
                }
            }
            else
            {
                dict.Add(key, Row);
            }
        }
        return dict.Values.ToList();
    }

}