using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// SRL统计
/// 关键字包含在那个SRL的WORD中，以及ARG的类型
/// </summary>
public class LTPTrainingSRL
{

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
                //Console.WriteLine(element);
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
        string XmlMark = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>";
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
                    var CloneArgs = new List<struWordSRLARG>();
                    for (int argIdx = 0; argIdx < wordsrllist[srlIdx].args.Count; argIdx++)
                    {
                        var arg = wordsrllist[srlIdx].args[argIdx];
                        var srl = String.Empty;
                        for (int idx = arg.Begin; idx <= arg.End; idx++)
                        {
                            srl += wordsrllist[idx].cont;
                        }
                        arg.cont = srl;
                        CloneArgs.Add(arg);
                    }
                    wordsrllist[srlIdx].args.Clear();
                    wordsrllist[srlIdx].args.AddRange(CloneArgs);
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
            var CloneArgs = new List<struWordSRLARG>();
            for (int argIdx = 0; argIdx < wordsrllist[srlIdx].args.Count; argIdx++)
            {
                var arg = wordsrllist[srlIdx].args[argIdx];
                var srl = String.Empty;
                for (int idx = arg.Begin; idx <= arg.End; idx++)
                {
                    srl += wordsrllist[idx].cont;
                }
                arg.cont = srl;
                CloneArgs.Add(arg);
            }
            wordsrllist[srlIdx].args.Clear();
            wordsrllist[srlIdx].args.AddRange(CloneArgs);
        }
        srllist.Add(wordsrllist);
        sr.Close();
        return srllist;
    }

    #endregion

    public Dictionary<struSrlTraning, int> WordArgDict = new Dictionary<struSrlTraning, int>();
    public struct struSrlTraning
    {
        public string word;

        public string relate;

        public string argtype;

        public override string ToString()
        {
            return "Arg Type:" + argtype + " Relate:" + relate + " Word:" + word;
        }
    }

    /// <summary>
    /// 训练
    /// </summary>
    /// <param name="srlList"></param>
    /// <param name="KeyWord"></param>
    /// <returns></returns>
    public List<struSrlTraning> Training(List<List<struWordSRL>> srlList, string KeyWord, bool OnlyFirstTime = true)
    {
        List<struSrlTraning> list = new List<struSrlTraning>();
        foreach (var paragragh in srlList)
        {
            foreach (var word in paragragh)
            {
                if (word.args.Count != 0)
                {
                    foreach (var arg in word.args)
                    {
                        if (arg.cont.NormalizeTextResult().Contains(KeyWord.NormalizeTextResult()))
                        {
                            var x = new struSrlTraning()
                            {
                                word = word.cont,
                                //argtype = arg.type,
                                //relate = word.relate
                            };
                            list.Add(x);
                            if (WordArgDict.ContainsKey(x))
                            {
                                WordArgDict[x]++;
                            }
                            else
                            {
                                WordArgDict.Add(x, 1);
                            }
                            if (OnlyFirstTime) return list;
                        }
                    }
                }
            }
        }
        return list;
    }

        public void WriteToLog(StreamWriter logger)
    {
        var ranks = Utility.FindTop(5, WordArgDict);
        logger.WriteLine("前导词语：");
        foreach (var rank in ranks)
        {
            logger.WriteLine(rank.ToString());
        }
    }
}