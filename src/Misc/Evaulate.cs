using System.Collections.Generic;
using static IncreaseStock;
using System;
using static StockChange;
using static Contract;
using FDDC;
using System.Linq;

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
        var IDList = TraningDataset.IncreaseStockList.Select((x) => { return x.id; }).Distinct().ToList();
        foreach (var id in IDList)
        {
            var trainingCnt = TraningDataset.IncreaseStockList.Count((x) => { return x.id == id; });
            var resultCnt = result.Count((x) => { return x.id == id; });
            if (Math.Abs(trainingCnt - resultCnt) > 5)
            {
                Program.Evaluator.WriteLine("ID:" + id + " Training:" + trainingCnt + "  Result:" + resultCnt);
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
                            Program.Evaluator.WriteLine("[" + stockchange.id + "]未发现持股数：" + stockchange.HoldNumberAfterChange);
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

        var IDList = TraningDataset.StockChangeList.Select((x) => { return x.id; }).Distinct().ToList();
        foreach (var id in IDList)
        {
            var trainingCnt = TraningDataset.StockChangeList.Count((x) => { return x.id == id; });
            var resultCnt = result.Count((x) => { return x.id == id; });
            if (Math.Abs(trainingCnt - resultCnt) > 5)
            {
                Program.Evaluator.WriteLine("ID:" + id + " Training:" + trainingCnt + "  Result:" + resultCnt);
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

        foreach (var contract in TraningDataset.ContractList)
        {
            foreach (var contract_Result in result)
            {
                if (contract.id == contract_Result.id)
                {
                    var key = contract.GetKey();
                    var key_Result = contract_Result.GetKey();
                    break;  //按照道理开说，不应该主键重复
                }
            }
        }

        /*        
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
        */
        var score = 0;
        Program.Score.WriteLine("合同score:" + score);
        Program.Score.Flush();

    }

    public class EvaluateItem
    {
        String ItemName = "";
        int POS = 0;
        int ACT = 0;
        int COR = 0;

        int CorrectCnt = 0;
        int WrongCnt = 0;
        int NotPickCnt = 0;
        int MistakePickCnt = 0;


        public void PutData(bool IsKeyMatch, string StardardValue, string EvaluateValue)
        {
            //POS:标准数据集中该字段不为空的记录数
            //ACT:选手提交结果中该字段不为空的记录数
            //COR:主键匹配 且 提交字段值=正确字段值 且 均不为空
            if (String.IsNullOrEmpty(StardardValue)) POS++;
            if (String.IsNullOrEmpty(EvaluateValue)) ACT++;
            if (!String.IsNullOrEmpty(StardardValue))
            {
                //存在标准值
                if (!String.IsNullOrEmpty(EvaluateValue))
                {
                    //存在标准值 存在测评值
                    if (StardardValue.Equals(EvaluateValue))
                    {
                        CorrectCnt++;
                        if (IsKeyMatch) COR++;
                    }
                    else
                    {
                        WrongCnt++;
                    }
                }
                else
                {
                    //存在标准值 不存在测评值
                    NotPickCnt++;
                }
            }
            else
            {
                //不存在标准值
                if (!String.IsNullOrEmpty(EvaluateValue))
                {
                    MistakePickCnt++;
                }
            }
        }

        public void WriteScore()
        {
            Program.Evaluator.WriteLine("标准数据集数：" + POS);
            Program.Evaluator.WriteLine("测评数据集数：" + ACT);
            Program.Evaluator.WriteLine("主键匹配据集数：" + COR);
            Program.Evaluator.WriteLine("正确：" + CorrectCnt);
            Program.Evaluator.WriteLine("错误：" + WrongCnt);
            Program.Evaluator.WriteLine("未检出：" + NotPickCnt);
            Program.Evaluator.WriteLine("错检出：" + MistakePickCnt);
        }

        public double F1
        {
            get
            {
                return Evaluate.GetF1(ItemName, POS, ACT, COR);
            }
        }
    }



    static double GetF1(String ItemName, double POS, double ACT, double COR)
    {
        //POS:标准数据集中该字段不为空的记录数
        //ACT:选手提交结果中该字段不为空的记录数
        //COR:主键匹配 且 提交字段值=正确字段值 且 均不为空
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