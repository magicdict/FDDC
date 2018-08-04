using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JiebaNet.Segmenter.PosSeg;
using static LTPTrainingNER;

public class ProjectNameLogic
{
    public static List<String> GetProjectNameByNer(AnnouceDocument doc)
    {
        //由于结果是多个XML构成的
        //1.掉所有的<?xml version="1.0" encoding="utf-8" ?>
        //2.加入<sentence></sentence> root节点    
        var ProjList = new List<String>();
        if (!File.Exists(doc.NerXMLFileName)) return ProjList;
        var sr = new StreamReader(doc.NerXMLFileName);
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
            }
        }
        if (wl != null) pl.Add(wl);
        sr.Close();
        var proj = String.Empty;

        foreach (var p in pl)
        {
            for (int ScanIdx = 0; ScanIdx < p.Count; ScanIdx++)
            {
                var word = p[ScanIdx];
                if (word.ne == "B-Ns" || word.ne == "S-Ns" ||
                    word.cont == "新建")
                {
                    //遇到地名开始或者单独地名，加入到项目字符中
                    if (!string.IsNullOrEmpty(proj) && proj.StartsWith("新建"))
                    {
                        proj += word.cont;
                    }
                    else
                    {
                        proj = word.cont;
                    }
                }
                else
                {
                    if (word.cont.Equals("项目") ||
                        word.cont.Equals("工程") ||
                        word.cont.Equals("标段") ||
                        word.cont.Equals("采购"))
                    {
                        if (!String.IsNullOrEmpty(proj))
                        {
                            proj += word.cont;
                            var FurtherTo = Math.Min(p.Count, ScanIdx + 5);
                            var ShardProj = proj;

                            //标段的后检索
                            if (word.cont == "标段")
                            {
                                //检查之后3个词汇的距离是否存在项目，工程，承包
                                for (int TrailingScanIdx = ScanIdx + 1;
                                         TrailingScanIdx < FurtherTo;
                                         TrailingScanIdx++)
                                {
                                    ShardProj += p[TrailingScanIdx].cont;
                                    if (p[TrailingScanIdx].cont == "项目" ||
                                        p[TrailingScanIdx].cont == "工程" ||
                                        p[TrailingScanIdx].cont == "承包")
                                    {
                                        proj = ShardProj;
                                        break;
                                    }
                                }
                            }

                            //工程
                            if (word.cont == "工程" || word.cont == "项目")
                            {
                                //检查之后3个词汇的距离是否存在项目，工程，承包
                                var isContranBrack = false;
                                for (int TrailingScanIdx = ScanIdx + 1; TrailingScanIdx < FurtherTo;
                                                                    TrailingScanIdx++)
                                {
                                    ShardProj += p[TrailingScanIdx].cont;
                                    if (p[TrailingScanIdx].cont.Trim() == "（")
                                    {
                                        isContranBrack = true;
                                    }
                                    if (p[TrailingScanIdx].cont.Trim() == "）")
                                    {
                                        isContranBrack = false;
                                    }
                                    if (p[TrailingScanIdx].cont == "标段")
                                    {
                                        ScanIdx = TrailingScanIdx;
                                        if (isContranBrack)
                                        {
                                            ShardProj += "）";
                                            ScanIdx++;
                                        }
                                        proj = ShardProj;
                                        break;
                                    }
                                }
                            }

                            //整体的再检查，是否下面一个单词还是工程，项目，标段
                            if (ScanIdx + 1 <= p.Count - 1)
                            {
                                if (p[ScanIdx + 1].cont == "工程" || p[ScanIdx + 1].cont == "项目" ||
                                    p[ScanIdx + 1].cont == "标段" || p[ScanIdx + 1].cont == "活动")
                                {
                                    proj += p[ScanIdx + 1].cont;
                                    ScanIdx++;
                                }
                            }

                            var isOK = true;
                            if (proj.Contains("重大工程")) isOK = false;
                            if (proj.Length > 50) isOK = false;
                            if (proj.Contains("；")) isOK = false;
                            if (proj.Contains("")) isOK = false;
                            if (isOK)
                            {
                                Console.WriteLine(doc.Id + " NER 发现工程：" + proj);
                                ProjList.Add(proj);
                            }
                            proj = string.Empty;
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(proj)) proj += word.cont;
                    }
                }
            }
        }
        return ProjList.Distinct().ToList();
    }

    public static List<String> GetProjectNameByCutWord(AnnouceDocument doc)
    {
        var posSeg = new PosSegmenter();
        var namelist = new List<String>();
        foreach (var paragrah in doc.root.Children)
        {
            foreach (var sentence in paragrah.Children)
            {
                if (string.IsNullOrEmpty(sentence.Content)) continue;
                var words = posSeg.Cut(sentence.Content).ToList();
                var PreviewEndIdx = -1;
                for (int baseInd = 0; baseInd < words.Count; baseInd++)
                {
                    var FullName = String.Empty;
                    if (words[baseInd].Word == "项目" || words[baseInd].Word == "工程" ||
                        words[baseInd].Word == "标段" || words[baseInd].Word == "采购")
                    {
                        var IsMarkClosed = true;
                        //是否能够在前面找到地名
                        for (int NRIdx = baseInd; NRIdx > PreviewEndIdx; NRIdx--)
                        {
                            //寻找地名?words[NRIdx].Flag == EntityWordAnlayzeTool.机构团体
                            //posSeg.Cut(words[NRIdx].Word + "市").First().Flag == EntityWordAnlayzeTool.地名
                            if (words[NRIdx].Flag == LTPTrainingNER.地名 || PosNS.NsDict.Contains(words[NRIdx].Word))
                            {
                                //注意，地名可能相连，例如：上海市嘉定
                                if (NRIdx != 0 && (words[NRIdx - 1].Flag == LTPTrainingNER.地名 || PosNS.NsDict.Contains(words[NRIdx - 1].Word))) continue;
                                FullName = String.Empty;
                                for (int companyFullNameInd = NRIdx; companyFullNameInd <= baseInd; companyFullNameInd++)
                                {
                                    FullName += words[companyFullNameInd].Word;
                                }
                                if (IsMarkClosed)
                                {
                                    //皆大欢喜的局面
                                    PreviewEndIdx = baseInd;
                                    Console.WriteLine(doc.Id + "发现工程：" + FullName);
                                    namelist.Add(FullName);
                                    break;  //不要继续寻找地名了
                                }
                            }
                            if (words[NRIdx].Flag == LTPTrainingNER.词性标点)
                            {
                                if (words[NRIdx].Word != "（" && words[NRIdx].Word != "）") break;
                                if (words[NRIdx].Word == "）") IsMarkClosed = false;    //打开
                                if (words[NRIdx].Word == "（") IsMarkClosed = true;     //关闭
                            }
                        }
                    }
                }
            }
        }
        return namelist;
    }
    public static List<String> GetProjectName(HTMLEngine.MyRootHtmlNode root)
    {
        var posSeg = new PosSegmenter();
        var namelist = new List<String>();
        foreach (var paragrah in root.Children)
        {
            foreach (var sentence in paragrah.Children)
            {
                var words = posSeg.Cut(sentence.Content).ToList();
                for (int baseInd = 0; baseInd < words.Count; baseInd++)
                {
                    if (words[baseInd].Word == "标段" ||
                        words[baseInd].Word == "工程" ||
                        words[baseInd].Word == "项目")
                    {
                        var projectName = String.Empty;
                        //是否能够在前面找到地名
                        for (int NRIdx = baseInd; NRIdx > -1; NRIdx--)
                        {
                            //地理
                            if (words[NRIdx].Flag == "ns")
                            {
                                projectName = String.Empty;
                                for (int companyFullNameInd = NRIdx; companyFullNameInd <= baseInd; companyFullNameInd++)
                                {
                                    projectName += words[companyFullNameInd].Word;
                                }
                                namelist.Add(projectName);
                                break;  //不要继续寻找地名了
                            }
                        }
                    }
                }
            }
        }
        return namelist;
    }
}