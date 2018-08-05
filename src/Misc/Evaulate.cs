using System.Collections.Generic;
using System;
using FDDC;
using System.Linq;
using static IncreaseStock;
using static StockChange;
using static Contract;
using System.IO;

public static class Evaluate
{
    /// <summary>
    /// 合同结果测评
    /// </summary>
    /// <param name="resultDataset"></param>
    public static void EvaluateContract(List<ContractRec> resultDataset)
    {
        //POS:标准数据集中该字段不为空的记录数
        //ACT:选手提交结果中该字段不为空的记录数
        //COR:主键匹配 且 提交字段值=正确字段值 且 均不为空
        //公告ID
        var F1_ID = new EvaluateItem("公告ID");
        var F1_JiaFang = new EvaluateItem("甲方");
        var F1_YiFang = new EvaluateItem("乙方");
        var F1_ProjectName = new EvaluateItem("项目名称");
        var F1_ContractName = new EvaluateItem("合同名称");
        var F1_ContractMoneyUpLimit = new EvaluateItem("金额上限");
        var F1_ContractMoneyDownLimit = new EvaluateItem("金额下限");
        var F1_UnionMember = new EvaluateItem("联合体成员");
        F1_UnionMember.IsList = true;

        foreach (var contract in TraningDataset.ContractList)
        {
            //POS:标准数据集中该字段不为空的记录数
            if (!String.IsNullOrEmpty(contract.Id)) F1_ID.POS++;
            if (!String.IsNullOrEmpty(contract.JiaFang)) F1_JiaFang.POS++;
            if (!String.IsNullOrEmpty(contract.YiFang)) F1_YiFang.POS++;
            if (!String.IsNullOrEmpty(contract.ProjectName)) F1_ProjectName.POS++;
            if (!String.IsNullOrEmpty(contract.ContractName)) F1_ContractName.POS++;
            if (!String.IsNullOrEmpty(contract.ContractMoneyUpLimit)) F1_ContractMoneyUpLimit.POS++;
            if (!String.IsNullOrEmpty(contract.ContractMoneyDownLimit)) F1_ContractMoneyDownLimit.POS++;
            if (!String.IsNullOrEmpty(contract.UnionMember)) F1_UnionMember.POS++;
        }

        foreach (var contract in resultDataset)
        {
            //ACT:选手提交结果中该字段不为空的记录数
            if (!String.IsNullOrEmpty(contract.Id)) F1_ID.ACT++;
            if (!String.IsNullOrEmpty(contract.JiaFang)) F1_JiaFang.ACT++;
            if (!String.IsNullOrEmpty(contract.YiFang)) F1_YiFang.ACT++;
            if (!String.IsNullOrEmpty(contract.ProjectName)) F1_ProjectName.ACT++;
            if (!String.IsNullOrEmpty(contract.ContractName)) F1_ContractName.ACT++;
            if (!String.IsNullOrEmpty(contract.ContractMoneyUpLimit)) F1_ContractMoneyUpLimit.ACT++;
            if (!String.IsNullOrEmpty(contract.ContractMoneyDownLimit)) F1_ContractMoneyDownLimit.ACT++;
            if (!String.IsNullOrEmpty(contract.UnionMember)) F1_UnionMember.ACT++;
        }

        foreach (var contract in TraningDataset.ContractList)
        {
            var key = contract.GetKey();
            foreach (var contract_Result in resultDataset)
            {
                var key_Result = contract_Result.GetKey();
                var IsKeyMatch = key.Equals(key_Result);
                if (IsKeyMatch)
                {
                    //COR:主键匹配 且 提交字段值=正确字段值 且 均不为空
                    F1_ID.PutCORData(contract.Id, contract_Result.Id);
                    F1_JiaFang.PutCORData(contract.JiaFang, contract_Result.JiaFang);
                    F1_YiFang.PutCORData(contract.YiFang, contract_Result.YiFang);
                    F1_ProjectName.PutCORData(contract.ProjectName, contract_Result.ProjectName);
                    F1_ContractName.PutCORData(contract.ContractName, contract_Result.ContractName);
                    F1_ContractMoneyUpLimit.PutCORData(contract.ContractMoneyUpLimit, contract_Result.ContractMoneyUpLimit);
                    F1_ContractMoneyDownLimit.PutCORData(contract.ContractMoneyDownLimit, contract_Result.ContractMoneyDownLimit);
                    F1_UnionMember.PutCORData(contract.UnionMember, contract_Result.UnionMember);
                    break; //防止测试集出现多条主键重复的记录
                }
            }
        }

        //单项测评
        foreach (var contract in TraningDataset.ContractList)
        {
            foreach (var contract_Result in resultDataset)
            {
                var IsIdMatch = contract.Id.Equals(contract_Result.Id);
                if (IsIdMatch)
                {
                    //这里假定数据都是ID为主键一对一的情况
                    //数据一对多的时候，这里的逻辑是不正确的
                    F1_ID.PutItemData(contract.Id, contract_Result.Id);
                    F1_JiaFang.PutItemData(contract.JiaFang, contract_Result.JiaFang);
                    F1_YiFang.PutItemData(contract.YiFang, contract_Result.YiFang);
                    F1_ProjectName.PutItemData(contract.ProjectName, contract_Result.ProjectName, contract.Id);
                    F1_ContractName.PutItemData(contract.ContractName, contract_Result.ContractName, contract.Id);
                    F1_ContractMoneyUpLimit.PutItemData(contract.ContractMoneyUpLimit, contract_Result.ContractMoneyUpLimit);
                    F1_ContractMoneyDownLimit.PutItemData(contract.ContractMoneyDownLimit, contract_Result.ContractMoneyDownLimit);
                    F1_UnionMember.PutItemData(contract.UnionMember, contract_Result.UnionMember, contract.Id);
                    break;
                }
            }
        }

        var score = (F1_ID.F1 + F1_JiaFang.F1 + F1_YiFang.F1 + F1_ProjectName.F1 +
        F1_ContractName.F1 + F1_ContractMoneyUpLimit.F1 + F1_ContractMoneyDownLimit.F1 + F1_UnionMember.F1) / 8;
        Program.Score.WriteLine("合同score:" + score);
        Program.Score.Flush();
    }


