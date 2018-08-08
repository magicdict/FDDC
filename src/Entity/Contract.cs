using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FDDC;
using static CompanyNameLogic;
using static HTMLEngine;
using static HTMLTable;
using static LocateProperty;

public partial class Contract : AnnouceDocument
{
    public override List<RecordBase> Extract()
    {
        var ContractList = ExtractMulti();
        if (ContractList.Count != 0) return ContractList;
        var SingleItem = ExtractSingle();
        if (!String.IsNullOrEmpty(SingleItem.JiaFang) || !String.IsNullOrEmpty(SingleItem.YiFang))
        {
            ContractList.Add(SingleItem);
        }
        return ContractList;
    }

    public string contractType = String.Empty;

    /// <summary>
    /// 多条记录的抽取
    /// </summary>
    /// <param name="root"></param>
    /// <param name="Id"></param>
    /// <returns></returns>
    List<RecordBase> ExtractMulti()
    {
        var Records = new List<RecordBase>();
        Records = ExtractMultiFromTable();
        if (Records.Count != 0) return Records;
        Records = ExtractMultiFromNorthVehicle();
        if (Records.Count != 0) return Records;
        Records = ExtractMultiCommon();
        Records.Clear(); //无法判断是否需要这些数据
        return Records;
    }

    List<RecordBase> ExtractMultiCommon()
    {
        var MainRec = ExtractSingle();
        //三项订单
        //中标通知书6份
        //中标通知书四份
        //履行进展情况
        var Records = new List<RecordBase>();
        var isMulti = false;
        foreach (var p in root.Children)
        {
            foreach (var s in p.Children)
            {

                if (isMulti)
                {
                    if (nermap.ParagraghlocateDict.ContainsKey(s.PositionId))
                    {
                        var nerlist = nermap.ParagraghlocateDict[s.PositionId];
                        if (nerlist.moneylist.Count == 1)
                        {
                            var ContractRec = new ContractRec();
                            ContractRec.Id = Id;
                            ContractRec.JiaFang = MainRec.JiaFang;
                            ContractRec.YiFang = MainRec.YiFang;
                            ContractRec.ContractMoneyUpLimit = MoneyUtility.Format(nerlist.moneylist.First().Value.MoneyAmount, String.Empty);
                            ContractRec.ContractMoneyDownLimit = ContractRec.ContractMoneyUpLimit;
                            Records.Add(ContractRec);
                        }
                    }
                }
                else
                {
                    var scan = NumberUtility.ConvertUpperToLower(s.Content).Replace(" ", "");
                    var cnt = RegularTool.GetRegular(scan, "中标通知书\\d份");
                    if (cnt.Count == 1)
                    {
                        Console.WriteLine(Id + ":" + cnt[0].RawData + "[" + scan + "]");
                        isMulti = true;
                    }
                    if (s.Content.Contains("履行进展情况"))
                    {
                        Console.WriteLine(Id + ":履行进展情况");
                        isMulti = true;
                    }
                }
            }
        }
        return Records;
    }

    List<RecordBase> ExtractMultiFromTable()
    {
        var Records = new List<RecordBase>();
        var JiaFang = new TableSearchTitleRule();
        JiaFang.Name = "甲方";
        JiaFang.Title = new string[] { "采购人" }.ToList();
        JiaFang.IsTitleEq = false;
        JiaFang.IsRequire = true;

        var YiFang = new TableSearchTitleRule();
        YiFang.Name = "乙方";
        //"投资者名称","股东名称"
        YiFang.Title = new string[] { "中标人" }.ToList();
        YiFang.IsTitleEq = false;
        YiFang.IsRequire = true;

        var ProjectName = new TableSearchTitleRule();
        ProjectName.Name = "项目名称";
        ProjectName.Title = new string[] { "项目名称" }.ToList();
        ProjectName.IsTitleEq = false;
        ProjectName.IsRequire = false;

        var Money = new TableSearchTitleRule();
        Money.Name = "中标金额";
        Money.Title = new string[] { "中标金额" }.ToList();
        Money.IsTitleEq = false;
        Money.IsRequire = false;

        var Rules = new List<TableSearchTitleRule>();
        Rules.Add(JiaFang);
        Rules.Add(YiFang);
        Rules.Add(ProjectName);
        Rules.Add(Money);

        var opt = new SearchOption();
        opt.IsMeger = false;
        var result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, opt);

