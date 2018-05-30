using System;
using System.Collections.Generic;

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
            if (x == "合计" || x == "小计" || 
                x == "—" || x == "－" || x == "-" || x == "/" || 
                x == "--" ||  x=="——")
            {
                IsTotalRow = true;
                break;
            }
        }
        return IsTotalRow;
    }

}