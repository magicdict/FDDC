using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FDDC;
using JiebaNet.Segmenter.PosSeg;

public class ContractTraning
{
    public static int MaxJiaFangLength = 999;
    public static string MaxJiaFang = String.Empty;

    public static int MaxYiFangLength = 999;
    public static string MaxYiFang = String.Empty;

    public static int MaxContractNameLength = 999;
    public static string MaxContractName = String.Empty;

    public static int MaxProjectNameLength = 999;
    public static string MaxProjectName = String.Empty;

    public static double MinAmount = double.MaxValue;


    public static void Train()
    {
        TraningMaxLenth();
        EntityWordPerperty();
        AnlayzeEntitySurroundWords();
    }

    public static void GetListLeadWords()
    {
        var ContractPath_TRAIN = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同";
        var dict = new Dictionary<String, int>();
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + @"\txt\"))
        {
            var SR = new StreamReader(filename);
            while (!SR.EndOfStream)
            {
                var line = SR.ReadLine();
                var idx = line.IndexOf("：");
                if (idx >= 1 && idx <= 7)
                {
                    var w = line.Substring(0, idx);
                    if (dict.ContainsKey(w))
                    {
                        dict[w] = dict[w] + 1;
                    }
                    else
                    {
                        dict.Add(w, 1);
                    }
                }
            }
        }
        Program.Training.WriteLine("列表前导词：");
        Utility.FindTop(20, dict);
    }

    public static void AnlayzeEntitySurroundWords()
    {
        var ContractPath_TRAIN = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同";
        var JiaFangS = new Surround();
        var YiFangS = new Surround();
        var ProjectNameS = new Surround();
        var ContractNameS = new Surround();

        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + @"\html\"))
        {
            var fi = new System.IO.FileInfo(filename);
            var Id = fi.Name.Replace(".html", String.Empty);
            if (TraningDataset.GetContractById(Id).Count == 0) continue;
            var contract = TraningDataset.GetContractById(Id).First();
            if (contract.JiaFang == String.Empty) continue;
            var root = HTMLEngine.Anlayze(filename,"");
            if (!string.IsNullOrEmpty(contract.JiaFang)) JiaFangS.AnlayzeEntitySurroundWords(root, contract.JiaFang);
            if (!string.IsNullOrEmpty(contract.YiFang)) YiFangS.AnlayzeEntitySurroundWords(root, contract.YiFang);
            if (!string.IsNullOrEmpty(contract.ProjectName)) ProjectNameS.AnlayzeEntitySurroundWords(root, contract.ProjectName);
            if (!string.IsNullOrEmpty(contract.ContractName)) ContractNameS.AnlayzeEntitySurroundWords(root, contract.ContractName);
        }
        Program.Training.WriteLine("甲方附近词语分析：");
        JiaFangS.WriteTop(10);
        Program.Training.WriteLine("乙方附近词语分析：");
        YiFangS.WriteTop(10);
        Program.Training.WriteLine("工程名附近词语分析：");
        ProjectNameS.WriteTop(10);
        Program.Training.WriteLine("合同名附近词语分析：");
        ContractNameS.WriteTop(10);

    }

    public static void AnlayzeEntitySurroundWordsLTP()
    {
        var ContractPath_TRAIN = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同";
        var ProjectNameS = new LTPTraining();
        var ContractNameS = new LTPTraining();
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + @"\html\"))
        {
            var fi = new System.IO.FileInfo(filename);
            var Id = fi.Name.Replace(".html", String.Empty);
            if (TraningDataset.GetContractById(Id).Count == 0) continue;
            var contract = TraningDataset.GetContractById(Id).First();
            var c = new Contract(filename);
            if (!string.IsNullOrEmpty(contract.ProjectName)) ProjectNameS.Training(c.Dplist, contract.ProjectName);
            if (!string.IsNullOrEmpty(contract.ContractName)) ContractNameS.Training(c.Dplist, contract.ContractName);
        }
        Program.Training.WriteLine("工程名附近词语分析：");
        ProjectNameS.WriteTop(10);
        Program.Training.WriteLine("合同名附近词语分析：");
        ContractNameS.WriteTop(10);

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

            if (!string.IsNullOrEmpty(c.ContractMoneyUpLimit))
            {
                var m = 0.0;
                if (double.TryParse(c.ContractMoneyUpLimit, out m))
                {
                    if (m < MinAmount) MinAmount = m;
                }
            }

            var ProjectNameList = c.ProjectName.Split("、");
            foreach (var jn in ProjectNameList)
            {
                if (jn.Contains(",")) continue;
                var TEProjectName = EntityWordAnlayzeTool.TrimEnglish(jn);
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
        Program.Training.WriteLine("最小金额:" + MinAmount);
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
        EntitySelf.Init();
        foreach (var contract in TraningDataset.ContractList)
        {
            EntitySelf.PutEntityWordPerperty(contract.JiaFang);
        }
        EntitySelf.WriteFirstAndLengthWordToLog();

        Program.Training.WriteLine("乙方统计：");
        EntitySelf.Init();
        foreach (var contract in TraningDataset.ContractList)
        {
            EntitySelf.PutEntityWordPerperty(contract.YiFang);
        }
        EntitySelf.WriteFirstAndLengthWordToLog();


        Program.Training.WriteLine("合同统计：");
        EntitySelf.Init();
        foreach (var contract in TraningDataset.ContractList)
        {
            EntitySelf.PutEntityWordPerperty(contract.ContractName);
        }
        EntitySelf.WriteFirstAndLengthWordToLog();

        Program.Training.WriteLine("工程统计：");
        EntitySelf.Init();
        foreach (var contract in TraningDataset.ContractList)
        {
            EntitySelf.PutEntityWordPerperty(contract.ProjectName);
        }
        EntitySelf.WriteFirstAndLengthWordToLog();
    }
}