        if (result.Count > 0)
        {
            Console.WriteLine("Table ExtractMulti ID:" + Id);
            foreach (var item in result)
            {
                var ContractRec = new ContractRec();
                ContractRec.Id = Id;
                ContractRec.JiaFang = item[0].RawData;
                ContractRec.JiaFang = ContractRec.JiaFang.NormalizeTextResult();
                ContractRec.YiFang = item[1].RawData;
                ContractRec.YiFang = ContractRec.YiFang.NormalizeTextResult();
                foreach (var cn in companynamelist)
                {
                    if (!String.IsNullOrEmpty(cn.secShortName) && cn.secShortName.Equals(ContractRec.YiFang))
                    {
                        if (!string.IsNullOrEmpty(cn.secFullName))
                        {
                            ContractRec.YiFang = cn.secFullName;
                            break;
                        }
                    }
                }
                ContractRec.ProjectName = item[2].RawData;
                ContractRec.ProjectName = ContractRec.ProjectName.NormalizeTextResult();
                ContractRec.ContractMoneyUpLimit = MoneyUtility.Format(item[3].RawData, item[3].Title);
                ContractRec.ContractMoneyDownLimit = ContractRec.ContractMoneyUpLimit;
                Records.Add(ContractRec);
            }
        }
        return Records;
    }

    /// <summary>
    /// 北车
    /// </summary>
    /// <returns></returns>
    List<RecordBase> ExtractMultiFromNorthVehicle()
    {
        //主合同的抽取：（北车专用）
        //#151135： 若干项重大合同
        //#153045： 若干项重大合同
        //#153271： 若干项重大合同
        //#175840： 若干项重大合同
        var Records = new List<RecordBase>();
        var isMulti = false;
        foreach (var p in root.Children)
        {
            foreach (var s in p.Children)
            {
                if (s.Content.Contains("若干项重大合同"))
                {
                    isMulti = true;
                    Console.WriteLine("若干项重大合同 ID:" + Id);
                }
                if (s.Content.StartsWith("<") && isMulti)
                {
                    var ContractRec = new ContractRec();
                    ContractRec.Id = Id;
                    //5 、本公司全资子公司中国北车集团大连机车车辆有限公司与大同地方铁路公司签订了约 3.26 亿元人民币的电力机车销售合同。

                    var i0 = s.Content.IndexOf("与");
                    var i1 = s.Content.IndexOf("签订");

                    if (i0 != -1 && i1 != -1 && i0 < i1)
                    {
                        ContractRec.JiaFang = s.Content.Substring(i0 + 1, i1 - i0 - 1);

                    }
                    foreach (var cn in companynamelist)
                    {
                        if (cn.isSubCompany && cn.positionId == s.PositionId)
                        {
                            ContractRec.YiFang = cn.secFullName;
                        }
                    }
                    var ml = moneylist.Where((x) => x.Loc == s.PositionId).ToList();
                    var SpecailContractNames = new string[]{
                        "地铁车辆出口合同",
                        "地铁车辆牵引系统销售合同",
                        "地铁车辆销售合同",
                        "地铁销售合同",
                        "电动客车销售合同",
                        "电力机车销售合同",
                        "动车组检修合同",
                        "动车组销售合同",
                        "风力发电机组销售合同",
                        "货车出口合同",
                        "货车检修合同",
                        "货车销售合同",
                        "货车修理合同",
                        "机车出口合同",
                        "机车大修及加改合同",
                        "客车检修合同",
                        "客车销售合同",
                        "客车修理合同",
                        "煤炭漏斗车销售合同",
                        "内燃电传动机车销售合同",
                        "内燃动车组销售合同",
                        "内燃机车订单",
                        "铁路客车修理合同",
                        "有轨电车销售合同"
                    }.ToList();

                    foreach (var scn in SpecailContractNames)
                    {
                        if (s.Content.Contains(scn))
                        {
                            ContractRec.ContractName = scn;
                            break;
                        }
                    }
                    if (ml.Count == 1)
                    {
                        if (!String.IsNullOrEmpty(ContractRec.JiaFang) && !String.IsNullOrEmpty(ContractRec.YiFang))
                        {
                            ContractRec.ContractMoneyUpLimit = MoneyUtility.Format(ml.First().Value.MoneyAmount, String.Empty);
                            ContractRec.ContractMoneyDownLimit = ContractRec.ContractMoneyUpLimit;
                            Records.Add(ContractRec);
                        }
                    }
                }
            }
        }
        return Records;
    }

    ContractRec ExtractSingle()
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

        var contract = new ContractRec();
        //公告ID
        contract.Id = Id;

        //乙方
        contract.YiFang = GetYiFang();
        if (contract.YiFang.Contains("本公司")) contract.YiFang = string.Empty;
        contract.YiFang = CompanyNameLogic.AfterProcessFullName(contract.YiFang).secFullName;
        contract.YiFang = contract.YiFang.NormalizeTextResult();
        //按照规定除去括号
        contract.YiFang = RegularTool.TrimBrackets(contract.YiFang);
        if(contract.YiFang.Length <3) contract.YiFang = string.Empty;


        //甲方
        contract.JiaFang = GetJiaFang(contract.YiFang);
        if (contract.JiaFang.Contains("本公司")) contract.JiaFang = string.Empty;
        contract.JiaFang = CompanyNameLogic.AfterProcessFullName(contract.JiaFang).secFullName;
        contract.JiaFang = contract.JiaFang.NormalizeTextResult();
        if (contract.JiaFang.Contains("简称"))
        {
            contract.JiaFang = Utility.GetStringBefore(contract.JiaFang, "(");
        }
        //机构列表
        if (Nerlist != null)
        {
            var NiList = Nerlist.Where((n) => n.Type == LTPTrainingNER.enmNerType.Ni).Select((m) => m.RawData);
            if (!NiList.Contains(contract.JiaFang))
            {
                if (NiList.Contains("国家电网公司")) contract.JiaFang = "国家电网公司";
            }
        }
        //项目
        contract.ProjectName = GetProjectName();
        contract.ProjectName = contract.ProjectName.NormalizeTextResult();
        if (contract.ProjectName.StartsWith("“") && contract.ProjectName.EndsWith("”"))
        {
            contract.ProjectName = contract.ProjectName.TrimStart("“".ToCharArray()).TrimEnd("”".ToCharArray());
        }
        if (contract.ProjectName.EndsWith("，签约双方"))
        {
            contract.ProjectName = Utility.GetStringAfter(contract.ProjectName, "，签约双方");
        }
        if (contract.ProjectName.Contains("(以下简称"))
        {
            contract.ProjectName = Utility.GetStringAfter(contract.ProjectName, "(以下简称");
        }
        if (contract.ProjectName.EndsWith(")"))
        {
            if (contract.ProjectName.Contains("(招标编号"))
            {
                contract.ProjectName = Utility.GetStringBefore(contract.ProjectName, "(招标编号");
            }
            if (contract.ProjectName.Contains("(合同编号"))
            {
                contract.ProjectName = Utility.GetStringBefore(contract.ProjectName, "(合同编号");
            }
        }
        contract.ProjectName = contract.ProjectName.Replace("的推荐中标", "");
        //特殊处理
        contract.ProjectName = contract.ProjectName.Replace("<1>", "1、");
        contract.ProjectName = contract.ProjectName.Replace("“","");
        contract.ProjectName = contract.ProjectName.Replace("”","");

        //合同名
        contract.ContractName = GetContractName();
        if (contract.ContractName.StartsWith("“") && contract.ContractName.EndsWith("”"))
        {
            contract.ContractName = contract.ContractName.TrimStart("“".ToCharArray()).TrimEnd("”".ToCharArray());
        }
        //去掉书名号
        contract.ContractName = contract.ContractName.Replace("《", String.Empty).Replace("》", String.Empty);
        contract.ContractName = contract.ContractName.NormalizeTextResult();
        if (contract.ContractName.Contains("(以下简称"))
        {
            contract.ContractName = Utility.GetStringAfter(contract.ContractName, "(以下简称");
        }
        contract.ContractName = ExtendContractName(contract.ContractName);

        //如果是采购协议，则工程名清空
        if (contract.ContractName.Contains("采购"))
        {
            if (contract.ProjectName.Contains("标段"))
            {
                //TODO:
            }
            else
            {
                contract.ProjectName = string.Empty;
            }
        }

        //金额
        var money = GetMoney();
        contract.ContractMoneyUpLimit = MoneyUtility.Format(money.MoneyAmount, String.Empty);
        contract.ContractMoneyDownLimit = contract.ContractMoneyUpLimit;

        //联合体
        contract.UnionMember = GetUnionMember(contract);
        contract.UnionMember = contract.UnionMember.NormalizeTextResult();
        //按照规定除去括号
        contract.UnionMember = RegularTool.TrimBrackets(contract.UnionMember);

        var YiFangArray = contract.YiFang.Split(Utility.SplitChar);
        if (YiFangArray.Length > 1)
        {
            contract.UnionMember = Utility.GetStringAfter(contract.YiFang, Utility.SplitChar);
            contract.YiFang = YiFangArray[0];
            Console.WriteLine("联合体：" + contract.UnionMember);
        }
        return contract;
    }



    /// <summary>
    /// 去除尾部的简称
    /// </summary>
    /// <param name="OrgString"></param>
    /// <returns></returns>
    public string TrimEndJianCheng(string OrgString)
    {
        if (OrgString.Contains("（以下简称"))
        {
            OrgString = Utility.GetStringBefore(OrgString, "（以下简称");
        }
        return OrgString;
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
        return AnnouceCompanyName;
    }

    /// <summary>
    /// 获得工程名
    /// </summary>
    /// <returns></returns>
    string GetProjectName()
    {

        var e = new EntityProperty();
        e.PropertyName = "工程名称";
        e.LeadingColonKeyWordList = new string[] { "项目名称：", "工程名称：", "中标项目：", "合同标的：", "工程内容：" };
        e.LeadingColonKeyWordCandidatePreprocess = TrimEndJianCheng;
        e.QuotationTrailingWordList_IsSkipBracket = true;
        e.QuotationTrailingWordList = new string[] {
            "标段施工项目","标段土建工程","标段施工总承包","标段的工程","标段工程","标段施工总价承包",
            "标段施工总承包工程","标段施工工程","标段土建工程建设项目","标段站前工程","标段工程(施工)",
            "工程施工工程","项目施工工程","施工工程",
            "工程项目", "工程标段","标段的施工项目","标段项目","标段施工",
            "招标采购项目","招标活动", "采购活动","招标项目",
            "项目", "采购","总承包",
            "工程", "标段", "标", };
        e.Extract(this);
        var prj = e.EvaluateCI();
        if (!String.IsNullOrEmpty(prj)) return prj;

        //var Stardard = TraningDataset.ContractList.Where(x => x.Id == this.Id).ToList();
        //if (Stardard.Count == 1)
        //{
        //Console.WriteLine("标准答案：" + Stardard[0].ProjectName);
        //}

        //var ProjectNameList = ProjectNameLogic.GetProjectNameByCutWord(this);
        //var ProjectNameListNER = ProjectNameLogic.GetProjectNameByNer(this);

        var StartArray = new string[] { "公司为", "参与了", "确定为" };
        var EndArray = new string[] { "的中标单位", "的公开招投标", "的中标人", "候选人" };
        e.ExternalStartEndStringFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        e.Extract(this);
        prj = e.EvaluateCI();
        if (!String.IsNullOrEmpty(prj))
        {
            if (ExtractPropertyByHTML.FindWordCnt(prj + "项目", root).Count >= 1)
            {
                return prj + "项目";
            }
            return prj;
        }

        foreach (var item in quotationList)
        {
            if (item.Value.Contains("推荐的中标候选人公示"))
            {
                prj = Utility.GetStringBefore(item.Value, "推荐的中标候选人公示");
                return prj;
            }
        }
        return string.Empty;
    }



    /// <summary>
    /// 获得合同名
    /// </summary>
    /// <returns></returns>
    string GetContractName()
    {
        var e = new EntityProperty();
        e.PropertyName = "合同名称";
        e.PropertyType = EntityProperty.enmType.NER;
        e.MaxLength = 200;
        e.MinLength = 4;
        e.LeadingColonKeyWordList = new string[] { "合同名称：" };
        e.QuotationTrailingWordList = new string[] {
            "商务合同补充协议","承包合同补充协议","补充协议","经营合同补充协议",
            "协议书", "合同书", "确认书", "合同", "协议"
        };
        e.QuotationTrailingWordList_IsSkipBracket = false;
        var StartArray = new string[] { "签署了", "签订了" };   //通过语境训练获得
        var EndArray = new string[] { "合同" };
        e.ExternalStartEndStringFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        e.ExternalStartEndStringFeatureCandidatePreprocess = (x) => { return x + "合同"; };
        e.MaxLengthCheckPreprocess = str =>
        {
            return Utility.TrimEnglish(str);
        };
        //最高级别的置信度，特殊处理器
        e.LeadingColonKeyWordCandidatePreprocess = str =>
        {
            var c = Normalizer.ClearTrailing(TrimEndJianCheng(str));
            return c;
        };

        e.CandidatePreprocess = str =>
        {
            var c = Normalizer.ClearTrailing(TrimEndJianCheng(str));
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
        //下面这个列表的根据不足,正确做法是【尚未签署】
        e.ExcludeEqualsWordList = new string[] { "若干项重大合同", "中标合同", "正式合同", "合同", "重大合同", "项目合同", "终止协议", "经营合同", "特别重大合同", "相关项目合同" };
        e.Extract(this);
        //冒号优先
        var contractname = e.EvaluateCI();
        return contractname;
    }

    string ExtendContractName(string contract)
    {
        //尝试看一下是否有Proj + 项目的词语
        var ExtendWords = new string[] { "补充协议" };
        foreach (var word in ExtendWords)
        {
            if (LocateCustomerWord(root, new string[] { contract.Replace(" ", "") + word }.ToList()).Count > 0) return contract + word;
        }
        return contract;
    }

    /// <summary>
    /// 获得金额
    /// </summary>
    /// <returns></returns>
    (String MoneyAmount, String MoneyCurrency) GetMoney()
    {
        var Extractor = new ExtractPropertyByText();
        Extractor.LeadingColonKeyWordList = new string[] {
            "订单总金额：","订单金额：","订单总价：","订单额：","金额：","中标成交金额（元）：",
            "合同总投资：", "合同总价：","合同金额：", "合同额：","合同总额：","合同总金额：","合同价：","合同价格：",
            "中标业务总额","中标总金额", "中标金额", "中标价","中标总价","交易价格：",
            "项目总价：","项目总投资：","项目估算总投资：", "项目投资额：","项目投资估算：","项目预计总投资：","投资总额：",
            "工程总价：","工程总投资：","工程估算总投资：", "工程投资额：","工程投资估算：","工程预计总投资：",
            "投标价格：","投标金额：","投标额：","投标总金额：","投标报价：","预算金额：","合同费用总额：","合同投资金额（人民币）："
        };
        Extractor.ExtractFromTextFile(TextFileName);
        var AllMoneyList = new List<(String MoneyAmount, String MoneyCurrency)>();
        foreach (var item in Extractor.CandidateWord)
        {
            var moneylist = MoneyUtility.SeekMoney(item.Value);
            AllMoneyList.AddRange(moneylist);
        }
        if (AllMoneyList.Count > 0)
        {
            foreach (var money in AllMoneyList)
            {
                if (money.MoneyCurrency == "人民币" ||
                    money.MoneyCurrency == "元")
                {
                    var amount = MoneyUtility.Format(money.MoneyAmount, String.Empty);
                    var m = 0.0;
                    if (double.TryParse(amount, out m))
                    {
                        if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("金额候补词：[" + money.MoneyAmount + ":" + money.MoneyCurrency + "]");
                        return money;
                    }
                }
            }
            //没有找到人民币，但是，其他货币存在的情况
            return AllMoneyList[0];
        }


        //这些关键字后面（暂时无法自动抽取）
        Extractor.LeadingColonKeyWordList = new string[] {
            "订单总金额","订单金额","订单总价","订单额",
            "合同总投资", "合同总价","合同金额", "合同额","合同总额","合同总金额","合同价","合同价格",
            "中标业务总额","中标总金额", "中标金额", "中标价","中标总价","交易总金额","交易价格",
            "项目总价","项目总投资","项目估算总投资", "项目投资额","项目投资估算","项目预计总投资",
            "工程总价","工程总投资","工程估算总投资", "工程投资额","工程投资估算","工程预计总投资",
            "投标价格","投标金额","投标额","投标总金额","投标报价","预算金额","预算总投资" };
        Extractor.ExtractFromTextFile(TextFileName);
        AllMoneyList = new List<(String MoneyAmount, String MoneyCurrency)>();
        foreach (var item in Extractor.CandidateWord)
        {
            var moneylist = MoneyUtility.SeekMoney(item.Value);
            AllMoneyList.AddRange(moneylist);
        }
        if (AllMoneyList.Count != 0)
        {
            foreach (var money in AllMoneyList)
            {
                if (money.MoneyCurrency == "人民币" ||
                    money.MoneyCurrency == "元")
                {
                    var amount = MoneyUtility.Format(money.MoneyAmount, String.Empty);
                    var m = 0.0;
                    if (double.TryParse(amount, out m))
                    {
                        if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("金额候补词：[" + money.MoneyAmount + ":" + money.MoneyCurrency + "]");
                        return money;
                    }
                }
            }
            if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("金额候补词：[" + AllMoneyList[0].MoneyAmount + ":" + AllMoneyList[0].MoneyCurrency + "]");
            return AllMoneyList[0];
        }

        if (moneylist.Count == 1) return moneylist.First().Value;
        return (string.Empty, string.Empty);
    }

    /// <summary>
    /// 获得联合体
    /// </summary>
    /// <param name="JiaFang">甲方</param>
    /// <param name="YiFang">乙方</param>
    /// <returns></returns>
    string GetUnionMember(ContractRec contract)
    {
        var Extractor = new ExtractPropertyByText();
        Extractor.LeadingColonKeyWordListInChineseBrackets = new string[] { "联合体成员：" };
        Extractor.ExtractFromTextFile(this.TextFileName);
        foreach (var union in Extractor.CandidateWord)
        {
            return union.Value;
        }

        var Union = new List<String>();

        //中标人：A公司B公司，A公司已经是甲方的情况下，B公司则为联合体
        Extractor.LeadingColonKeyWordList = new string[] {
        "乙方：","供应商名称：","中标单位：",
        "中标人：","中标单位：","中标人：","乙方（供方）：",
        "承包人：","承包方：","中标方：","供应商名称：","中标人名称：" };
        Extractor.ExtractFromTextFile(this.TextFileName);
        foreach (var union in Extractor.CandidateWord)
        {
            //联合体牵头方——博天环境集团股份有限公司，联合体成员——广西木盛投资管理有限公司。
            if (union.Value.Contains("联合体牵头方"))
            {
                var UnionArray = union.Value.Split(',', '、');
                if (UnionArray.Length == 2)
                {
                    contract.YiFang = UnionArray[0];
                    contract.YiFang = contract.YiFang.Replace("联合体牵头方", "");
                    contract.YiFang = contract.YiFang.Replace("—", "");
                    contract.YiFang = contract.YiFang.Replace("（", "");
                    contract.YiFang = contract.YiFang.Replace("）", "");
                    contract.YiFang = contract.YiFang.NormalizeTextResult();

                    contract.UnionMember = UnionArray[1];
                    contract.UnionMember = contract.UnionMember.Replace("联合体成员", "");
                    contract.UnionMember = contract.UnionMember.Replace("—", "");
                    contract.UnionMember = contract.UnionMember.Replace("（", "");
                    contract.UnionMember = contract.UnionMember.Replace("）", "");
                    contract.UnionMember = contract.UnionMember.NormalizeTextResult();
                    Console.WriteLine("联合体牵头方:" + contract.YiFang);
                    Console.WriteLine("联合体成员:" + contract.UnionMember);
                    return contract.UnionMember;
                }
            }


            var foundCompanys = new List<string>();
            foreach (var comp in companynamelist)
            {
                if (union.Value.Contains(comp.secFullName))
                {
                    //检索出来的乙方是一个公司名字
                    foundCompanys.Add(comp.secFullName);
                    //如果这里的A和B是父子关系，可能会出问题。
                    //可以考虑追加一个Flag，是否检查父子关系
                    var IsJiaYiFang = IsUnionEqualJiaYiFang(contract.JiaFang, contract.YiFang, comp.secFullName, false);
                    if (!IsJiaYiFang)
                    {
                        Union.Add(comp.secFullName);
                    }
                }
            }
            foundCompanys = foundCompanys.Distinct().ToList();
            if (foundCompanys.Count > 1)
            {
                //可能出现的关联交易的情报，所以，必须保证，这里出现多个公司名字
                if (Union.Count != 0)
                {
                    Union = Union.Distinct().ToList();
                    return String.Join(Utility.SplitChar, Union);
                }
            }
        }


        var UnionKeyList = LocateCustomerWord(root, new string[] { "联合体" }.ToList());
        foreach (var UnionKey in UnionKeyList)
        {
            if (!nermap.ParagraghlocateDict.ContainsKey(UnionKey.Loc)) continue;
            foreach (var item in nermap.ParagraghlocateDict[UnionKey.Loc].NerList)
            {
                if (item.Description == "公司名" || item.Description == "机构")
                {
                    //LTP修正
                    var union = item.Value.Replace("通知", "");
                    if (!Union.Contains(union))
                    {
                        var IsJiaYiFang = IsUnionEqualJiaYiFang(contract.JiaFang, contract.YiFang, union);
                        if (!IsJiaYiFang)
                        {
                            //联合体一般在公司名之前
                            if (Union.Count > 0 && item.StartIdx > UnionKey.StartIdx) continue;
                            //这里可能同时出现简称和全称，需要再过滤掉甲方和乙方的简称
                            Union.Add(union);
                        }
                    }
                }
            }
            //第一次即可，之后的可能出现干扰项
            if (Union.Count != 0) break;
        }
        Union = Union.Distinct().ToList();
        return String.Join(Utility.SplitChar, Union);
    }

    private bool IsUnionEqualJiaYiFang(string JiaFang, string YiFang, string union, bool isCheckSubCompany = true)
    {
        bool IsJiaYiFang = false;

        foreach (var org in LTPTrainingNER.OrgniazationList)
        {
            //政府一般不是联合体或者乙方
            if (union.EndsWith(org)) return true;
        }

        //乙方的去括号问题
        if (!String.IsNullOrEmpty(YiFang) && RegularTool.TrimBrackets(union.NormalizeTextResult()).Equals(YiFang))
        {
            return true;
        }
        foreach (var cn in companynamelist)
        {
            if (!String.IsNullOrEmpty(cn.secShortName) && union.Equals(cn.secShortName))
            {
                //去除简称
                return true;
            }
        }
        foreach (var cn in companynamelist)
        {
            if (cn.isSubCompany && isCheckSubCompany)
            {
                if (union.Equals(cn.FatherName))
                {
                    return true;
                }
            }

            if (!String.IsNullOrEmpty(cn.secFullName))
            {
                if (!String.IsNullOrEmpty(JiaFang))
                {
                    if (cn.secFullName.Equals(JiaFang))
                    {
                        //当前的公司是全称，是甲方    
                        if (union.Equals(JiaFang))
                        {
                            //如果联合体是甲方公司的全称
                            return true;
                        }
                        if (!String.IsNullOrEmpty(cn.secShortName))
                        {
                            if (union.Equals(cn.secShortName))
                            {
                                return true;
                            }
                        }
                    }
                }
                if (!String.IsNullOrEmpty(YiFang))
                {
                    if (cn.secFullName.Equals(YiFang))
                    {
                        if (union.Equals(YiFang))
                        {
                            return true;
                        }
                        if (!String.IsNullOrEmpty(cn.secShortName))
                        {
                            if (union.Equals(cn.secShortName))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            if (!String.IsNullOrEmpty(cn.secShortName))
            {
                if (!String.IsNullOrEmpty(JiaFang))
                {
                    if (cn.secShortName.Equals(JiaFang))
                    {
                        if (union.Equals(JiaFang))
                        {
                            return true;
                        }
                        if (!String.IsNullOrEmpty(cn.secFullName))
                        {
                            if (union.Equals(cn.secFullName))
                            {
                                return true;
                            }
                        }
                    }
                }
                if (!String.IsNullOrEmpty(YiFang))
                {
                    if (cn.secShortName.Equals(YiFang))
                    {
                        if (union.Equals(YiFang))
                        {
                            return true;
                        }
                        if (!String.IsNullOrEmpty(cn.secFullName))
                        {
                            if (union.Equals(cn.secFullName))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return IsJiaYiFang;
    }
}