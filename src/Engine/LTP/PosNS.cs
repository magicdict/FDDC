using System;
using System.Collections.Generic;
using System.IO;
/// <summary>
/// 地名词性的增强
/// </summary>
public class PosNS
{
    public static List<String> NsDict = new List<String>();

    public static void ImportNS(string dictname)
    {
        var sr = new StreamReader(dictname);
        while (!sr.EndOfStream)
        {
            NsDict.Add(sr.ReadLine());
        }
        sr.Close();
    }
    public static void ExtractNsFromDP()
    {
        var NsDict = new StreamWriter("ns.dict");
        List<String> Dirs = new List<string>();
        List<String> Ns = new List<String>();
        Dirs.Add(@"E:\WorkSpace2018\FDDC2018\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\dp");
        Dirs.Add(@"E:\WorkSpace2018\FDDC2018\FDDC_announcements_round1_test_a_20180605\重大合同\dp");
        foreach (var dir in Dirs)
        {
            foreach (var filename in System.IO.Directory.GetFiles(dir))
            {
                var dp = LTPTrainingDP.AnlayzeDP(filename);
                foreach (var p in dp)
                {
                    foreach (var s in p)
                    {
                        if (s.pos == LTPTrainingNER.地名)
                        {
                            if (!Ns.Contains(s.cont))
                            {
                                Ns.Add(s.cont);
                                NsDict.WriteLine(s.cont);
                            }
                        }
                    }
                }
            }
        }
        NsDict.Close();
    }
}