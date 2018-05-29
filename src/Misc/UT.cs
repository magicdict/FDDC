using System;
using System.Text.RegularExpressions;
using 金融数据整理大赛;

public static class UT
{

    public static void RunWordAnlayze()
    {
        var root = HTMLEngine.Anlayze(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1044779.html");
        WordAnlayze.Anlayze(root);
    }



    public static void GenericTest()
    {

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

        Contract.Extract(@"E:\WorkSpace2018\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\5258.html");
    }

    public static void RegularExpress()
    {

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