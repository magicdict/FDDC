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

        public int PositionId = -1;
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
            var sentence = DateUtility.ConvertUpperToLower(LastSentence);
            var dateList = DateUtility.GetDate(sentence);
            if (dateList.Count > 0)
            {
                var strDate = dateList.Last();
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
        }

        
        if (File.Exists(AnnouceDocument.TextFileName))
        {
            AdjustItemList(root, AnnouceDocument.TextFileName);
            AdjustTwoLine(root, AnnouceDocument.TextFileName);
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
            root.Children[i].PositionId = i + 1;
            for (int j = 0; j < root.Children[i].Children.Count; j++)
            {
                root.Children[i].Children[j].PositionId = (i + 1) * 100 + j + 1;
            }
        }
        root.TableList = TableList;
        root.DetailItemList = DetailItemList;
        return root;
    }

    static void AdjustItemList(MyRootHtmlNode root, string txtfilename)
    {
        var SR = new StreamReader(txtfilename);
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
        }
        SR.Close();
    }

    static void AdjustTwoLine(MyRootHtmlNode root, string txtfilename)
    {
        //Line Before:招标人：国家电网公司
        //Content: 招标人：国家电网公司注册资本：2000亿元
        //如果出现行1 + 行2 == Content，则Content则变为行1，增加Content之后的项目
        var SR = new StreamReader(txtfilename);
        var TxtList = new List<String>();
        while (!SR.EndOfStream)
        {
            string TxtLine = Normalizer.NormalizeItemListNumber(SR.ReadLine().Trim());
            TxtLine = TxtLine.Replace(" ", "");    //HTML是去空格的,PDF有空格
            if (!String.IsNullOrEmpty(TxtLine)) TxtList.Add(TxtLine);
        }
        for (int i = 1; i < TxtList.Count - 1; i++)
        {
            var CombineLine = TxtList[i] + TxtList[i + 1];
            foreach (var paragrah in root.Children)
            {
                //从各个段落的内容中取得：内容包含了内置列表，所以，这里不再重复
                for (int pid = 0; pid < paragrah.Children.Count; pid++)
                {
                    var contentNode = paragrah.Children[pid];
                    if (contentNode.Content.Equals(CombineLine) && TxtList[i].Contains("：") && TxtList[i + 1].Contains("："))
                    {
                        contentNode.Content = TxtList[i];
                        paragrah.Children.Add(new MyHtmlNode() { Content = TxtList[i + 1] });
                    }
                }
            }
        }
        SR.Close();
    }

    static void AnlayzeParagraph(HtmlNode paragraph, MyHtmlNode root, String subTitle = "")
    {
        //原始HTML的第二阶层无法保证嵌套结构是正确的，
        //所以决定第二阶层不分层
        if (paragraph.ChildNodes.Count == 1)
        {
            if (paragraph.ChildNodes[0].Name == "#text")
            {
                if (subTitle != "")
                {
                    var txtnode = new MyHtmlNode();
                    txtnode.Content = subTitle;
                    root.Children.Add(txtnode);
                    return;
                }
            }
        }

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
                            tablenode.Content = "";
                            TableId++;
                            tablenode.TableId = TableId;
                            var tablecontentlist = HTMLTable.GetTable(node.ChildNodes[1], TableId);
                            TableList.Add(TableId, tablecontentlist);
                            root.Children.Add(tablenode);
                        }
                        else
                        {
                            var content = Normalizer.Normalize(node.InnerText);
                            if (!String.IsNullOrEmpty(content))
                            {
                                var contentnode = new MyHtmlNode();
                                contentnode.Content = subTitle + content;
                                root.Children.Add(contentnode);
                            }
                            else
                            {
                                if (subTitle != "")
                                {
                                    var contentnode = new MyHtmlNode();
                                    contentnode.Content = subTitle;
                                    root.Children.Add(contentnode);
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
                tablenode.Content = "";
                TableId++;
                tablenode.TableId = TableId;
                var tablecontentlist = HTMLTable.GetTable(node, TableId);
                TableList.Add(TableId, tablecontentlist);
                root.Children.Add(tablenode);
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
    #endregion
}