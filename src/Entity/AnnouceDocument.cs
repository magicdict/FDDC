using System;
using System.Collections.Generic;
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

    public static List<LocAndValue<(String MoneyAmount, String MoneyCurrency)>> moneylist;

    public static void Init(string htmlFileName)
    {
        var fi = new System.IO.FileInfo(htmlFileName);
        Program.Logger.WriteLine("Start FileName:[" + fi.Name + "]");
        Id = fi.Name.Replace(".html", "");
        Program.Logger.WriteLine("公告ID:" + Id);
        root = HTMLEngine.Anlayze(htmlFileName);
        companynamelist = BussinessLogic.GetCompanyNameByCutWord(root);
        datelist = LocateProperty.LocateDate(root);
        moneylist = LocateProperty.LocateMoney(root);
        foreach (var cn in companynamelist)
        {
            Program.Logger.WriteLine("公司名称：" + cn.secFullName);
            Program.Logger.WriteLine("公司简称：" + cn.secShortName);
        }
    }

}