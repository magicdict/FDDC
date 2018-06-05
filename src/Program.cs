using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Contract;
using static IncreaseStock;
using static StockChange;

namespace FDDC
{
    class Program
    {

        public static StreamWriter Logger = new StreamWriter("Log.log");

        public static String DocBase = @"E:\FDDC";

        static void Main(string[] args)
        {


            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            BussinessLogic.LoadCompanyName(@"Resources\FDDC_announcements_company_name_20180531.json");

            //PDFToTXT.GetBatchFile();    
            //分词系统
            //WordAnlayze.CompanyAnlayze();
            //UT.RunWordAnlayze();
            //UT.StockChangeTest();
            //UT.IncreaseStockTest();
            //UT.ContractTest();
            //UT.RegularExpress();
            //UT.JianchengTest();
            //StockChange.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持\html\20596890.html");
            //IncreaseStock.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\定增\html\7880.html");
            //Logger.Close();
            //StockChange.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持\html\20526193.html");
            //return;


            var IsRunContract = false;
            var IsRunContract_TEST = false;

            var IsRunStockChange = true;
            var IsRunStockChange_TEST = false;

            var IsRunIncreaseStock = false;
            var IsRunIncreaseStock_TEST = false;

            if (IsRunContract)
            {
                //合同处理
                var ContractPath_TRAIN = DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同";
                Console.WriteLine("Start To Extract Info Contract TRAIN");
                StreamWriter ResultCSV = new StreamWriter("Result\\hetong_train.csv", false, Encoding.GetEncoding("gb2312"));
                var StockChange_Result = new List<struContract>();
                foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + @"\html\"))
                {
                    foreach (var item in Contract.Extract(filename))
                    {
                        StockChange_Result.Add(item);
                        ResultCSV.WriteLine(Contract.ConvertToString(item));
                    }
                }
                ResultCSV.Close();
                Traning.InitContract();
                Evaluate.EvaluateContract(StockChange_Result);
                Console.WriteLine("Complete Extract Info Contract");
            }
            if (IsRunContract_TEST)
            {
                var ContractPath_TEST = DocBase + @"\FDDC_announcements_round1_test_a_20180524\重大合同";
                StreamWriter ResultCSV = new StreamWriter("Result\\hetong.csv", false, Encoding.GetEncoding("gb2312"));
                Console.WriteLine("Start To Extract Info Contract TEST");
                foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TEST + @"\html\"))
                {
                    foreach (var item in Contract.Extract(filename))
                    {
                        ResultCSV.WriteLine(Contract.ConvertToString(item));
                    }
                }
                ResultCSV.Close();
                Console.WriteLine("Complete Extract Info Contract");
            }


            if (IsRunStockChange)
            {
                //增减持
                Console.WriteLine("Start To Extract Info StockChange TRAIN");
                StreamWriter ResultCSV = new StreamWriter("Result\\zengjianchi_Train.csv", false, Encoding.GetEncoding("gb2312"));
                var StockChangePath_TRAIN = DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持";
                var StockChange_Result = new List<struStockChange>();
                foreach (var filename in System.IO.Directory.GetFiles(StockChangePath_TRAIN + @"\html\"))
                {
                    foreach (var item in StockChange.Extract(filename))
                    {
                        StockChange_Result.Add(item);
                        ResultCSV.WriteLine(StockChange.ConvertToString(item));
                    }

                }
                ResultCSV.Close();
                Traning.InitStockChange();
                Evaluate.EvaluateStockChange(StockChange_Result);
                Console.WriteLine("Complete Extract Info StockChange");
            }
            if (IsRunStockChange_TEST)
            {
                StreamWriter ResultCSV = new StreamWriter("Result\\zengjianchi.csv", false, Encoding.GetEncoding("gb2312"));
                var StockChangePath_TEST = DocBase + @"\FDDC_announcements_round1_test_a_20180524\增减持";
                Console.WriteLine("Start To Extract Info StockChange TEST");
                foreach (var filename in System.IO.Directory.GetFiles(StockChangePath_TEST + @"\html\"))
                {
                    foreach (var item in StockChange.Extract(filename))
                    {
                        ResultCSV.WriteLine(StockChange.ConvertToString(item));
                    }
                }
                ResultCSV.Close();
                Console.WriteLine("Complete Extract Info StockChange");
            }



            if (IsRunIncreaseStock)
            {
                //定增
                StreamWriter ResultCSV = new StreamWriter("Result\\dingzeng_train.csv", false, Encoding.GetEncoding("gb2312"));
                var IncreaseStockPath_TRAIN = DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\定增";
                Console.WriteLine("Start To Extract Info IncreaseStock TRAIN");
                var Increase_Result = new List<struIncreaseStock>();
                foreach (var filename in System.IO.Directory.GetFiles(IncreaseStockPath_TRAIN + @"\html\"))
                {
                    foreach (var item in IncreaseStock.Extract(filename))
                    {
                        Increase_Result.Add(item);
                        ResultCSV.WriteLine(IncreaseStock.ConvertToString(item));
                    }
                }
                ResultCSV.Close();
                Traning.InitIncreaseStock();
                Evaluate.EvaluateIncreaseStock(Increase_Result);
                Console.WriteLine("Complete Extract Info IncreaseStock");
            }

            if (IsRunIncreaseStock_TEST)
            {
                StreamWriter ResultCSV = new StreamWriter("Result\\dingzeng.csv", false, Encoding.GetEncoding("gb2312"));
                var IncreaseStockPath_TEST = DocBase + @"\FDDC_announcements_round1_test_a_20180524\定增";
                Console.WriteLine("Start To Extract Info IncreaseStock TRAIN");
                foreach (var filename in System.IO.Directory.GetFiles(IncreaseStockPath_TEST + @"\html\"))
                {
                    foreach (var item in IncreaseStock.Extract(filename))
                    {
                        ResultCSV.WriteLine(IncreaseStock.ConvertToString(item));
                    }
                }
                ResultCSV.Close();
                Console.WriteLine("Complete Extract Info IncreaseStock");
            }


            Logger.Close();
        }
    }
}
