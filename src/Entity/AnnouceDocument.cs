using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FDDC;
using static CompanyNameLogic;
using static HTMLEngine;
using static LocateProperty;
using static LTPTrainingNER;
using static LTPTrainingDP;

/// <summary>
/// 公告
/// </summary>
public abstract class AnnouceDocument
{
    /// <summary>
    /// 公告ID
    /// </summary>
    public String Id;
    /// <summary>
    /// HTML根节点
    /// </summary>
    public MyRootHtmlNode root;
    /// <summary>
    /// 公司
    /// </summary>
    public List<struCompanyName> companynamelist;
    /// <summary>
    /// 日期
    /// </summary>
    public List<LocAndValue<DateTime>> datelist;
    /// <summary>
    /// 金额
    /// </summary>
    /// <param name="MoneyAmount"></param>
    /// <param name="MoneyCurrency"></param>
    public List<LocAndValue<(String MoneyAmount, String MoneyCurrency)>> moneylist;

    /// <summary>
    /// 引号
    /// </summary>
    public List<LocAndValue<String>> quotationList;

    /// <summary>
    /// NER列表(机构)
    /// </summary>
    public List<struNerInfo> Nerlist;

    /// <summary>
    /// NER列表（人名）
    /// </summary>
    public List<String> PersonNamelist;

    public NerMap nermap;

    /// <summary>
    /// DP列表
    /// </summary>
    public List<List<struWordDP>> Dplist;

    /// <summary>
    /// SRL列表
    /// </summary>
    public List<List<LTPTrainingSRL.struWordSRL>> Srllist;

    /// <summary>
    /// 公告日期
    /// </summary>
    public DateTime AnnouceDate;
    /// <summary>
    /// 发布公告的公司
    /// </summary>
    public String AnnouceCompanyName;
    /// <summary>
    /// 文本文件
    /// </summary>
    public String TextFileName;
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="htmlFileName"></param>
    public void Init(string htmlFileName)
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
        if (!Program.IsMultiThreadMode) Program.CIRecord.WriteLine("公告ID" + Id);
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
        var Nerlist = LTPTrainingNER.AnlayzeNER(XMLPath + Path.DirectorySeparatorChar + "" + XMLFileName);
        foreach (var ner in Nerlist)
        {
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("识别实体：" + ner.RawData + ":" + ner.Type);
        }
        if (Nerlist.Count != 0)
        {
            //最后一个机构名
            AnnouceCompanyName = Nerlist.
                                Where((n) => n.Type == enmNerType.Ni)
                                .Select((m) => m.RawData).Last();
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
        Dplist = LTPTrainingDP.AnlayzeDP(XMLPath + Path.DirectorySeparatorChar + "" + XMLFileName);
        XMLPath = fi.DirectoryName.Replace("html", "srl");
        Srllist = LTPTrainingSRL.AnlayzeSRL(XMLPath + Path.DirectorySeparatorChar + "" + XMLFileName);

        datelist = LocateProperty.LocateDate(root);
        foreach (var m in datelist)
        {
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("位置：" + m.Loc);
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("日期：" + m.Value.ToString("yyyy-MM-dd"));
        }
        //增减持公告中出现的最后一个日期作为公告发布日
        if (StockChange.PublishTime.ContainsKey(Id))
        {
            int year = int.Parse(StockChange.PublishTime[Id].Substring(0, 4));
            int month = int.Parse(StockChange.PublishTime[Id].Substring(5, 2));
            int day = int.Parse(StockChange.PublishTime[Id].Substring(8, 2));
            AnnouceDate = new DateTime(year, month, day);
        }
        else
        {
            if (datelist.Count > 0) AnnouceDate = datelist.Last().Value;
        }

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

        var newname = new List<struCompanyName>();
        foreach (var cn in companynamelist)
        {
            if (!string.IsNullOrEmpty(cn.secFullName) && string.IsNullOrEmpty(cn.secShortName))
            {
                //是否存在引号里面的词语正好是公司全称
                foreach (var item in quotationList)
                {
                    if (cn.secFullName.StartsWith(item.Value))
                    {
                        var newComp = new struCompanyName()
                        {
                            secFullName = cn.secFullName,
                            secShortName = item.Value
                        };
                        newname.Add(newComp);
                        break;
                    }
                }
            }
        }

        companynamelist.AddRange(newname);

        foreach (var cn in companynamelist)
        {
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("公司名称：" + cn.secFullName);
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("公司简称：" + cn.secShortName);
        }

        if (root.TableList == null) return;
        //表格的处理(表分页)
        HTMLTable.FixSpiltTable(this);
        //NULL的对应
        HTMLTable.FixNullValue(this);
        //指代表
        GetReplaceTable();

        //实体地图
        nermap = new NerMap();
        nermap.Anlayze(this);

    }

