using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FDDC;
using JiebaNet.Segmenter.PosSeg;
using static CIBase;

public class ContractTraning
{
    public static void Train()
    {
        Console.WriteLine("开始分析");
        //实体周围语境的统计
        AnlayzeEntitySurroundWords(); FDDC.Program.Training.Flush();
        //实体周围语境的统计LTP角度
        //AnlayzeEntitySurroundWordsLTP(); FDDC.Program.Training.Flush();
        //实体长度的统计
        TraningMaxLenth(); FDDC.Program.Training.Flush();
        //实体自身属性的统计
        EntityWordPerperty(); FDDC.Program.Training.Flush();
        Console.WriteLine("结束分析");
    }

    #region 周围环境


    public static Dictionary<String, int> JiaFangLeadingDict = new Dictionary<String, int>();
    public static Dictionary<String, int> YiFangLeadingDict = new Dictionary<String, int>();
    public static Dictionary<String, int> ProjectNameLeadingDict = new Dictionary<String, int>();
    public static Dictionary<String, int> ContractNameLeadingDict = new Dictionary<String, int>();

    public static void AnlayzeEntitySurroundWords()
    {
        var ContractPath_TRAIN = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\重大合同";
        Surround JiaFangSurround = new Surround();
        Surround YiFangSurround = new Surround();
        Surround ProjectNameSurround = new Surround();
        Surround ContractNameSurround = new Surround();
        LeadingWord JiaFangNameLeadingWord = new LeadingWord();
        LeadingWord YiFangNameLeadingWord = new LeadingWord();
        LeadingWord ProjectNameLeadingWord = new LeadingWord();
        LeadingWord ContractNameLeadingWord = new LeadingWord();
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + @"\html\"))
        {
            var fi = new System.IO.FileInfo(filename);
            var Id = fi.Name.Replace(".html", String.Empty);
            if (TraningDataset.GetContractById(Id).Count == 0) continue;
            var contract = TraningDataset.GetContractById(Id).First();
            var doc = new AnnouceDocument(filename);
            //if (!string.IsNullOrEmpty(contract.JiaFang)) JiaFangSurround.AnlayzeEntitySurroundWords(doc, contract.JiaFang);
            //if (!string.IsNullOrEmpty(contract.YiFang)) YiFangSurround.AnlayzeEntitySurroundWords(doc, contract.YiFang);
            //if (!string.IsNullOrEmpty(contract.ProjectName)) ProjectNameSurround.AnlayzeEntitySurroundWords(doc, contract.ProjectName);
            //if (!string.IsNullOrEmpty(contract.ContractName)) ContractNameSurround.AnlayzeEntitySurroundWords(doc, contract.ContractName);
            if (!string.IsNullOrEmpty(contract.JiaFang)) JiaFangNameLeadingWord.AnlayzeLeadingWord(doc, contract.JiaFang);
            if (!string.IsNullOrEmpty(contract.YiFang)) YiFangNameLeadingWord.AnlayzeLeadingWord(doc, contract.YiFang);
            if (!string.IsNullOrEmpty(contract.ProjectName)) ProjectNameLeadingWord.AnlayzeLeadingWord(doc, contract.ProjectName);
            if (!string.IsNullOrEmpty(contract.ContractName)) ContractNameLeadingWord.AnlayzeLeadingWord(doc, contract.ContractName);
        }
        //JiaFangSurround.GetTop(10);
        //YiFangSurround.GetTop(10);
        //ProjectNameSurround.GetTop(10);
        //ContractNameSurround.GetTop(10);

        JiaFangLeadingDict = JiaFangNameLeadingWord.GetTop(5);
        YiFangLeadingDict = YiFangNameLeadingWord.GetTop(5);
        ProjectNameLeadingDict = ProjectNameLeadingWord.GetTop(5);
        ContractNameLeadingDict = ContractNameLeadingWord.GetTop(5);
    }

