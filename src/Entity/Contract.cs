using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FDDC;
using static CompanyNameLogic;
using static HTMLEngine;
using static LocateProperty;

public class Contract : AnnouceDocument
{
    public struct struContract
    {
        //公告id
        public string id;

        //甲方
        public string JiaFang;

        //乙方
        public string YiFang;

        //项目名称
        public string ProjectName;

        //合同名称
        public string ContractName;

        //合同金额上限
        public string ContractMoneyUpLimit;

        //合同金额下限
        public string ContractMoneyDownLimit;

        //联合体成员
        public string UnionMember;

        public string GetKey()
        {
            //去空格转小写
            return id + ":" + JiaFang.NormalizeKey() + ":" + YiFang.NormalizeKey();
        }
        public static struContract ConvertFromString(string str)
        {
            var Array = str.Split("\t");
            var c = new struContract();
            c.id = Array[0];
            c.JiaFang = Array[1];
            c.YiFang = Array[2];
            c.ProjectName = Array[3];
            if (Array.Length > 4)
            {
                c.ContractName = Array[4];
            }
            if (Array.Length > 6)
            {
                c.ContractMoneyUpLimit = Array[5];
                c.ContractMoneyDownLimit = Array[6];
            }
            if (Array.Length == 8)
            {
                c.UnionMember = Array[7];
            }
            return c;
        }

        public string ConvertToString(struContract contract)
        {
            var record = contract.id + "\t" +
                         contract.JiaFang + "\t" +
                         contract.YiFang + "\t" +
                         contract.ProjectName + "\t" +
                         contract.ContractName + "\t";
            record += contract.ContractMoneyUpLimit + "\t";
            record += contract.ContractMoneyDownLimit + "\t";
            record += contract.UnionMember;
            return record;
        }
    }

    static List<String> ProjectNameList = new List<String>();

    public static List<struContract> Extract(string htmlFileName)
    {
        Init(htmlFileName);
        ProjectNameList = ProjectNameLogic.GetProjectNameByCutWord(root);
        foreach (var m in ProjectNameList)
        {
            Program.Logger.WriteLine("工程名：" + m);
        }
        var ContractList = new List<struContract>();
        //主合同的抽取
        ContractList.Add(ExtractSingle(root, Id));
        return ContractList;
    }

    public static string contractType = "";


    static struContract ExtractSingle(MyRootHtmlNode root, String Id)
    {
        contractType = "";
        foreach (var paragrah in root.Children)
        {
            foreach (var item in paragrah.Children)
            {
                if (item.Content.Contains("中标"))
                {
                    contractType = "中标";
                    break;
                }
                if (item.Content.Contains("合同"))
                {
                    contractType = "合同";
                    break;
                }
            }
            if (contractType != "") break;
        }


        var contract = new struContract();
        //公告ID
        contract.id = Id;
        //甲方
        contract.JiaFang = GetJiaFang();
        contract.JiaFang = CompanyNameLogic.AfterProcessFullName(contract.JiaFang).secFullName;
        contract.JiaFang = contract.JiaFang.NormalizeTextResult();
        if (!Nerlist.Contains(contract.JiaFang))
        {
            //作为特殊单位，国家电网公司一般都是甲方
            if (Nerlist.Contains("国家电网公司")) contract.JiaFang = "国家电网公司";
        }

        //乙方
        contract.YiFang = GetYiFang();
        contract.YiFang = CompanyNameLogic.AfterProcessFullName(contract.YiFang).secFullName;
        contract.YiFang = contract.YiFang.NormalizeTextResult();
        //按照规定除去括号
        contract.YiFang = RegularTool.Trimbrackets(contract.YiFang);


        //项目
        contract.ProjectName = GetProjectName();
        if (contract.ProjectName.StartsWith("“") && contract.ProjectName.EndsWith("”"))
        {
            contract.ProjectName = contract.ProjectName.TrimStart("“".ToCharArray()).TrimEnd("”".ToCharArray());
        }
        if (contract.ProjectName.EndsWith("，签约双方"))
        {
            contract.ProjectName = Utility.GetStringAfter(contract.ProjectName, "，签约双方");
        }
        if (contract.ProjectName.Contains("（以下简称"))
        {
            contract.ProjectName = Utility.GetStringAfter(contract.ProjectName, "（以下简称");
        }
        contract.ProjectName = contract.ProjectName.NormalizeTextResult();

        //合同
        if (contractType == "中标")
        {
            //按照数据分析来看，应该工程名 在中标的时候填写，合同名在合同的时候填写
            contract.ContractName = "";
        }
        else
        {
            contract.ContractName = GetContractName();
            if (contract.ContractName.StartsWith("“") && contract.ContractName.EndsWith("”"))
            {
                contract.ContractName = contract.ContractName.TrimStart("“".ToCharArray()).TrimEnd("”".ToCharArray());
            }
            //去掉书名号
            contract.ContractName = contract.ContractName.Replace("《", "").Replace("》", "");
            if (contract.ContractName.Contains("（以下简称"))
            {
                contract.ContractName = Utility.GetStringAfter(contract.ContractName, "（以下简称");
            }
            contract.ContractName = contract.ContractName.NormalizeTextResult();
        }


        //金额
        var money = GetMoney();
        contract.ContractMoneyUpLimit = MoneyUtility.Format(money.MoneyAmount, "");
        contract.ContractMoneyDownLimit = contract.ContractMoneyUpLimit;

        //联合体
        contract.UnionMember = GetUnionMember(contract.YiFang);
        contract.UnionMember = contract.UnionMember.NormalizeTextResult();
        //按照规定除去括号
        contract.UnionMember = RegularTool.Trimbrackets(contract.UnionMember);
        return contract;
    }

