using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FDDC;

public class PDFToTXT
{
    public static StreamWriter Logger = new StreamWriter("pdf2txt.bat", false, Encoding.GetEncoding("gb2312"));

    public static void GetBatchFile()
    {
        var ContractPath_TRAIN = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同";
        Logger.WriteLine("mkdir " + ContractPath_TRAIN + "\\txt");
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + @"\pdf\"))
        {
            Logger.WriteLine("pdf2txt.py " + filename + " >>" + filename.Replace("pdf", "txt"));
        }
        var ContractPath_TEST = Program.DocBase + @"\FDDC_announcements_round1_test_a_20180605\重大合同";
        Logger.WriteLine("mkdir " + ContractPath_TEST + "\\txt");
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TEST + @"\pdf\"))
        {
            Logger.WriteLine("pdf2txt.py " + filename + " >>" + filename.Replace("pdf", "txt"));
        }


        ContractPath_TRAIN = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持";
        Logger.WriteLine("mkdir " + ContractPath_TRAIN + "\\txt");
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + @"\pdf\"))
        {
            Logger.WriteLine("pdf2txt.py " + filename + " >>" + filename.Replace("pdf", "txt"));
        }
        ContractPath_TEST = Program.DocBase + @"\FDDC_announcements_round1_test_a_20180605\增减持";
        Logger.WriteLine("mkdir " + ContractPath_TEST + "\\txt");
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TEST + @"\pdf\"))
        {
            Logger.WriteLine("pdf2txt.py " + filename + " >>" + filename.Replace("pdf", "txt"));
        }


        ContractPath_TRAIN = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\定增";
        Logger.WriteLine("mkdir " + ContractPath_TRAIN + "\\txt");
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + @"\pdf\"))
        {
            Logger.WriteLine("pdf2txt.py " + filename + " >>" + filename.Replace("pdf", "txt"));
        }
        ContractPath_TEST = Program.DocBase + @"\FDDC_announcements_round1_test_a_20180605\定增";
        Logger.WriteLine("mkdir " + ContractPath_TEST + "\\txt");
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TEST + @"\pdf\"))
        {
            Logger.WriteLine("pdf2txt.py " + filename + " >>" + filename.Replace("pdf", "txt"));
        }
        Logger.Close();
    }


    public static void GetXMLFile()
    {
        Logger.WriteLine(@"D:");
        Logger.WriteLine(@"cd D:\Download\ltp-3.4.0-win-x64-Release\bin\Release");
        var ContractPath_TRAIN = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同";
        Logger.WriteLine("mkdir " + ContractPath_TRAIN + "\\xml");
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + @"\txt\"))
        {
            var txt = filename;
            var xml = filename.Replace("txt", "xml");
            Logger.Write("ltp_test.exe --segmentor-model cws.model --postagger-model pos.model --ner-model ner.model");
            Logger.WriteLine(" --parser-model parser.model --last-stage dp --input " + txt + " >> " + xml);
        }

        var ContractPath_TEST = Program.DocBase + @"\FDDC_announcements_round1_test_a_20180605\重大合同";
        Logger.WriteLine("mkdir " + ContractPath_TEST + "\\xml");
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TEST + @"\txt\"))
        {
            var txt = filename;
            var xml = filename.Replace("txt", "xml");
            Logger.Write("ltp_test.exe --segmentor-model cws.model --postagger-model pos.model --ner-model ner.model");
            Logger.WriteLine(" --parser-model parser.model  --last-stage dp --input " + txt + " >> " + xml);
        }

        Logger.Close(); return;
        
        ContractPath_TRAIN = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持";
        Logger.WriteLine("mkdir " + ContractPath_TRAIN + "\\xml");
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + @"\txt\"))
        {
            var txt = filename;
            var xml = filename.Replace("txt", "xml");
            Logger.Write("ltp_test.exe --segmentor-model cws.model --postagger-model pos.model --ner-model ner.model");
            Logger.WriteLine(" --parser-model parser.model  --last-stage dp --input " + txt + " >> " + xml);
        }
        ContractPath_TEST = Program.DocBase + @"\FDDC_announcements_round1_test_a_20180605\增减持";
        Logger.WriteLine("mkdir " + ContractPath_TEST + "\\xml");
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TEST + @"\txt\"))
        {
            var txt = filename;
            var xml = filename.Replace("txt", "xml");
            Logger.Write("ltp_test.exe --segmentor-model cws.model --postagger-model pos.model --ner-model ner.model");
            Logger.WriteLine(" --parser-model parser.model  --last-stage dp --input " + txt + " >> " + xml);
        }


        ContractPath_TRAIN = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\定增";
        Logger.WriteLine("mkdir " + ContractPath_TRAIN + "\\xml");
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + @"\txt\"))
        {
            var txt = filename;
            var xml = filename.Replace("txt", "xml");
            Logger.Write("ltp_test.exe --segmentor-model cws.model --postagger-model pos.model --ner-model ner.model");
            Logger.WriteLine(" --parser-model parser.model  --last-stage dp --input " + txt + " >> " + xml);
        }
        ContractPath_TEST = Program.DocBase + @"\FDDC_announcements_round1_test_a_20180605\定增";
        Logger.WriteLine("mkdir " + ContractPath_TEST + "\\xml");
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TEST + @"\txt\"))
        {
            var txt = filename;
            var xml = filename.Replace("txt", "xml");
            Logger.Write("ltp_test.exe --segmentor-model cws.model --postagger-model pos.model --ner-model ner.model");
            Logger.WriteLine(" --parser-model parser.model  --last-stage dp --input " + txt + " >> " + xml);
        }
        Logger.Close();
    }

    public static void FormatTxtFile()
    {
        format(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同");
        format(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持");
        format(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\定增");
        format(Program.DocBase + @"\FDDC_announcements_round1_test_a_20180605\重大合同");
        format(Program.DocBase + @"\FDDC_announcements_round1_test_a_20180605\增减持");
        format(Program.DocBase + @"\FDDC_announcements_round1_test_a_20180605\定增");
    }

    static void format(string path)
    {
        foreach (var filename in System.IO.Directory.GetFiles(path + @"\pdf\"))
        {
            var txtfile = filename.Replace("pdf", "txt");
            if (File.Exists(txtfile))
            {
                var Lines = new List<string>();
                var sr = new StreamReader(txtfile);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (!String.IsNullOrEmpty(line)) Lines.Add(line);
                }
                sr.Close();

                var sw = new StreamWriter(txtfile);
                foreach (var line in Lines)
                {
                    if (line.EndsWith(" "))
                    {
                        sw.WriteLine(line);
                    }
                    else
                    {
                        sw.Write(line);
                    }
                }
                sw.Close();
            }
        }
    }

}