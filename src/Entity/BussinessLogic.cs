using System;
using System.Collections.Generic;
using FDDC;

public class BussinessLogic
{
    public static string GetCompanyShortName(HTMLEngine.MyRootHtmlNode root)
    {
        var companyList = new Dictionary<string, string>();
        //从第一行开始找到  有限公司 有限责任公司, 如果有简称的话Value是简称
        //股票简称：东方电气
        //东方电气股份有限公司董事会
        var Extractor = new ExtractProperty();
        Extractor.LeadingWordList = new string[] { "股票简称", "证券简称" };
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            var ShortName = item.Replace(":", "").Replace("：", "").Trim();
            if (Utility.GetStringBefore(ShortName, "、") != "")
            {
                ShortName = Utility.GetStringBefore(ShortName, "、");
            }
            if (Utility.GetStringBefore(ShortName, "）") != "")
            {
                ShortName = Utility.GetStringBefore(ShortName, "）");
            }
            if (Utility.GetStringBefore(ShortName, "公告") != "")
            {
                ShortName = Utility.GetStringBefore(ShortName, "公告");
            }
            if (Utility.GetStringBefore(ShortName, "股票") != "")
            {
                ShortName = Utility.GetStringBefore(ShortName, "股票");
            }
            if (Utility.GetStringBefore(ShortName, "证券") != "")
            {
                ShortName = Utility.GetStringBefore(ShortName, "证券");
            }
            if (Utility.GetStringBefore(ShortName, " ") != "")
            {
                ShortName = Utility.GetStringBefore(ShortName, " ");
            }
            FDDC.Program.Logger.WriteLine("简称:[" + ShortName + "]");
            return ShortName;
        }
        return "";
    }
    public static string GetCompanyFullName(HTMLEngine.MyRootHtmlNode root)
    {
        var Extractor = new ExtractProperty();
        Extractor.TrailingWordList = new string[] { "公司董事会" };
        Extractor.Extract(root);
        Extractor.CandidateWord.Reverse();
        foreach (var item in Extractor.CandidateWord)
        {
            Program.Logger.WriteLine("全称：[" + item + "公司]");
            return item;
        }
        return "";
    }
}