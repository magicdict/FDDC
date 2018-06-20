using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;
using static CompanyNameLogic;
using static HTMLEngine;
using static LocateProperty;


public class AnnouceDocument
{
    public static String Id;
    public static MyRootHtmlNode root;
    //公司
    public static List<struCompanyName> companynamelist;
    //日期
    public static List<LocAndValue<DateTime>> datelist;
    //金额
    public static List<LocAndValue<(String MoneyAmount, String MoneyCurrency)>> moneylist;

    public static List<LocAndValue<String>> bracketlist;

    //公告日期
    public static DateTime AnnouceDate;

    public static String AnnouceCompanyName;

    public static String TextFileName;

    public static void Init(string htmlFileName)
    {
        var fi = new System.IO.FileInfo(htmlFileName);
        TextFileName = htmlFileName.Replace("html", "txt");
        Program.Logger.WriteLine("Start FileName:[" + fi.Name + "]");
        Id = fi.Name.Replace(".html", "");
        Program.Logger.WriteLine("公告ID:" + Id);
        root = HTMLEngine.Anlayze(htmlFileName);

        datelist = LocateProperty.LocateDate(root);
        foreach (var m in datelist)
        {
            Program.Logger.WriteLine("位置：" + m.Loc);
            Program.Logger.WriteLine("日期：" + m.Value.ToString("yyyy-MM-dd"));
        }
        //公告中出现的最后一个日期作为公告发布日
        if (datelist.Count > 0) AnnouceDate = datelist.Last().Value;

        bracketlist = LocateProperty.LocateBracket(root);
        foreach (var m in bracketlist)
        {
            Program.Logger.WriteLine("位置：" + m.Loc);
            Program.Logger.WriteLine("括号内容：" + m.Value);
        }

        moneylist = LocateProperty.LocateMoney(root);
        foreach (var m in moneylist)
        {
            Program.Logger.WriteLine("位置：" + m.Loc);
            Program.Logger.WriteLine("数量：" + m.Value.MoneyAmount);
            Program.Logger.WriteLine("货币：" + m.Value.MoneyCurrency);
        }

        companynamelist = CompanyNameLogic.GetCompanyNameByCutWord(root);
        foreach (var cn in companynamelist)
        {
            Program.Logger.WriteLine("公司名称：" + cn.secFullName);
            Program.Logger.WriteLine("公司简称：" + cn.secShortName);
        }
    }

}