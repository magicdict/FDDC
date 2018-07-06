using System;
using System.Collections.Generic;
using System.Linq;
using JiebaNet.Segmenter.PosSeg;

public class ProjectNameLogic
{

    public static List<String> GetProjectNameByCutWord(HTMLEngine.MyRootHtmlNode root)
    {
        var posSeg = new PosSegmenter();
        var namelist = new List<String>();
        foreach (var paragrah in root.Children)
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
                            if (words[NRIdx].Flag == LTP.地名 || PosNS.NsDict.Contains(words[NRIdx].Word))
                            {
                                //注意，地名可能相连，例如：上海市嘉定
                                if (NRIdx != 0 && (words[NRIdx - 1].Flag == LTP.地名 || PosNS.NsDict.Contains(words[NRIdx - 1].Word))) continue;
                                FullName = String.Empty;
                                for (int companyFullNameInd = NRIdx; companyFullNameInd <= baseInd; companyFullNameInd++)
                                {
                                    FullName += words[companyFullNameInd].Word;
                                }
                                if (IsMarkClosed)
                                {
                                    //皆大欢喜的局面
                                    PreviewEndIdx = baseInd;
                                    namelist.Add(FullName);
                                    break;  //不要继续寻找地名了
                                }
                            }
                            if (words[NRIdx].Flag == LTP.词性标点)
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