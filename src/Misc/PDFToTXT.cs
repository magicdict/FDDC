using System;
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

}