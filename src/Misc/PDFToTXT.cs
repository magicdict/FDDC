using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FDDC;

public class PDFToTXT
{
    /// <summary>
    /// PDF转TXT的批处理文件做成
    /// </summary>
    public static void GetPdf2TxtBatchFile()
    {
        var batchwriter = new StreamWriter("pdf2txt.bat", false, Encoding.GetEncoding("gb2312"));

        batchwriter.WriteLine("mkdir " + Program.ReorganizationPath_TRAIN + "\\txt");
        foreach (var filename in System.IO.Directory.GetFiles(Program.ReorganizationPath_TRAIN + @"/pdf/"))
        {
            batchwriter.WriteLine("pdf2txt.py " + filename + " >" + filename.Replace("pdf", "txt"));
        }
        foreach (var filename in System.IO.Directory.GetFiles(Program.ReorganizationPath_TEST + @"\pdf\"))
        {
            batchwriter.WriteLine("pdf2txt.py " + filename + " >" + filename.Replace("pdf", "txt"));
        }

        batchwriter.WriteLine("mkdir " + Program.ContractPath_TRAIN + "\\txt");
        foreach (var filename in System.IO.Directory.GetFiles(Program.ContractPath_TRAIN + @"/pdf/"))
        {
            batchwriter.WriteLine("pdf2txt.py " + filename + " >" + filename.Replace("pdf", "txt"));
        }
        batchwriter.WriteLine("mkdir " + Program.ContractPath_TEST + "\\txt");
        foreach (var filename in System.IO.Directory.GetFiles(Program.ContractPath_TEST + @"\pdf\"))
        {
            batchwriter.WriteLine("pdf2txt.py " + filename + " >" + filename.Replace("pdf", "txt"));
        }

        batchwriter.WriteLine("mkdir " + Program.StockChangePath_TRAIN + "\\txt");
        foreach (var filename in System.IO.Directory.GetFiles(Program.ContractPath_TRAIN + @"\pdf\"))
        {
            batchwriter.WriteLine("pdf2txt.py " + filename + " >" + filename.Replace("pdf", "txt"));
        }
        batchwriter.WriteLine("mkdir " + Program.StockChangePath_TEST + "\\txt");
        foreach (var filename in System.IO.Directory.GetFiles(Program.ContractPath_TEST + @"\pdf\"))
        {
            batchwriter.WriteLine("pdf2txt.py " + filename + " >" + filename.Replace("pdf", "txt"));
        }
        batchwriter.Close();
    }

    /// <summary>
    /// LTP的XML文件做成
    /// </summary>
    public static void GetLTPXMLBatchFileMac()
    {
        var Logger = new StreamWriter("ltp.sh", false, Encoding.GetEncoding("utf-8"));
        var ContractPath_TRAIN = "/Users/hu/Desktop/FDDCTraing/TrainingText";
        Logger.WriteLine("mkdir /Users/hu/Desktop/FDDCTraing/TrainingSrl");
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN))
        {
            var srlPath = "/Users/hu/Desktop/FDDCTraing/TrainingSrl/";
            var fi = new FileInfo(filename);
            var xml = fi.Name.Replace("txt", "xml");
            if (!xml.Contains("xml")) xml += ".xml";
            Logger.WriteLine("./ltp_test --input " + filename + " > " + srlPath + xml);
        }

        var ContractPath_TEST = "/Users/hu/Desktop/FDDCTraing/TestText";
        Logger.WriteLine("mkdir /Users/hu/Desktop/FDDCTraing/TestSrl");
        foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TEST))
        {
            var srlPath = "/Users/hu/Desktop/FDDCTraing/TestSrl/";
            var fi = new FileInfo(filename);
            var xml = fi.Name.Replace("txt", "xml");
            if (!xml.Contains("xml")) xml += ".xml";
            Logger.WriteLine("./ltp_test --input " + filename + " > " + srlPath + xml);
        }
        Logger.Close();
    }

    /// <summary>
    /// LTP的XML文件做成
    /// </summary>
    public static void GetLTPXMLBatchFile()
    {
        var Logger = new StreamWriter("ltp.bat", false, Encoding.GetEncoding("gb2312"));

        Logger.WriteLine(@"D:");
        Logger.WriteLine(@"cd D:\Download\ltp-3.3.2-win-x64-Release\bin\Release");
        Logger.WriteLine("mkdir " + Program.ContractPath_TRAIN + "\\ner");
        Logger.WriteLine("mkdir " + Program.ContractPath_TRAIN + "\\dp");
        Logger.WriteLine("mkdir " + Program.ContractPath_TRAIN + "\\srl");
        foreach (var filename in System.IO.Directory.GetFiles(Program.ContractPath_TRAIN + @"\txt\"))
        {
            var nerPath = Program.ContractPath_TRAIN + @"\ner\";
            var dpPath = Program.ContractPath_TRAIN + @"\dp\";
            var srlPath = Program.ContractPath_TRAIN + @"\srl\";
            var fi = new FileInfo(filename);
            var xml = fi.Name.Replace("txt", "xml");
            if (!xml.Contains("xml")) xml += ".xml";
            Logger.WriteLine("ltp_test.exe --last-stage ner --input " + filename + " > " + nerPath + xml);
            Logger.WriteLine("ltp_test.exe --last-stage dp --input " + filename + " > " + dpPath + xml);
            Logger.WriteLine("ltp_test.exe --input " + filename + " > " + srlPath + xml);
        }

        Logger.WriteLine("mkdir " + Program.ContractPath_TEST + "\\ner");
        Logger.WriteLine("mkdir " + Program.ContractPath_TEST + "\\dp");
        Logger.WriteLine("mkdir " + Program.ContractPath_TEST + "\\srl");
        foreach (var filename in System.IO.Directory.GetFiles(Program.ContractPath_TEST + @"\txt\"))
        {
            var nerPath = Program.ContractPath_TEST + @"\ner\";
            var dpPath = Program.ContractPath_TEST + @"\dp\";
            var srlPath = Program.ContractPath_TEST + @"\srl\";
            var fi = new FileInfo(filename);
            var xml = fi.Name.Replace("txt", "xml");
            if (!xml.Contains("xml")) xml += ".xml";
            Logger.WriteLine("ltp_test.exe --last-stage ner --input " + filename + " > " + nerPath + xml);
            Logger.WriteLine("ltp_test.exe --last-stage dp --input " + filename + " > " + dpPath + xml);
            Logger.WriteLine("ltp_test.exe --input " + filename + " > " + srlPath + xml);
        }


        Logger.WriteLine("mkdir " + Program.StockChangePath_TRAIN + "\\xml");
        foreach (var filename in System.IO.Directory.GetFiles(Program.StockChangePath_TRAIN + @"\txt\"))
        {
            var txt = filename;
            var xml = filename.Replace("txt", "xml");
            Logger.Write("ltp_test.exe --last-stage ner --input " + txt + " > " + xml);
        }

        Logger.WriteLine("mkdir " + Program.StockChangePath_TEST + "\\xml");
        foreach (var filename in System.IO.Directory.GetFiles(Program.StockChangePath_TEST + @"\txt\"))
        {
            var txt = filename;
            var xml = filename.Replace("txt", "xml");
            Logger.Write("ltp_test.exe --last-stage ner --input " + txt + " > " + xml);
        }


        Logger.WriteLine("mkdir " + Program.ReorganizationPath_TRAIN + "\\xml");
        foreach (var filename in System.IO.Directory.GetFiles(Program.ReorganizationPath_TRAIN + @"\txt\"))
        {
            var txt = filename;
            var xml = filename.Replace("txt", "xml");
            Logger.Write("ltp_test.exe --last-stage ner --input " + txt + " > " + xml);
        }

        Logger.WriteLine("mkdir " + Program.ReorganizationPath_TEST + "\\xml");
        foreach (var filename in System.IO.Directory.GetFiles(Program.ReorganizationPath_TEST + @"\txt\"))
        {
            var txt = filename;
            var xml = filename.Replace("txt", "xml");
            Logger.Write("ltp_test.exe --last-stage ner --input " + txt + " > " + xml);
        }
        Logger.Close();
    }

    /// <summary>
    /// 整理TXT文件入口
    /// </summary>
    public static void FormatTxtFile()
    {
        FormatTextFile(Program.ContractPath_TRAIN);
        FormatTextFile(Program.ContractPath_TEST);
        FormatTextFile(Program.StockChangePath_TRAIN);
        FormatTextFile(Program.StockChangePath_TEST);
        FormatTextFile(Program.ReorganizationPath_TRAIN);
        FormatTextFile(Program.ReorganizationPath_TEST);
    }

    /// <summary>
    /// 整理TXT文件具体方法
    /// </summary>
    /// <param name="path"></param>
    internal static void FormatTextFile(string path)
    {
        foreach (var filename in System.IO.Directory.GetFiles(path + @"\txt\"))
        {
            if (File.Exists(filename))
            {
                var Lines = new List<string>();
                var sr = new StreamReader(filename);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        Lines.Add(line);
                    }
                }
                sr.Close();

                var sw = new StreamWriter(filename, false);
                foreach (var line in Lines)
                {
                    if (line.Equals(" ")) continue;
                    if (line.Contains("\f")) line.Replace("\f", String.Empty);
                    //是否以空格结尾
                    if (line.EndsWith(" "))
                    {
                        //Trim之后是否为空
                        if (!String.IsNullOrEmpty(line.Trim())) sw.WriteLine(line.Trim());
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