using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
public class LTP
{

    public static string XmlMark = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>";

    #region  POS
    public const string 地名 = "ns";
    public const string 机构团体 = "nt";
    public const string 副词 = "d";
    public const string 助词 = "ul";
    public const string 英语 = "eng";

    public const string 词性标点 = "x";
    public const string 动词 = "v";
    #endregion

    #region  DP

    public const string 主谓关系 = "SBV";
    public const string 动宾关系 = "VOB";
    public const string 间宾关系 = "IOB";
    public const string 前置宾语 = "FOB";
    public const string 兼语 = "DBL";
    public const string 定中关系 = "ATT";
    public const string 状中结构 = "ADV";
    public const string 动补结构 = "CMP";
    public const string 并列关系 = "COO";
    public const string 介宾关系 = "POB";
    public const string 左附加关系 = "LAD";
    public const string 右附加关系 = "RAD";
    public const string 独立结构 = "IS";
    public const string 核心关系 = "HED";

    public const string 句型标点 = "WP";

    public struct struWordDP
    {
        public int id;

        public string cont;

        public string pos;

        public int parent;

        public string relate;

        public override string ToString()
        {
            return cont + "/" + relate;
        }

        public struWordDP(string element)
        {
            var x = RegularTool.GetMultiValueBetweenMark(element, "\"", "\"");
            if (x.Count != 5)
            {
                Console.WriteLine(element);
                id = int.Parse(x[0]);
                cont = "\"";    //&quot;
                pos = x[1];
                parent = int.Parse(x[2]);
                relate = x[3];
            }
            else
            {
                id = int.Parse(x[0]);
                cont = x[1];
                pos = x[2];
                parent = int.Parse(x[3]);
                relate = x[4];
            }
        }
    }
    public static List<List<struWordDP>> AnlayzeDP(string xmlfilename)
    {
        //由于结果是多个XML构成的
        //1.掉所有的<?xml version="1.0" encoding="utf-8" ?>
        //2.加入<sentence></sentence> root节点    
        var DPList = new List<List<struWordDP>>();

        if (!File.Exists(xmlfilename)) return DPList;

        var sr = new StreamReader(xmlfilename);
        List<struWordDP> WordList = null;
        while (!sr.EndOfStream)
        {
            var line = sr.ReadLine().Trim();
            if (line.StartsWith("<sent"))
            {
                if (WordList != null) DPList.Add(WordList);
                //一个新的句子
                WordList = new List<struWordDP>();
            }
            if (line.StartsWith("<word"))
            {
                var word = new struWordDP(line);
                WordList.Add(word);
            }
        }
        if (WordList != null) DPList.Add(WordList);
        sr.Close();
        return DPList;
    }
    #endregion

    #region  NER
    public struct struWordNER
    {
        public int id;

        public string cont;

        public string pos;

        public string ne;

        public struWordNER(string element)
        {
            var x = RegularTool.GetMultiValueBetweenMark(element, "\"", "\"");
            if (x.Count != 4)
            {
                Console.WriteLine(element);
                id = int.Parse(x[0]);
                cont = "\"";    //&quot;
                pos = x[1];
                ne = x[2];
            }
            else
            {
                id = int.Parse(x[0]);
                cont = x[1];
                pos = x[2];
                ne = x[3];
            }
        }
    }


    public static List<String> AnlayzeNER(string xmlfilename)
    {
        //由于结果是多个XML构成的
        //1.掉所有的<?xml version="1.0" encoding="utf-8" ?>
        //2.加入<sentence></sentence> root节点    
        var NerList = new List<String>();

        if (!File.Exists(xmlfilename)) return NerList;

        var sr = new StreamReader(xmlfilename);
        List<struWordNER> wl = null;
        var pl = new List<List<struWordNER>>();
        var ner = String.Empty;
        while (!sr.EndOfStream)
        {
            var line = sr.ReadLine().Trim();
            if (line.StartsWith("<sent"))
            {
                if (wl != null) pl.Add(wl);
                //一个新的句子
                wl = new List<struWordNER>();
            }
            if (line.StartsWith("<word"))
            {
                var word = new struWordNER(line);
                wl.Add(word);
                switch (word.ne)
                {
                    case "B-Ni":
                        ner = word.cont;
                        break;
                    case "I-Ni":
                        ner += word.cont;
                        break;
                    case "E-Ni":
                        ner += word.cont;
                        NerList.Add(ner);
                        break;
                }
            }
        }
        if (wl != null) pl.Add(wl);
        sr.Close();
        return NerList;
    }
    #endregion

