using System;
using System.Collections.Generic;
using System.IO;
public class LTP
{

    public static string XmlMark = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>";

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
        }
    }

    public struct struWordSRLARG
    {
        public int id;

        public string type;

        public int Begin;

        public int End;
        //  <arg id="0" type="�&#x0D;" beg="7" end="12" />

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

        }
    }
    public static List<String> AnlayzeSRL(string xmlfilename)
    {
        //由于结果是多个XML构成的
        //1.掉所有的<?xml version="1.0" encoding="utf-8" ?>
        //2.加入<sentence></sentence> root节点    
        var SRLList = new List<String>();
        if (!File.Exists(xmlfilename)) return SRLList;
        var sr = new StreamReader(xmlfilename);
        List<struWordSRL> wl = new List<struWordSRL>();
        var al = new List<struWordSRLARG>();
        var ner = String.Empty;
        while (!sr.EndOfStream)
        {
            var line = sr.ReadLine().Trim();
            if (line == XmlMark)
            {
                foreach (var arg in al)
                {
                    ner = "";
                    for (int i = arg.Begin; i <= arg.End; i++)
                    {
                        ner += wl[i].cont;
                    }
                    SRLList.Add(ner);
                }
                al.Clear();
                wl.Clear();
            }
            if (line.StartsWith("<arg"))
            {
                al.Add(new struWordSRLARG(line));
            }
            if (line.StartsWith("<word"))
            {
                var word = new struWordSRL(line);
                wl.Add(word);
            }
        }

        foreach (var arg in al)
        {
            ner = "";
            for (int i = arg.Begin; i <= arg.End; i++)
            {
                ner += wl[i].cont;
            }
            SRLList.Add(ner);
        }
        sr.Close();
        return SRLList;
    }

#endregion
}