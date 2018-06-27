using System;
using System.Collections.Generic;
using static HTMLEngine;
using static LocateProperty;
using System.IO;
using static LTP;
using System.Linq;

public class ExtractPropertyByDP
{
    public List<LocAndValue<String>> CandidateWord = new List<LocAndValue<String>>();
    public struct DPKeyWord
    {
        //关键字
        public string StartWord;
        //句型属性
        public string[] StartDPValue;

        public string EndWord;
        //句型属性
        public string[] EndDPValue;
    }



    public void StartWithKey(List<DPKeyWord> keys, List<List<struWordDP>> dplist)
    {

        foreach (var key in keys)
        {
            bool isStart = false;
            string x = "";
            foreach (var paragragh in dplist)
            {
                foreach (var word in paragragh)
                {
                    if (isStart)
                    {
                        if (word.relate == LTP.右附加关系) continue;
                        x += word.cont;
                    }
                    if (word.cont == key.StartWord && key.StartDPValue.Contains(word.relate))
                    {
                        isStart = true;
                    }
                    if (word.cont == key.EndWord && key.EndDPValue.Contains( word.relate))
                    {
                        if (isStart) CandidateWord.Add(new LocAndValue<string>() { Value = x });
                        isStart = false;
                        x = "";
                    }
                }
            }
        }


    }
}