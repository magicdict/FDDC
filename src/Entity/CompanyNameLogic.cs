using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FDDC;
using JiebaNet.Segmenter.PosSeg;
using static LTPTrainingNER;

public class CompanyNameLogic
{
    /// <summary>
    /// 公司结构体
    /// </summary>
    public struct struCompanyName
    {
        /// <summary>
        /// 简称
        /// </summary>
        public string secShortName;
        /// <summary>
        /// 全称
        /// </summary>
        public string secFullName;
        /// <summary>
        /// 曾用名
        /// </summary>
        public string secShortNameChg;
        /// <summary>
        /// 是否为子公司
        /// </summary>
        public bool isSubCompany;
        /// <summary>
        /// 母公司
        /// </summary>
        public string FatherName;
        /// <summary>
        /// 段落编号
        /// </summary>
        public int positionId;
        /// <summary>
        /// 词位置
        /// </summary>
        public int WordIdx;
        /// <summary>
        /// 可信度评分
        /// </summary>
        public int Score;
    }

    /// <summary>
    /// 使用NerInfo抽取
    /// </summary>
    public static void GetCompanyNameByNerInfo(List<List<struWordNER>> paragragh)
    {
        var Rule1 = new NerExtract.NerExtractRule();
        Rule1.MaxWordLength = 10;
        Rule1.StartWord = new List<struWordNER>();
        //首词NER属性
        var word = new struWordNER();
        word.pos = LTPTrainingNER.地名;  //只设定类型
        Rule1.StartWord.Add(word);
        //结束词NER属性
        Rule1.EndWord = new List<struWordNER>();
        word = new struWordNER();
        word.cont = "有限公司";  //只设定词语
        Rule1.EndWord.Add(word);

        var Rule2 = new NerExtract.NerExtractRule();
        Rule2.MaxWordLength = 10;
        Rule2.StartWord = new List<struWordNER>();
        //首词NER属性
        word = new struWordNER();
        word.pos = LTPTrainingNER.地名;  //只设定类型
        Rule2.StartWord.Add(word);
        //结束词NER属性
        Rule2.EndWord = new List<struWordNER>();
        word = new struWordNER();
        word.cont = "有限";  //只设定词语
        Rule2.EndWord.Add(word);
        word = new struWordNER();
        word.cont = "责任";  //只设定词语
        Rule2.EndWord.Add(word);
        word = new struWordNER();
        word.cont = "公司";  //只设定词语
        Rule2.EndWord.Add(word);

        var Rule3 = new NerExtract.NerExtractRule();
        Rule3.MaxWordLength = 10;
        Rule3.StartWord = new List<struWordNER>();
        //首词NER属性
        word = new struWordNER();
        word.pos = LTPTrainingNER.地名;  //只设定类型
        Rule3.StartWord.Add(word);
        //结束词NER属性
        Rule3.EndWord = new List<struWordNER>();
        word = new struWordNER();
        word.cont = "（";  //只设定词语
        Rule3.EndWord.Add(word);
        word = new struWordNER();
        word.cont = "有限";  //只设定词语
        Rule3.EndWord.Add(word);
        word = new struWordNER();
        word.cont = "合伙";  //只设定词语
        Rule3.EndWord.Add(word);
        word = new struWordNER();
        word.cont = "）";  //只设定词语
        Rule3.EndWord.Add(word);

        var company1 = NerExtract.Extract(Rule1, paragragh);
        var company2 = NerExtract.Extract(Rule2, paragragh);
        var company3 = NerExtract.Extract(Rule3, paragragh);
    }

    public static List<struCompanyName> GetCompanyNameByCutWordFromTextFile(string TextFileName)
    {
        var posSeg = new PosSegmenter();
        var namelist = new List<struCompanyName>();
        var Lines = new List<string>();
        var sr = new StreamReader(TextFileName);
        while (!sr.EndOfStream)
        {
            var line = sr.ReadLine();
            if (!String.IsNullOrEmpty(line))
            {
                Lines.Add(line.Replace(" ", ""));
            }
        }
        sr.Close();
        foreach (var sentence in Lines)
        {
            GetCompany(namelist, sentence, 0);
        }
        return namelist;
    }

    public static List<struCompanyName> GetCompanyNameByCutWordFromHTML(HTMLEngine.MyRootHtmlNode root)
    {
        var namelist = new List<struCompanyName>();
        foreach (var paragrah in root.Children)
        {
            foreach (var sentence in paragrah.Children)
            {
                GetCompany(namelist, sentence.Content, sentence.PositionId);
            }
        }
        return namelist;
    }

