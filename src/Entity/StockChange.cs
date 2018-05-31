using System;
using System.Collections.Generic;
using FDDC;

public class StockChange
{
    public struct struStockChange
    {
        //公告id
        public string id;

        //股东全称
        public string HolderFullName;

        //股东简称
        public string HolderName;

        //变动截止日期
        public string ChangeEndDate;

        //变动价格
        public string ChangePrice;

        //变动数量
        public string ChangeNumber;

        //变动后持股数
        public string HoldNumberAfterChange;

        //变动后持股比例
        public string HoldPercentAfterChange;

    }

    internal static struStockChange ConvertFromString(string str)
    {
        var Array = str.Split("\t");
        var c = new struStockChange();
        c.id = Array[0];
        c.HolderFullName = Array[1];
        c.HolderName = Array[2];
        if (Array.Length > 3)
        {
            c.ChangeEndDate = Array[3];
        }
        if (Array.Length > 4)
        {
            c.ChangePrice = Array[4];
        }
        if (Array.Length > 5)
        {
            c.ChangeNumber = Array[5];
        }
        if (Array.Length > 6)
        {
            c.HoldNumberAfterChange = Array[6];
        }
        if (Array.Length == 8)
        {
            c.HoldPercentAfterChange = Array[7];
        }
        return c;
    }



    internal static string ConvertToString(struStockChange increaseStock)
    {
        var record = increaseStock.id + "," +
        increaseStock.HolderFullName + "," +
        increaseStock.HolderName + "," +
        increaseStock.ChangeEndDate + ",";
        record += Normalizer.NormalizeNumberResult(increaseStock.ChangePrice) + ",";
        record += Normalizer.NormalizeNumberResult(increaseStock.ChangeNumber) + ",";
        record += Normalizer.NormalizeNumberResult(increaseStock.HoldNumberAfterChange) + ",";
        record += Normalizer.NormalizeNumberResult(increaseStock.HoldPercentAfterChange) + ",";
        return record;
    }

    public static List<struStockChange> Extract(string htmlFileName)
    {
        var list = new List<struStockChange>();
        var fi = new System.IO.FileInfo(htmlFileName);
        Program.Logger.WriteLine("Start FileName:[" + fi.Name + "]");
        var node = HTMLEngine.Anlayze(htmlFileName);

        list = ExtractFromTable(node, fi.Name.Replace(".html", ""));
        if (list.Count > 0) return list;

        var stockchange = new struStockChange();
        //公告ID
        stockchange.id = fi.Name.Replace(".html", "");
        Program.Logger.WriteLine("公告ID:" + stockchange.id);
        var Name = GetHolderFullName(node);
        stockchange.HolderFullName = Name.Item1;
        stockchange.HolderName = Name.Item2;
        stockchange.ChangeEndDate = GetChangeEndDate(node);
        list.Add(stockchange);
        return list;
    }


    static List<struStockChange> ExtractFromTable(HTMLEngine.MyRootHtmlNode root, string id)
    {
        //相同持股人，多次增减持日期不同，产生多条记录
        var list = new List<struStockChange>();
        for (int tableIndex = 0; tableIndex < root.TableList.Count; tableIndex++)
        {
            //寻找表头是"发行对象" 或者 "发行对象名称" 的列号
            var table = new HTMLTable(root.TableList[tableIndex + 1]);
            var HeaderRow = table.GetHeaderRow();
            var pos = -1;
            for (int j = 0; j < HeaderRow.Length; j++)
            {
                //认购对象必须先于发行对象
                if (HeaderRow[j] == "股东名称")
                {
                    pos = j + 1;          //Index从0开始
                    var DateRow = -1;     //截止日期
                    var NumberRow = -1;   //股票数
                    var PriceRow = -1;    //金额数
                    for (int DateIndex = 0; DateIndex < HeaderRow.Length; DateIndex++)
                    {
                        if (HeaderRow[DateIndex].Contains("减持期间") ||
                            HeaderRow[DateIndex].Contains("增持期间"))
                        {
                            DateRow = DateIndex + 1;    //Index从0开始
                        }
                    }

                    for (int NumberIndex = 0; NumberIndex < HeaderRow.Length; NumberIndex++)
                    {
                        if (HeaderRow[NumberIndex].Contains("减持股数") ||
                            HeaderRow[NumberIndex].Contains("增持股数"))
                        {
                            NumberRow = NumberIndex + 1;    //Index从0开始
                        }
                    }


                    for (int NumberIndex = 0; NumberIndex < HeaderRow.Length; NumberIndex++)
                    {
                        if (HeaderRow[NumberIndex].Contains("减持均价") ||
                            HeaderRow[NumberIndex].Contains("增持均价"))
                        {
                            PriceRow = NumberIndex + 1;    //Index从0开始
                        }
                    }


                    for (int k = 2; k <= table.RowCount + 1; k++)
                    {
                        var target = table.CellValue(k, pos);
                        if (table.IsTotalRow(k)) continue;
                        if (target == "" || target == "<rowspan>" || target == "<colspan>" || target == "<null>") continue;
                        var stockchange = new struStockChange();
                        stockchange.id = id;

                        stockchange.HolderFullName = target;
                        Program.Logger.WriteLine("候补增减持对象:" + target + " @TableIndex:" + tableIndex);
                        //是否能提取其他信息：
                        if (NumberRow != -1)
                        {
                            stockchange.ChangeNumber = table.CellValue(k, NumberRow);
                            Program.Logger.WriteLine("候补增减持数量:" + table.CellValue(k, NumberRow) + " @TableIndex:" + tableIndex);
                        }
                        if (PriceRow != -1)
                        {
                            stockchange.ChangePrice = table.CellValue(k, PriceRow);
                            Program.Logger.WriteLine("候补增减持金额:" + table.CellValue(k, PriceRow) + " @TableIndex:" + tableIndex);
                        }
                        if (DateRow != -1)
                        {
                            stockchange.ChangeEndDate = table.CellValue(k, DateRow);
                            Program.Logger.WriteLine("候补截止期:" + table.CellValue(k, DateRow) + " @TableIndex:" + tableIndex);
                        }
                        list.Add(stockchange);

                    }
                }
            }
        }
        return list;
    }


    static Tuple<String, String> GetHolderFullName(HTMLEngine.MyRootHtmlNode root)
    {
        var Extractor = new ExtractProperty();
        var StartArray = new string[] { "接到", "收到", "股东" };
        var EndArray = new string[] { "的", "通知", "告知函", "减持", "增持", "《" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            Program.Logger.WriteLine("候补股东全称：[" + item + "]");
            var fullname = Utility.GetStringBefore(item, "（以下简称");
            Program.Logger.WriteLine("候补股东全称修正：[" + fullname + "]");
            var shortname = RegularTool.GetValueBetweenMark(Utility.GetStringAfter(item, "（以下简称"), "“", "”");
            Program.Logger.WriteLine("候补股东简称：[" + shortname + "]");
            return Tuple.Create(fullname, shortname);
        }
        return Tuple.Create("", "");
    }


    //变动截止日期
    static string GetChangeEndDate(HTMLEngine.MyRootHtmlNode root)
    {
        var Extractor = new ExtractProperty();
        var StartArray = new string[] { "截止", "截至" };
        var EndArray = new string[] { "日" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            Program.Logger.WriteLine("候补变动截止日期：[" + item + "]");
            return item;
        }
        return "";
    }
}