using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using 金融数据整理大赛;

public static class Traning
{
    static string ContractPath_TRAIN = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\hetong.train";
    static string StockChangePath_TRAIN = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持\zengjianchi.train";
    static string IncreaseStockPath_TRAIN = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\定增\dingzeng.train";

    public static List<Contract.struContract> ContractList = new List<Contract.struContract>();

    public static void InitContract()
    {
        var sr = new StreamReader(ContractPath_TRAIN);
        while (!sr.EndOfStream)
        {
            ContractList.Add(Contract.ConvertFromString(sr.ReadLine()));
        }
        Console.WriteLine("Training Count:" + ContractList.Count);
        sr.Close();
    }

    public static List<Contract.struContract> GetContractById(string id)
    {
        return ContractList.Where((c) => { return c.id == id; }).ToList();
    }
}