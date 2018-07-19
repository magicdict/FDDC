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
    /// NER列表
    /// </summary>
    public List<String> Nerlist;

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
        Nerlist = LTPTrainingNER.AnlayzeNER(XMLPath + "\\" + XMLFileName);
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
        Dplist = LTPTrainingDP.AnlayzeDP(XMLPath + "\\" + XMLFileName);
        XMLPath = fi.DirectoryName.Replace("html", "srl");
        Srllist = LTPTrainingSRL.AnlayzeSRL(XMLPath + "\\" + XMLFileName);

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

        foreach (var cn in companynamelist)
        {
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("公司名称：" + cn.secFullName);
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("公司简称：" + cn.secShortName);
        }

        var ParagraghlocateValue = new List<ParagraghLoc>();
        foreach (var paragragh in root.Children)
        {
            foreach (var s in paragragh.Children)
            {
                var p = SentenceLocate(s.PositionId);
                if (p.bracketlist.Count + p.datelist.Count + p.moneylist.Count == 0) continue;
                ParagraghlocateValue.Add(p);
            }
        }

        if (root.TableList == null) return;
        //表格的处理(表分页)
        HTMLTable.FixSpiltTable(this, new string[] { "集中竞价交易", "竞价交易", "大宗交易", "约定式购回" });
        //NULL的对应
        HTMLTable.FixNullValue(this);
        //指代表
        GetReplaceTable();
    }

    public abstract List<RecordBase> Extract();

    /// <summary>
    /// 每句句子中，各种实体的聚合
    /// </summary>
    /// <param name="PosId"></param>
    /// <returns></returns>
    ParagraghLoc SentenceLocate(int PosId)
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

    /// <summary>
    /// 指代表的字典
    /// </summary>
    /// <typeparam name="String"></typeparam>
    /// <typeparam name="String"></typeparam>
    /// <returns></returns>
    public Dictionary<String, String> ReplacementDict = new Dictionary<String, String>();
    /// <summary>
    /// 公告中特殊的指代表
    /// </summary>
    /// <returns></returns>
    public void GetReplaceTable()
    {
        //指代表的抽取
        foreach (var table in root.TableList)
        {
            var htmltable = new HTMLTable(table.Value);
            if (htmltable.ColumnCount != 3) continue;
            for (int RowNo = 1; RowNo <= htmltable.RowCount; RowNo++)
            {
                if (htmltable.CellValue(RowNo, 2) == "指")
                {
                    var key = htmltable.CellValue(RowNo, 1);
                    var value = htmltable.CellValue(RowNo, 3);
                    if (!ReplacementDict.ContainsKey(key)) ReplacementDict.Add(key, value);
                }
            }
        }

        //寻找指代表中表示公司简称和公司全称的项目，加入到Companylist中
        //注意，左边的主键需要根据分隔符好切分
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
                    //一般来说简称是3，4个字的
                    if (item.Value.EndsWith("公司") || item.Value.EndsWith("有限合伙"))
                    {
                        companynamelist.Add(new struCompanyName()
                        {
                            secFullName = item.Value,
                            secShortName = key,
                            positionId = -1 //表示从释义表格来的
                        });
                    }
                }
            }
        }
    }
}