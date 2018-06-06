using HtmlAgilityPack;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public static class HTMLEngine
{

    #region Anlayze

    public class MyHtmlNode
    {
        public int TableId = -1;
        public string Content;
        public List<MyHtmlNode> Children = new List<MyHtmlNode>();
        public MyHtmlNode NextBrother;
        public MyHtmlNode PreviewBrother;

        public int ParagrahId = -1;
        public string FullText
        {
            get
            {
                var strFull = "";
                foreach (var child in Children)
                {
                    if (child.TableId == -1)
                    {
                        if (child.Content.StartsWith("<")) strFull += System.Environment.NewLine;
                        strFull += child.Content;
                    }
                }
                return strFull;
            }
        }
    }

    public class MyRootHtmlNode : MyHtmlNode
    {
        //所有单元格的内容按表分组
        public Dictionary<int, List<String>> TableList;
        //内置列表内容
        public Dictionary<int, List<String>> DetailItemList;
    }


    static int TableId = 0;
    static int DetailItemId = 0;
    static Dictionary<int, List<String>> TableList;

    static Dictionary<int, List<String>> DetailItemList;

    public static MyRootHtmlNode Anlayze(string htmlfile)
    {
        TableId = 0;
        DetailItemId = 0;
        TableList = new Dictionary<int, List<String>>();
        DetailItemList = new Dictionary<int, List<String>>();
        //一般来说第一个都是DIV， <div title="关于重大合同中标的公告" type="pdf">
        var doc = new HtmlDocument();
        doc.Load(htmlfile);
        var node = doc.DocumentNode.SelectNodes("//div[@type='pdf']");
        var root = new MyRootHtmlNode();
        root.Content = node[0].Attributes["title"].Value;
        //第二层是所有的一定是Paragraph
        foreach (var SecondLayerNode in node[0].ChildNodes)
        {
            //Console.WriteLine(SecondLayerNode.Name);
            //跳过#text的节
            if (SecondLayerNode.Name == "div")
            {
                var title = "";
                if (SecondLayerNode.Attributes.Contains("title"))
                {
                    title = SecondLayerNode.Attributes["title"].Value;
                }
                else
                {
                    title = SecondLayerNode.InnerText;
                }
                var secondNode = new MyHtmlNode();
                secondNode.Content = title;
                AnlayzeParagraph(SecondLayerNode, secondNode);
                FindContentWithList(secondNode.Children);
                for (int i = 0; i < secondNode.Children.Count - 1; i++)
                {
                    secondNode.Children[i].NextBrother = secondNode.Children[i + 1];
                }

                for (int i = 1; i < secondNode.Children.Count; i++)
                {
                    secondNode.Children[i].PreviewBrother = secondNode.Children[i - 1];
                }
                root.Children.Add(secondNode);
            }
        }

        //最后一个段落的检索
        var LastParagrah = root.Children.Last();
        if (LastParagrah.Children.Count > 0)
        {
            //重大合同:1232951  
            var LastSentence = LastParagrah.Children.Last().Content;
            var sentence = Utility.ConvertUpperDateToLittle(LastSentence);
            var strDate = RegularTool.GetDate(sentence);
            if (!String.IsNullOrEmpty(strDate))
            {
                var strBefore = Utility.GetStringBefore(sentence, strDate);
                if (!String.IsNullOrEmpty(strBefore))
                {
                    //尾部除去
                    LastParagrah.Children.RemoveAt(LastParagrah.Children.Count - 1);
                    strBefore = LastSentence.Substring(0, LastSentence.LastIndexOf("年") - 4);
                    LastParagrah.Children.Add(new MyHtmlNode() { Content = strBefore });
                    LastParagrah.Children.Add(new MyHtmlNode() { Content = strDate });
                }
            }
        }
        for (int i = 0; i < root.Children.Count - 1; i++)
        {
            root.Children[i].NextBrother = root.Children[i + 1];
        }

        for (int i = 1; i < root.Children.Count; i++)
        {
            root.Children[i].PreviewBrother = root.Children[i - 1];
        }

        for (int i = 0; i < root.Children.Count; i++)
        {
            root.Children[i].ParagrahId = i + 1;
        }

        root.TableList = TableList;
        root.DetailItemList = DetailItemList;

        var txtfilename = htmlfile.Replace("html", "txt");
        if (File.Exists(txtfilename))
        {
            Adjust(root, txtfilename);
        }
        return root;
    }

    private static void Adjust(MyRootHtmlNode root, string txtfilename)
    {
        var SR = new StreamReader(txtfilename);
        var TxtList = new List<String>();
        while (!SR.EndOfStream)
        {
            string TxtLine = Normalizer.NormalizeItemListNumber(SR.ReadLine().Trim());
            TxtLine = TxtLine.Replace(" ", "");    //HTML是去空格的,PDF有空格
            //通过TXT补偿列表分裂的情况
            if (TxtLine.StartsWith("<"))
            {
                foreach (var paragrah in root.Children)
                {
                    //从各个段落的内容中取得：内容包含了内置列表，所以，这里不再重复
                    foreach (var contentNode in paragrah.Children)
                    {
                        if (contentNode.TableId == -1)
                        {
                            //非表格
                            if (TxtLine.StartsWith(contentNode.Content))
                            {
                                //重大合同：401597
                                if (!contentNode.Content.Equals(TxtLine))
                                {
                                    //Line:<1>合同名称：天津市公安局南开分局南开区 2016 年视频监控网建设运维服
                                    //Content:<1>合同名称：
                                    //Next Content Line:天津市公安局南开分局南开区2016年视频监控网建设运维服务项目建设运维服务项目合同

                                    //Line Before:<1>甲方：山东省临朐县人民政府
                                    //Content:<1>甲方：
                                    //Next Content Line:山东省临朐县人民政府地址：临朐县民主路102号

                                    //Console.WriteLine("Line Before:" + TxtLine);
                                    //Console.WriteLine("Content:" + contentNode.Content);
                                    if (contentNode.NextBrother != null &&
                                       !contentNode.NextBrother.Content.StartsWith("<"))
                                    {
                                        string NextContent = contentNode.NextBrother.Content;
                                        //Console.WriteLine("Next Content Line:" + NextContent);
                                        var CombineLine = contentNode.Content + NextContent;
                                        if ((CombineLine).StartsWith(TxtLine))
                                        {
                                            if (!NextContent.Contains("："))
                                            {
                                                //如果上一行和下一行的拼接体不包含：号
                                                //则用拼接体，然后的话，用文本文件的结果
                                                TxtLine = CombineLine;
                                                contentNode.NextBrother.Content = "";
                                            }
                                        }
                                    }
                                    contentNode.Content = TxtLine;
                                    //Console.WriteLine("Line After:" + TxtLine);
                                }
                            }
                        }
                    }
                }
            }
            TxtList.Add(TxtLine);
        }
        SR.Close();
    }

    static void AnlayzeParagraph(HtmlNode paragraph, MyHtmlNode root, String subTitle = "")
    {
        //原始HTML的第二阶层无法保证嵌套结构是正确的，
        //所以决定第二阶层不分层
        foreach (var node in paragraph.ChildNodes)
        {
            if (node.Name == "div")
            {
                if (node.Attributes.Contains("type"))
                {
                    if (node.Attributes["type"].Value == "content")
                    {
                        if (node.ChildNodes.Count == 3 && node.ChildNodes[1].Name == "table")
                        {
                            var tablenode = new MyHtmlNode();
                            TableId++;
                            tablenode.TableId = TableId;
                            GetTable(node.ChildNodes[1]);
                        }
                        else
                        {
                            var content = Normalizer.Normalize(node.InnerText);
                            if (!String.IsNullOrEmpty(content))
                            {
                                var s = new MyHtmlNode();
                                s.Content = subTitle + content;
                                root.Children.Add(s);
                            }
                            else
                            {
                                if (subTitle != "")
                                {
                                    var s = new MyHtmlNode();
                                    s.Content = subTitle;
                                    root.Children.Add(s);
                                }
                            }
                            subTitle = "";
                        }
                    }
                    if (node.Attributes["type"].Value == "paragraph")
                    {
                        var title = "";
                        if (node.Attributes.Contains("title"))
                        {
                            title = node.Attributes["title"].Value;
                            title = Normalizer.Normalize(title);
                        }
                        AnlayzeParagraph(node, root, title);
                    }
                }
            }
            if (node.Name == "table")
            {
                var tablenode = new MyHtmlNode();
                TableId++;
                tablenode.TableId = TableId;
                GetTable(node);
            }
        }
    }

    //找一下Content列表里面是否存在明确带有数字的列表
    //由于先导处理的效果应该统一化为 <1> 的形式了
    static void FindContentWithList(List<MyHtmlNode> Children)
    {
        var lst = new List<String>();
        var pos = -1;
        foreach (var child in Children)
        {
            if (child.TableId != -1) continue;
            if (pos != -1)
            {
                if (child.Content.StartsWith("<" + pos.ToString() + ">"))
                {
                    lst.Add(child.Content);
                    pos++;
                }
            }
            else
            {
                if (child.Content.StartsWith("<1>") && pos == -1)
                {
                    lst.Add(child.Content);
                    pos = 2;
                }
            }
        }
        if (lst.Count > 1)
        {
            DetailItemId++;
            DetailItemList.Add(DetailItemId, lst);
        }
    }



    public static List<String> GetTable(HtmlNode table)
    {
        var tablecontentlist = new List<String>();
        var dict = new Dictionary<String, String>();

        //表格处理：
        foreach (var tablebody in table.ChildNodes)
        {
            //整理出最大行列数
            int MaxRow = 0;
            int MaxColumn = 0;

            foreach (var tableRows in tablebody.ChildNodes)
            {
                if (tableRows.ChildNodes.Count != 0)
                {
                    int xc = 0;
                    foreach (var tableData in tableRows.ChildNodes)
                    {
                        if (tableData.Name == "td")
                        {
                            xc++;
                        }
                    }
                    if (xc > MaxColumn) MaxColumn = xc;
                    MaxRow++;
                }
            }


            //准备Cell内容字典
            for (int Row = 1; Row < MaxRow + 1; Row++)
            {
                for (int Col = 1; Col < MaxColumn + 1; Col++)
                {
                    dict.Add(Row + "," + Col, "");
                }
            }

            int CurrentRow = 1;
            int NextNeedToFillColumn = 1;

            foreach (var tableRows in tablebody.ChildNodes)
            {
                if (tableRows.ChildNodes.Count != 0)
                {
                    foreach (var tableData in tableRows.ChildNodes)
                    {
                        //对于#text的过滤
                        if (tableData.Name == "td")
                        {

                            //寻找该行下一个需要填充的格子的列号
                            for (int Col = 1; Col < MaxColumn + 1; Col++)
                            {
                                if (dict[CurrentRow + "," + Col] == "")
                                {
                                    NextNeedToFillColumn = Col;
                                    break;
                                }
                            }

                            var cellvalue = Normalizer.Normalize(tableData.InnerText);
                            var cellpos = CurrentRow + "," + NextNeedToFillColumn;
                            if (cellvalue == "")
                            {
                                cellvalue = "<null>";
                            }
                            dict[CurrentRow + "," + NextNeedToFillColumn] = cellvalue;
                            if (tableData.Attributes["rowspan"] != null)
                            {
                                //具有RowSpan特性的情况
                                for (int i = 1; i < int.Parse(tableData.Attributes["rowspan"].Value); i++)
                                {
                                    dict[(CurrentRow + i) + "," + NextNeedToFillColumn] = "<rowspan>";
                                }
                            }
                            if (tableData.Attributes["colspan"] != null)
                            {
                                //具有RowSpan特性的情况
                                for (int i = 1; i < int.Parse(tableData.Attributes["colspan"].Value); i++)
                                {
                                    dict[CurrentRow + "," + (NextNeedToFillColumn + i)] = "<colspan>";
                                }
                            }
                        }
                    }
                    CurrentRow++;
                }
            }
        }
        foreach (var item in dict)
        {
            tablecontentlist.Add(TableId + "," + item.Key + "|" + item.Value);
        }
        TableList.Add(TableId, tablecontentlist);
        return tablecontentlist;
    }

    #endregion
}