using System;
using System.Collections.Generic;
using 金融数据整理大赛;

public class IncreaseStock
{
    public struct struIncreaseStock
    {
        //公告id
        public string id;

        //增发对象
        public string PublishTarget;


        //发行方式(定价/竞价)
        public string PublishMethod;

        //增发数量
        public string IncreaseNumber;

        //增发金额
        public string IncreaseMoney;

        //锁定期（一般原则：定价36个月，竞价12个月）
        //但是这里牵涉到不同对象不同锁定期的可能性
        public string FreezeYear;

        //认购方式（现金股票）
        public string BuyMethod;

    }

    internal static struIncreaseStock ConvertFromString(string str)
    {
        var Array = str.Split("\t");
        var c = new struIncreaseStock();
        c.id = Array[0];
        c.PublishTarget = Array[1];
        if (Array.Length > 2)
        {
            c.PublishMethod = Array[2];
        }
        if (Array.Length > 3)
        {
            c.IncreaseNumber = Array[3];
        }
        if (Array.Length > 4)
        {
            c.IncreaseMoney = Array[4];
        }
        if (Array.Length > 5)
        {
            c.FreezeYear = Array[5];
        }
        if (Array.Length == 7)
        {
            c.BuyMethod = Array[6];
        }
        return c;
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


    static Dictionary<int,List<string>> GetBuyer(HTMLEngine.MyRootHtmlNode node)
    {
        var Info = new Dictionary<int,List<string>>();
        //按照POS标志分表
        for (int tableIndex = 0; tableIndex < node.TableList.Count; tableIndex++)
        {
            //寻找表头是"发行对象" 或者 "发行对象名称" 的列号
            var table = new HTMLTable(node.TableList[tableIndex + 1]);
            var HeaderRow = table.GetHeaderRow();
            var pos = -1;
            for (int j = 0; j < HeaderRow.Length; j++)
            {
                if (HeaderRow[j] == "认购对象" || HeaderRow[j] == "发行对象" || HeaderRow[j] == "发行对象名称")
                {
                    var NumberRow = -1;   //股票数
                    var MoneyRow = -1;    //金额数  
                    
                    pos = j + 1;
                    var Buyer = new List<String>();
                    for (int k = 2; k < table.RowCount + 1; k++)
                    {
                        var value = table.CellValue(k, pos);
                        if (value != "" && value != "<rowspan>" && value != "<colspan>" && value != "<null>")
                        {
                            Program.Logger.WriteLine("候补增发对象:" + value + " @TableIndex:" + tableIndex);
                            //是否能提取其他信息：
                            if (NumberRow != -1){

                            }
                            if (MoneyRow != -1){

                            }
                            Buyer.Add(value);
                        }
                    }
                    Info.Add(tableIndex,Buyer);
                    break;      //可能出现同时含有认购对象和发行对象的表格14925945
                }
            }
        }
        return Info;
    }


    static string getPublishMethod(HTMLEngine.MyRootHtmlNode root)
    {
        //是否包含关键字 "询价发行"
        var Extractor = new ExtractProperty();
        var cnt = Extractor.FindWordCnt("询价发行", root);
        Program.Logger.WriteLine("询价发行(文本):" + cnt);
        return "";
    }

    static string getBuyMethod(HTMLEngine.MyRootHtmlNode root)
    {
        //是否包含关键字 "现金认购"
        var Extractor = new ExtractProperty();
        var cnt = Extractor.FindWordCnt("现金认购", root);
        Program.Logger.WriteLine("现金认购(文本):" + cnt);
        return "";
    }

}