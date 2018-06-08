using System.Collections.Generic;
using static IncreaseStock;
using System;
using static StockChange;
using static Contract;
using FDDC;

public static class Evaluate
{
    public static void EvaluateIncreaseStock(List<struIncreaseStock> result)
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
            if (!String.IsNullOrEmpty(increase.id)) POS_ID++;
            if (!String.IsNullOrEmpty(increase.PublishTarget)) POS_PublishTarget++;
            if (!String.IsNullOrEmpty(increase.IncreaseNumber)) POS_IncreaseNumber++;
            if (!String.IsNullOrEmpty(increase.IncreaseMoney)) POS_IncreaseMoney++;
            if (!String.IsNullOrEmpty(increase.FreezeYear)) POS_FreezeYear++;
            if (!String.IsNullOrEmpty(increase.BuyMethod)) POS_BuyMethod++;
        }
        foreach (var increase in result)
        {
            if (!String.IsNullOrEmpty(increase.id)) ACT_ID++;
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
        var F1_ID = GetF1("公告ID", POS_ID, ACT_ID, COR_ID);
        var F1_PublishTarget = GetF1("增发对象", POS_PublishTarget, ACT_PublishTarget, COR_PublishTarget);
        var F1_IncreaseNumber = GetF1("增发数量", POS_IncreaseNumber, ACT_IncreaseNumber, COR_IncreaseNumber);
        var F1_IncreaseMoney = GetF1("增发金额", POS_IncreaseMoney, ACT_IncreaseMoney, COR_IncreaseMoney);
        var F1_FreezeYear = GetF1("锁定期", POS_FreezeYear, ACT_FreezeYear, COR_FreezeYear);
        var F1_BuyMethod = GetF1("认购方式", POS_BuyMethod, ACT_BuyMethod, COR_BuyMethod);
        var score = (F1_ID + F1_PublishTarget + F1_IncreaseNumber + F1_IncreaseMoney + F1_FreezeYear + F1_BuyMethod) / 6;
        Program.Score.WriteLine("定向增发score:" + score);
        Program.Score.Flush();
    }

    public static void EvaluateStockChange(List<struStockChange> result)
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
            if (!String.IsNullOrEmpty(stockchange.id)) POS_ID++;
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
            if (!String.IsNullOrEmpty(stockchange.id)) ACT_ID++;
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
            foreach (var increase_Result in result)
            {
                var key_Result = increase_Result.GetKey();
                if (key.Equals(key_Result))
                {
                    COR_ID++;
                    COR_HolderFullName++;
                    COR_ChangeEndDate++;
                    if (!String.IsNullOrEmpty(stockchange.HolderShortName) &&
                        !String.IsNullOrEmpty(increase_Result.HolderShortName) &&
                        stockchange.HolderShortName.NormalizeTextResult().Equals(increase_Result.HolderShortName.NormalizeTextResult()))
                    {
                        COR_HolderShortName++;
                    }

                    if (!String.IsNullOrEmpty(stockchange.ChangePrice) &&
                        !String.IsNullOrEmpty(increase_Result.ChangePrice) &&
                        stockchange.ChangePrice.Equals(increase_Result.ChangePrice))
                    {
                        COR_ChangePrice++;
                    }

                    if (!String.IsNullOrEmpty(stockchange.ChangeNumber) &&
                        !String.IsNullOrEmpty(increase_Result.ChangeNumber) &&
                        stockchange.ChangeNumber.Equals(increase_Result.ChangeNumber))
                    {
                        COR_ChangeNumber++;
                    }
                    if (!String.IsNullOrEmpty(stockchange.HoldNumberAfterChange) &&
                        !String.IsNullOrEmpty(increase_Result.HoldNumberAfterChange) &&
                        stockchange.HoldNumberAfterChange.Equals(increase_Result.HoldNumberAfterChange))
                    {
                        COR_HoldNumberAfterChange++;
                    }
                    if (!String.IsNullOrEmpty(stockchange.HoldPercentAfterChange) &&
                        !String.IsNullOrEmpty(increase_Result.HoldPercentAfterChange) &&
                        stockchange.HoldPercentAfterChange.Equals(increase_Result.HoldPercentAfterChange))
                    {
                        COR_HoldPercentAfterChange++;
                    }
                    break;  //按照道理开说，不应该主键重复
                }
            }
        }
        var F1_ID = GetF1("公告ID", POS_ID, ACT_ID, COR_ID);
        var F1_HolderFullName = GetF1("股东全称", POS_HolderFullName, ACT_HolderFullName, COR_HolderFullName);
        var F1_HolderName = GetF1("股东简称", POS_HolderShortName, ACT_HolderShortName, COR_HolderShortName);
        var F1_ChangeEndDate = GetF1("变动截止日期", POS_ChangeEndDate, ACT_ChangeEndDate, COR_ChangeEndDate);
        var F1_ChangePrice = GetF1("变动价格", POS_ChangePrice, ACT_ChangePrice, COR_ChangePrice);
        var F1_ChangeNumber = GetF1("变动数量", POS_ChangeNumber, ACT_ChangeNumber, COR_ChangeNumber);

        var F1_HoldNumberAfterChange = GetF1("变动后持股数", POS_HoldNumberAfterChange, ACT_HoldNumberAfterChange, COR_HoldNumberAfterChange);
        var F1_HoldPercentAfterChange = GetF1("变动后持股比例", POS_HoldPercentAfterChange, ACT_HoldPercentAfterChange, COR_HoldPercentAfterChange);


        var score = (F1_ID + F1_HolderFullName + F1_HolderName + F1_ChangeEndDate +
                     F1_ChangePrice + F1_ChangeNumber + F1_HoldNumberAfterChange + F1_HoldPercentAfterChange) / 8;
        Program.Score.WriteLine("增减持score:" + score);
        Program.Score.Flush();

    }

    public static void EvaluateContract(List<struContract> result)
    {
        //POS:标准数据集中该字段不为空的记录数
        //ACT:选手提交结果中该字段不为空的记录数
        //COR:主键匹配 且 提交字段值=正确字段值 且 均不为空
        //公告ID
        var POS_ID = 0;
        var ACT_ID = 0;
        var COR_ID = 0;

        var POS_JiaFang = 0;
        var ACT_JiaFang = 0;
        var COR_JiaFang = 0;

        var POS_YiFang = 0;
        var ACT_YiFang = 0;
        var COR_YiFang = 0;

        var POS_ProjectName = 0;
        var ACT_ProjectName = 0;
        var COR_ProjectName = 0;

        var POS_ContractName = 0;
        var ACT_ContractName = 0;
        var COR_ContractName = 0;

        var POS_ContractMoneyUpLimit = 0;
        var ACT_ContractMoneyUpLimit = 0;
        var COR_ContractMoneyUpLimit = 0;

        var POS_ContractMoneyDownLimit = 0;
        var ACT_ContractMoneyDownLimit = 0;
        var COR_ContractMoneyDownLimit = 0;

        var POS_UnionMember = 0;
        var ACT_UnionMember = 0;
        var COR_UnionMember = 0;


        foreach (var contract in TraningDataset.ContractList)
        {
            if (!String.IsNullOrEmpty(contract.id)) POS_ID++;
            if (!String.IsNullOrEmpty(contract.JiaFang)) POS_JiaFang++;
            if (!String.IsNullOrEmpty(contract.YiFang)) POS_YiFang++;
            if (!String.IsNullOrEmpty(contract.ProjectName)) POS_ProjectName++;
            if (!String.IsNullOrEmpty(contract.ContractName)) POS_ContractName++;
            if (!String.IsNullOrEmpty(contract.ContractMoneyUpLimit)) POS_ContractMoneyUpLimit++;
            if (!String.IsNullOrEmpty(contract.ContractMoneyDownLimit)) POS_ContractMoneyDownLimit++;
            if (!String.IsNullOrEmpty(contract.UnionMember)) POS_UnionMember++;

        }
        foreach (var contract in result)
        {
            if (!String.IsNullOrEmpty(contract.id)) ACT_ID++;
            if (!String.IsNullOrEmpty(contract.JiaFang)) ACT_JiaFang++;
            if (!String.IsNullOrEmpty(contract.YiFang)) ACT_YiFang++;
            if (!String.IsNullOrEmpty(contract.ProjectName)) ACT_ProjectName++;
            if (!String.IsNullOrEmpty(contract.ContractName)) ACT_ContractName++;
            if (!String.IsNullOrEmpty(contract.ContractMoneyUpLimit)) ACT_ContractMoneyUpLimit++;
            if (!String.IsNullOrEmpty(contract.ContractMoneyDownLimit)) ACT_ContractMoneyDownLimit++;
            if (!String.IsNullOrEmpty(contract.UnionMember)) ACT_UnionMember++;
        }

        foreach (var contract in TraningDataset.ContractList)
        {
            var key = contract.GetKey();
            foreach (var contract_Result in result)
            {
                var key_Result = contract_Result.GetKey();
                if (key.Equals(key_Result))
                {
                    COR_ID++;
                    COR_JiaFang++;
                    COR_YiFang++;
                    if (!String.IsNullOrEmpty(contract.ProjectName) &&
                        !String.IsNullOrEmpty(contract_Result.ProjectName) &&
                        contract.ProjectName.NormalizeTextResult().Equals(contract_Result.ProjectName.NormalizeTextResult()))
                    {
                        COR_ProjectName++;
                    }

                    if (!String.IsNullOrEmpty(contract.ContractName) &&
                        !String.IsNullOrEmpty(contract_Result.ContractName) &&
                        contract.ContractName.NormalizeTextResult().Equals(contract_Result.ContractName.NormalizeTextResult()))
                    {
                        COR_ContractName++;
                    }

                    if (!String.IsNullOrEmpty(contract.ContractMoneyUpLimit) &&
                        !String.IsNullOrEmpty(contract_Result.ContractMoneyUpLimit) &&
                        contract.ContractMoneyUpLimit.Equals(contract_Result.ContractMoneyUpLimit))
                    {
                        COR_ContractMoneyUpLimit++;
                    }
                    if (!String.IsNullOrEmpty(contract.ContractMoneyDownLimit) &&
                        !String.IsNullOrEmpty(contract_Result.ContractMoneyDownLimit) &&
                        contract.ContractMoneyDownLimit.Equals(contract_Result.ContractMoneyDownLimit))
                    {
                        COR_ContractMoneyDownLimit++;
                    }
                    if (!String.IsNullOrEmpty(contract.UnionMember) &&
                        !String.IsNullOrEmpty(contract_Result.UnionMember) &&
                        contract.UnionMember.Equals(contract_Result.UnionMember))
                    {
                        COR_UnionMember++;
                    }
                    break;  //按照道理开说，不应该主键重复
                }
            }
        }
        var F1_ID = GetF1("公告ID", POS_ID, ACT_ID, COR_ID);
        var F1_JiaFang = GetF1("甲方", POS_JiaFang, ACT_JiaFang, COR_JiaFang);
        var F1_YiFang = GetF1("乙方", POS_YiFang, ACT_YiFang, COR_YiFang);
        var F1_ProjectName = GetF1("项目名称", POS_ProjectName, ACT_ProjectName, COR_ProjectName);
        var F1_ContractName = GetF1("合同名称", POS_ContractName, ACT_ContractName, COR_ContractName);
        var F1_ContractMoneyUpLimit = GetF1("金额上限", POS_ContractMoneyUpLimit, ACT_ContractMoneyUpLimit, COR_ContractMoneyUpLimit);
        var F1_ContractMoneyDownLimit = GetF1("金额下限", POS_ContractMoneyDownLimit, ACT_ContractMoneyDownLimit, COR_ContractMoneyDownLimit);
        var F1_UnionMember = GetF1("联合体成员", POS_UnionMember, ACT_UnionMember, COR_UnionMember);

        var score = (F1_ID + F1_JiaFang + F1_YiFang + F1_ProjectName +
        F1_ContractName + F1_ContractMoneyUpLimit + F1_ContractMoneyDownLimit + F1_UnionMember) / 8;
        Program.Score.WriteLine("合同score:" + score);
        Program.Score.Flush();

    }

    static double GetF1(String ItemName, double POS, double ACT, double COR)
    {
        //Recall = COR / POS
        //Precision = COR/ ACT
        //F1 = 2 * Recall * Precision / (Recall + Precision)
        double Recall = COR / POS;
        double Precision = COR / ACT;
        double F1 = 2 * Recall * Precision / (Recall + Precision);
        if (POS == 0 || ACT == 0) F1 = 0;
        Program.Score.WriteLine("Item:" + ItemName);
        Program.Score.WriteLine("POS:" + POS.ToString());
        Program.Score.WriteLine("ACT:" + ACT.ToString());
        Program.Score.WriteLine("COR:" + COR.ToString());
        Program.Score.WriteLine("F1:" + F1.ToString());
        return F1;
    }
}