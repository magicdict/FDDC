using System;
using System.Collections.Generic;
using 金融数据整理大赛;

public class IncreaseStock
{
    public struct struIncreaseStock
    {
        //公告id
        public string id;

        //发行方式(定价/竞价)
        public string PublishMethod;

        //认购方式（现金股票）
        public string BuyMethod;

        //锁定期（一般原则：定价36个月，竞价12个月）
        //但是这里牵涉到不同对象不同锁定期的可能性
        public string FreezeYear;

    }


    public static int findPublishMethodcount = 0;

    public static int findBuyMethodcount = 0;

    public static struIncreaseStock Extract(string htmlFileName)
    {
        var fi = new System.IO.FileInfo(htmlFileName);
        Program.Logger.WriteLine("Start FileName:[" + fi.Name + "]");
        var increaseStock = new struIncreaseStock();
        var node = HTMLEngine.Anlayze(htmlFileName);
        //公告ID
        increaseStock.id = fi.Name.Replace(".html", "");
        Program.Logger.WriteLine("公告ID:" + increaseStock.id);

        GetBuyer(node);

        //发行方式
        increaseStock.PublishMethod = getPublishMethod(node);

        //认购方式
        increaseStock.BuyMethod = getBuyMethod(node);
        return increaseStock;
    }


    static List<string> GetBuyer(HTMLEngine.MyHtmlNode node)
    {
        var Buyer = new List<string>();
        //按照POS标志分表
        for (int i = 0; i < node.TableList.Count; i++)
        {
            //寻找表头是"发行对象" 或者 "发行对象名称" 的列号
            var table = new HTMLTable(node.TableList[i + 1]);
            var HeaderRow = table.GetHeaderRow();
            var pos = -1;
            for (int j = 0; j < HeaderRow.Length; j++)
            {
                if (HeaderRow[j] == "发行对象" || HeaderRow[j] == "发行对象名称")
                {
                    pos = j + 1;
                    break;
                }
            }
            if (pos != -1){
                for (int k = 2; k < table.RowCount +1; k++)
                {
                    var value = table.CellValue(k,pos);
                    if (value != "" && value != "<rowspan>" && value != "<colspan>" && value != "<null>"){
                        Program.Logger.WriteLine("发行对象:" + value);
                        Buyer.Add(value);
                    } 
                }
                break;
            }    
        }
        return Buyer;
    }


    static string getPublishMethod(HTMLEngine.MyHtmlNode node)
    {
        var titles = HTMLEngine.searchKeyWord(node, "询价发行");
        if (titles.Count > 0)
        {
            Program.Logger.WriteLine("发行方式:[竞价]");
            findPublishMethodcount++;
            return "竞价";
        }
        return "";

    }

    static string getBuyMethod(HTMLEngine.MyHtmlNode node)
    {
        var titles = HTMLEngine.searchKeyWord(node, "现金认购");
        if (titles.Count > 0)
        {
            Program.Logger.WriteLine("认购方式:[现金认购]");
            findBuyMethodcount++;
            return "现金认购";
        }
        return "";
    }



}