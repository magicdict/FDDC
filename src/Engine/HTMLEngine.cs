using HtmlAgilityPack;
using System;
using System.Collections.Generic;

public static class HTMLEngine
{

    #region Anlayze

    public class MyHtmlNode
    {
        public bool IsTable;
        public List<String> TableContent;
        public string Content;
        public List<MyHtmlNode> Children;
        public MyHtmlNode(string content)
        {
            IsTable = false;
            Content = content;
            Children = new List<MyHtmlNode>();
            TableContent = new List<String>();
        }
        public MyHtmlNode NextBrother;

        public MyHtmlNode PreviewBrother;


        //OnlyRootNode
        public Dictionary<int, List<String>> TableList;

        public Dictionary<int, List<String>> DetailItemList;
    }


    static int TableId = 0;
    static int DetailItemId = 0;
    static Dictionary<int, List<String>> TableList;

    static Dictionary<int, List<String>> DetailItemList;

    public static MyHtmlNode Anlayze(string htmlfile)
    {
        TableId = 0;
        DetailItemId = 0;
        TableList = new Dictionary<int, List<String>>();
        DetailItemList = new Dictionary<int, List<String>>();
        //一般来说第一个都是DIV， <div title="关于重大合同中标的公告" type="pdf">
        var doc = new HtmlDocument();
        doc.Load(htmlfile);
        var node = doc.DocumentNode.SelectNodes("//div[@type='pdf']");
        var root = new MyHtmlNode(node[0].Attributes["title"].Value);
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
                var sencondNode = new MyHtmlNode(title);
                AnlayzeParagraph(SecondLayerNode, sencondNode);
                root.Children.Add(sencondNode);
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

        root.TableList = TableList;
        root.DetailItemList = DetailItemList;
        return root;
    }

    static void AnlayzeParagraph(HtmlNode paragraph, MyHtmlNode root)
    {
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
                            var tablenode = new MyHtmlNode("");
                            tablenode.IsTable = true;
                            TableId++;
                            tablenode.TableContent = GetTable(node.ChildNodes[1]);
                            root.Children.Add(tablenode);
                        }
                        else
                        {
                            var content = Normalizer.Normalize(node.InnerText);
                            if (!String.IsNullOrEmpty(content))
                            {
                                root.Children.Add(new MyHtmlNode(content));
                            }
                        }
                    }
                    if (node.Attributes["type"].Value == "paragraph")
                    {
                        var title = "";
                        if (node.Attributes.Contains("title"))
                        {
                            title = node.Attributes["title"].Value;
                        }
                        var pNode = new MyHtmlNode(title);
                        AnlayzeParagraph(node, pNode);
                        root.Children.Add(pNode);
                    }
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
        FindContentWithList(root.Children);
    }

    //找一下Content列表里面是否存在明确带有数字的列表
    //由于先导处理的效果应该统一化为 <1> 的形式了
    static void FindContentWithList(List<MyHtmlNode> Children)
    {
        var lst = new List<String>();
        var pos = -1;
        foreach (var child in Children)
        {
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

    public static List<MyHtmlNode> searchKeyWord(MyHtmlNode node, string keyword, string exclude = "")
    {
        var content = new List<MyHtmlNode>();
        if (!node.IsTable)
        {
            //非表格
            if (node.Content.IndexOf(keyword) != -1)
            {
                if (exclude != "")
                {
                    //具有排除词
                    if (node.Content.IndexOf(exclude) == -1)
                    {
                        content.Add(node);
                    }
                }
                else
                {
                    //没有排除词
                    content.Add(node);
                }
            }
            foreach (var subNode in node.Children)
            {
                content.AddRange(searchKeyWord(subNode, keyword, exclude));
            }
        }
        return content;
    }

    public static List<MyHtmlNode> searchKeyWordAtTable(MyHtmlNode node, string keyword, string exclude = "")
    {
        var content = new List<MyHtmlNode>();
        if (node.IsTable)
        {
            //表格
            foreach (var Content in node.TableContent)
            {
                if (Content.IndexOf(keyword) != -1)
                {
                    //没有排除词
                    content.Add(node);
                    break;
                }
            }
        }
        foreach (var subNode in node.Children)
        {
            content.AddRange(searchKeyWordAtTable(subNode, keyword, exclude));
        }
        return content;
    }

    public static string GetValueFromTableNode(MyHtmlNode node, string Keyword)
    {
        //Table,Row,Column|Keyword
        //这里假设值应该在[Table,Row + 1,Column][Value]
        foreach (var content in node.TableContent)
        {
            var pos = content.Split("|")[0];
            var value = content.Split("|")[1];
            if (value.IndexOf(Keyword) != -1)
            {
                //寻找
                var valuePos = pos.Split(",")[0] + "," + (int.Parse(pos.Split(",")[1]) + 1).ToString() + "," + pos.Split(",")[2];
                foreach (var item in node.TableContent)
                {
                    if (item.StartsWith(valuePos))
                    {
                        return item.Split("|")[1];
                    }
                }
            }
        }
        return string.Empty;
    }


    public static string GetValueBetweenString(MyHtmlNode node, string Start, string End)
    {
        string rtn = "";
        var contentlist = searchKeyWord(node, Start);
        foreach (var item in contentlist)
        {
            rtn = Utility.GetValueBetweenString(item.Content, Start, End);
            if (rtn != "") return rtn;
        }
        return rtn;
    }

    public static string GetValueAfterString(MyHtmlNode node, string Keyword)
    {
        string rtn = "";
        var contentlist = searchKeyWord(node, Keyword);
        foreach (var item in contentlist)
        {
            rtn = Utility.GetStringAfter(item.Content, Keyword);
            if (rtn != "") return rtn;
        }
        return rtn;
    }


    public static string GetValueFromNextContent(MyHtmlNode node, string Keyword)
    {
        string rtn = "";
        var contentlist = searchKeyWord(node, Keyword);
        foreach (var item in contentlist)
        {
            //寻找该节点的下一个节点
            if (item.NextBrother != null)
            {
                return item.NextBrother.Content;
            }
        }
        return rtn;
    }

}