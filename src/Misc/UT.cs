using System;
using System.Text.RegularExpressions;

public static class UT{
    public static void GenericTest()
    {

        IncreaseStock.Extract(@"E:\WorkSpace2018\FDDC_announcements_round1_train_20180518\round1_train_20180518\定增\html\7880.html");



        //数字金额的测试
        var TestString = "中标价为人民币共计16928.79754万元（大写：人民币壹亿陆仟玖佰贰拾捌万柒仟玖佰柒拾伍元肆角整）。";
        var Result = Utility.SeekMoney(TestString,"中标价");
        Console.WriteLine(Result);

        TestString = "安徽盛运环保（集团）股份有限公司";
        Result = Utility.GetStringBefore(TestString,"有限公司");
        Console.WriteLine(Result);

        Contract.Extract(@"E:\WorkSpace2018\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\5258.html");
    }

    public static void RegularExpress(){
        
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