    public static void AnlayzeEntitySurroundWordsLTP()
    {
        var ContractPath_TRAIN = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\重大合同";

        var JiaFangDP = new LTPTrainingDP();
        var JiaFangSRL = new LTPTrainingSRL();
        var YiFnagDP = new LTPTrainingDP();
        var YiFnagSRL = new LTPTrainingSRL();
        var ContractNameDP = new LTPTrainingDP();
        var ContractNameSRL = new LTPTrainingSRL();
        var ProjectNameDP = new LTPTrainingDP();
        var ProjectNameSRL = new LTPTrainingSRL();

        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + @"\html\"))
        {
            var fi = new System.IO.FileInfo(filename);
            var Id = fi.Name.Replace(".html", String.Empty);
            if (TraningDataset.GetContractById(Id).Count == 0) continue;
            var contract = TraningDataset.GetContractById(Id).First();
            var c = new Contract(filename);
            if (!string.IsNullOrEmpty(contract.JiaFang))
            {
                JiaFangDP.Training(c.Dplist, contract.JiaFang);
                JiaFangSRL.Training(c.Srllist, contract.JiaFang);
            }

            if (!string.IsNullOrEmpty(contract.YiFang))
            {
                YiFnagDP.Training(c.Dplist, contract.YiFang);
                YiFnagSRL.Training(c.Srllist, contract.YiFang);
            }

            if (!string.IsNullOrEmpty(contract.ContractName))
            {
                ContractNameDP.Training(c.Dplist, contract.ContractName);
                ContractNameSRL.Training(c.Srllist, contract.ContractName);
            }
            if (!string.IsNullOrEmpty(contract.ProjectName))
            {
                ProjectNameDP.Training(c.Dplist, contract.ProjectName);
                ProjectNameSRL.Training(c.Srllist, contract.ProjectName);
            }
        }


        Program.Training.WriteLine("甲方附近词语分析（DP）：");
        JiaFangDP.WriteTop(10);
        Program.Training.WriteLine("甲方附近词语分析（SRL）：");
        JiaFangSRL.WriteTop(10);
        Program.Training.WriteLine("乙方附近词语分析（DP）：");
        YiFnagDP.WriteTop(10);
        Program.Training.WriteLine("乙方附近词语分析（SRL）：");
        YiFnagSRL.WriteTop(10);
        Program.Training.WriteLine("合同名附近词语分析（DP）：");
        ContractNameDP.WriteTop(10);
        Program.Training.WriteLine("合同名附近词语分析（SRL）：");
        ContractNameSRL.WriteTop(10);
        Program.Training.WriteLine("工程名附近词语分析（DP）：");
        ProjectNameDP.WriteTop(10);
        Program.Training.WriteLine("工程名附近词语分析（SRL）：");
        ProjectNameSRL.WriteTop(10);
    }
    #endregion

    #region 实体自身特性分析
    public static int MaxJiaFangLength = 999;
    public static string MaxJiaFang = String.Empty;

    public static int MaxYiFangLength = 999;
    public static string MaxYiFang = String.Empty;

    public static int MaxContractNameLength = 999;
    public static string MaxContractName = String.Empty;

    public static int MaxProjectNameLength = 999;
    public static string MaxProjectName = String.Empty;

    public static double MinAmount = double.MaxValue;
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

    }


    public static EntitySelf JiaFangES = new EntitySelf();
    public static EntitySelf YiFangES = new EntitySelf();
    public static EntitySelf ContractES = new EntitySelf();
    public static EntitySelf ProjectNameES = new EntitySelf();
    /// <summary>
    /// 实体自身特性分析
    /// </summary>
    public static void EntityWordPerperty()
    {
        JiaFangES.InitFactorItem();
        YiFangES.InitFactorItem();
        ContractES.InitFactorItem();
        ProjectNameES.InitFactorItem();
        foreach (var contract in TraningDataset.ContractList)
        {
            JiaFangES.PutEntityWordPerperty(contract.JiaFang);
            YiFangES.PutEntityWordPerperty(contract.YiFang);
            ContractES.PutEntityWordPerperty(contract.ContractName);
            ProjectNameES.PutEntityWordPerperty(contract.ProjectName);
        }
        Program.Training.WriteLine("甲方统计：");
        JiaFangES.Commit();
        Program.Training.WriteLine("乙方统计：");
        YiFangES.Commit();
        Program.Training.WriteLine("合同名统计：");
        ContractES.Commit();
        Program.Training.WriteLine("工程名统计：");
        ProjectNameES.Commit();
    }
    #endregion 
}