    static void GetCompany(List<struCompanyName> namelist, string sentence, int PositionId = 0)
    {
        var posSeg = new PosSegmenter();
        if (string.IsNullOrEmpty(sentence)) return;
        var words = posSeg.Cut(sentence).ToList();
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
                    positionId = PositionId,
                    WordIdx = baseInd,
                    Score = 100
                });
                continue;
            }
            if (
                 words[baseInd].Word == "有限公司" ||
                (words[baseInd].Word == "公司" && baseInd != 0 && words[baseInd - 1].Word == "有限责任") ||
                (words[baseInd].Word == "公司" && baseInd != 0 && words[baseInd - 1].Word == "承包") ||
                (words[baseInd].Word == "有限" && baseInd != words.Count - 1 && words[baseInd + 1].Word == "合伙")
               )
            {
                //是否能够在后面找到简称
                for (int JCIdx = baseInd + 1; JCIdx < words.Count; JCIdx++)
                {
                    //注意，这个简称还必须在下一个出现公司之前才可以！
                    if (
                        words[JCIdx].Word == "有限公司" ||
                        (words[JCIdx].Word == "公司" && JCIdx != 0 && words[JCIdx - 1].Word == "有限责任") ||
                        (words[JCIdx].Word == "公司" && JCIdx != 0 && words[JCIdx - 1].Word == "承包") ||
                        (words[JCIdx].Word == "有限" && JCIdx != words.Count - 1 && words[JCIdx + 1].Word == "合伙")
                    )
                    {
                        //宁波凯数咨询有限合伙企业(有限合伙)(以下简称“凯数投资”)的补救
                        if (
                             words[JCIdx - 1].Word == "（" && words[JCIdx].Word == "有限" &&
                            (JCIdx != words.Count - 1 && words[JCIdx + 1].Word == "合伙") &&
                            (JCIdx != words.Count - 2 && words[JCIdx + 2].Word == "）")
                             && baseInd == JCIdx - 4
                            )
                        {
                            //合伙企业(有限合伙)
                            Console.WriteLine("合伙企业(有限合伙)");
                            baseInd += 4;
                        }
                        else
                        {
                            break;
                        }
                    }
                    //简称关键字
                    if (words[JCIdx].Word.Equals("简称") || words[JCIdx].Word.Equals("下称") || words[JCIdx].Word.Equals("称"))
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
                    if (words[NRIdx].Flag == LTPTrainingNER.地名 || PosNS.NsDict.Contains(words[NRIdx].Word))
                    {
                        //注意，地名可能相连，例如：上海市嘉定
                        if (NRIdx != 0 && (words[NRIdx - 1].Flag == LTPTrainingNER.地名 || PosNS.NsDict.Contains(words[NRIdx - 1].Word))) continue;
                        FullName = String.Empty;
                        for (int companyFullNameInd = NRIdx; companyFullNameInd <= baseInd; companyFullNameInd++)
                        {
                            FullName += words[companyFullNameInd].Word;
                        }
                        //(有限合伙)
                        if (words[baseInd].Word == "有限")
                        {
                            FullName += words[baseInd + 1].Word;
                            if ((baseInd + 2) < words.Count) FullName += words[baseInd + 2].Word;
                        }
                        //子公司判断
                        if (NRIdx != 0 && words[NRIdx - 1].Word == "子公司")
                        {
                            IsSubCompany = true;
                        }
                        if (NRIdx > 2 && (words[NRIdx - 1].Word == "下属" || words[NRIdx - 2].Word == "下属"))
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
                    if (words[NRIdx].Flag == LTPTrainingNER.词性标点)
                    {
                        if (words[NRIdx].Word != "（" && words[NRIdx].Word != "）" &&
                           words[NRIdx].Word != "(" && words[NRIdx].Word != ")") break;
                        if (words[NRIdx].Word == "）" || words[NRIdx].Word == ")") IsMarkClosed = false;    //打开
                        if (words[NRIdx].Word == "（" || words[NRIdx].Word == "(") IsMarkClosed = true;     //关闭
                    }
                }

                if (CompanyStartIdx == -1)
                {
                    if (FirstShortNameIdx == -1) continue;
                    if (posSeg.Cut(ShortName).First().Flag == LTPTrainingNER.地名) continue;
                    FullName = String.Empty;
                    for (int NRIdx = FirstShortNameIdx; NRIdx <= baseInd; NRIdx++)
                    {
                        FullName += words[NRIdx].Word;
                    }


                    //有限合伙
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
                    if (FirstShortNameIdx > 2 && (words[FirstShortNameIdx - 1].Word == "下属" || words[FirstShortNameIdx - 2].Word == "下属"))
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
                    if (!String.IsNullOrEmpty(FullName))
                    {
                        var x = FullName.ToCharArray();
                        if (x.Length > 0 && (int)(x.First()) == 61548)
                        {
                            //PDF转换问题
                            FullName = FullName.Substring(1);
                        }
                    }

                    namelist.Add(new struCompanyName()
                    {
                        secFullName = FullName == null ? FullName : FullName.Trim(),
                        secShortName = ShortName == null ? ShortName : ShortName.Trim(),
                        isSubCompany = IsSubCompany,
                        positionId = PositionId,
                        WordIdx = CompanyStartIdx,
                        Score = 100
                    });
                }
            }

        }
    }



    #region  JSON文件

    static Dictionary<string, struCompanyName> dictFullName = new Dictionary<string, struCompanyName>();

    static Dictionary<string, struCompanyName> dictShortName = new Dictionary<string, struCompanyName>();

    public static void LoadCompanyName(string JSONfilename)
    {
        if (!File.Exists(JSONfilename))
        {
            Console.WriteLine("Can't find CompanyName Json file");
            return;
        }
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

    /// <summary>
    /// 从字典中寻找字典
    /// </summary>
    /// <param name="FullName"></param>
    /// <returns></returns>
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
                        if (!String.IsNullOrEmpty(ShortName))
                        {
                            ShortName = ShortName.Substring(1, ShortName.Length - 2);
                            break;
                        }
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
        FullName = Utility.TrimLeadingUL(FullName);
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
            if (cuts[0].Flag == LTPTrainingNER.地名)
            {
                if (word.EndsWith("公司") || word.Contains("有限合伙")) return word;
            }
        }
        if (CandidateWords.Count == 0) return String.Empty;
        return CandidateWords[0];
    }

    /// <summary>
    /// 公司名称的获得
    /// </summary>
    /// <param name="FullName"></param>
    /// <param name="ShortName"></param>
    /// <returns></returns>
    public static (String FullName, String ShortName) NormalizeCompanyName(AnnouceDocument doc, string word)
    {
        if (String.IsNullOrEmpty(word)) return (String.Empty, String.Empty);
        var fullname = word.Replace(" ", String.Empty);
        var shortname = String.Empty;
        foreach (var companyname in doc.companynamelist)
        {
            if (companyname.secFullName == fullname)
            {
                //注意：这里可能出现两个具有相同FullName，但是某个没有ShortName的可能性！
                if (shortname == String.Empty && !String.IsNullOrEmpty(companyname.secShortName))
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

        if (string.IsNullOrEmpty(shortname))
        {
            //字典
            shortname = CompanyNameLogic.GetCompanyNameByFullName(fullname).secShortName;
        }

        if (string.IsNullOrEmpty(shortname))
        {
            //在原文中寻找该字符名称，然后看一下，其后是否有【简称】字样，
            //简称后是否有引号字样“XXXX”有的话，差不多就是了
            shortname = GetShortNameByFullName(fullname, doc);
            if (!string.IsNullOrEmpty(shortname)) Console.WriteLine(fullname + ":" + shortname);
        }

        return (fullname, shortname);
    }

    public static string GetShortNameByFullName(String FullName, AnnouceDocument doc)
    {
        if (FullName.Length <= 4) return string.Empty; //名称或者已经是简称的场合，退出
        var quotationList = LocateProperty.LocateQuotation(doc.root, false);
        var fullnamelist = LocateProperty.LocateCustomerWord(doc.root, new string[] { FullName }.ToList());
        var jianchenglist = LocateProperty.LocateCustomerWord(doc.root, new string[] { "简称", "下称" }.ToList());

        foreach (var fn in fullnamelist)
        {
            var ql = quotationList.Where((x) =>
            {
                return x.Loc == fn.Loc && x.Description == "引号" && x.StartIdx > fn.StartIdx;
            });
            foreach (var shrotmane in ql)
            {
                foreach (var jc in jianchenglist)
                {
                    if (jc.Loc == fn.Loc && jc.StartIdx > fn.StartIdx &&
                        jc.StartIdx < shrotmane.StartIdx &&
                        (shrotmane.StartIdx - jc.StartIdx) <= 4)
                    {
                        if (shrotmane.Value.Length < FullName.Length)
                        {
                            return shrotmane.Value;
                        }
                    }
                }
            }
        }
        return string.Empty;
    }

}