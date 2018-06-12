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
        EntityWordPerperty();
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
            var TEJiaFang = EntityWordAnlayzeTool.TrimEnglish(c.JiaFang);
            if (TEJiaFang.Length > MaxJiaFangLength)
            {
                MaxJiaFangLength = TEJiaFang.Length;
                MaxJiaFang = TEJiaFang;
            }

            var TEYiFang = EntityWordAnlayzeTool.TrimEnglish(c.YiFang);
            if (TEYiFang.Length > MaxYiFangLength)
            {
                MaxYiFangLength = TEYiFang.Length;
                MaxYiFang = TEYiFang;
            }

            var ContractList = c.ContractName.Split("、");
            foreach (var cn in ContractList)
            {
                var TEContractName = EntityWordAnlayzeTool.TrimEnglish(cn);
                if (TEContractName.Length > MaxContractNameLength)
                {
                    MaxContractNameLength = TEContractName.Length;
                    MaxContractName = TEContractName;
                }
            }

            var ProjectNameList = c.ProjectName.Split("、");
            foreach (var jn in ProjectNameList)
            {
                if (jn.Contains(",")) continue;
                var TEProjectName =  EntityWordAnlayzeTool.TrimEnglish(jn);
                if (TEProjectName.Length > MaxContractNameLength)
                {
                    MaxProjectNameLength = TEProjectName.Length;
                    MaxProjectName = TEProjectName;
                }
            }

        }
        Program.Training.WriteLine("最大甲方(除去英语)长度:" + MaxJiaFangLength);
        Program.Training.WriteLine("最大甲方(除去英语):" + MaxJiaFang);
        Program.Training.WriteLine("最大乙方(除去英语)长度:" + MaxYiFangLength);
        Program.Training.WriteLine("最大乙方(除去英语):" + MaxYiFang);
        Program.Training.WriteLine("最大合同(除去英语)长度:" + MaxContractNameLength);
        Program.Training.WriteLine("最大合同(除去英语):" + MaxContractName);
        Program.Training.WriteLine("最大工程(除去英语)长度:" + MaxProjectNameLength);
        Program.Training.WriteLine("最大工程(除去英语):" + MaxProjectName);
        //新建北京至石家庄铁路客运专线石家庄枢纽(北京局代建部分)站场工程一个标段
        //新建大塔至四眼井铁路吴四圪堵至四眼井段站前工程wssg-1标段
    }

    //实体自身特性分析
    public static void EntityWordPerperty()
    {
        var posSeg = new PosSegmenter();
        //首单词统计
        var FirstWordPos = new Dictionary<String, int>();
        var WordLength = new Dictionary<int, int>();

        Program.Training.WriteLine("甲方统计：");
        EntityWordAnlayzeTool.Init();
        foreach (var contract in TraningDataset.ContractList)
        {
            EntityWordAnlayzeTool.PutEntityWordPerperty(contract.JiaFang);
        }
        EntityWordAnlayzeTool.WriteFirstAndLengthWordToLog();

        Program.Training.WriteLine("乙方统计：");
        EntityWordAnlayzeTool.Init();
        foreach (var contract in TraningDataset.ContractList)
        {
            EntityWordAnlayzeTool.PutEntityWordPerperty(contract.YiFang);
        }
        EntityWordAnlayzeTool.WriteFirstAndLengthWordToLog();


        Program.Training.WriteLine("合同统计：");
        EntityWordAnlayzeTool.Init();
        foreach (var contract in TraningDataset.ContractList)
        {
            EntityWordAnlayzeTool.PutEntityWordPerperty(contract.ContractName);
        }
        EntityWordAnlayzeTool.WriteFirstAndLengthWordToLog();

        Program.Training.WriteLine("工程统计：");
        EntityWordAnlayzeTool.Init();
        foreach (var contract in TraningDataset.ContractList)
        {
            EntityWordAnlayzeTool.PutEntityWordPerperty(contract.ProjectName);
        }
        EntityWordAnlayzeTool.WriteFirstAndLengthWordToLog();
    }
}