    public static string TrimJianCheng(string OrgString)
    {
        if (OrgString.Contains("以下简称"))
        {
            OrgString = Utility.GetStringAfter(OrgString, "以下简称");
            if (OrgString == "")
            {
                return "";
            }
            else
            {
                return OrgString.Substring(0, OrgString.Length - 1);
            }
        }
        return OrgString;
    }

    static string GetJiaFang()
    {
        var ExtractorText = new ExtractPropertyByText();
        //这些关键字后面:注意：TEXT版本可能存在空格，所以HTML版本也检查一遍
        ExtractorText.LeadingColonKeyWordList = new string[] {
            "甲方：",
            "发包人：","发包单位：","发包方：","发包机构：","发包人名称：",
            "招标人：","招标单位：","招标方：","招标机构：","招标人名称：",
            "业主："  ,"业主单位：" ,"业主方：", "业主机构：","业主名称：",
            "采购单位：","采购单位名称：","采购人：", "采购人名称：","采购方：","采购方名称："
        };
        ExtractorText.ExtractFromTextFile(TextFileName);
        foreach (var item in ExtractorText.CandidateWord)
        {
            var JiaFang = CompanyNameLogic.AfterProcessFullName(item.Value.Trim());
            if (EntityWordAnlayzeTool.TrimEnglish(JiaFang.secFullName).Length > ContractTraning.MaxJiaFangLength) continue;
            if (JiaFang.secFullName.Length < 3) continue;     //使用实际长度排除全英文的情况
            Program.Logger.WriteLine("甲方候补词(关键字)：[" + JiaFang + "]");
            return JiaFang.secFullName;
        }

        var Extractor = new ExtractPropertyByHTML();
        Extractor.LeadingColonKeyWordList = ExtractorText.LeadingColonKeyWordList;
        Extractor.Extract(root);
        foreach (var item in ExtractorText.CandidateWord)
        {
            var JiaFang = CompanyNameLogic.AfterProcessFullName(item.Value.Trim());
            if (EntityWordAnlayzeTool.TrimEnglish(JiaFang.secFullName).Length > ContractTraning.MaxJiaFangLength) continue;
            if (JiaFang.secFullName.Length < 3) continue;     //使用实际长度排除全英文的情况
            Program.Logger.WriteLine("甲方候补词(关键字)：[" + JiaFang + "]");
            return JiaFang.secFullName;
        }

        //招标
        var CandidateWord = new List<String>();
        var StartArray = new string[] { "招标单位", "业主", "收到", "接到" };
        var EndArray = new string[] { "发来", "发出", "的中标" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            var JiaFang = CompanyNameLogic.AfterProcessFullName(item.Value.Trim());
            JiaFang.secFullName = JiaFang.secFullName.Replace("业主", "").Trim();
            JiaFang.secFullName = JiaFang.secFullName.Replace("招标单位", "").Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(JiaFang.secFullName).Length > ContractTraning.MaxJiaFangLength) continue;
            if (JiaFang.secFullName.Length < 3) continue;     //使用实际长度排除全英文的情况
            Program.Logger.WriteLine("甲方候补词(招标)：[" + JiaFang + "]");
            CandidateWord.Add(JiaFang.secFullName);
        }

