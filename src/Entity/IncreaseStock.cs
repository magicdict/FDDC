using System;
using System.Linq;
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


    internal static string ConvertToString(struIncreaseStock increaseStock)
    {
        var record = increaseStock.id + "," +
        increaseStock.PublishTarget + "," +
        increaseStock.PublishMethod + ",";
        record += Normalizer.NormalizeNumberResult(increaseStock.IncreaseNumber) + ",";
        record += Normalizer.NormalizeNumberResult(increaseStock.IncreaseMoney) + ",";
        if (!String.IsNullOrEmpty(increaseStock.FreezeYear) && increaseStock.FreezeYear.EndsWith("个月"))
        {
            increaseStock.FreezeYear = increaseStock.FreezeYear.Replace("个月", "");
        }
        record += increaseStock.FreezeYear + "," +
        increaseStock.BuyMethod;
        return record;
    }

    public static List<struIncreaseStock> Extract(string htmlFileName)
    {
        var fi = new System.IO.FileInfo(htmlFileName);
        Program.Logger.WriteLine("Start FileName:[" + fi.Name + "]");
        var node = HTMLEngine.Anlayze(htmlFileName);
        //公告ID
        var id = fi.Name.Replace(".html", "");
        Program.Logger.WriteLine("公告ID:" + id);

        //发行方式
        var publishMethod = getPublishMethod(node);
        //认购方式
        var buyMethod = getBuyMethod(node);
        //样本
        var increaseStock = new struIncreaseStock();
        increaseStock.id = id;
        increaseStock.PublishMethod = publishMethod;
        increaseStock.BuyMethod = buyMethod;
        return GetMultiTarget(node, increaseStock);
    }


    static List<struIncreaseStock> GetMultiTarget(HTMLEngine.MyRootHtmlNode root, struIncreaseStock SampleincreaseStock)
    {
        var Info = new Dictionary<int, List<string>>();     //每张表格的认购者名单
        var increaseStocklist = new Dictionary<String, struIncreaseStock>();

        for (int tableIndex = 0; tableIndex < root.TableList.Count; tableIndex++)
        {
            //寻找表头是"发行对象" 或者 "发行对象名称" 的列号
            var table = new HTMLTable(root.TableList[tableIndex + 1]);
            var HeaderRow = table.GetHeaderRow();
            var pos = -1;
            for (int j = 0; j < HeaderRow.Length; j++)
            {
                //认购对象必须先于发行对象
                if (HeaderRow[j] == "认购对象" || HeaderRow[j] == "发行对象" || HeaderRow[j] == "发行对象名称")
                {
                    var NumberRow = -1;   //股票数
                    var MoneyRow = -1;    //金额数
                    var FreezeRow = -1;  //禁售期  
                    for (int NumberIndex = 0; NumberIndex < HeaderRow.Length; NumberIndex++)
                    {
                        if (HeaderRow[NumberIndex].Contains("配售股数") ||
                            HeaderRow[NumberIndex].Contains("认购数量") ||
                            HeaderRow[NumberIndex].Contains("认购股份数"))
                        {
                            NumberRow = NumberIndex + 1;    //Index从0开始
                        }
                    }

                    for (int MoneyIndex = 0; MoneyIndex < HeaderRow.Length; MoneyIndex++)
                    {
                        if (HeaderRow[MoneyIndex].Contains("配售金额") || HeaderRow[MoneyIndex].Contains("认购金额"))
                        {
                            MoneyRow = MoneyIndex + 1;  //Index从0开始
                        }
                    }

                    for (int FreezeRowIndex = 0; FreezeRowIndex < HeaderRow.Length; FreezeRowIndex++)
                    {
                        if (HeaderRow[FreezeRowIndex].Contains("限售期") || HeaderRow[FreezeRowIndex].Contains("锁定期"))
                        {
                            FreezeRow = FreezeRowIndex + 1;  //Index从0开始
                        }
                    }

                    pos = j + 1;    //Index从0开始
                    var Buyer = new List<String>();
                    for (int k = 2; k <= table.RowCount + 1; k++)
                    {
                        var target = table.CellValue(k, pos);
                        if (table.IsTotalRow(k)) continue;
                        if (target == "" || target == "<rowspan>" || target == "<colspan>" || target == "<null>" ) continue;

                        struIncreaseStock increase;

                        if (increaseStocklist.ContainsKey(target))
                        {
                            increase = increaseStocklist[target];
                        }
                        else
                        {
                            increase = new struIncreaseStock();
                        }

                        increase.PublishTarget = target;
                        increase.id = SampleincreaseStock.id;
                        increase.PublishMethod = SampleincreaseStock.PublishMethod;
                        increase.BuyMethod = SampleincreaseStock.BuyMethod;

                        Program.Logger.WriteLine("候补增发对象:" + target + " @TableIndex:" + tableIndex);
                        //是否能提取其他信息：
                        if (NumberRow != -1)
                        {
                            increase.IncreaseNumber = table.CellValue(k, NumberRow);
                            Program.Logger.WriteLine("候补增发数量:" + table.CellValue(k, NumberRow) + " @TableIndex:" + tableIndex);
                        }
                        if (MoneyRow != -1)
                        {
                            increase.IncreaseMoney = table.CellValue(k, MoneyRow);
                            Program.Logger.WriteLine("候补增发金额:" + table.CellValue(k, MoneyRow) + " @TableIndex:" + tableIndex);
                        }
                        if (FreezeRow != -1)
                        {
                            increase.FreezeYear = table.CellValue(k, FreezeRow);
                            Program.Logger.WriteLine("候补锁定期:" + table.CellValue(k, FreezeRow) + " @TableIndex:" + tableIndex);
                        }
                        if (!increaseStocklist.ContainsKey(target))
                        {
                            increaseStocklist.Add(target, increase);
                        }
                        Buyer.Add(target);
                    }
                    Info.Add(tableIndex, Buyer);
                    //可能出现同时含有认购对象和发行对象的表格14925945
                    break;
                }
            }
        }
        return increaseStocklist.Values.ToList();
    }


    static string getPublishMethod(HTMLEngine.MyRootHtmlNode root)
    {
        //是否包含关键字 "询价发行"
        var Extractor = new ExtractProperty();
        var cnt = Extractor.FindWordCnt("询价发行", root);
        Program.Logger.WriteLine("询价发行(文本):" + cnt);
        if (cnt > 0) return "竞价";
        return "";
    }

    static string getBuyMethod(HTMLEngine.MyRootHtmlNode root)
    {
        //是否包含关键字 "现金认购"
        var Extractor = new ExtractProperty();
        var cnt = Extractor.FindWordCnt("现金认购", root);
        Program.Logger.WriteLine("现金认购(文本):" + cnt);
        if (cnt > 0) return "现金";
        return "";
    }

}