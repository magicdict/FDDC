using FDDC;
using JiebaNet.Analyser;
using JiebaNet.Segmenter;
using JiebaNet.Segmenter.PosSeg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
public static class EntityWordAnlayzeTool
{

    public static string GetMainWordSentence(string OrgString)
    {
        var MainWordSentence = "";
        var pos = new JiebaNet.Segmenter.PosSeg.PosSegmenter();
        var list = pos.Cut(OrgString);
        foreach (var word in list)
        {
            //去除“副词”和“了”之后的句子
            if (word.Flag != WordUtility.助词 &&
                word.Flag != WordUtility.副词)
            {
                MainWordSentence += word.Word;
            }
        }
        return MainWordSentence;
    }

    public static string TrimEnglish(string OrgString)
    {
        var MainWordSentence = "";
        var pos = new JiebaNet.Segmenter.PosSeg.PosSegmenter();
        var list = pos.Cut(OrgString);
        foreach (var word in list)
        {
            //去除“副词”和“了”之后的句子
            if (word.Flag != WordUtility.英语)
            {
                MainWordSentence += word.Word;
            }
        }
        return MainWordSentence;
    }

    public static string TrimLeadingUL(string OrgString)
    {
        var MainWordSentence = "";
        var pos = new JiebaNet.Segmenter.PosSeg.PosSegmenter();
        var list = pos.Cut(OrgString);
        var HasStart = false;
        foreach (var word in list)
        {
            if (HasStart || (word.Flag != WordUtility.助词))
            {
                HasStart = true;
                MainWordSentence += word.Word;
            }
        }
        return MainWordSentence;
    }


    public static void ConsoleWritePos(string OrgString)
    {
        var pos = new JiebaNet.Segmenter.PosSeg.PosSegmenter();
        var list = pos.Cut(OrgString);
        foreach (var item in list)
        {
            Console.WriteLine(item.Word + ":" + item.Flag);
        }
    }

}