        //合同
        Extractor = new ExtractPropertyByHTML();
        StartArray = new string[] { "与", "与业主" };
        EndArray = new string[] { "签署", "签订" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            var JiaFang = CompanyNameLogic.AfterProcessFullName(item.Value.Trim());
            JiaFang.secFullName = JiaFang.secFullName.Replace("业主", "").Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(JiaFang.secFullName).Length > ContractTraning.MaxJiaFangLength) continue;
            if (JiaFang.secFullName.Length < 3) continue;     //使用实际长度排除全英文的情况
            Program.Logger.WriteLine("甲方候补词(合同)：[" + JiaFang + "]");
            CandidateWord.Add(JiaFang.secFullName);
        }
        return CompanyNameLogic.MostLikeCompanyName(CandidateWord);
    }
    static string GetYiFang()
    {
        var Extractor = new ExtractPropertyByText();
        //这些关键字后面
        Extractor.LeadingColonKeyWordList = new string[] { "乙方：" };
        //"供应商名称：","中标单位：","中标人：","中标单位：","中标人：","乙方（供方）：","承包人：","承包方：","中标方：","供应商名称：","中标人名称："
        Extractor.ExtractFromTextFile(TextFileName);
        foreach (var item in Extractor.CandidateWord)
        {
            var YiFang = item.Value.Trim();
            Program.Logger.WriteLine("乙方候补词(关键字)：[" + YiFang + "]");
            return YiFang;
        }

        //乙方:"有限公司"
        //如果有子公司的话，优先使用子公司
        foreach (var c in companynamelist)
        {
            if (c.isSubCompany) return c.secFullName;
        }

        var ExtractorHTML = new ExtractPropertyByHTML();
        //这些关键字后面
        ExtractorHTML.TrailingWordList = new string[] { "有限公司董事会" };
        ExtractorHTML.Extract(root);
        ExtractorHTML.CandidateWord.Reverse();
        foreach (var item in ExtractorHTML.CandidateWord)
        {
            Program.Logger.WriteLine("乙方候补词(关键字)：[" + item.Value.Trim() + "有限公司]");
            return item.Value.Trim() + "有限公司";
        }
        return AnnouceCompanyName;
    }
    static string GetContractName()
    {

        var Extractor = new ExtractPropertyByText();
        //这些关键字后面(最优先)
        Extractor.LeadingColonKeyWordList = new string[] { "合同名称：" };
        Extractor.ExtractTextByColonKeyWord(TextFileName);
        foreach (var item in Extractor.CandidateWord)
        {
            var ContractName = item.Value.Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(ContractName).Length > ContractTraning.MaxContractNameLength) continue;
            if (TrimJianCheng(ContractName) == "") continue;
            Program.Logger.WriteLine("合同候补词(合同)：[" + ContractName + "]");
            return ContractName;
        }

        //合同
        var ExtractorHTML = new ExtractPropertyByHTML();
        ExtractorHTML.LeadingColonKeyWordList = Extractor.LeadingColonKeyWordList;
        ExtractorHTML.Extract(root);
        foreach (var item in ExtractorHTML.CandidateWord)
        {
            var ContractName = item.Value.Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(ContractName).Length > ContractTraning.MaxContractNameLength) continue;
            if (TrimJianCheng(ContractName) == "") continue;
            Program.Logger.WriteLine("合同候补词(合同)：[" + ContractName + "]");
            return ContractName;
        }


        //《》，“”里面的内容
        foreach (var bracket in bracketlist)
        {
            if (bracket.Value.EndsWith("合同") ||
                bracket.Value.EndsWith("确认书") ||
                bracket.Value.EndsWith("协议") ||
                bracket.Value.EndsWith("协议书"))
            {
                Program.Logger.WriteLine("合同候补词(合同)：[" + bracket.Value + "]");
                return bracket.Value;
            }
        }

        var ExtractDP = new ExtractPropertyByDP();
        var KeyList = new List<ExtractPropertyByDP.DPKeyWord>();
        KeyList.Add(new ExtractPropertyByDP.DPKeyWord()
        {
            StartWord = "签署",
            StartDPValue = new string[] { LTP.核心关系 },
            EndWord = "合同",
            EndDPValue = new string[] { LTP.动宾关系 }
        });
        KeyList.Add(new ExtractPropertyByDP.DPKeyWord()
        {
            StartWord = "签订",
            StartDPValue = new string[] { LTP.核心关系 },
            EndWord = "合同",
            EndDPValue = new string[] { LTP.动宾关系 }
        });
        ExtractDP.StartWithKey(KeyList, Dplist);
        foreach (var item in ExtractDP.CandidateWord)
        {
            var ContractName = item.Value.Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(ContractName).Length > ContractTraning.MaxContractNameLength) continue;
            if (ContractName.Length <= 4) continue;
            Program.Logger.WriteLine("合同候补词(合同)：[" + item + "]");
            return ContractName;
        }


        //一部分无法提取TEXT的情况
        var StartArray = new string[] { "签署了" };
        var EndArray = new string[] { "合同" };
        ExtractorHTML.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        ExtractorHTML.Extract(root);
        foreach (var item in ExtractorHTML.CandidateWord)
        {
            var ContractName = item.Value.Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(ContractName).Length > ContractTraning.MaxContractNameLength) continue;
            if (ContractName.Length <= 4) continue;
            Program.Logger.WriteLine("合同候补词(合同)：[" + item + "]");
            return ContractName;
        }

        return "";
    }

    static string GetProjectName()
    {
        var ExtractorText = new ExtractPropertyByText();
        //这些关键字后面(最优先)
        ExtractorText.LeadingColonKeyWordList = new string[] { "项目名称：", "工程名称：", "中标项目：", "合同标的：", "工程内容：" };
        ExtractorText.ExtractFromTextFile(TextFileName);
        foreach (var item in ExtractorText.CandidateWord)
        {
            var ProjectName = item.Value.Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(ProjectName).Length > ContractTraning.MaxContractNameLength) continue;
            if (TrimJianCheng(ProjectName) == "") continue;
            Program.Logger.WriteLine("项目名称候补词(关键字)：[" + item + "]");
            return ProjectName;
        }

        var Extractor = new ExtractPropertyByHTML();
        Extractor.LeadingColonKeyWordList = ExtractorText.LeadingColonKeyWordList;
        foreach (var item in Extractor.CandidateWord)
        {
            var ProjectName = item.Value.Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(ProjectName).Length > ContractTraning.MaxContractNameLength) continue;
            if (TrimJianCheng(ProjectName) == "") continue;
            Program.Logger.WriteLine("项目名称候补词(关键字)：[" + item + "]");
            return ProjectName;
        }

        var ExtractDP = new ExtractPropertyByDP();
        var KeyList = new List<ExtractPropertyByDP.DPKeyWord>();
        KeyList.Add(new ExtractPropertyByDP.DPKeyWord()
        {
            StartWord = "为",
            StartDPValue = new string[] { LTP.定中关系, LTP.状中结构 },
            EndWord = "标段",
            EndDPValue = new string[] { LTP.介宾关系, LTP.动宾关系, LTP.间宾关系 }
        });
        KeyList.Add(new ExtractPropertyByDP.DPKeyWord()
        {
            StartWord = "为",
            StartDPValue = new string[] { LTP.定中关系, LTP.状中结构 },
            EndWord = "标",
            EndDPValue = new string[] { LTP.介宾关系, LTP.动宾关系, LTP.间宾关系 }
        });
        KeyList.Add(new ExtractPropertyByDP.DPKeyWord()
        {
            StartWord = "中标",
            StartDPValue = new string[] { LTP.核心关系 },
            EndWord = "工程",
            EndDPValue = new string[] { LTP.介宾关系, LTP.动宾关系, LTP.间宾关系 }
        });
        KeyList.Add(new ExtractPropertyByDP.DPKeyWord()
        {
            StartWord = "中标",
            StartDPValue = new string[] { LTP.核心关系 },
            EndWord = "项目",
            EndDPValue = new string[] { LTP.介宾关系, LTP.动宾关系, LTP.间宾关系 }
        });
        ExtractDP.StartWithKey(KeyList, Dplist);
        foreach (var item in ExtractDP.CandidateWord)
        {
            var ProjectName = item.Value.Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(ProjectName).Length > ContractTraning.MaxProjectNameLength) continue;
            if (ProjectName.Length <= 4) continue;
            Program.Logger.WriteLine("工程候补词：[" + item + "]");
            return ProjectName;
        }




        foreach (var bracket in bracketlist)
        {
            if (bracket.Value.EndsWith("工程") ||
                bracket.Value.EndsWith("标段"))
            {
                return bracket.Value;
            }
        }

        var MarkFeature = new ExtractPropertyByHTML.struMarkFeature();
        MarkFeature.MarkStartWith = "“";
        MarkFeature.MarkEndWith = "”";
        MarkFeature.InnerEndWith = "标段";

        var MarkFeatureConfirm = new ExtractPropertyByHTML.struMarkFeature();
        MarkFeatureConfirm.MarkStartWith = "“";
        MarkFeatureConfirm.MarkEndWith = "”";
        MarkFeatureConfirm.InnerEndWith = "标";

        Extractor.MarkFeature = new ExtractPropertyByHTML.struMarkFeature[] { MarkFeature, MarkFeatureConfirm };
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            var ProjectName = item.Value.Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(ProjectName).Length > ContractTraning.MaxContractNameLength) continue;
            Program.Logger.WriteLine("工程名称候补词（《XXX》）：[" + item + "]");
            return ProjectName;
        }

        var list = ProjectNameLogic.GetProjectName(root);
        if (list.Count > 0)
        {
            return list[0];
        }
        return "";
    }

    static (String MoneyAmount, String MoneyCurrency) GetMoney()
    {
        var Extractor = new ExtractPropertyByHTML();
        //这些关键字后面
        Extractor.LeadingColonKeyWordList = new string[] {
            "订单总金额","订单金额","订单总价","订单额",
            "合同总投资", "合同总价","合同金额", "合同额","合同总额","合同总金额","合同价","合同价格",
            "中标业务总额","中标总金额", "中标金额", "中标价","中标总价",
            "项目总价","项目总投资","项目估算总投资", "项目投资额","项目投资估算","项目预计总投资",
            "工程总价","工程总投资","工程估算总投资", "工程投资额","工程投资估算","工程预计总投资",
            "投标价格","投标金额","投标额","投标总金额","投标报价","预算金额"
        };
        Extractor.Extract(root);
        var AllMoneyList = new List<(String MoneyAmount, String MoneyCurrency)>();
        foreach (var item in Extractor.CandidateWord)
        {
            var moneylist = MoneyUtility.SeekMoney(item.Value);
            AllMoneyList.AddRange(moneylist);
        }
        if (AllMoneyList.Count == 0) return ("", "");
        foreach (var money in AllMoneyList)
        {
            if (money.MoneyCurrency == "人民币" ||
                money.MoneyCurrency == "元")
            {
                var amount = MoneyUtility.Format(money.MoneyAmount, "");
                var m = 0.0;
                if (double.TryParse(amount, out m))
                {
                    if (m >= ContractTraning.MinAmount)
                    {
                        Program.Logger.WriteLine("金额候补词：[" + money.MoneyAmount + ":" + money.MoneyCurrency + "]");
                        return money;
                    }
                }
            }
        }
        Program.Logger.WriteLine("金额候补词：[" + AllMoneyList[0].MoneyAmount + ":" + AllMoneyList[0].MoneyCurrency + "]");
        return AllMoneyList[0];
    }

    static string GetUnionMember(String YiFang)
    {
        var paragrahlist = ExtractPropertyByHTML.FindWordCnt("联合体", root);
        var Union = new List<String>();
        foreach (var paragrahId in paragrahlist)
        {
            foreach (var comp in companynamelist)
            {
                if (comp.positionId == paragrahId)
                {
                    if (!Union.Contains(comp.secFullName))
                    {
                        if (!comp.secFullName.Equals(YiFang))
                        {
                            Union.Add(comp.secFullName);
                        }
                    }
                }
            }
        }
        return String.Join("、", Union);
    }

}