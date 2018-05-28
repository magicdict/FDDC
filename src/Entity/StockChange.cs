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



    static Tuple<String, String> GetHolderFullName(HTMLEngine.MyHtmlNode node)
    {
        var KeyWordListArray = new string[][]
        {
            new string[]{"公司股东", "的通知"},
            new string[]{"公司股东", "通知"},

            new string[]{"控股股东", "的通知"},
            new string[]{"控股股东", "通知"},

            new string[]{"公司第一大股东", "的通知"},
            new string[]{"公司第一大股东", "通知"},
            new string[]{"第一大股东", "的通知"},
            new string[]{"第一大股东", "通知"},

            new string[]{"公司第二大股东", "的通知"},
            new string[]{"公司第二大股东", "通知"},
            new string[]{"第二大股东", "的通知"},
            new string[]{"第二大股东", "通知"},

            new string[]{"公司第三大股东", "的通知"},
            new string[]{"公司第三大股东", "通知"},
            new string[]{"第三大股东", "的通知"},
            new string[]{"第三大股东", "通知"},

            new string[]{"以上股东", "通知"},
            new string[]{"以上股东", "的通知"},
            new string[]{"以上股份的股东", "通知"},
            new string[]{"以上股份的股东", "的通知"},


            new string[]{"公司股东", "告知函"},
            new string[]{"公司股东", "的告知函"},

            new string[]{"控股股东", "告知函"},
            new string[]{"控股股东", "的告知函"},

            new string[]{"公司第一大股东", "告知函"},
            new string[]{"公司第一大股东", "的告知函"},
            new string[]{"第一大股东", "告知函"},
            new string[]{"第一大股东", "的告知函"},

            new string[]{"公司第二大股东", "告知函"},
            new string[]{"公司第二大股东", "的告知函"},
            new string[]{"第二大股东", "告知函"},
            new string[]{"第二大股东", "的告知函"},

            new string[]{"公司第三大股东", "告知函"},
            new string[]{"公司第三大股东", "的告知函"},
            new string[]{"第三大股东", "告知函"},
            new string[]{"第三大股东", "的告知函"},

            new string[]{"以上股东", "告知函"},
            new string[]{"以上股东", "的告知函"},
            new string[]{"以上股份的股东", "告知函"},
            new string[]{"以上股份的股东", "的告知函"},

            new string[]{"接到股东", "《"},
            new string[]{"接到第一大股东", "《"},
            new string[]{"接到第二大股东", "《"},
            new string[]{"接到第三大股东", "《"},

            new string[]{"接到控股股东", "减持"},
            new string[]{"接到控股股东", "增持"},
            new string[]{"接到公司股东", "减持"},
            new string[]{"接到公司股东", "增持"},

            new string[]{"接到公司股东", "的"},
            new string[]{"接到控股股东", "的"},

            new string[]{"接到", "的通知"},
            new string[]{"接到", "通知"},
            new string[]{"接到", "告知函"},
            new string[]{"接到", "的告知函"},


            new string[]{"收到股东", "《"},
            new string[]{"收到第一大股东", "《"},
            new string[]{"收到第二大股东", "《"},
            new string[]{"收到第三大股东", "《"},

            new string[]{"收到控股股东", "减持"},
            new string[]{"收到控股股东", "增持"},
            new string[]{"收到公司股东", "减持"},
            new string[]{"收到公司股东", "增持"},

            new string[]{"收到公司股东", "的"},
            new string[]{"收到控股股东", "的"},

            new string[]{"收到", "的通知"},
            new string[]{"收到", "通知"},
            new string[]{"收到", "告知函"},
            new string[]{"收到", "的告知函"},
            };
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