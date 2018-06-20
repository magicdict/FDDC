using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FDDC;
using JiebaNet.Segmenter.PosSeg;

public class CompanyNameLogic
{

    public static string GetCompanyFullName(HTMLEngine.MyRootHtmlNode root)
    {
        var Extractor = new ExtractProperty();
        Extractor.TrailingWordList = new string[] { "公司董事会" };
        Extractor.Extract(root);
        Extractor.CandidateWord.Reverse();
        foreach (var item in Extractor.CandidateWord)
        {
            Program.Logger.WriteLine("全称：[" + item + "公司]");
            return item.Value;
        }
        return "";
    }

    public static List<struCompanyName> GetCompanyNameByCutWord(HTMLEngine.MyRootHtmlNode root)
    {
        var posSeg = new PosSegmenter();
        var namelist = new List<struCompanyName>();
        foreach (var paragrah in root.Children)
        {
            foreach (var sentence in paragrah.Children)
            {
                if (string.IsNullOrEmpty(sentence.Content)) continue;
                var words = posSeg.Cut(sentence.Content).ToList();
                var PreviewEndIdx = -1;
                for (int baseInd = 0; baseInd < words.Count; baseInd++)
                {
                    var FullName = "";
                    var ShortName = "";
                    var IsSubCompany = false;

                    if (
                         words[baseInd].Word == "有限公司" ||
                        (words[baseInd].Word == "公司" && baseInd != 0 && words[baseInd - 1].Word == "承包") ||
                        (words[baseInd].Word == "有限" && baseInd != words.Count - 1 && words[baseInd + 1].Word == "合伙")
                       )
                    {
                        //是否能够在后面找到简称
                        for (int JCIdx = baseInd; JCIdx < words.Count; JCIdx++)
                        {
                            //简称关键字
                            if (words[JCIdx].Word.Equals("简称") || words[JCIdx].Word.Equals("称"))
                            {
                                var ShortNameStart = -1;
                                var ShortNameEnd = -1;
                                for (int ShortNameIdx = JCIdx; ShortNameIdx < words.Count; ShortNameIdx++)
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
                                break;
                            }
                        }

                        var FirstShortNameWord = "";
                        if (ShortName.Length == 4)
                        {
                            FirstShortNameWord = ShortName.Substring(0, 2);
                        }
                        var IsMarkClosed = true;
                        var CompanyStartIdx = -1;
                        var FirstShortNameIdx = -1; //包含简称的位置
                        //是否能够在前面找到地名
                        for (int NRIdx = baseInd; NRIdx > PreviewEndIdx; NRIdx--)
                        {
                            if (words[NRIdx].Word == FirstShortNameWord)
                            {
                                FirstShortNameIdx = NRIdx;   //备用
                            }
                            //寻找地名?words[NRIdx].Flag == EntityWordAnlayzeTool.机构团体
                            //posSeg.Cut(words[NRIdx].Word + "市").First().Flag == EntityWordAnlayzeTool.地名
                            if (words[NRIdx].Flag == WordUtility.地名 || WordUtility.DictNSAdjust.Contains(words[NRIdx].Word))
                            {
                                //注意，地名可能相连，例如：上海市嘉定
                                if (NRIdx != 0 && (words[NRIdx - 1].Flag == WordUtility.地名 || WordUtility.DictNSAdjust.Contains(words[NRIdx - 1].Word))) continue;
                                FullName = "";
                                for (int companyFullNameInd = NRIdx; companyFullNameInd <= baseInd; companyFullNameInd++)
                                {
                                    FullName += words[companyFullNameInd].Word;
                                }
                                //(有限合伙)
                                if (words[baseInd].Word == "有限")
                                {
                                    FullName += words[baseInd + 1].Word;
                                    FullName += words[baseInd + 2].Word;
                                }
                                //子公司判断
                                if (NRIdx != 0 && words[NRIdx - 1].Word == "子公司")
                                {
                                    IsSubCompany = true;
                                }
                                if (IsMarkClosed)
                                {
                                    //皆大欢喜的局面
                                    CompanyStartIdx = NRIdx;
                                    PreviewEndIdx = baseInd;
                                    break;  //不要继续寻找地名了
                                }
                            }
                            if (words[NRIdx].Flag == WordUtility.标点)
                            {
                                if (words[NRIdx].Word != "（" && words[NRIdx].Word != "）") break;
                                if (words[NRIdx].Word == "）") IsMarkClosed = false;    //打开
                                if (words[NRIdx].Word == "（") IsMarkClosed = true;     //关闭
                            }
                        }

                        if (CompanyStartIdx == -1)
                        {
                            if (FirstShortNameIdx == -1) continue;
                            if (posSeg.Cut(ShortName).First().Flag == WordUtility.地名) continue;
                            FullName = "";
                            for (int NRIdx = FirstShortNameIdx; NRIdx <= baseInd; NRIdx++)
                            {
                                FullName += words[NRIdx].Word;
                            }
                            //(有限合伙)
                            if (words[baseInd].Word == "有限")
                            {
                                FullName += words[baseInd + 1].Word;
                                FullName += words[baseInd + 2].Word;
                            }
                            //子公司判断
                            if (FirstShortNameIdx != 0 && words[FirstShortNameIdx - 1].Word == "子公司")
                            {
                                IsSubCompany = true;
                            }
                        }

                        if (FullName != "")
                        {
                            FullName = FullName.Replace(" ", "").Trim();
                            ShortName = ShortName.Replace(" ", "").Trim();
                            if (ShortName == "公司" || ShortName == "本公司") ShortName = "";
                            namelist.Add(new struCompanyName()
                            {
                                secFullName = FullName,
                                secShortName = ShortName,
                                isSubCompany = IsSubCompany,
                                positionId = sentence.PositionId,
                                WordIdx = CompanyStartIdx,
                                Score = 100
                            });
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
        //是否为子公司
        public bool isSubCompany;
        //段落编号
        public int positionId;
        //词位置
        public int WordIdx;
        //可信度评分
        public int Score;
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