    public static void EvaluateReorganizationByFile(string txtfilename)
    {
        List<ReorganizationRec> resultDataset = new List<ReorganizationRec>();
        var sr = new StreamReader(txtfilename);
        while (!sr.EndOfStream)
        {
            resultDataset.Add(ReorganizationRec.ConvertFromString(sr.ReadLine()));
        }
        Console.WriteLine("资产重组标准结果数:" + resultDataset.Count);
        sr.Close();
        EvaluateReorganization(resultDataset);
    }

    /// <summary>
    /// 资产重组结果测评
    /// </summary>
    /// <param name="result"></param>
    public static void EvaluateReorganization(List<ReorganizationRec> resultDataset)
    {
        var F1_ID = new EvaluateItem("公告ID");
        var F1_Target = new EvaluateItem("交易标的");
        var F1_TargetCompany = new EvaluateItem("标的公司");
        F1_TargetCompany.IsOptional = true;
        var F1_TradeCompany = new EvaluateItem("交易对方");
        F1_TradeCompany.IsList = true;
        F1_TradeCompany.IsOptional = true;
        var F1_Price = new EvaluateItem("交易标的作价");
        var F1_EvaluateMethod = new EvaluateItem("评估方法");
        F1_EvaluateMethod.IsList = true;

        foreach (var contract in TraningDataset.ReorganizationList)
        {
            //POS:标准数据集中该字段不为空的记录数
            if (!String.IsNullOrEmpty(contract.Id)) F1_ID.POS++;
            if (!String.IsNullOrEmpty(contract.Target)) F1_Target.POS++;
            if (!String.IsNullOrEmpty(contract.TargetCompany)) F1_TargetCompany.POS++;
            if (!String.IsNullOrEmpty(contract.TradeCompany)) F1_TradeCompany.POS++;
            if (!String.IsNullOrEmpty(contract.Price)) F1_Price.POS++;
            if (!String.IsNullOrEmpty(contract.EvaluateMethod)) F1_EvaluateMethod.POS++;
        }

        foreach (var contract in resultDataset)
        {
            //ACT:选手提交结果中该字段不为空的记录数
            if (!String.IsNullOrEmpty(contract.Id)) F1_ID.ACT++;
            if (!String.IsNullOrEmpty(contract.Target)) F1_Target.ACT++;
            if (!String.IsNullOrEmpty(contract.TargetCompany)) F1_TargetCompany.ACT++;
            if (!String.IsNullOrEmpty(contract.TradeCompany)) F1_TradeCompany.ACT++;
            if (!String.IsNullOrEmpty(contract.Price)) F1_Price.ACT++;
            if (!String.IsNullOrEmpty(contract.EvaluateMethod)) F1_EvaluateMethod.ACT++;
        }

        foreach (var reorg in TraningDataset.ReorganizationList)
        {
            var key = reorg.GetKey();
            foreach (var reorg_Result in resultDataset)
            {
                var keys_Result = reorg_Result.GetKey().Split("|");
                var IsKeyMatch = false;
                if (keys_Result.Length == 1)
                {
                    IsKeyMatch = key.Equals(keys_Result[0]);
                }
                else
                {
                    IsKeyMatch = key.Equals(keys_Result[0]) || key.Equals(keys_Result[1]);
                }
                if (IsKeyMatch)
                {
                    //COR:主键匹配 且 提交字段值=正确字段值 且 均不为空
                    F1_ID.PutCORData(reorg.Id, reorg_Result.Id);
                    F1_Target.PutCORData(reorg.Target, reorg_Result.Target);
                    F1_TargetCompany.PutCORData(reorg.TargetCompany, reorg_Result.TargetCompany);
                    F1_TradeCompany.PutCORData(reorg.TradeCompany, reorg_Result.TradeCompany);
                    F1_Price.PutCORData(reorg.Price, reorg_Result.Price);
                    F1_EvaluateMethod.PutCORData(reorg.EvaluateMethod, reorg_Result.EvaluateMethod);
                    break; //防止测试集出现多条主键重复的记录
                }
            }
        }

        //单项测评
        foreach (var reorg in TraningDataset.ReorganizationList)
        {
            foreach (var reorg_Result in resultDataset)
            {
                var IsIdMatch = reorg.Id.Equals(reorg_Result.Id);
                if (IsIdMatch)
                {
                    //COR:主键匹配 且 提交字段值=正确字段值 且 均不为空
                    F1_ID.PutItemData(reorg.Id, reorg_Result.Id);
                    F1_Target.PutItemData(reorg.Target, reorg_Result.Target);
                    F1_TargetCompany.PutItemData(reorg.TargetCompany, reorg_Result.TargetCompany);
                    F1_TradeCompany.PutItemData(reorg.TradeCompany, reorg_Result.TradeCompany, reorg.Id);
                    F1_Price.PutItemData(reorg.Price, reorg_Result.Price, reorg.Id);
                    F1_EvaluateMethod.PutItemData(reorg.EvaluateMethod, reorg_Result.EvaluateMethod, reorg.Id);
                    break; //防止测试集出现多条主键重复的记录
                }
            }
        }

        var score = (F1_ID.F1 + F1_Target.F1 + F1_TargetCompany.F1 + F1_TradeCompany.F1 +
                     F1_Price.F1 + F1_EvaluateMethod.F1) / 6;
        Program.Score.WriteLine("资产重组分数：" + score);
        Program.Score.Flush();

    }

