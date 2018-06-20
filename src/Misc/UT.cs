using System;
using System.Text.RegularExpressions;
using FDDC;

public static class UT
{
  

    public static void RunWordAnlayze()
    {
        var SProjectName = new Surround();
        var root = HTMLEngine.Anlayze(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1044779.html");
        var Contract = TraningDataset.GetContractById("1044779")[0];
        SProjectName.AnlayzeEntitySurroundWords(root, Contract.ProjectName);

        root = HTMLEngine.Anlayze(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1450.html");
        Contract = TraningDataset.GetContractById("1450")[0];
        SProjectName.AnlayzeEntitySurroundWords(root, Contract.ProjectName);

        root = HTMLEngine.Anlayze(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1042224.html");
        Contract = TraningDataset.GetContractById("1042224")[0];
        SProjectName.AnlayzeEntitySurroundWords(root, Contract.ProjectName);

        root = HTMLEngine.Anlayze(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\917362.html");
        Contract = TraningDataset.GetContractById("917362")[0];
        SProjectName.AnlayzeEntitySurroundWords(root, Contract.ProjectName);
        SProjectName.WriteTop(10);
        var TestString = "承运市";
        var pos = new JiebaNet.Segmenter.PosSeg.PosSegmenter();
        foreach (var item in pos.Cut(TestString))
        {
            Console.WriteLine(item.Word + ":" + item.Flag);
        }
    }

    public static void ContractTest()
    {
        StockChange.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持\html\20526193.html");
        StockChange.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持\html\20596890.html");
        StockChange.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持\html\1018217.html");
        StockChange.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持\html\314146.html");
        IncreaseStock.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\定增\html\7880.html");

        var x1 = Normalizer.NormalizeItemListNumber("（4）2012 年 4 月，公司与中国华西企业股份");
        var x2 = Normalizer.NormalizeItemListNumber("4 、承包方式： 从深化设计、制作、运输、");
        var x3 = Normalizer.NormalizeItemListNumber("4、承包方式： 从深化设计、制作、运输、");
        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1153.html");
        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1008828.html");
        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\3620.html");
        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1518.html");
        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1120707.html");
        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1044779.html");
        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1450.html");
        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1042224.html");
        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\917362.html");
        IncreaseStock.Extract(@"E:\WorkSpace2018\FDDC_announcements_round1_train_20180518\round1_train_20180518\定增\html\7880.html");
        //数字金额的测试
        var TestString = "中标价为人民币共计16928.79754万元（大写：人民币壹亿陆仟玖佰贰拾捌万柒仟玖佰柒拾伍元肆角整）。";
        var Result = MoneyUtility.SeekMoney(TestString);
        Console.WriteLine(Result[0].MoneyAmount);

        TestString = "安徽盛运环保（集团）股份有限公司";
        //Result = Utility.GetStringBefore(TestString, "有限公司");
        //Console.WriteLine(Result);

        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\5258.html");

        var x0 = "在此之前，2003年6月30日，本公司曾与MICROS US和MICROS Singapore（以下简称 “MICROS”）签订了《技术许可与代理协议》，并分别于2005年11月、2006年12月和2007年 10月与MICROS相继签署了第一、二、三次补充协议。";
        var t0 = EntityWordAnlayzeTool.GetMainWordSentence(x0);
        //在此之前，2003年6月30日，本公司曾与MICROS US和MICROS Singapore（以下简称 “MICROS”）签订了《技术许可与代理协议》，并分别于2005年11月、2006年12月和2007年 10月与MICROS相继签署了第一、二、三次补充协议。"
        //在此之前，2003年6月30日，本公司  与MICROS US和MICROS Singapore（以下简称 “MICROS”）签订  《技术许可与代理协议》，并   于2005年11月、2006年12月和2007年 10月与MICROS    签署  第一、二、三次补充协议。

    }

    public static void RegularExpress()
    {



        var d0 = "宏润建设集团股份有限公司(以下简称“公司”)于2014年1月7日收到西安市建设工程中标通知书，“西安市地铁四号线工程（航天东路站—北客站）土建施工D4TJSG-5标”项目由公司中标承建，工程中标价49,290万元。";
        var x0 = RegularTool.GetMultiValueBetweenMark(d0, "“", "”");

        var d1 = DateUtility.GetDate("河北先河环保科技股份有限公司董事会二○一二年十一月三十日");
        Console.WriteLine(d1);

        var d2 = "公司第五届董事会第七次会议审议通过了《关于公司与神华铁路货车运输有限责任公司签订企业自用货车购置供货合同的议案》，2014年1月20日，公司与神华铁路货车运输有限责任公司签署了《企业自用货车购置供货合同》。";
        var x2 = RegularTool.GetValueBetweenString(d2, "与", "签订");

        var s0 = "2010年12月3日，中工国际工程股份有限公司与委内瑞拉农业土地部下属的委内瑞拉农业公司签署了委内瑞拉农副产品加工设备制造厂工业园项目商务合同，与委内瑞拉农签署了委内瑞拉奥里合同。";
        var x = RegularTool.GetMultiValueBetweenString(s0, "与", "签署");

        var s1 = "收到贵州高速公路开发总公司发出的通知";
        var s2 = "接到贵州高速公路开发总公司发出的通知";
        var s3 = "收到贵州高速公路开发总公司发出的告知";
        var s4 = "接到贵州高速公路开发总公司发出的告知";
        Regex rg = new Regex("(?<=(" + "收到|接到" + "))[.\\s\\S]*?(?=(" + "通知|告知" + "))", RegexOptions.Multiline | RegexOptions.Singleline);

        Console.WriteLine(rg.Match(s1).Value);
        Console.WriteLine(rg.Match(s2).Value);
        Console.WriteLine(rg.Match(s3).Value);
        Console.WriteLine(rg.Match(s4).Value);

    }
}