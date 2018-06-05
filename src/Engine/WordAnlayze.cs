using JiebaNet.Analyser;
using JiebaNet.Segmenter;
using JiebaNet.Segmenter.PosSeg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class WordAnlayze
{
    public static JiebaSegmenter segmenter = new JiebaSegmenter();


    public static void CompanyAnlayze()
    {
        var posSeg = new PosSegmenter();
        //甲方乙方首单词统计
        var FirstWordPos = new Dictionary<String, int>();
        var WordLength = new Dictionary<int, int>();
        FDDC.Program.Logger.WriteLine("甲方乙方统计：");
        PropertyWordAnlayze.Init();
        foreach (var contract in Traning.ContractList)
        {
            PropertyWordAnlayze.PutWord(contract.JiaFang);
            PropertyWordAnlayze.PutWord(contract.YiFang);
        }
        PropertyWordAnlayze.WriteToLog();

        FDDC.Program.Logger.WriteLine("合同统计：");
        PropertyWordAnlayze.Init();
        foreach (var contract in Traning.ContractList)
        {
            PropertyWordAnlayze.PutWord(contract.ContractName);
        }
        PropertyWordAnlayze.WriteToLog();

        FDDC.Program.Logger.WriteLine("工程统计：");
        PropertyWordAnlayze.Init();
        foreach (var contract in Traning.ContractList)
        {
            PropertyWordAnlayze.PutWord(contract.ProjectName);
        }
        PropertyWordAnlayze.WriteToLog();

    }


    public static void AddCustomerWord()
    {
        //加入所有实体：甲方，乙方
        foreach (var contract in Traning.ContractList)
        {
            if (!String.IsNullOrEmpty(contract.JiaFang))
            {
                segmenter.AddWord(contract.JiaFang);
            }
            if (!String.IsNullOrEmpty(contract.YiFang))
            {
                segmenter.AddWord(contract.YiFang);
            }
            if (!String.IsNullOrEmpty(contract.ContractName))
            {
                segmenter.AddWord(contract.ContractName);
            }
            if (!String.IsNullOrEmpty(contract.ProjectName))
            {
                segmenter.AddWord(contract.ProjectName);
            }
        }
    }

    public static void Anlayze(HTMLEngine.MyRootHtmlNode root, string KeyWord)
    {
        Console.WriteLine("关键字：[" + KeyWord + "]");

        foreach (var paragrah in root.Children)
        {
            var segments = segmenter.Cut(paragrah.FullText).ToList();  // 默认为精确模式
            Console.WriteLine("【精确模式】：{0}", string.Join("/ ", segments));
            //寻找关键字的位置
            for (int i = 0; i < segments.Count; i++)
            {
                if (segments[i].Equals(KeyWord))
                {
                    //前5个词语和后五个词语
                    var startInx = Math.Max(0, i - 5);
                    var EndInx = Math.Min(i + 5, segments.Count);
                    for (int s = startInx; s < i; s++)
                    {
                        Console.WriteLine("前导关键字：[" + segments[s] + "]");
                    }
                    Console.WriteLine("关键字：[" + KeyWord + "]");
                    for (int s = i + 1; s < EndInx; s++)
                    {
                        Console.WriteLine("后续关键字：[" + segments[s] + "]");
                    }

                }
            }
        }
    }
}