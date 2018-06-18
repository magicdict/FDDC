using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FDDC;

public static class TraningDataset
{
    static string ContractPath_TRAIN = Program.DocBase + @"\FDDC_SRC\Result\Train\hetong.train";
    static string StockChangePath_TRAIN = Program.DocBase + @"\FDDC_SRC\Result\Train\zengjianchi.train";
    static string IncreaseStockPath_TRAIN = Program.DocBase + @"\FDDC_SRC\Result\Train\dingzeng.train";

    public static List<Contract.struContract> ContractList = new List<Contract.struContract>();

    public static void InitContract()
    {
        var sr = new StreamReader(ContractPath_TRAIN);
        while (!sr.EndOfStream)
        {
            var c = Contract.struContract.ConvertFromString(sr.ReadLine());
            ContractList.Add(c);
        }
        Console.WriteLine("合同标准结果数:" + ContractList.Count);
        sr.Close();
    }

    public static List<Contract.struContract> GetContractById(string id)
    {
        return ContractList.Where((c) => { return c.id == id; }).ToList();
    }


    public static List<IncreaseStock.struIncreaseStock> IncreaseStockList = new List<IncreaseStock.struIncreaseStock>();
    public static void InitIncreaseStock()
    {
        var sr = new StreamReader(IncreaseStockPath_TRAIN);
        while (!sr.EndOfStream)
        {
            IncreaseStockList.Add(IncreaseStock.struIncreaseStock.ConvertFromString(sr.ReadLine()));
        }
        Console.WriteLine("定增标准结果数:" + IncreaseStockList.Count);
        sr.Close();
    }



    public static List<StockChange.struStockChange> StockChangeList = new List<StockChange.struStockChange>();
    public static void InitStockChange()
    {
        var sr = new StreamReader(StockChangePath_TRAIN);
        while (!sr.EndOfStream)
        {
            StockChangeList.Add(StockChange.struStockChange.ConvertFromString(sr.ReadLine()));
        }
        Console.WriteLine("增减持标准结果数:" + StockChangeList.Count);
        sr.Close();
    }


}