using System;
using System.Collections.Generic;
using 金融数据整理大赛;

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
        if (Array.Length > 2)
        {
            c.ChangeEndDate = Array[2];
        }
        if (Array.Length > 3)
        {
            c.ChangePrice = Array[3];
        }
        if (Array.Length > 4)
        {
            c.ChangeNumber = Array[4];
        }
        if (Array.Length > 5)
        {
            c.HoldNumberAfterChange = Array[5];
        }
        if (Array.Length == 7)
        {
            c.HoldPercentAfterChange = Array[6];
        }
        return c;
    }


    public static int HolderFullNameCnt = 0;
    public static int HolderNameCnt = 0;

    public static int ChangeEndDateCnt = 0;
    public static struStockChange Extract(string htmlFileName)
    {
        var fi = new System.IO.FileInfo(htmlFileName);
        Program.Logger.WriteLine("Start FileName:[" + fi.Name + "]");
        var stockchange = new struStockChange();
        var node = HTMLEngine.Anlayze(htmlFileName);
        //公告ID
        stockchange.id = fi.Name.Replace(".html", "");
        Program.Logger.WriteLine("公告ID:" + stockchange.id);
        var Name = GetHolderFullName(node);
        stockchange.HolderFullName = Name.Item1;
        stockchange.HolderName = Name.Item2;
        stockchange.HolderFullName = Utility.GetStringBefore(stockchange.HolderFullName, "（以下简称");
        stockchange.ChangeEndDate = GetChangeEndDate(node);

        return stockchange;
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
            Program.Logger.WriteLine("候补股东全称修正：[" + Utility.GetStringBefore(item, "（以下简称") + "]");
            Program.Logger.WriteLine("候补股东简称：[" + RegularTool.GetValueBetweenMark(Utility.GetStringAfter(item, "（以下简称"),"“","”")  + "]");
        }
        return Tuple.Create("", "");
    }


    //变动截止日期
    static string GetChangeEndDate(HTMLEngine.MyHtmlNode node)
    {
        var KeyWordListArray = new string[][]
        {
            new string[]{"截止", "日"},
            new string[]{"截至", "日"},
        };
        return "";
    }

}