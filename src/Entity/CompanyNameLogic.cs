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
                    var FullName = String.Empty;
                    var ShortName = String.Empty;
                    var IsSubCompany = false;
                    if (words[baseInd].Word == "国家电网" &&
                        (baseInd + 1) < words.Count &&
                        words[baseInd + 1].Word == "公司")
                    {
                        namelist.Add(new struCompanyName()
                        {
                            secFullName = "国家电网公司",
                            positionId = sentence.PositionId,
                            WordIdx = baseInd,
                            Score = 100
                        });
                        continue;
                    }
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
                                    ShortName = String.Empty;
                                    for (int i = ShortNameStart; i <= ShortNameEnd; i++)
                                    {
                                        ShortName += words[i].Word;
                                    }
                                }
                                break;
                            }
                        }

                        var FirstShortNameWord = String.Empty;
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
                                FullName = String.Empty;
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
                            FullName = String.Empty;
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

                        if (FullName != String.Empty)
                        {
                            FullName = FullName.Replace(" ", String.Empty).Trim();
                            ShortName = ShortName.Replace(" ", String.Empty).Trim();
                            if (ShortName == "公司" || ShortName == "本公司") ShortName = String.Empty;
                            if (ShortName == String.Empty)
                            {
                                var json = GetCompanyNameByFullName(FullName);
                                ShortName = json.secShortName;
                            }
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

    #region  JSON文件

    static Dictionary<string, struCompanyName> dictFullName = new Dictionary<string, struCompanyName>();

    static Dictionary<string, struCompanyName> dictShortName = new Dictionary<string, struCompanyName>();

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

    #endregion

    public static struCompanyName AfterProcessFullName(string FullName)
    {
        var ShortName = String.Empty;
        var CompanyNameTrailingwords = new string[] {
            "（以下简称", "（下称", "（以下称", "（简称", "(以下简称", "(下称", "(以下称", "(简称"
        };

        //暂时不做括号的正规化
        foreach (var trailing in CompanyNameTrailingwords)
        {
            if (FullName.Contains(trailing))
            {
                //获取简称
                var BracketsList = RegularTool.GetChineseBrackets(FullName);
                foreach (var bracketItem in BracketsList)
                {
                    var ShortNameList = RegularTool.GetChineseQuotation(bracketItem);
                    if (ShortNameList.Count > 0)
                    {
                        ShortName = ShortNameList.First();
                        if (!String.IsNullOrEmpty(ShortName)) ShortName = ShortName.Substring(1, ShortName.Length - 2);
                    }
                }
                FullName = Utility.GetStringBefore(FullName, trailing);
            }
        }
        if (FullName.Contains("及其"))
        {
            FullName = Utility.GetStringBefore(FullName, "及其");
        }
        if (FullName.Contains("股东"))
        {
            FullName = Utility.GetStringAfter(FullName, "股东");
        }
        if (FullName.Contains("一致行动人"))
        {
            FullName = Utility.GetStringAfter(FullName, "一致行动人");
        }
        if (!String.IsNullOrEmpty(CompanyNameLogic.GetCompanyNameByShortName(FullName).secFullName))
        {
            FullName = CompanyNameLogic.GetCompanyNameByShortName(FullName).secFullName;
        }
        //删除前导
        FullName = EntityWordAnlayzeTool.TrimLeadingUL(FullName);
        FullName = CutOtherLeadingWords(FullName);
        if (ShortName != String.Empty)
        {
            return new struCompanyName() { secFullName = FullName, secShortName = ShortName, Score = 80 };
        }
        else
        {
            return new struCompanyName() { secFullName = FullName, Score = 60 };
        }
    }

    static string CutOtherLeadingWords(String OrgString)
    {
        var LeadingWords = new string[]
        {
            "证券代码","招标人","注册资本","注册地址","法定代表人","主营业务",
            "项目名称","证券简称","住所","项目名称","股票代码",
            "经营范围","公司名称","证券代码","注册地","备查文件",
            "成立日期","名称","类型"
        };
        foreach (var lw in LeadingWords)
        {
            if (OrgString.IndexOf(lw + "：") != -1)
            {
                return OrgString.Substring(0, OrgString.IndexOf(lw + "："));
            }
            if (OrgString.IndexOf(lw) != -1)
            {
                return OrgString.Substring(0, OrgString.IndexOf(lw));
            }
        }
        return OrgString;
    }

    public static string MostLikeCompanyName(List<string> CandidateWords)
    {
        foreach (var word in CandidateWords)
        {
            if (string.IsNullOrEmpty(word)) continue;
            var posSeg = new PosSegmenter();
            var cuts = posSeg.Cut(word).ToList();
            if (cuts[0].Flag == WordUtility.地名)
            {
                if (word.EndsWith("公司") || word.Contains("有限合伙")) return word;
            }
        }
        if (CandidateWords.Count == 0) return String.Empty;
        return CandidateWords[0];
    }


    public static (String FullName, String ShortName) NormalizeCompanyName(AnnouceDocument doc, string word)
    {
        if (String.IsNullOrEmpty(word)) return (String.Empty, String.Empty);
        var fullname = word.Replace(" ", String.Empty);
        var shortname = String.Empty;
        foreach (var companyname in doc.companynamelist)
        {
            if (companyname.secFullName == fullname)
            {
                if (shortname == String.Empty)
                {
                    shortname = companyname.secShortName;
                    break;
                }
            }
            if (companyname.secShortName == fullname)
            {
                fullname = companyname.secFullName;
                shortname = companyname.secShortName;
                break;
            }
            //如果进来的是简称，而提取的公司信息里面，只有全称，这里简单推断一下
            //简称和全称的关系
            if (companyname.secFullName.Contains(fullname) &&
                companyname.secFullName.Length > fullname.Length)
            {
                fullname = companyname.secFullName;
                shortname = word;
            }
        }

        if (shortname == String.Empty)
        {
            shortname = CompanyNameLogic.GetCompanyNameByFullName(fullname).secShortName;
        }
        return (fullname, shortname);
    }
}