    public abstract List<RecordBase> Extract();

    /// <summary>
    /// 释义表编号
    /// </summary>
    public List<int> ReplaceTableId = new List<int>();

    /// <summary>
    /// 释义表的字典
    /// </summary>
    /// <typeparam name="String"></typeparam>
    /// <typeparam name="String"></typeparam>
    /// <returns></returns>
    public Dictionary<String, String> ReplacementDict = new Dictionary<String, String>();
    /// <summary>
    /// 公告中特殊的释义表
    /// </summary>
    /// <returns></returns>
    public void GetReplaceTable()
    {
        //释义表的抽取
        foreach (var table in root.TableList)
        {
            var htmltable = new HTMLTable(table.Value);

            if (htmltable.ColumnCount == 2){
                for (int RowNo = 1; RowNo <= htmltable.RowCount; RowNo++)
                {
                    if (htmltable.CellValue(RowNo, 2).StartsWith("指"))
                    {
                        var key = htmltable.CellValue(RowNo, 1);
                        var value = htmltable.CellValue(RowNo, 2).Substring(1);
                        if (!ReplacementDict.ContainsKey(key)) ReplacementDict.Add(key, value);
                        if (!ReplaceTableId.Contains(table.Key)) ReplaceTableId.Add(table.Key);
                    }
                }
            }

            if (htmltable.ColumnCount == 3)
            {
                for (int RowNo = 1; RowNo <= htmltable.RowCount; RowNo++)
                {
                    if (htmltable.CellValue(RowNo, 2) == "指")
                    {
                        var key = htmltable.CellValue(RowNo, 1);
                        var value = htmltable.CellValue(RowNo, 3);
                        if (!ReplacementDict.ContainsKey(key)) ReplacementDict.Add(key, value);
                        if (!ReplaceTableId.Contains(table.Key)) ReplaceTableId.Add(table.Key);
                    }
                }
            }
        }

        if(ReplacementDict.Count == 0) return;

        //寻找指释义表中表示公司简称和公司全称的项目，加入到Companylist中
        //注意，左边的主键，右边的值，都可能需要根据分隔符好切分
        foreach (var item in ReplacementDict)
        {
            var keys = item.Key.Split(Utility.SplitChar);
            var keys2 = item.Key.Split("/");
            if (keys.Length == 1 && keys2.Length > 1)
            {
                keys = keys2;
            }
            foreach (var key in keys)
            {
                if (key.Length >= 3 && key.Length <= 6)
                {
                    var values = item.Value.Split(Utility.SplitChar);
                    var values2 = item.Value.Split("；");
                    if (values.Length == 1 && values2.Length > 1)
                    {
                        values = values2;
                    }
                    //一般来说简称是3-6个字的
                    foreach (var value in values)
                    {
                        if (value.EndsWith("公司") || value.EndsWith("有限合伙") || value.EndsWith("Co.,Ltd."))
                        {
                            companynamelist.Add(new struCompanyName()
                            {
                                secFullName = value,
                                secShortName = key,
                                positionId = -1 //表示从释义表格来的
                            });
                        }
                    }
                }
            }
        }
    }
}