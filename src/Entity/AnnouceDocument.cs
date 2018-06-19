using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;
using static BussinessLogic;
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

    //公告日期
    public static DateTime AnnouceDate;

    public static String AnnouceCompanyName;

    public static void Init(string htmlFileName)
    {
        var fi = new System.IO.FileInfo(htmlFileName);
        Program.Logger.WriteLine("Start FileName:[" + fi.Name + "]");
        Id = fi.Name.Replace(".html", "");
        Program.Logger.WriteLine("公告ID:" + Id);
        root = HTMLEngine.Anlayze(htmlFileName);
        companynamelist = BussinessLogic.GetCompanyNameByCutWord(root);
        datelist = LocateProperty.LocateDate(root);
        //公告中出现的最后一个日期作为公告发布日
        if (datelist.Count > 0) AnnouceDate = datelist.Last().Value;
        moneylist = LocateProperty.LocateMoney(root);
        foreach (var cn in companynamelist)
        {
            Program.Logger.WriteLine("公司名称：" + cn.secFullName);
            Program.Logger.WriteLine("公司简称：" + cn.secShortName);
        }
    }

}