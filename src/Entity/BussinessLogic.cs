using System;
using System.Collections.Generic;
using System.IO;
using FDDC;
using JiebaNet.Segmenter.PosSeg;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

public class BussinessLogic
{
    //固定搭配
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


    //词法分析
    public static List<struCompanyName> GetCompanyNameByCutWord(HTMLEngine.MyRootHtmlNode root)
    {
        var posSeg = new PosSegmenter();
        var namelist = new List<struCompanyName>();
        foreach (var paragrah in root.Children)
        {
            foreach (var sentence in paragrah.Children)
            {
                var words = posSeg.Cut(sentence.Content).ToList();
                for (int baseInd = 0; baseInd < words.Count; baseInd++)
                {
                    var FullName = "";
                    var ShortName = "";
                    if (words[baseInd].Word == "有限公司" ||
                       (words[baseInd].Word == "有限" && baseInd != words.Count - 1 && words[baseInd + 1].Word == "合伙"))
                    {
                        //是否能够在前面找到地名
                        for (int NRIdx = baseInd; NRIdx > -1; NRIdx--)
                        {
                            //地理
                            if (words[NRIdx].Flag == "ns")
                            {
                                FullName = "";
                                for (int companyFullNameInd = NRIdx; companyFullNameInd <= baseInd; companyFullNameInd++)
                                {
                                    FullName += words[companyFullNameInd].Word;
                                }
                                if (words[baseInd].Word == "有限")
                                {
                                    FullName += words[baseInd + 1].Word;
                                    FullName += words[baseInd + 2].Word;
                                }
                                break;  //不要继续寻找地名了
                            }
                        }
                        //是否能够在后面找到简称
                        for (int JCIdx = baseInd; JCIdx < words.Count; JCIdx++)
                        {
                            //地理
                            if (words[JCIdx].Word.Equals("简称"))
                            {
                                var ShortNameStart = -1;
                                var ShortNameEnd = -1;
                                for (int ShortNameIdx = baseInd; ShortNameIdx < words.Count; ShortNameIdx++)
                                {
                                    if (words[ShortNameIdx].Word.Equals("“"))
                                    {
                                        ShortNameStart = ShortNameIdx + 1;
                                    }
                                    if (words[ShortNameIdx].Word.Equals("”"))
                                    {
                                        ShortNameEnd = ShortNameIdx - 1;
                                        break;
                                    }
                                }
                                if (ShortNameStart != -1 && ShortNameEnd != -1)
                                {
                                    ShortName = "";
                                    for (int i = ShortNameStart; i <= ShortNameEnd; i++)
                                    {
                                        ShortName += words[i].Word;
                                    }
                                }
                            }
                        }
                        if (FullName != "")
                        {
                            namelist.Add(new struCompanyName() { secFullName = FullName, secShortName = ShortName });
                        }
                    }

                }
            }
        }
        return namelist;
    }


    //JSON文件

    static Dictionary<string, struCompanyName> dictFullName = new Dictionary<string, struCompanyName>();

    static Dictionary<string, struCompanyName> dictShortName = new Dictionary<string, struCompanyName>();

    public struct struCompanyName
    {
        public string secShortName;
        public string secFullName;
        public string secShortNameChg;
    }

    public static void LoadCompanyName(string JSONfilename)
    {
        JObject o = JObject.Parse(File.ReadAllText(JSONfilename));
        JArray list = (JArray)o["data"];
        List<struCompanyName> company = list.ToObject<List<struCompanyName>>();
        foreach (var item in company)
        {
            if (!dictFullName.ContainsKey(item.secFullName))
            {
                dictFullName.Add(item.secFullName, item);
            }
            if (!dictShortName.ContainsKey(item.secShortName))
            {
                dictShortName.Add(item.secShortName, item);
            }
        }
    }

    public static struCompanyName GetCompanyNameByFullName(string FullName)
    {
        if (dictFullName.ContainsKey(FullName)) return dictFullName[FullName];
        return new struCompanyName();
    }

    public static struCompanyName GetCompanyNameByShortName(string ShortName)
    {
        if (dictShortName.ContainsKey(ShortName)) return dictShortName[ShortName];
        return new struCompanyName();
    }

}