using System;
using System.Collections.Generic;
using static HTMLEngine;
using static LocateProperty;
using System.IO;

public class ExtractPropertyByText : ExtractProperyBase
{
    //候选词

    #region 常规文本
    public void ExtractFromTextFile(string filename)
    {
        if (!File.Exists(filename)) return;
        CandidateWord.Clear();
        if (LeadingColonKeyWordList.Length > 0) ExtractTextByColonKeyWord(filename);
        if (StartEndFeature.Length > 0) ExtractByStartEndStringFeature(filename);
        if (LeadingColonKeyWordListInChineseBrackets.Length > 0) ExtractTextByInChineseBracketsColonKeyWord(filename);
    }

    public void ExtractTextByColonKeyWord(string filename)
    {
        var lines = new List<String>();
        var sr = new StreamReader(filename);
        while (!sr.EndOfStream)
        {
            var line = sr.ReadLine();
            if (!String.IsNullOrEmpty(line)) lines.Add(line);
        }
        sr.Close();

        for (int CurrentLineIdx = 0; CurrentLineIdx < lines.Count; CurrentLineIdx++)
        {
            var line = lines[CurrentLineIdx];
            foreach (var word in LeadingColonKeyWordList)
            {
                if (Utility.GetStringAfter(line, word) != String.Empty)
                {
                    var result = Utility.GetStringAfter(line, word);
                    if (string.IsNullOrEmpty(result)) continue;
                    CandidateWord.Add(new LocAndValue<string>()
                    {
                        Loc = CurrentLineIdx,
                        Value = result
                    });
                    break;
                }
            }
        }
    }

    public void ExtractTextByInChineseBracketsColonKeyWord(string filename)
    {
        var lines = new List<String>();
        var sr = new StreamReader(filename);
        while (!sr.EndOfStream)
        {
            var line = sr.ReadLine();
            if (!String.IsNullOrEmpty(line)) lines.Add(line);
        }
        sr.Close();

        for (int CurrentLineIdx = 0; CurrentLineIdx < lines.Count; CurrentLineIdx++)
        {
            var line = lines[CurrentLineIdx];
            foreach (var word in LeadingColonKeyWordListInChineseBrackets)
            {
                var result = RegularTool.GetValueInChineseBracketsLeadingKeyWord(line, word);
                foreach (var item in result)
                {
                    CandidateWord.Add(new LocAndValue<string>()
                    {
                        Loc = CurrentLineIdx,
                        Value = item
                    });
                }
            }
        }
    }


    public void ExtractTextByTrailingKeyWord(string filename)
    {
        var lines = new List<String>();
        var sr = new StreamReader(filename);
        while (!sr.EndOfStream)
        {
            var line = sr.ReadLine();
            if (!String.IsNullOrEmpty(line)) lines.Add(line);
        }
        sr.Close();

        for (int CurrentLineIdx = 0; CurrentLineIdx < lines.Count; CurrentLineIdx++)
        {
            var line = lines[CurrentLineIdx];
            foreach (var word in TrailingWordList)
            {
                if (Utility.GetStringBefore(line, word) != String.Empty)
                {
                    var result = Utility.GetStringBefore(line, word);
                    if (string.IsNullOrEmpty(result)) continue;
                    CandidateWord.Add(new LocAndValue<string>()
                    {
                        Loc = CurrentLineIdx,
                        Value = result
                    });
                    break;
                }
            }
        }
    }
    void ExtractByStartEndStringFeature(string filename)
    {
        var lines = new List<String>();
        var sr = new StreamReader(filename);
        while (!sr.EndOfStream)
        {
            var line = sr.ReadLine();
            if (!String.IsNullOrEmpty(line)) lines.Add(line);
        }
        sr.Close();
        foreach (var word in StartEndFeature)
        {
            for (int CurrentLineIdx = 0; CurrentLineIdx < lines.Count; CurrentLineIdx++)
            {
                var line = lines[CurrentLineIdx];
                var list = RegularTool.GetMultiValueBetweenString(line, word.StartWith, word.EndWith);
                foreach (var item in list)
                {
                    CandidateWord.Add(new LocAndValue<string>() { Value = item });
                }
            };
        }

    }

    #endregion
}