    /// <summary>
    /// 增减持结果测评
    /// </summary>
    /// <param name="result"></param>
    public static void EvaluateStockChange(List<StockChangeRec> result)
    {
        //POS:标准数据集中该字段不为空的记录数
        //ACT:选手提交结果中该字段不为空的记录数
        //COR:主键匹配 且 提交字段值=正确字段值 且 均不为空
        //公告ID
        var POS_ID = 0;
        var ACT_ID = 0;
        var COR_ID = 0;

        var POS_HolderFullName = 0;
        var ACT_HolderFullName = 0;
        var COR_HolderFullName = 0;

        var POS_HolderShortName = 0;
        var ACT_HolderShortName = 0;
        var COR_HolderShortName = 0;

        var POS_ChangeEndDate = 0;
        var ACT_ChangeEndDate = 0;
        var COR_ChangeEndDate = 0;

        var POS_ChangePrice = 0;
        var ACT_ChangePrice = 0;
        var COR_ChangePrice = 0;

        var POS_ChangeNumber = 0;
        var ACT_ChangeNumber = 0;
        var COR_ChangeNumber = 0;

        var POS_HoldNumberAfterChange = 0;
        var ACT_HoldNumberAfterChange = 0;
        var COR_HoldNumberAfterChange = 0;

        var POS_HoldPercentAfterChange = 0;
        var ACT_HoldPercentAfterChange = 0;
        var COR_HoldPercentAfterChange = 0;

        foreach (var stockchange in TraningDataset.StockChangeList)
        {
            if (!String.IsNullOrEmpty(stockchange.Id)) POS_ID++;
            if (!String.IsNullOrEmpty(stockchange.HolderFullName)) POS_HolderFullName++;
            if (!String.IsNullOrEmpty(stockchange.HolderShortName)) POS_HolderShortName++;
            if (!String.IsNullOrEmpty(stockchange.ChangeEndDate)) POS_ChangeEndDate++;
            if (!String.IsNullOrEmpty(stockchange.ChangePrice)) POS_ChangePrice++;
            if (!String.IsNullOrEmpty(stockchange.ChangeNumber)) POS_ChangeNumber++;

            if (!String.IsNullOrEmpty(stockchange.HoldNumberAfterChange)) POS_HoldNumberAfterChange++;
            if (!String.IsNullOrEmpty(stockchange.HoldPercentAfterChange)) POS_HoldPercentAfterChange++;

        }
        foreach (var stockchange in result)
        {
            if (!String.IsNullOrEmpty(stockchange.Id)) ACT_ID++;
            if (!String.IsNullOrEmpty(stockchange.HolderFullName)) ACT_HolderFullName++;
            if (!String.IsNullOrEmpty(stockchange.HolderShortName)) ACT_HolderShortName++;
            if (!String.IsNullOrEmpty(stockchange.ChangeEndDate)) ACT_ChangeEndDate++;
            if (!String.IsNullOrEmpty(stockchange.ChangePrice)) ACT_ChangePrice++;
            if (!String.IsNullOrEmpty(stockchange.ChangeNumber)) ACT_ChangeNumber++;

            if (!String.IsNullOrEmpty(stockchange.HoldNumberAfterChange)) ACT_HoldNumberAfterChange++;
            if (!String.IsNullOrEmpty(stockchange.HoldPercentAfterChange)) ACT_HoldPercentAfterChange++;

        }

        foreach (var stockchange in TraningDataset.StockChangeList)
        {
            var key = stockchange.GetKey();
            foreach (var stockchange_Result in result)
            {
                var key_Result = stockchange_Result.GetKey();
                if (key.Equals(key_Result))
                {
                    COR_ID++;
                    COR_HolderFullName++;
                    COR_ChangeEndDate++;
                    if (!String.IsNullOrEmpty(stockchange.HolderShortName) &&
                        !String.IsNullOrEmpty(stockchange_Result.HolderShortName) &&
                        stockchange.HolderShortName.NormalizeKey().Equals(stockchange_Result.HolderShortName.NormalizeKey()))
                    {
                        COR_HolderShortName++;
                    }

                    if (!String.IsNullOrEmpty(stockchange.ChangePrice) &&
                        !String.IsNullOrEmpty(stockchange_Result.ChangePrice) &&
                        stockchange.ChangePrice.Equals(stockchange_Result.ChangePrice))
                    {
                        COR_ChangePrice++;
                    }

                    if (!String.IsNullOrEmpty(stockchange.ChangeNumber) &&
                        !String.IsNullOrEmpty(stockchange_Result.ChangeNumber) &&
                        stockchange.ChangeNumber.Equals(stockchange_Result.ChangeNumber))
                    {
                        COR_ChangeNumber++;
                    }
                    if (!String.IsNullOrEmpty(stockchange.HoldNumberAfterChange) &&
                        !String.IsNullOrEmpty(stockchange_Result.HoldNumberAfterChange) &&
                        stockchange.HoldNumberAfterChange.Equals(stockchange_Result.HoldNumberAfterChange))
                    {
                        COR_HoldNumberAfterChange++;
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(stockchange.HoldNumberAfterChange) && String.IsNullOrEmpty(stockchange_Result.HoldNumberAfterChange))
                        {
                            Program.Evaluator.WriteLine("[" + stockchange.Id + "]未发现持股数：" + stockchange.HoldNumberAfterChange);
                        }
                    }
                    if (!String.IsNullOrEmpty(stockchange.HoldPercentAfterChange) &&
                        !String.IsNullOrEmpty(stockchange_Result.HoldPercentAfterChange) &&
                        stockchange.HoldPercentAfterChange.Equals(stockchange_Result.HoldPercentAfterChange))
                    {
                        COR_HoldPercentAfterChange++;
                    }
                    break;  //按照道理开说，不应该主键重复
                }
            }
        }

