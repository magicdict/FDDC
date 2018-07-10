using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FDDC;
using static CompanyNameLogic;
using static HTMLEngine;
using static LocateProperty;

public partial class Contract : AnnouceDocument
{
    public Contract(string htmlFileName) : base(htmlFileName)
    {

    }
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


    List<String> ProjectNameList = new List<String>();

    public List<struContract> Extract()
    {
        ProjectNameList = ProjectNameLogic.GetProjectNameByCutWord(root);
        foreach (var m in ProjectNameList)
        {
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("工程名：" + m);
        }
        var ContractList = new List<struContract>();
        //主合同的抽取
        ContractList.Add(ExtractSingle(root, Id));
        return ContractList;
    }

    public string contractType = String.Empty;

    struContract ExtractSingle(MyRootHtmlNode root, String Id)
    {
        contractType = String.Empty;
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
            if (contractType != String.Empty) break;
        }

        if (contractType == String.Empty)
        {
            Console.WriteLine("contractType Null:" + Id);
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
        contract.YiFang = RegularTool.TrimBrackets(contract.YiFang);


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
            contract.ContractName = String.Empty;
        }
        else
        {
            contract.ContractName = GetContractName();
            if (contract.ContractName.StartsWith("“") && contract.ContractName.EndsWith("”"))
            {
                contract.ContractName = contract.ContractName.TrimStart("“".ToCharArray()).TrimEnd("”".ToCharArray());
            }
            //去掉书名号
            contract.ContractName = contract.ContractName.Replace("《", String.Empty).Replace("》", String.Empty);
            if (contract.ContractName.Contains("（以下简称"))
            {
                contract.ContractName = Utility.GetStringAfter(contract.ContractName, "（以下简称");
            }
            contract.ContractName = contract.ContractName.NormalizeTextResult();
        }


        //金额
        var money = GetMoney();
        contract.ContractMoneyUpLimit = MoneyUtility.Format(money.MoneyAmount, String.Empty);
        contract.ContractMoneyDownLimit = contract.ContractMoneyUpLimit;

