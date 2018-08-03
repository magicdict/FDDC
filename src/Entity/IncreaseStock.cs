using System;
using System.Linq;
using System.Collections.Generic;
using FDDC;
using static HTMLTable;
using static HTMLEngine;
using static CompanyNameLogic;

public class IncreaseStock : AnnouceDocument
{
    public override List<RecordBase> Extract()
    {
        //认购方式
        var buyMethod = getBuyMethod(root);
        //样本
        var increaseStock = new IncreaseStockRec();
        increaseStock.Id = Id;
        increaseStock.BuyMethod = buyMethod;
        var list = GetMultiTarget(root, increaseStock);
        return list;
    }


    List<RecordBase> GetMultiTarget(HTMLEngine.MyRootHtmlNode root, IncreaseStockRec SampleincreaseStock)
    {
        var PublishTarget = new TableSearchTitleRule();
        PublishTarget.Name = "认购对象";
        //"投资者名称","股东名称"
        PublishTarget.Title = new string[] { "发行对象", "认购对象", "发行对象名称" }.ToList();
        PublishTarget.IsTitleEq = true;
        PublishTarget.IsRequire = true;

        var IncreaseNumber = new TableSearchTitleRule();
        IncreaseNumber.Name = "增发数量";
        IncreaseNumber.Title = new string[] { "配售股数", "认购数量", "认购股数", "认购股份数", "发行股份数", "配售数量" }.ToList();
        IncreaseNumber.IsTitleEq = false;             //包含即可
        IncreaseNumber.Normalize = NumberUtility.NormalizerStockNumber;

        var IncreaseMoney = new TableSearchTitleRule();
        IncreaseMoney.Name = "增发金额";
        IncreaseMoney.Title = new string[] { "配售金额", "认购金额", "获配金额" }.ToList();
        IncreaseMoney.IsTitleEq = false;             //包含即可
        IncreaseMoney.Normalize = MoneyUtility.Format;

        var FreezeYear = new TableSearchTitleRule();
        FreezeYear.Name = "锁定期";
        FreezeYear.Title = new string[] { "锁定期", "限售期" }.ToList();
        FreezeYear.IsTitleEq = false;             //包含即可
        FreezeYear.Normalize = NormalizerFreezeYear;

        var BuyPrice = new TableSearchTitleRule();
        BuyPrice.Name = "价格";
        BuyPrice.Title = new string[] { "认购价格", "配售价格", "申购报价" }.ToList();
        BuyPrice.IsTitleEq = false;             //包含即可
        BuyPrice.Normalize = MoneyUtility.Format;

        var Rules = new List<TableSearchTitleRule>();
        Rules.Add(PublishTarget);
        Rules.Add(IncreaseNumber);
        Rules.Add(IncreaseMoney);
        Rules.Add(FreezeYear);
        Rules.Add(BuyPrice);

        var opt = new SearchOption();
        opt.IsMeger = true;
        var result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, opt);
        var increaseStocklist = new List<RecordBase>();
        foreach (var item in result)
        {
            var increase = new IncreaseStockRec();
            increase.Id = SampleincreaseStock.Id;
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

    /// <summary>
    /// 认购方式
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    string getBuyMethod(HTMLEngine.MyRootHtmlNode root)
    {
        var p = new EntityProperty();
        //是否包含关键字 "现金认购"
        p.KeyWordMap.Add("现金认购", "现金");
        p.Extract(this);
        if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("认购方式:" + string.Join(Utility.SplitChar, p.WordMapResult));
        return string.Join(Utility.SplitChar, p.WordMapResult);
    }
}