        var IDList = TraningDataset.StockChangeList.Select((x) => { return x.Id; }).Distinct().ToList();
        foreach (var id in IDList)
        {
            var trainingCnt = TraningDataset.StockChangeList.Count((x) => { return x.Id == id; });
            var resultCnt = result.Count((x) => { return x.Id == id; });
            if (Math.Abs(trainingCnt - resultCnt) > 5)
            {
                Program.Evaluator.WriteLine("ID:" + id + " Training:" + trainingCnt + "  Result:" + resultCnt);
            }
        }

        var F1_ID = EvaluateItem.GetF1("公告ID", POS_ID, ACT_ID, COR_ID);
        var F1_HolderFullName = EvaluateItem.GetF1("股东全称", POS_HolderFullName, ACT_HolderFullName, COR_HolderFullName);
        var F1_HolderName = EvaluateItem.GetF1("股东简称", POS_HolderShortName, ACT_HolderShortName, COR_HolderShortName);
        var F1_ChangeEndDate = EvaluateItem.GetF1("变动截止日期", POS_ChangeEndDate, ACT_ChangeEndDate, COR_ChangeEndDate);
        var F1_ChangePrice = EvaluateItem.GetF1("变动价格", POS_ChangePrice, ACT_ChangePrice, COR_ChangePrice);
        var F1_ChangeNumber = EvaluateItem.GetF1("变动数量", POS_ChangeNumber, ACT_ChangeNumber, COR_ChangeNumber);
        var F1_HoldNumberAfterChange = EvaluateItem.GetF1("变动后持股数", POS_HoldNumberAfterChange, ACT_HoldNumberAfterChange, COR_HoldNumberAfterChange);
        var F1_HoldPercentAfterChange = EvaluateItem.GetF1("变动后持股比例", POS_HoldPercentAfterChange, ACT_HoldPercentAfterChange, COR_HoldPercentAfterChange);


