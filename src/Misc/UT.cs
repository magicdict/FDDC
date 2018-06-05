using System;
using System.Text.RegularExpressions;
using FDDC;

public static class UT
{


    public static void JianchengTest()
    {
        BussinessLogic.GetCompanyNameByCutWord(HTMLEngine.Anlayze(FDDC.Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持\html\20526193.html"));

        var ContractPath_TRAIN = FDDC.Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同";
        Console.WriteLine("Start To Extract Info Contract TRAIN");
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + @"\html\"))
        {
            var root = HTMLEngine.Anlayze(filename);
            var fi = new System.IO.FileInfo(filename);
            FDDC.Program.Logger.WriteLine("FileName:" + fi.Name);
            BussinessLogic.GetCompanyShortName(root);
            BussinessLogic.GetCompanyFullName(root);
        }
        Console.WriteLine("Complete Extract Info Contract");

        var StockChangePath_TRAIN = FDDC.Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持";
        Console.WriteLine("Start To Extract Info Contract TRAIN");
        foreach (var filename in System.IO.Directory.GetFiles(StockChangePath_TRAIN + @"\html\"))
        {
            var root = HTMLEngine.Anlayze(filename);
            var fi = new System.IO.FileInfo(filename);
            FDDC.Program.Logger.WriteLine("FileName:" + fi.Name);
            BussinessLogic.GetCompanyShortName(root);
            BussinessLogic.GetCompanyFullName(root);
        }

        Console.WriteLine("Complete Extract Info Contract");

        var IncreaseStockPath_TRAIN = FDDC.Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\定增";
        Console.WriteLine("Start To Extract Info Contract TRAIN");
        foreach (var filename in System.IO.Directory.GetFiles(IncreaseStockPath_TRAIN + @"\html\"))
        {
            var root = HTMLEngine.Anlayze(filename);
            var fi = new System.IO.FileInfo(filename);
            FDDC.Program.Logger.WriteLine("FileName:" + fi.Name);
            BussinessLogic.GetCompanyShortName(root);
            BussinessLogic.GetCompanyFullName(root);
        }
        Console.WriteLine("Complete Extract Info Contract");
    }

    public static void RunWordAnlayze()
    {
        var root = HTMLEngine.Anlayze(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1044779.html");
        var Contract = Traning.GetContractById("1044779")[0];
        WordAnlayze.Anlayze(root, Contract.ProjectName);

        root = HTMLEngine.Anlayze(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1450.html");
        Contract = Traning.GetContractById("1450")[0];
        WordAnlayze.Anlayze(root, Contract.ProjectName);

        root = HTMLEngine.Anlayze(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1042224.html");
        Contract = Traning.GetContractById("1042224")[0];
        WordAnlayze.Anlayze(root, Contract.ProjectName);

        root = HTMLEngine.Anlayze(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\917362.html");
        Contract = Traning.GetContractById("917362")[0];
        WordAnlayze.Anlayze(root, Contract.ProjectName);

    }


    public static void StockChangeTest()
    {
        StockChange.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持\html\20596890.html");
        StockChange.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持\html\1018217.html");
    }

    public static void IncreaseStockTest()
    {
        IncreaseStock.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\定增\html\7880.html");
    }

    public static void ContractTest()
    {
        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\3620.html");
        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1518.html");
        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1120707.html");
        var x1 = Normalizer.NormalizeItemListNumber("（4）2012 年 4 月，公司与中国华西企业股份");
        var x2 = Normalizer.NormalizeItemListNumber("4 、承包方式： 从深化设计、制作、运输、");
        var x3 = Normalizer.NormalizeItemListNumber("4、承包方式： 从深化设计、制作、运输、");
        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1044779.html");
        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1450.html");
        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1042224.html");
        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\917362.html");
        IncreaseStock.Extract(@"E:\WorkSpace2018\FDDC_announcements_round1_train_20180518\round1_train_20180518\定增\html\7880.html");
        //数字金额的测试
        var TestString = "中标价为人民币共计16928.79754万元（大写：人民币壹亿陆仟玖佰贰拾捌万柒仟玖佰柒拾伍元肆角整）。";
        var Result = Utility.SeekMoney(TestString, "中标价");
        Console.WriteLine(Result);

        TestString = "安徽盛运环保（集团）股份有限公司";
        Result = Utility.GetStringBefore(TestString, "有限公司");
        Console.WriteLine(Result);

        Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\5258.html");
    }

    public static void RegularExpress()
    {

        var d1 = RegularTool.GetDate("河北先河环保科技股份有限公司董事会二○一二年十一月三十日");
        Console.WriteLine(d1);

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