using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FDDC;
using static CompanyNameLogic;
using static HTMLEngine;
using static LocateProperty;
using static LTP;

public class AnnouceDocument
{
    public String Id;
    public MyRootHtmlNode root;
    //公司
    public List<struCompanyName> companynamelist;
    //日期
    public List<LocAndValue<DateTime>> datelist;
    //金额
    public List<LocAndValue<(String MoneyAmount, String MoneyCurrency)>> moneylist;

    public List<LocAndValue<String>> quotationList;

    public List<String> Nerlist;


    public List<List<struWordDP>> Dplist;


    public List<String> Srllist;

    //公告日期
    public DateTime AnnouceDate;

    public String AnnouceCompanyName;

    public String TextFileName;

    public AnnouceDocument(string htmlFileName)
    {
        var fi = new System.IO.FileInfo(htmlFileName);
        TextFileName = htmlFileName.Replace("html", "txt");
        if (!TextFileName.EndsWith(".txt"))
        {
            //防止无扩展名的html文件
            TextFileName += ".txt";
        }
        if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("Start FileName:[" + fi.Name + "]");
        Id = fi.Name.Replace(".html", String.Empty);
        if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("公告ID:" + Id);
        root = new HTMLEngine().Anlayze(htmlFileName, TextFileName);
        AnnouceCompanyName = String.Empty;

        var XMLFileName = fi.Name.Replace("html", "xml");
        if (!XMLFileName.EndsWith(".xml"))
        {
            //防止无扩展名的html文件
            XMLFileName += ".xml";
        }
        var XMLPath = fi.DirectoryName.Replace("html", "ner");
        Nerlist = LTP.AnlayzeNER(XMLPath + "\\" + XMLFileName);
        foreach (var ner in Nerlist)
        {
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("识别实体：" + ner);
        }
        if (Nerlist.Count != 0)
        {
            AnnouceCompanyName = Nerlist.Last();
            Nerlist = Nerlist.Distinct().ToList();
        }
        else
        {
            //从最后向前查找
            for (int i = root.Children.Count - 1; i >= 0; i--)
            {
                for (int j = root.Children[i].Children.Count - 1; j >= 0; j--)
                {
                    var content = root.Children[i].Children[j].Content;
                    content = content.Replace(" ", String.Empty);
                    if (content.EndsWith("有限公司董事会"))
                    {
                        AnnouceCompanyName = content.Substring(0, content.Length - 3);
                        break;
                    }
                }
                if (!String.IsNullOrEmpty(AnnouceCompanyName)) break;
            }
        }

        XMLPath = fi.DirectoryName.Replace("html", "dp");
        Dplist = LTP.AnlayzeDP(XMLPath + "\\" + XMLFileName);
        XMLPath = fi.DirectoryName.Replace("html", "srl");
        Srllist = LTP.AnlayzeSRL(XMLPath + "\\" + XMLFileName);
        foreach (var m in Srllist)
        {
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("SRL：" + m);
        }

        datelist = LocateProperty.LocateDate(root);
        foreach (var m in datelist)
        {
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("位置：" + m.Loc);
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("日期：" + m.Value.ToString("yyyy-MM-dd"));
        }
        //公告中出现的最后一个日期作为公告发布日
        if (datelist.Count > 0) AnnouceDate = datelist.Last().Value;

        quotationList = LocateProperty.LocateQuotation(root, true);
        foreach (var m in quotationList)
        {
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("位置：" + m.Loc);
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("括号内容：" + m.Value);
        }

        moneylist = LocateProperty.LocateMoney(root);
        foreach (var m in moneylist)
        {
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("位置：" + m.Loc);
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("数量：" + m.Value.MoneyAmount);
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("货币：" + m.Value.MoneyCurrency);
        }

        companynamelist = CompanyNameLogic.GetCompanyNameByCutWord(root);

        foreach (var cn in companynamelist)
        {
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("公司名称：" + cn.secFullName);
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("公司简称：" + cn.secShortName);
        }
    }


    public struct ParagraghLoc
    {
        //日期
        public List<LocAndValue<DateTime>> datelist;
        //金额
        public List<LocAndValue<(String MoneyAmount, String MoneyCurrency)>> moneylist;

        public List<LocAndValue<String>> bracketlist;


        public void Init()
        {
            datelist = new List<LocAndValue<DateTime>>();
            moneylist = new List<LocAndValue<(String MoneyAmount, String MoneyCurrency)>>();
            bracketlist = new List<LocAndValue<String>>();
        }
    }

    public ParagraghLoc SentenceLocate(int PosId)
    {
        var paragragh = new ParagraghLoc();
        paragragh.Init();
        foreach (var item in datelist)
        {
            if (item.Loc == PosId) paragragh.datelist.Add(item);
        }
        foreach (var item in moneylist)
        {
            if (item.Loc == PosId) paragragh.moneylist.Add(item);
        }
        foreach (var item in quotationList)
        {
            if (item.Loc == PosId) paragragh.bracketlist.Add(item);
        }
        return paragragh;
    }
}