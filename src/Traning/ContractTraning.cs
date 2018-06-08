using System;
using System.Collections.Generic;
using System.IO;
using FDDC;
using JiebaNet.Segmenter.PosSeg;

public class ContractTraning
{
    public static int MaxJiaFangLength = 999;
    public static string MaxJiaFang = "";

    public static int MaxYiFangLength = 999;
    public static string MaxYiFang = "";

    public static int MaxContractNameLength = 999;
    public static string MaxContractName = "";

    public static int MaxProjectNameLength = 999;
    public static string MaxProjectName = "";



    public static void Train()
    {
        TraningMaxLenth();
        EntityWordAnlayze();
    }

    static void TraningMaxLenth()
    {
        MaxJiaFangLength = 0;
        MaxYiFangLength = 0;
        MaxContractNameLength = 0;
        MaxProjectNameLength = 0;
        foreach (var c in TraningDataset.ContractList)
        {
            if (c.JiaFang.Length > MaxJiaFangLength)
            {
                MaxJiaFangLength = c.JiaFang.Length;
                MaxJiaFang = c.JiaFang;
            }
            if (c.YiFang.Length > MaxYiFangLength)
            {
                MaxYiFangLength = c.YiFang.Length;
                MaxYiFang = c.YiFang;
            }

            var ContractList = c.ContractName.Split("、");
            foreach (var cn in ContractList)
            {
                if (cn.Length > MaxContractNameLength)
                {
                    MaxContractNameLength = cn.Length;
                    MaxContractName = cn;
                }
            }

            var ProjectNameList = c.ProjectName.Split("、");
            foreach (var jn in ProjectNameList)
            {
                if (jn.Contains(",")) continue;
                if (jn.Length > MaxContractNameLength)
                {
                    MaxProjectNameLength = jn.Length;
                    MaxProjectName = jn;
                }
            }

        }
        Program.Training.WriteLine("最大甲方长度:" + MaxJiaFangLength);
        Program.Training.WriteLine("最大甲方:" + MaxJiaFang);
        Program.Training.WriteLine("最大乙方长度:" + MaxYiFangLength);
        Program.Training.WriteLine("最大乙方:" + MaxYiFang);
        Program.Training.WriteLine("最大合同长度:" + MaxContractNameLength);
        Program.Training.WriteLine("最大合同:" + MaxContractName);
        Program.Training.WriteLine("最大工程长度:" + MaxProjectNameLength);
        Program.Training.WriteLine("最大工程:" + MaxProjectName);
        //新建北京至石家庄铁路客运专线石家庄枢纽(北京局代建部分)站场工程一个标段
        //新建大塔至四眼井铁路吴四圪堵至四眼井段站前工程wssg-1标段
    }

    static void EntityWordAnlayze()
    {
        var posSeg = new PosSegmenter();
        //首单词统计
        var FirstWordPos = new Dictionary<String, int>();
        var WordLength = new Dictionary<int, int>();

        Program.Training.WriteLine("甲方统计：");
        PropertyWordAnlayze.Init();
        foreach (var contract in TraningDataset.ContractList)
        {
            PropertyWordAnlayze.PutWord(contract.JiaFang);
        }
        PropertyWordAnlayze.WriteToLog();

        Program.Training.WriteLine("乙方统计：");
        PropertyWordAnlayze.Init();
        foreach (var contract in TraningDataset.ContractList)
        {
            PropertyWordAnlayze.PutWord(contract.YiFang);
        }
        PropertyWordAnlayze.WriteToLog();


        Program.Training.WriteLine("合同统计：");
        PropertyWordAnlayze.Init();
        foreach (var contract in TraningDataset.ContractList)
        {
            PropertyWordAnlayze.PutWord(contract.ContractName);
        }
        PropertyWordAnlayze.WriteToLog();

        Program.Training.WriteLine("工程统计：");
        PropertyWordAnlayze.Init();
        foreach (var contract in TraningDataset.ContractList)
        {
            PropertyWordAnlayze.PutWord(contract.ProjectName);
        }
        PropertyWordAnlayze.WriteToLog();
    }
}