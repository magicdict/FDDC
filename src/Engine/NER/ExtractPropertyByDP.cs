using System;
using System.Collections.Generic;
using static HTMLEngine;
using static LocateProperty;
using System.IO;

public class ExtractPropertyByDP
{
    public List<LocAndValue<String>> CandidateWord = new List<LocAndValue<String>>();
    struct DPKeyWord
    {
        //关键字
        public string Word;
        //句型属性
        public string DPValue;
    }
}