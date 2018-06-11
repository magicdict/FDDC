using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        FirstWordAndLength();
        AnlayzeEntitySurroundWords();
    }



    public static void AnlayzeEntitySurroundWords()
    {
        var ContractPath_TRAIN = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同";
        Console.WriteLine("前导词：甲方");
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + @"\html\"))
        {
            var fi = new System.IO.FileInfo(filename);
            var Id = fi.Name.Replace(".html", "");
            if (TraningDataset.GetContractById(Id).Count == 0) continue;
            var contract = TraningDataset.GetContractById(Id).First();
            if (contract.JiaFang == "") continue;
            var root = HTMLEngine.Anlayze(filename);
            EntityWordAnlayzeTool.AnlayzeEntitySurroundWords(root, contract.JiaFang);
        }
    }

    //最大长度
    public static void TraningMaxLenth()
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

    public static void FirstWordAndLength()
    {
        var posSeg = new PosSegmenter();
        //首单词统计
        var FirstWordPos = new Dictionary<String, int>();
        var WordLength = new Dictionary<int, int>();

        Program.Training.WriteLine("甲方统计：");
        EntityWordAnlayzeTool.Init();
        foreach (var contract in TraningDataset.ContractList)
        {
            EntityWordAnlayzeTool.PutFirstAndLengthWord(contract.JiaFang);
        }
        EntityWordAnlayzeTool.WriteFirstAndLengthWordToLog();

        Program.Training.WriteLine("乙方统计：");
        EntityWordAnlayzeTool.Init();
        foreach (var contract in TraningDataset.ContractList)
        {
            EntityWordAnlayzeTool.PutFirstAndLengthWord(contract.YiFang);
        }
        EntityWordAnlayzeTool.WriteFirstAndLengthWordToLog();


        Program.Training.WriteLine("合同统计：");
        EntityWordAnlayzeTool.Init();
        foreach (var contract in TraningDataset.ContractList)
        {
            EntityWordAnlayzeTool.PutFirstAndLengthWord(contract.ContractName);
        }
        EntityWordAnlayzeTool.WriteFirstAndLengthWordToLog();

        Program.Training.WriteLine("工程统计：");
        EntityWordAnlayzeTool.Init();
        foreach (var contract in TraningDataset.ContractList)
        {
            EntityWordAnlayzeTool.PutFirstAndLengthWord(contract.ProjectName);
        }
        EntityWordAnlayzeTool.WriteFirstAndLengthWordToLog();
    }
}