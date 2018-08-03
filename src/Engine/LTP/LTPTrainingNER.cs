using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
public class LTPTrainingNER
{

    public const string 地名 = "ns";
    public const string 机构团体 = "nt";
    public const string 副词 = "d";
    public const string 助词 = "ul";
    public const string 英语 = "eng";
    public const string 词性标点 = "x";
    public const string 动词 = "v";
    public const string 数词 = "m";
    /// <summary>
    /// 连词
    /// </summary>
    public const string 连词 = "c";

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
                if (x.Count == 3)
                {
                    //Console.WriteLine(element);
                    id = int.Parse(x[0]);
                    cont = "\"";    //&quot;
                    pos = x[1];
                    ne = x[2];
                }
                else
                {
                    id = int.Parse(x[0]);
                    cont = "";
                    pos = "";
                    ne = "";
                }
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

    public enum enmNerType
    {
        /// <summary>
        /// 人名
        /// </summary>
        Nh,
        /// <summary>
        /// 机构名
        /// </summary>
        Ni,
        /// <summary>
        /// 地名
        /// </summary>
        Ns
    }

    public struct struNerInfo
    {
        public string RawData;

        public enmNerType Type;
    }

    public static List<struNerInfo> AnlayzeNER(string xmlfilename)
    {
        //由于结果是多个XML构成的
        //1.掉所有的<?xml version="1.0" encoding="utf-8" ?>
        //2.加入<sentence></sentence> root节点    
        var NerList = new List<struNerInfo>();
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
                        if ((int)(ner.ToCharArray()[0]) == 61548)
                        {
                            ner = ner.Substring(1);
                        }
                        NerList.Add(new struNerInfo()
                        {
                            RawData = ner.Replace("股东", ""),
                            Type = enmNerType.Ni
                        });
                        break;
                    case "B-Ns":
                        ner = word.cont;
                        break;
                    case "I-Ns":
                        ner += word.cont;
                        break;
                    case "E-Ns":
                        ner += word.cont;
                        NerList.Add(new struNerInfo() { RawData = ner, Type = enmNerType.Ns });
                        break;

                    case "B-Nh":
                        ner = word.cont;
                        break;
                    case "I-Nh":
                        ner += word.cont;
                        break;
                    case "E-Nh":
                        ner += word.cont;
                        NerList.Add(new struNerInfo() { RawData = ner, Type = enmNerType.Nh });
                        break;

                    case "S-Nh":
                        NerList.Add(new struNerInfo() { RawData = word.cont, Type = enmNerType.Nh });
                        break;
                    case "S-Ni":
                        NerList.Add(new struNerInfo() { RawData = word.cont, Type = enmNerType.Ni });
                        break;
                    case "S-Ns":
                        NerList.Add(new struNerInfo() { RawData = word.cont, Type = enmNerType.Ns });
                        break;
                }
            }
        }
        if (wl != null) pl.Add(wl);
        sr.Close();
        return NerList;
    }
}