        //联合体
        contract.UnionMember = GetUnionMember(contract.JiaFang, contract.YiFang);
        contract.UnionMember = contract.UnionMember.NormalizeTextResult();
        //按照规定除去括号
        contract.UnionMember = RegularTool.TrimBrackets(contract.UnionMember);
        return contract;
    }

    public string TrimJianCheng(string OrgString)
    {
        if (OrgString.Contains("（以下简称"))
        {
            OrgString = Utility.GetStringBefore(OrgString, "（以下简称");
        }
        return OrgString;
    }
    /// <summary>
    /// 获得甲方
    /// </summary>
    /// <returns></returns>
    public string GetJiaFang()
    {
        //最高置信度的抽取
        EntityProperty e = new EntityProperty();
        e.ExcludeContainsWordList = new string[] { "招标代理" };
        e.LeadingColonKeyWordList = new string[] {
            "甲方：","合同买方：",
            "发包人：","发包单位：","发包方：","发包机构：","发包人名称：",
            "招标人：","招标单位：","招标方：","招标机构：","招标人名称：",
            "业主："  ,"业主单位：" ,"业主方：", "业主机构：","业主名称：",
            "采购单位：","采购单位名称：","采购人：", "采购人名称：","采购方：","采购方名称："
        };
        e.CandidatePreprocess = (x =>
        {
            x = Normalizer.ClearTrailing(x);
            return CompanyNameLogic.AfterProcessFullName(x).secFullName;
        });
        e.MaxLength = ContractTraning.MaxJiaFangLength;
        e.MaxLengthCheckPreprocess = EntityWordAnlayzeTool.TrimEnglish;
        e.MinLength = 3;
        e.Extract(this);

        //这里不直接做Distinct，出现频次越高，则可信度越高
        //多个甲方的时候，可能意味着没有甲方！
        if (e.LeadingColonKeyWordCandidate.Distinct().Count() > 1)
        {
            foreach (var candidate in e.LeadingColonKeyWordCandidate)
            {
                Program.Logger.WriteLine("发现多个甲方：" + candidate);
            }
        }
        if (e.LeadingColonKeyWordCandidate.Count > 0) return e.LeadingColonKeyWordCandidate[0];


        //招标
        var Extractor = new ExtractPropertyByHTML();
        var CandidateWord = new List<String>();
        var StartArray = new string[] { "招标单位", "业主", "收到", "接到" };
        var EndArray = new string[] { "发来", "发出", "的中标" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            var JiaFang = CompanyNameLogic.AfterProcessFullName(item.Value.Trim());
            if (JiaFang.secFullName.Contains("招标代理")) continue; //特殊业务规则
            JiaFang.secFullName = JiaFang.secFullName.Replace("业主", String.Empty).Trim();
            JiaFang.secFullName = JiaFang.secFullName.Replace("招标单位", String.Empty).Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(JiaFang.secFullName).Length > ContractTraning.MaxJiaFangLength) continue;
            if (JiaFang.secFullName.Length < 3) continue;     //使用实际长度排除全英文的情况
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("甲方候补词(招标)：[" + JiaFang.secFullName + "]");
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
            JiaFang.secFullName = JiaFang.secFullName.Replace("业主", String.Empty).Trim();
            if (JiaFang.secFullName.Contains("招标代理")) continue; //特殊业务规则
            if (EntityWordAnlayzeTool.TrimEnglish(JiaFang.secFullName).Length > ContractTraning.MaxJiaFangLength) continue;
            if (JiaFang.secFullName.Length < 3) continue;     //使用实际长度排除全英文的情况
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("甲方候补词(合同)：[" + JiaFang.secFullName + "]");
            CandidateWord.Add(JiaFang.secFullName);
        }
        return CompanyNameLogic.MostLikeCompanyName(CandidateWord);
    }
    /// <summary>
    /// 获得乙方
    /// </summary>
    /// <returns></returns>
    string GetYiFang()
    {
        var Extractor = new ExtractPropertyByText();
        //这些关键字后面
        Extractor.LeadingColonKeyWordList = new string[] { "乙方：" };
        //"供应商名称：","中标单位：","中标人：","中标单位：","中标人：","乙方（供方）：","承包人：","承包方：","中标方：","供应商名称：","中标人名称："
        Extractor.ExtractFromTextFile(TextFileName);
        foreach (var item in Extractor.CandidateWord)
        {
            var YiFang = item.Value.Trim();
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("乙方候补词(关键字)：[" + YiFang + "]");
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
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("乙方候补词(关键字)：[" + item.Value.Trim() + "有限公司]");
            return item.Value.Trim() + "有限公司";
        }
        return AnnouceCompanyName;
    }
    /// <summary>
    /// 获得合同名
    /// </summary>
    /// <returns></returns>
    string GetContractName()
    {
        var e = new EntityProperty();
        e.PropertyType = EntityProperty.enmType.Normal;
        e.PropertyName = "合同名称";
        e.PropertyType = EntityProperty.enmType.Normal;
        e.MaxLength = ContractTraning.MaxContractNameLength;
        e.MinLength = 5;
        e.LeadingColonKeyWordList = new string[] { "合同名称：" };
        e.QuotationTrailingWordList = new string[] { "协议书", "合同书", "确认书", "合同", "协议" };
        e.QuotationTrailingWordList_IsSkipBracket = true;   //暂时只能选True
        var KeyList = new List<ExtractPropertyByDP.DPKeyWord>();
        KeyList.Add(new ExtractPropertyByDP.DPKeyWord()
        {
            StartWord = new string[] { "签署", "签订" },    //通过SRL训练获得
            StartDPValue = new string[] { LTPTrainingDP.核心关系, LTPTrainingDP.定中关系, LTPTrainingDP.并列关系 },
            EndWord = new string[] { "补充协议", "合同书", "合同", "协议书", "协议", },
            EndDPValue = new string[] { LTPTrainingDP.核心关系, LTPTrainingDP.定中关系, LTPTrainingDP.并列关系, LTPTrainingDP.动宾关系, LTPTrainingDP.主谓关系 }
        });
        e.DpKeyWordList = KeyList;

        var StartArray = new string[] { "签署了", "签订了" };
        var EndArray = new string[] { "合同" };
        e.ExternalStartEndStringFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        e.ExternalStartEndStringFeatureCandidatePreprocess = (x) => { return x + "合同"; };
        e.MaxLengthCheckPreprocess = str =>
        {
            return EntityWordAnlayzeTool.TrimEnglish(str);
        };
        //最高级别的置信度，特殊处理器
        e.LeadingColonKeyWordCandidatePreprocess = str =>
        {
            var c = Normalizer.ClearTrailing(TrimJianCheng(str));
            return c;
        };

        e.CandidatePreprocess = str =>
        {
            var c = Normalizer.ClearTrailing(TrimJianCheng(str));
            var RightQMarkIdx = c.IndexOf("”");
            if (!(RightQMarkIdx != -1 && RightQMarkIdx != c.Length - 1))
            {
                //对于"XXX"合同，有右边引号，但不是最后的时候，不用做
                c = c.TrimStart("“".ToCharArray());
            }
            c = c.TrimStart("《".ToCharArray());
            c = c.TrimEnd("》".ToCharArray()).TrimEnd("”".ToCharArray());
            return c;
        };
        e.ExcludeContainsWordList = new string[] { "日常经营重大合同" };
        //下面这个列表的根据不足
        e.ExcludeEqualsWordList = new string[] { "合同", "重大合同", "项目合同", "终止协议", "经营合同", "特别重大合同", "相关项目合同" };
        e.Extract(this);

        //是否所有的候选词里面包括（测试集无法使用）
        var contractlist = TraningDataset.ContractList.Where((x) => { return x.id == this.Id; });
        if (contractlist.Count() > 0)
        {
            var contract = contractlist.First();
            var contractname = contract.ContractName;
            if (!String.IsNullOrEmpty(contractname))
            {
                e.CheckIsCandidateContainsTarget(contractname);
            }
        }
        //置信度
        e.Confidence = ContractTraning.ContractS.GetStardardCI();
        return e.EvaluateCI();
    }
    /// <summary>
    /// 获得工程名
    /// </summary>
    /// <returns></returns>
    string GetProjectName()
    {
        var ExtractorText = new ExtractPropertyByText();
        //这些关键字后面(最优先)
        ExtractorText.LeadingColonKeyWordList = new string[] { "项目名称：", "工程名称：", "中标项目：", "合同标的：", "工程内容：" };
        ExtractorText.ExtractFromTextFile(TextFileName);
        foreach (var item in ExtractorText.CandidateWord)
        {
            var ProjectName = item.Value.Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(ProjectName).Length > ContractTraning.MaxContractNameLength) continue;
            if (TrimJianCheng(ProjectName) == String.Empty) continue;
            ProjectName = TrimJianCheng(ProjectName);
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("项目名称候补词(关键字)：[" + ProjectName + "]");
            return ProjectName;
        }

        var Extractor = new ExtractPropertyByHTML();
        Extractor.LeadingColonKeyWordList = ExtractorText.LeadingColonKeyWordList;
        foreach (var item in Extractor.CandidateWord)
        {
            var ProjectName = item.Value.Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(ProjectName).Length > ContractTraning.MaxContractNameLength) continue;
            if (TrimJianCheng(ProjectName) == String.Empty) continue;
            ProjectName = TrimJianCheng(ProjectName);
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("项目名称候补词(关键字)：[" + ProjectName + "]");
            return ProjectName;
        }

        foreach (var bracket in quotationList)
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
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("工程名称候补词（《XXX》）：[" + item + "]");
            return ProjectName;
        }

        var ExtractDP = new ExtractPropertyByDP();
        var KeyList = new List<ExtractPropertyByDP.DPKeyWord>();
        KeyList.Add(new ExtractPropertyByDP.DPKeyWord()
        {
            StartWord = new string[] { "确定为", "确定", "中标", "参与", "发布", "为" },
            StartDPValue = new string[] { LTPTrainingDP.核心关系, LTPTrainingDP.定中关系, LTPTrainingDP.并列关系 },
            EndWord = new string[] { "采购", "项目", "工程", "标段" },
            EndDPValue = new string[] { }
        });
        ExtractDP.StartWithKey(KeyList, Dplist);
        foreach (var item in ExtractDP.CandidateWord)
        {
            var ProjectName = item.Value.Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(ProjectName).Length > ContractTraning.MaxProjectNameLength) continue;
            if (ProjectName.Length <= 4) continue;
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("工程候补词：[" + ProjectName + "]");
            return ProjectName;
        }

        return String.Empty;
    }
    /// <summary>
    /// 获得金额
    /// </summary>
    /// <returns></returns>
    (String MoneyAmount, String MoneyCurrency) GetMoney()
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
        if (AllMoneyList.Count == 0) return (String.Empty, String.Empty);
        foreach (var money in AllMoneyList)
        {
            if (money.MoneyCurrency == "人民币" ||
                money.MoneyCurrency == "元")
            {
                var amount = MoneyUtility.Format(money.MoneyAmount, String.Empty);
                var m = 0.0;
                if (double.TryParse(amount, out m))
                {
                    if (m >= ContractTraning.MinAmount)
                    {
                        if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("金额候补词：[" + money.MoneyAmount + ":" + money.MoneyCurrency + "]");
                        return money;
                    }
                }
            }
        }
        if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("金额候补词：[" + AllMoneyList[0].MoneyAmount + ":" + AllMoneyList[0].MoneyCurrency + "]");
        return AllMoneyList[0];
    }

    /// <summary>
    /// 获得联合体
    /// </summary>
    /// <param name="JiaFang">甲方</param>
    /// <param name="YiFang">乙方</param>
    /// <returns></returns>
    string GetUnionMember(String JiaFang, String YiFang)
    {
        var Extractor = new ExtractPropertyByText();
        Extractor.LeadingColonKeyWordListInChineseBrackets = new string[] { "联合体成员：" };
        Extractor.ExtractFromTextFile(this.TextFileName);
        foreach (var union in Extractor.CandidateWord)
        {
            return union.Value;
        }
        var ExtractDP = new ExtractPropertyByDP();
        var KeyList = new List<ExtractPropertyByDP.DPKeyWord>();
        KeyList.Add(new ExtractPropertyByDP.DPKeyWord()
        {
            StartWord = new string[] { "与", },
            StartDPValue = new string[] { LTPTrainingDP.核心关系, LTPTrainingDP.定中关系, LTPTrainingDP.并列关系 },
            EndWord = new string[] { "联合体" },
            EndDPValue = new string[] { }
        });
        ExtractDP.StartWithKey(KeyList, Dplist);
        foreach (var union in ExtractDP.CandidateWord)
        {
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("联合体候补词：[" + union + "]");
            return union.Value;
        }
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
                        if (!comp.secFullName.Equals(YiFang) && !comp.secFullName.Equals(JiaFang))
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