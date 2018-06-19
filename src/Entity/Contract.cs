using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FDDC;
using static BussinessLogic;
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
            var record = contract.id + "," +
                         contract.JiaFang + "," +
                         contract.YiFang + "," +
                         contract.ProjectName + "," +
                         contract.ContractName + ",";
            record += contract.ContractMoneyUpLimit + ",";
            record += contract.ContractMoneyDownLimit + ",";
            record += contract.UnionMember;
            return record;
        }
    }

    public static List<struContract> Extract(string htmlFileName)
    {
        Init(htmlFileName);
        var ContractList = new List<struContract>();
        //主合同的抽取
        ContractList.Add(ExtractSingle(root, Id));
        return ContractList;
    }


    static struContract ExtractSingle(MyRootHtmlNode root, String Id)
    {
        var contract = new struContract();
        //公告ID
        contract.id = Id;
        //甲方
        contract.JiaFang = GetJiaFang(root);
        contract.JiaFang = AfterProcessJiaFang(contract.JiaFang);
        contract.JiaFang = contract.JiaFang.NormalizeTextResult();

        //乙方
        contract.YiFang = GetYiFang(root);
        //暂时不做括号的正规化
        foreach (var trail in StockChange.CompanyNameTrailingwords)
        {
            if (contract.YiFang.Contains(trail))
            {
                contract.YiFang = Utility.GetStringBefore(contract.YiFang, trail);
            }
        }
        contract.YiFang = contract.YiFang.NormalizeTextResult();

        //合同
        contract.ContractName = GetContractName(root);
        contract.ContractName = contract.ContractName.NormalizeTextResult();

        //项目
        contract.ProjectName = GetProjectName(root);
        if (contract.ProjectName == "" && contract.ContractName.EndsWith("项目合同"))
        {
            contract.ProjectName = contract.ContractName.Substring(0, contract.ContractName.Length - 2);
        }
        contract.ProjectName = contract.ProjectName.NormalizeTextResult();


        //金额
        var money = GetMoney(root);
        contract.ContractMoneyUpLimit = MoneyUtility.Format(money.MoneyAmount, "");
        contract.ContractMoneyDownLimit = contract.ContractMoneyUpLimit;

        //联合体
        contract.UnionMember = GetUnionMember(root, contract.YiFang);
        return contract;
    }

    private static String AfterProcessJiaFang(string JiaFang)
    {
        //暂时不做括号的正规化
        foreach (var trailing in StockChange.CompanyNameTrailingwords)
        {
            if (JiaFang.Contains(trailing))
            {
                JiaFang = Utility.GetStringBefore(JiaFang, trailing);
            }
        }

        var st = JiaFang.IndexOf("（");
        var ed = JiaFang.IndexOf("）");
        if (ed < st) return JiaFang;
        if (st != -1 && ed != -1)
        {
            var InMarkString = JiaFang.Substring(st, ed - st + 1);
            if (InMarkString.Contains("简称"))
            {
                JiaFang = JiaFang.Substring(0, st) + JiaFang.Substring(ed + 1);
            }
        }

        //删除前导
        JiaFang = EntityWordAnlayzeTool.TrimLeadingUL(JiaFang);
        return JiaFang;
    }

    //处理两行并作一行的问题
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
        }
        return OrgString;
    }

    static string GetJiaFang(MyRootHtmlNode root)
    {
        var Extractor = new ExtractProperty();
        //这些关键字后面
        Extractor.LeadingWordList = new string[] {
            "甲方：",
            "发包人：","发包单位：","发包方：","发包机构：","发包人名称：",
            "招标人：","招标单位：","招标方：","招标机构：","招标人名称：",
            "业主："  ,"业主单位：" ,"业主方：", "业主机构：","业主名称：",
            "采购单位：","采购人：", "采购人名称：","采购方："
        };
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            var JiaFang = AfterProcessJiaFang(item.Value.Trim());
            if (EntityWordAnlayzeTool.TrimEnglish(JiaFang).Length > ContractTraning.MaxJiaFangLength) continue;
            if (JiaFang.Length < 3) continue;     //使用实际长度排除全英文的情况
            JiaFang = CutOtherLeadingWords(JiaFang);
            Program.Logger.WriteLine("甲方候补词(关键字)：[" + JiaFang + "]");
            return JiaFang;
        }

        //招标
        Extractor = new ExtractProperty();
        var StartArray = new string[] { "招标单位", "业主", "收到", "接到" };
        var EndArray = new string[] { "发来", "发出", "的中标" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            var JiaFang = AfterProcessJiaFang(item.Value.Trim());
            JiaFang = JiaFang.Replace("业主", "").Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(JiaFang).Length > ContractTraning.MaxJiaFangLength) continue;
            if (JiaFang.Length < 3) continue;     //使用实际长度排除全英文的情况
            Program.Logger.WriteLine("甲方候补词(招标)：[" + JiaFang + "]");
            return JiaFang;
        }

        //合同
        Extractor = new ExtractProperty();
        StartArray = new string[] { "与", "与业主" };
        EndArray = new string[] { "签署", "签订" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            var JiaFang = AfterProcessJiaFang(item.Value.Trim());
            JiaFang = JiaFang.Replace("业主", "").Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(JiaFang).Length > ContractTraning.MaxJiaFangLength) continue;
            if (JiaFang.Length < 3) continue;     //使用实际长度排除全英文的情况
            Program.Logger.WriteLine("甲方候补词(合同)：[" + JiaFang + "]");
            return JiaFang;
        }
        return "";
    }
    static string GetYiFang(HTMLEngine.MyRootHtmlNode root)
    {
        var Extractor = new ExtractProperty();
        //这些关键字后面
        Extractor.LeadingWordList = new string[] { "供应商名称：", "乙方：" };
        //"中标单位：","中标人：","中标单位：","中标人：","乙方（供方）：","承包人：","承包方：","中标方：","供应商名称：","中标人名称："
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            var YiFang = item.Value.Trim();
            YiFang = CutOtherLeadingWords(YiFang);
            Program.Logger.WriteLine("乙方候补词(关键字)：[" + item + "]");
            return YiFang;
        }

        //乙方:"有限公司"
        Extractor = new ExtractProperty();
        //这些关键字后面
        Extractor.TrailingWordList = new string[] { "有限公司董事会" };
        Extractor.Extract(root);
        Extractor.CandidateWord.Reverse();
        foreach (var item in Extractor.CandidateWord)
        {
            //如果有子公司的话，优先使用子公司
            foreach (var c in companynamelist)
            {
                if (c.isSubCompany) return c.secFullName;
            }
            Program.Logger.WriteLine("乙方候补词(关键字)：[" + item + "有限公司]");
            return item.Value.Trim() + "有限公司";
        }

        if (companynamelist.Count > 0)
        {
            return companynamelist[companynamelist.Count - 1].secFullName;
        }
        return "";
    }
    static string GetContractName(MyRootHtmlNode root)
    {
        var Extractor = new ExtractProperty();
        var MarkFeature = new ExtractProperty.struMarkFeature();
        MarkFeature.MarkStartWith = "《";
        MarkFeature.MarkEndWith = "》";
        MarkFeature.InnerEndWith = "合同";

        var MarkFeatureConfirm = new ExtractProperty.struMarkFeature();
        MarkFeatureConfirm.MarkStartWith = "《";
        MarkFeatureConfirm.MarkEndWith = "》";
        MarkFeatureConfirm.InnerEndWith = "确认书";


        Extractor.MarkFeature = new ExtractProperty.struMarkFeature[] { MarkFeature, MarkFeatureConfirm };
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            var ContractName = item.Value.Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(ContractName).Length > ContractTraning.MaxContractNameLength) continue;
            Program.Logger.WriteLine("合同名称候补词（《XXX》）：[" + item + "]");
            return ContractName;
        }

        Extractor = new ExtractProperty();
        //这些关键字后面
        Extractor.LeadingWordList = new string[] { "合同名称：" };
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            var ContractName = item.Value.Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(ContractName).Length > ContractTraning.MaxContractNameLength) continue;
            Program.Logger.WriteLine("合同名称候补词(关键字)：[" + item + "]");
            return ContractName;
        }

        //合同
        Extractor = new ExtractProperty();
        var StartArray = new string[] { "签署了" };
        var EndArray = new string[] { "合同" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            var ContractName = item.Value.Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(ContractName).Length > ContractTraning.MaxContractNameLength) continue;
            Program.Logger.WriteLine("合同候补词(合同)：[" + item + "]");
            return ContractName;
        }
        return "";
    }

    static string GetProjectName(MyRootHtmlNode root)
    {
        var Extractor = new ExtractProperty();
        //这些关键字后面
        Extractor.LeadingWordList = new string[] { "项目名称：", "工程名称：", "中标项目：", "合同标的：", "工程内容：" };
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            var ProjectName = item.Value.Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(ProjectName).Length > ContractTraning.MaxContractNameLength) continue;
            Program.Logger.WriteLine("项目名称候补词(关键字)：[" + item + "]");
            return ProjectName;
        }

        var MarkFeature = new ExtractProperty.struMarkFeature();
        MarkFeature.MarkStartWith = "“";
        MarkFeature.MarkEndWith = "”";
        MarkFeature.InnerEndWith = "标段";

        var MarkFeatureConfirm = new ExtractProperty.struMarkFeature();
        MarkFeatureConfirm.MarkStartWith = "“";
        MarkFeatureConfirm.MarkEndWith = "”";
        MarkFeatureConfirm.InnerEndWith = "标";

        Extractor.MarkFeature = new ExtractProperty.struMarkFeature[] { MarkFeature, MarkFeatureConfirm };
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            var ProjectName = item.Value.Trim();
            if (EntityWordAnlayzeTool.TrimEnglish(ProjectName).Length > ContractTraning.MaxContractNameLength) continue;
            Program.Logger.WriteLine("工程名称候补词（《XXX》）：[" + item + "]");
            return ProjectName;
        }

        var list = BussinessLogic.GetProjectName(root);
        if (list.Count > 0)
        {
            return list[0];
        }
        return "";
    }

    static (String MoneyAmount, String MoneyCurrency) GetMoney(HTMLEngine.MyRootHtmlNode root)
    {
        var Extractor = new ExtractProperty();
        //这些关键字后面
        Extractor.LeadingWordList = new string[] { "中标金额", "中标价", "合同金额", "合同总价", "订单总金额" };
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
                Program.Logger.WriteLine("金额候补词：[" + money.MoneyAmount + ":" + money.MoneyCurrency + "]");
                return money;
            }
        }
        Program.Logger.WriteLine("金额候补词：[" + AllMoneyList[0].MoneyAmount + ":" + AllMoneyList[0].MoneyCurrency + "]");
        return AllMoneyList[0];
    }

    static string GetUnionMember(HTMLEngine.MyRootHtmlNode root, String YiFang)
    {
        var paragrahlist = ExtractProperty.FindWordCnt("联合体", root);
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