using FDDC;
using JiebaNet.Analyser;
using JiebaNet.Segmenter;
using JiebaNet.Segmenter.PosSeg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class WordAnlayze
{

    public static void TraningByWordAnlyaze()
    {
        var ContractPath_TRAIN = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同";
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + @"\html\"))
        {
            var fi = new System.IO.FileInfo(filename);
            var Id = fi.Name.Replace(".html", "");
            if (TraningDataset.GetContractById(Id).Count == 0) continue;
            var contract = TraningDataset.GetContractById(Id).First();
            if (contract.ProjectName == "") continue;
            var root = HTMLEngine.Anlayze(filename);
            Anlayze(root, contract.ProjectName);
        }
    }

    public static void Anlayze(HTMLEngine.MyRootHtmlNode root, string KeyWord)
    {
        Console.WriteLine("关键字：[" + KeyWord + "]");
        JiebaSegmenter segmenter = new JiebaSegmenter();
        segmenter.AddWord(KeyWord);
        foreach (var paragrah in root.Children)
        {
            var segments = segmenter.Cut(paragrah.FullText.NormalizeTextResult()).ToList();  // 默认为精确模式
            //Console.WriteLine("【精确模式】：{0}", string.Join("/ ", segments));
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