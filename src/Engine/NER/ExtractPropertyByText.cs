using System;
using System.Collections.Generic;
using static HTMLEngine;
using static LocateProperty;
using System.IO;

public class ExtractPropertyByText
{
    //候选词
    public List<LocAndValue<String>> CandidateWord = new List<LocAndValue<String>>();


    //先导词（直接取先导词的后面的内容）
    public string[] LeadingColonKeyWordList = new string[] { };

    //先导词（直接取先导词的后面的内容）
    public string[] TrailingWordList = new string[] { };

    #region 常规文本
    public void ExtractFromTextFile(string filename)
    {
        if (!File.Exists(filename)) return;
        CandidateWord.Clear();
        if (LeadingColonKeyWordList.Length > 0) ExtractTextByColonKeyWord(filename);
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

    #endregion


}