    #region  SRL
    public struct struWordSRL
    {
        // <word id="7" cont="键桥" pos="n" ne="O" parent="8" relate="ATT" />
        public int id;

        public string cont;

        public string pos;

        public string ne;

        public string parent;

        public string relate;

        public List<struWordSRLARG> args;

        public struWordSRL(string element)
        {
            var x = RegularTool.GetMultiValueBetweenMark(element, "\"", "\"");
            if (x.Count != 6)
            {
                Console.WriteLine(element);
                id = int.Parse(x[0]);
                cont = String.Empty;    //&quot;
                pos = x[1];
                ne = x[2];
                parent = x[3];
                relate = x[4];
            }
            else
            {
                id = int.Parse(x[0]);
                cont = x[1];
                pos = x[2];
                ne = x[3];
                parent = x[4];
                relate = x[5];
            }
            args = new List<struWordSRLARG>();
        }
    }

    /// <summary>
    /// Srl Arg
    /// </summary>
    public struct struWordSRLARG
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int id;
        /// <summary>
        /// 类型
        /// </summary>
        public string type;
        /// <summary>
        /// 开始索引
        /// </summary>
        public int Begin;
        /// <summary>
        /// 结束索引
        /// </summary>
        public int End;
        /// <summary>
        /// 内容
        /// </summary>        
        public string cont;

        public struWordSRLARG(string element)
        {
            var x = RegularTool.GetMultiValueBetweenMark(element, "\"", "\"");

            if (x.Count == 3)
            {
                id = int.Parse(x[0]);
                type = "";
                Begin = int.Parse(x[1]);
                End = int.Parse(x[2]);
            }
            else
            {
                id = int.Parse(x[0]);
                type = x[1];
                Begin = int.Parse(x[2]);
                End = int.Parse(x[3]);
            }
            cont = string.Empty;
        }
    }
    public static List<List<struWordSRL>> AnlayzeSRL(string xmlfilename)
    {
        if (!File.Exists(xmlfilename)) return new List<List<struWordSRL>>();
        var srllist = new List<List<struWordSRL>>();
        var sr = new StreamReader(xmlfilename);
        var wordsrllist = new List<struWordSRL>();
        while (!sr.EndOfStream)
        {
            var line = sr.ReadLine().Trim();
            //<?xml version="1.0" encoding="utf-8" ?>
            if (line == XmlMark)
            {
                //第一次跳过
                if (wordsrllist.Count == 0) continue;
                //Arg内容的填充
                for (int srlIdx = 0; srlIdx < wordsrllist.Count; srlIdx++)
                {
                    if (wordsrllist[srlIdx].args.Count == 0) continue;
                    for (int argIdx = 0; argIdx < wordsrllist[srlIdx].args.Count; argIdx++)
                    {
                        var arg = wordsrllist[srlIdx].args[argIdx];
                        var srl = String.Empty;
                        for (int idx = arg.Begin; idx <= arg.End; idx++)
                        {
                            srl += wordsrllist[idx].cont;
                        }
                        arg.cont = srl;
                    }
                }
                srllist.Add(wordsrllist);
                wordsrllist = new List<struWordSRL>();
            }
            if (line.StartsWith("<arg"))
            {
                var wordsrl = wordsrllist.Last();
                wordsrl.args.Add(new struWordSRLARG(line));
            }
            if (line.StartsWith("<word"))
            {
                var word = new struWordSRL(line);
                wordsrllist.Add(word);
            }
        }
        //Arg内容的填充
        for (int srlIdx = 0; srlIdx < wordsrllist.Count; srlIdx++)
        {
            if (wordsrllist[srlIdx].args.Count == 0) continue;
            for (int argIdx = 0; argIdx < wordsrllist[srlIdx].args.Count; argIdx++)
            {
                var arg = wordsrllist[srlIdx].args[argIdx];
                var srl = String.Empty;
                for (int idx = arg.Begin; idx <= arg.End; idx++)
                {
                    srl += wordsrllist[idx].cont;
                }
            }
        }
        srllist.Add(wordsrllist);
        sr.Close();
        return srllist;
    }

    #endregion
}