        var score = (F1_ID + F1_HolderFullName + F1_HolderName + F1_ChangeEndDate +
                     F1_ChangePrice + F1_ChangeNumber + F1_HoldNumberAfterChange + F1_HoldPercentAfterChange) / 8;
        Program.Score.WriteLine("增减持score:" + score);
        Program.Score.Flush();

    }

    /// <summary>
    /// 定增结果测评
    /// </summary>
    /// <param name="result"></param>
    public static void EvaluateIncreaseStock(List<IncreaseStockRec> result)
    {
        //POS:标准数据集中该字段不为空的记录数
        //ACT:选手提交结果中该字段不为空的记录数
        //COR:主键匹配 且 提交字段值=正确字段值 且 均不为空
        //公告ID
        var POS_ID = 0;
        var ACT_ID = 0;
        var COR_ID = 0;

        var POS_PublishTarget = 0;
        var ACT_PublishTarget = 0;
        var COR_PublishTarget = 0;

        var POS_IncreaseNumber = 0;
        var ACT_IncreaseNumber = 0;
        var COR_IncreaseNumber = 0;

        var POS_IncreaseMoney = 0;
        var ACT_IncreaseMoney = 0;
        var COR_IncreaseMoney = 0;

        var POS_FreezeYear = 0;
        var ACT_FreezeYear = 0;
        var COR_FreezeYear = 0;

        var POS_BuyMethod = 0;
        var ACT_BuyMethod = 0;
        var COR_BuyMethod = 0;


        foreach (var increase in TraningDataset.IncreaseStockList)
        {
            if (!String.IsNullOrEmpty(increase.Id)) POS_ID++;
            if (!String.IsNullOrEmpty(increase.PublishTarget)) POS_PublishTarget++;
            if (!String.IsNullOrEmpty(increase.IncreaseNumber)) POS_IncreaseNumber++;
            if (!String.IsNullOrEmpty(increase.IncreaseMoney)) POS_IncreaseMoney++;
            if (!String.IsNullOrEmpty(increase.FreezeYear)) POS_FreezeYear++;
            if (!String.IsNullOrEmpty(increase.BuyMethod)) POS_BuyMethod++;
        }
        foreach (var increase in result)
        {
            if (!String.IsNullOrEmpty(increase.Id)) ACT_ID++;
            if (!String.IsNullOrEmpty(increase.PublishTarget)) ACT_PublishTarget++;
            if (!String.IsNullOrEmpty(increase.IncreaseNumber)) ACT_IncreaseNumber++;
            if (!String.IsNullOrEmpty(increase.IncreaseMoney)) ACT_IncreaseMoney++;
            if (!String.IsNullOrEmpty(increase.FreezeYear)) ACT_FreezeYear++;
            if (!String.IsNullOrEmpty(increase.BuyMethod)) ACT_BuyMethod++;
        }

        foreach (var increase in TraningDataset.IncreaseStockList)
        {
            var key = increase.GetKey();
            foreach (var increase_Result in result)
            {
                var key_Result = increase_Result.GetKey();
                if (key.Equals(key_Result))
                {
                    COR_ID++;
                    COR_PublishTarget++;
                    if (!String.IsNullOrEmpty(increase.IncreaseNumber) &&
                        !String.IsNullOrEmpty(increase_Result.IncreaseNumber) &&
                        increase.IncreaseNumber.Equals(increase_Result.IncreaseNumber))
                    {
                        COR_IncreaseNumber++;
                    }

                    if (!String.IsNullOrEmpty(increase.IncreaseMoney) &&
                        !String.IsNullOrEmpty(increase_Result.IncreaseMoney) &&
                        increase.IncreaseMoney.Equals(increase_Result.IncreaseMoney))
                    {
                        COR_IncreaseMoney++;
                    }

                    if (!String.IsNullOrEmpty(increase.FreezeYear) &&
                        !String.IsNullOrEmpty(increase_Result.FreezeYear) &&
                        increase.FreezeYear.Equals(increase_Result.FreezeYear))
                    {
                        COR_FreezeYear++;
                    }
                    if (!String.IsNullOrEmpty(increase.BuyMethod) &&
                        !String.IsNullOrEmpty(increase_Result.BuyMethod) &&
                        increase.BuyMethod.Equals(increase_Result.BuyMethod))
                    {
                        COR_BuyMethod++;
                    }
                    break;  //按照道理开说，不应该主键重复
                }
            }
        }
        var IDList = TraningDataset.IncreaseStockList.Select((x) => { return x.Id; }).Distinct().ToList();
        foreach (var id in IDList)
        {
            var trainingCnt = TraningDataset.IncreaseStockList.Count((x) => { return x.Id == id; });
            var resultCnt = result.Count((x) => { return x.Id == id; });
            if (Math.Abs(trainingCnt - resultCnt) > 5)
            {
                Program.Evaluator.WriteLine("ID:" + id + " Training:" + trainingCnt + "  Result:" + resultCnt);
            }
        }

        var F1_ID = EvaluateItem.GetF1("公告ID", POS_ID, ACT_ID, COR_ID);
        var F1_PublishTarget = EvaluateItem.GetF1("增发对象", POS_PublishTarget, ACT_PublishTarget, COR_PublishTarget);
        var F1_IncreaseNumber = EvaluateItem.GetF1("增发数量", POS_IncreaseNumber, ACT_IncreaseNumber, COR_IncreaseNumber);
        var F1_IncreaseMoney = EvaluateItem.GetF1("增发金额", POS_IncreaseMoney, ACT_IncreaseMoney, COR_IncreaseMoney);
        var F1_FreezeYear = EvaluateItem.GetF1("锁定期", POS_FreezeYear, ACT_FreezeYear, COR_FreezeYear);
        var F1_BuyMethod = EvaluateItem.GetF1("认购方式", POS_BuyMethod, ACT_BuyMethod, COR_BuyMethod);
        var score = (F1_ID + F1_PublishTarget + F1_IncreaseNumber + F1_IncreaseMoney + F1_FreezeYear + F1_BuyMethod) / 6;
        Program.Score.WriteLine("定向增发score:" + score);
        Program.Score.Flush();
    }

}