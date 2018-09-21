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
    /// 百分比
    /// </summary>
    public List<LocAndValue<String>> percentList;
    /// <summary>
    /// 股票数
    /// </summary>
    public List<LocAndValue<String>> StockNumberList;
    /// <summary>
    /// 自定义
    /// </summary>
    /// <returns></returns>
    public List<LocAndValue<String>> CustomerList = new List<LocAndValue<String>>();

    /// <summary>
    /// NER列表(机构)
    /// </summary>
    public List<struNerInfo> Nerlist;

    /// <summary>
    /// NER列表（人名）
    /// </summary>
    public List<String> PersonNamelist;
    /// <summary>
    /// 实体地图
    /// </summary>
    public NerMap nermap;

    /// <summary>
    /// DP列表
    /// </summary>
    public List<List<struWordDP>> Dplist = new List<List<struWordDP>>();

    /// <summary>
    /// SRL列表
    /// </summary>
    public List<List<LTPTrainingSRL.struWordSRL>> Srllist = new List<List<LTPTrainingSRL.struWordSRL>>();

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
    public String HTMLFileName;
    /// <summary>
    /// 文本文件
    /// </summary>
    public String TextFileName;
    /// <summary>
    /// NER的XML文件位置
    /// </summary>
    public String NerXMLFileName;
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="htmlFileName"></param>
    public void Init()
    {
        var fi = new System.IO.FileInfo(HTMLFileName);
        if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("Start FileName:[" + fi.Name + "]");
        if (!Program.IsMultiThreadMode) Program.CIRecord.WriteLine("公告ID" + Id);
        if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("公告ID:" + Id);
        if (this is StockChange || this is Contract)
        {
            //增减持和合同的时候，一定要确保文本和XML的存在
            if (!File.Exists(TextFileName)) Console.WriteLine(TextFileName + "Not Found");
            if (!File.Exists(NerXMLFileName)) Console.WriteLine(NerXMLFileName + "Not Found");
        }
        root = new HTMLEngine().Anlayze(HTMLFileName, TextFileName);
        AnnouceCompanyName = String.Empty;
        Nerlist = LTPTrainingNER.AnlayzeNER(NerXMLFileName);
        foreach (var ner in Nerlist)
        {
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("识别实体：" + ner.RawData + ":" + ner.Type);
        }
        Nerlist = Nerlist.Distinct().ToList();

        datelist = LocateProperty.LocateDate(root);
        foreach (var m in datelist)
        {
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("位置：" + m.Loc);
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("日期：" + m.Value.ToString("yyyy-MM-dd"));
        }
        //增减持公告中出现的最后一个日期作为公告发布日
        if (StockChange.PublishTime.ContainsKey(Id))
        {
            //18-2月-2017
            AnnouceDate = StockChange.PublishTime[Id];
            //Console.WriteLine("Found AnnouceDate" + Id + "  AnnouceDate:  " + AnnouceDate.ToString("yyyy-MM-dd"));
        }
        else
        {
            if (datelist.Count > 0) AnnouceDate = datelist.Last().Value;
        }

        //引号和书名号
        quotationList = LocateProperty.LocateQuotation(root, true);
        foreach (var m in quotationList)
        {
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("位置：" + m.Loc);
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("括号内容：" + m.Value);
        }

        //百分比
        percentList = LocatePercent(root);

        StockNumberList = LocateStockNumber(root);
        //货币
        moneylist = LocateProperty.LocateMoney(root);
        foreach (var m in moneylist)
        {
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("位置：" + m.Loc);
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("数量：" + m.Value.MoneyAmount);
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("货币：" + m.Value.MoneyCurrency);
        }

        //实体地图
        CompanyNameAnlayze();
        nermap = new NerMap();
        nermap.Anlayze(this);
        //使用简称表
        FillShortName();

        if (root.TableList == null) return;
        //表格的处理(表分页)
        HTMLTable.FixSpiltTable(this);
        //NULL的对应
        HTMLTable.FixNullValue(this);
        //指代表
        GetExplainTable();
        //公告公司
        GetAnnouceCompanyName();
        //使用NER表，修正子公司，
        //本公司所属子公司中铁六局集团有限公司
        FixSubCompany();
    }

    private void FixSubCompany()
    {
        var loc = LocateCustomerWord(root, new string[] { "子公司" }.ToList());
        var FatherCompany = "";
        foreach (var subcompMarkloc in loc)
        {
            if (nermap.ParagraghlocateDict.ContainsKey(subcompMarkloc.Loc))
            {
                foreach (var ner in nermap.ParagraghlocateDict[subcompMarkloc.Loc].NerList)
                {
                    if (ner.StartIdx < subcompMarkloc.StartIdx)
                    {
                        var Words = "";
                        foreach (var p in root.Children)
                        {
                            foreach (var s in p.Children)
                            {
                                if (s.PositionId == subcompMarkloc.Loc)
                                {
                                    var length = subcompMarkloc.StartIdx - ner.StartIdx - ner.Value.Length;
                                    var startidx = ner.StartIdx + ner.Value.Length;
                                    if (startidx + length >= s.Content.Length) continue;
                                    if (s.Content.Length < startidx) continue;
                                    if (length <= 0) continue;
                                    Words = s.Content.Substring(startidx, length);
                                    Words = RegularTool.TrimChineseBrackets(Words);
                                    if (Words.Length <= 5) FatherCompany = ner.Value;
                                }
                            }
                        }
                    }
                    if (ner.Distance(subcompMarkloc) == 0)
                    {
                        if (ner.Value.EndsWith("有限公司"))
                        {
                            //如果公司表没有这个公司，则追加
                            var m = companynamelist.Where(x => x.secFullName == ner.Value).ToList();
                            if (m.Count == 0)
                            {
                                companynamelist.Add(new struCompanyName()
                                {
                                    secFullName = ner.Value,
                                    isSubCompany = true,
                                    FatherName = FatherCompany
                                });
                            }
                            else
                            {
                                var Clone = new List<struCompanyName>();
                                //使用NER表对于残缺公司名称的修补：
                                foreach (var item in companynamelist)
                                {
                                    Clone.Add(item);
                                }
                                foreach (var clone in Clone)
                                {
                                    if (!String.IsNullOrEmpty(clone.secFullName) &&
                                       clone.secFullName.Equals(ner.Value))
                                    {
                                        companynamelist.Add(new struCompanyName()
                                        {
                                            isSubCompany = true,
                                            secFullName = ner.Value,
                                            secShortName = clone.secShortName,
                                            FatherName = FatherCompany
                                        });
                                        companynamelist.Remove(clone);
                                    }
                                }
                            }
                            return;
                        }
                    }
                }
            }
        }
    }

    private void GetAnnouceCompanyName()
    {
        //江苏林洋电子股份有限公司
        //董事会
        //江苏林洋电子股份有限公司董事会
        //从文本中获得
        if (File.Exists(TextFileName))
        {
            var Lines = new List<string>();
            var sr = new StreamReader(TextFileName);
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (!String.IsNullOrEmpty(line))
                {
                    Lines.Add(line.Replace(" ", ""));
                }
            }
            sr.Close();

            for (int lineidx = Lines.Count - 1; lineidx >= 0; lineidx--)
            {
                var line = Lines[lineidx];
                if (line.EndsWith("有限公司"))
                {
                    AnnouceCompanyName = line;
                    break;
                }
                if (line.Contains("董事会"))
                {
                    if (line.Equals("董事会"))
                    {
                        //有些奇葩公司董事会后面是公司名称
                        if ((lineidx + 1) != Lines.Count)
                        {
                            foreach (var cn in companynamelist)
                            {
                                if (Lines[lineidx + 1].Equals(cn.secFullName))
                                {
                                    AnnouceCompanyName = cn.secFullName;
                                    return;
                                }
                            }
                        }
                        //董事会之前，也可能是日期
                        AnnouceCompanyName = Lines[lineidx - 1];
                        if (AnnouceCompanyName.Contains("年") && AnnouceCompanyName.Contains("月"))
                        {
                            if (Lines[lineidx - 2].EndsWith("有限公司")) AnnouceCompanyName = Lines[lineidx - 2];
                        }
                    }
                    else
                    {
                        AnnouceCompanyName = Utility.GetStringBefore(line, "董事会");
                    }
                    return;
                }
            }
        }



        //从实体中寻找最后一个公司名称
        foreach (var p in root.Children)
        {
            foreach (var s in p.Children)
            {
                if (!nermap.ParagraghlocateDict.ContainsKey(s.PositionId)) continue;
                var nerlist = nermap.ParagraghlocateDict[s.PositionId].NerList;
                foreach (var ner in nerlist)
                {
                    if (ner.Description == "公司名" || ner.Description == "机构")
                    {
                        AnnouceCompanyName = ner.Value;
                    }
                }
            }
        }

    }

    private void FillShortName()
    {
        foreach (var p in root.Children)
        {
            foreach (var s in p.Children)
            {
                if (!nermap.ParagraghlocateDict.ContainsKey(s.PositionId)) continue;
                var nerlist = nermap.ParagraghlocateDict[s.PositionId].NerList;
                if (nerlist == null) continue;
                for (int nerIdx = 0; nerIdx < nerlist.Count; nerIdx++)
                {
                    if (nerlist[nerIdx].Description == "中文小括号" && nerlist[nerIdx].Value.Contains("简称"))
                    {
                        if (nerIdx == 0) continue;
                        var Preview = nerlist[nerIdx - 1];
                        if (Preview.Description == "公司名" || Preview.Description == "机构")
                        {
                            var QL = RegularTool.GetChineseQuotation(nerlist[nerIdx].Value);
                            foreach (var strQ in QL)
                            {
                                var Q = strQ.Substring(1, strQ.Length - 2);
                                if (Q == "公司" || Q == "本公司" || Q == "招标人" || Q == "发包人") continue;
                                var Clone = new List<struCompanyName>();
                                //使用NER表对于残缺公司名称的修补：但是这里在某些情况下，不能保证肯定是全称，特别是机构的时候
                                if (!companynamelist.Select(x => x.secFullName).Contains(Preview.Value))
                                {
                                    companynamelist.Add(
                                        new struCompanyName()
                                        {
                                            secFullName = Preview.Value
                                        }
                                    );
                                }
                                foreach (var item in companynamelist)
                                {
                                    Clone.Add(item);
                                }
                                foreach (var cn in Clone)
                                {
                                    if (!String.IsNullOrEmpty(cn.secFullName) && cn.secFullName.Equals(Preview.Value))
                                    {
                                        if (String.IsNullOrEmpty(cn.secShortName))
                                        {
                                            companynamelist.Add(new struCompanyName()
                                            {
                                                secFullName = cn.secFullName,
                                                secShortName = Q
                                            });
                                            companynamelist.Remove(cn);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void CompanyNameAnlayze()
    {
        companynamelist = CompanyNameLogic.GetCompanyNameByCutWordFromHTML(root);
        if (File.Exists(TextFileName))
        {
            var companynamelistText = CompanyNameLogic.GetCompanyNameByCutWordFromTextFile(TextFileName);
            companynamelist.AddRange(companynamelistText);
        }

        var Clone = new List<struCompanyName>();
        //使用NER表对于残缺公司名称的修补：
        foreach (var item in companynamelist)
        {
            Clone.Add(item);
        }
        foreach (var ner in Nerlist)
        {
            if (ner.Type == enmNerType.Ni)
            {
                foreach (var cn in Clone)
                {
                    if (!ner.RawData.Equals(cn.secFullName) &&
                         ner.RawData.Contains(cn.secFullName))
                    {
                        companynamelist.Add(new struCompanyName()
                        {
                            secFullName = ner.RawData,
                            secShortName = cn.secShortName
                        });
                        companynamelist.Remove(cn);
                        continue;
                    }
                }
            }
        }

        var newname = new List<struCompanyName>();
        foreach (var cn in companynamelist)
        {
            if (!string.IsNullOrEmpty(cn.secFullName) && string.IsNullOrEmpty(cn.secShortName))
            {
                foreach (var item in quotationList)
                {
                    //是否存在引号里面的词语正好是公司全称的开始
                    if (item.Description != "引号") continue;
                    if (cn.secFullName.StartsWith(item.Value) && !cn.secFullName.Equals(item.Value))
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
    }

    public abstract List<RecordBase> Extract();

    /// <summary>
    /// 释义表编号
    /// </summary>
    public List<int> ExplainTableId = new List<int>();

    /// <summary>
    /// 释义表的字典
    /// </summary>
    /// <typeparam name="String"></typeparam>
    /// <typeparam name="String"></typeparam>
    /// <returns></returns>
    public Dictionary<String, String> ExplainDict = new Dictionary<String, String>();
    /// <summary>
    /// 公告中特殊的释义表
    /// </summary>
    /// <returns></returns>
    public void GetExplainTable()
    {
        //释义表的抽取
        foreach (var table in root.TableList)
        {
            var htmltable = new HTMLTable(table.Value);

            if (htmltable.ColumnCount == 2)
            {
                for (int RowNo = 1; RowNo <= htmltable.RowCount; RowNo++)
                {
                    if (htmltable.CellValue(RowNo, 2).StartsWith("指"))
                    {
                        var key = htmltable.CellValue(RowNo, 1);
                        var value = htmltable.CellValue(RowNo, 2).Substring(1);
                        if (!ExplainDict.ContainsKey(key)) ExplainDict.Add(key, value);
                        if (!ExplainTableId.Contains(table.Key)) ExplainTableId.Add(table.Key);
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
                        if (!ExplainDict.ContainsKey(key)) ExplainDict.Add(key, value);
                        if (!ExplainTableId.Contains(table.Key)) ExplainTableId.Add(table.Key);
                    }
                }
            }

            if (htmltable.ColumnCount == 4)
            {
                for (int RowNo = 1; RowNo <= htmltable.RowCount; RowNo++)
                {
                    if (htmltable.CellValue(RowNo, 3) == "指")
                    {
                        var key = htmltable.CellValue(RowNo, 2);
                        var value = htmltable.CellValue(RowNo, 4);
                        if (!ExplainDict.ContainsKey(key)) ExplainDict.Add(key, value);
                        if (!ExplainTableId.Contains(table.Key)) ExplainTableId.Add(table.Key);
                    }
                }
            }

        }

        if (ExplainDict.Count == 0) return;

        //寻找指释义表中表示公司简称和公司全称的项目，加入到Companylist中
        //注意，左边的主键，右边的值，都可能需要根据分隔符好切分
        foreach (var item in ExplainDict)
        {
            var keys = item.Key.Split(Utility.SplitChar);
            var keys2 = item.Key.Split(new char[] { '／', '/' });
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
                        var tempvalue = value;
                        var chineseName = Utility.TrimEnglish(value);
                        //公司的情况，需要去掉括号的干扰
                        chineseName = chineseName.Replace("（", "").Replace("）", "").Replace("(", "").Replace(")", "").Replace(" ", "");
                        //公司的情况，需要去掉逗号之后的干扰
                        if (chineseName.Contains("，")) chineseName = Utility.GetStringBefore(chineseName, "，");
                        if (chineseName.EndsWith("公司") || chineseName.EndsWith("有限合伙") || chineseName.EndsWith("有限责任公司") ||
                            value.EndsWith("Co.,Ltd.") || chineseName.EndsWith("厂") || chineseName.EndsWith("研究所"))
                        {
                            //公司的情况，需要去掉逗号之后的干扰
                            if (tempvalue.Contains("，")) tempvalue = Utility.GetStringBefore(tempvalue, "，");
                            if (tempvalue.Contains("及其前身"))
                            {
                                tempvalue = Utility.GetStringBefore(tempvalue, "及其前身");
                            }
                            companynamelist.Add(new struCompanyName()
                            {
                                secFullName = tempvalue,
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