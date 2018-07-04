using System;
using System.Linq;
using System.Collections.Generic;
using FDDC;
using static HTMLTable;
using static HTMLEngine;
using static CompanyNameLogic;

public class IncreaseStock : AnnouceDocument
{
    public IncreaseStock(string htmlFileName) : base(htmlFileName)
    {

    }

    public struct struIncreaseStock
    {
        //公告id
        public string id;

        //增发对象
        public string PublishTarget;

        //增发数量
        public string IncreaseNumber;

        //增发金额
        public string IncreaseMoney;

        //锁定期（一般原则：定价36个月，竞价12个月）
        //但是这里牵涉到不同对象不同锁定期的可能性
        public string FreezeYear;

        //认购方式（现金股票）
        public string BuyMethod;
        public string GetKey()
        {
            return id + ":" + PublishTarget.NormalizeKey();
        }
        public static struIncreaseStock ConvertFromString(string str)
        {
            var Array = str.Split("\t");
            var c = new struIncreaseStock();
            c.id = Array[0];
            c.PublishTarget = Array[1];
            if (Array.Length > 2)
            {
                c.IncreaseNumber = Array[2];
            }
            if (Array.Length > 3)
            {
                c.IncreaseMoney = Array[3];
            }
            if (Array.Length > 4)
            {
                c.FreezeYear = Array[4];
            }
            if (Array.Length > 5)
            {
                c.BuyMethod = Array[5];
            }
            return c;
        }


        public string ConvertToString(struIncreaseStock increaseStock)
        {
            var record = increaseStock.id + "\t" +
            increaseStock.PublishTarget + "\t";
            record += Normalizer.NormalizeNumberResult(increaseStock.IncreaseNumber) + "\t";
            record += Normalizer.NormalizeNumberResult(increaseStock.IncreaseMoney) + "\t";
            record += increaseStock.FreezeYear + "\t" + increaseStock.BuyMethod;
            return record;
        }
    }

    public List<struIncreaseStock> Extract()
    {
        //认购方式
        var buyMethod = getBuyMethod(root);
        //样本
        var increaseStock = new struIncreaseStock();
        increaseStock.id = Id;
        increaseStock.BuyMethod = buyMethod;
        var list = GetMultiTarget(root, increaseStock);
        return list;
    }


