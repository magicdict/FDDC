using JiebaNet.Analyser;
using JiebaNet.Segmenter;
using System;
using System.Text;

public static class WordAnlayze
{
    public static JiebaSegmenter segmenter = new JiebaSegmenter();
    public static void Init()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
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

    public static void Anlayze(HTMLEngine.MyRootHtmlNode root)
    {
        foreach (var paragrah in root.Children)
        {
            var segments = segmenter.Cut(paragrah.FullText);  // 默认为精确模式
            Console.WriteLine("【精确模式】：{0}", string.Join("/ ", segments));
        }
    }
}