    List<struIncreaseStock> GetMultiTarget(HTMLEngine.MyRootHtmlNode root, struIncreaseStock SampleincreaseStock)
    {
        var BuyerRule = new TableSearchRule();
        BuyerRule.Name = "认购对象";
        //"投资者名称","股东名称"
        BuyerRule.Title = new string[] { "发行对象", "认购对象", "发行对象名称" }.ToList();
        BuyerRule.IsTitleEq = true;
        BuyerRule.IsRequire = true;

        var BuyNumber = new TableSearchRule();
        BuyNumber.Name = "增发数量";
        BuyNumber.Title = new string[] { "配售股数", "认购数量", "认购股数", "认购股份数", "发行股份数", "配售数量" }.ToList();
        BuyNumber.IsTitleEq = false;             //包含即可
        BuyNumber.Normalize = NumberUtility.NormalizerStockNumber;

        var BuyMoney = new TableSearchRule();
        BuyMoney.Name = "增发金额";
        BuyMoney.Title = new string[] { "配售金额", "认购金额", "获配金额" }.ToList();
        BuyMoney.IsTitleEq = false;             //包含即可
        BuyMoney.Normalize = MoneyUtility.Format;

        var FreezeYear = new TableSearchRule();
        FreezeYear.Name = "锁定期";
        FreezeYear.Title = new string[] { "锁定期", "限售期" }.ToList();
        FreezeYear.IsTitleEq = false;             //包含即可
        FreezeYear.Normalize = NormalizerFreezeYear;

        var BuyPrice = new TableSearchRule();
        BuyPrice.Name = "价格";
        BuyPrice.Title = new string[] { "认购价格", "配售价格", "申购报价" }.ToList();
        BuyPrice.IsTitleEq = false;             //包含即可
        BuyPrice.Normalize = MoneyUtility.Format;

        var Rules = new List<TableSearchRule>();
        Rules.Add(BuyerRule);
        Rules.Add(BuyNumber);
        Rules.Add(BuyMoney);
        Rules.Add(FreezeYear);
        Rules.Add(BuyPrice);
        var result = HTMLTable.GetMultiInfo(root, Rules, true);
        var increaseStocklist = new List<struIncreaseStock>();
        foreach (var item in result)
        {
            var increase = new struIncreaseStock();
            increase.id = SampleincreaseStock.id;
            increase.BuyMethod = SampleincreaseStock.BuyMethod;
            increase.PublishTarget = item[0].RawData;
            if (String.IsNullOrEmpty(increase.PublishTarget)) continue;
            increase.PublishTarget = increase.PublishTarget.NormalizeTextResult();

            increase.IncreaseNumber = item[1].RawData;
            if (!String.IsNullOrEmpty(increase.IncreaseNumber) && increase.IncreaseNumber.Equals("0")) continue;
            if (!String.IsNullOrEmpty(increase.IncreaseNumber) && increase.IncreaseNumber.Contains("|"))
            {
                increase.IncreaseNumber = increase.IncreaseNumber.Split("|").Last();
            }
            increase.IncreaseMoney = item[2].RawData;
            if (!String.IsNullOrEmpty(increase.IncreaseMoney) && increase.IncreaseMoney.Equals("0")) continue;
            if (!String.IsNullOrEmpty(increase.IncreaseMoney) && increase.IncreaseMoney.Contains("|"))
            {
                increase.IncreaseMoney = increase.IncreaseMoney.Split("|").Last();
            }

            //手工计算金额
            if (String.IsNullOrEmpty(increase.IncreaseMoney))
            {
                if (!String.IsNullOrEmpty(increase.IncreaseNumber))
                {
                    if (!String.IsNullOrEmpty(item[4].RawData))
                    {
                        double price;
                        if (double.TryParse(item[4].RawData, out price))
                        {
                            double number;
                            if (double.TryParse(increase.IncreaseNumber, out number))
                            {
                                double money = price * number;
                                if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("通过计算获得金额：" + money.ToString());
                            }
                        }
                    }
                }
            }

            increase.FreezeYear = item[3].RawData;
            increaseStocklist.Add(increase);
        }
        return increaseStocklist;
    }


    static string NormalizerFreezeYear(string orgString, string TitleWord)
    {
        orgString = orgString.Replace(" ", String.Empty);
        if (orgString.Equals("十二")) return "12";
        var x1 = Utility.GetStringAfter(orgString, "日起");
        int x2;
        if (int.TryParse(x1, out x2)) return x2.ToString();
        x1 = Utility.GetStringBefore(orgString, "个月");
        if (int.TryParse(x1, out x2)) return x2.ToString();
        x1 = RegularTool.GetValueBetweenString(orgString, "日起", "个月");
        if (x1.Equals("十二")) return "12";
        if (int.TryParse(x1, out x2)) return x2.ToString();
        if (orgString.Equals("十二")) return "12";
        if (orgString.Equals("十二个月")) return "12";
        if (orgString.Equals("1年")) return "12";
        if (orgString.Equals("3年")) return "36";
        //自2007年2月3日至2010年2月2日止
        var numbers = RegularTool.GetNumberList(orgString);
        if (numbers.Count == 6)
        {
            var sty = 0;
            var edy = 0;
            if (int.TryParse(numbers[3], out edy) && int.TryParse(numbers[0], out sty))
            {
                if (edy - sty == 1) return "12";
                if (edy - sty == 3) return "36";
                if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("限售期确认：" + orgString);
            }
        }

        return orgString.Trim();
    }

    //认购方式
    static string getBuyMethod(HTMLEngine.MyRootHtmlNode root)
    {
        var p = new EntityProperty();
        //是否包含关键字 "现金认购"
        p.KeyWordMap.Add("现金认购", "现金");
        var result = p.ExtractByKeyWordMap(root);
        if (result.Count == 1)
        {
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("认购方式:" + result[0]);
            return result[0];
        }
        return String.Empty;
    }

}