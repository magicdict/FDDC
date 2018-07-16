using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Linq;
using JiebaNet.Segmenter;
using JiebaNet.Segmenter.PosSeg;
using System.Threading.Tasks;
using static Contract;
using static IncreaseStock;
using static StockChange;

namespace FDDC
{
    class Program
    {
        public static Encoding utf8WithoutBom = new System.Text.UTF8Encoding(false);
        public static StreamWriter Training;
        public static StreamWriter Logger;
        public static StreamWriter Evaluator;
        public static StreamWriter CIRecord;
        public static StreamWriter Score;
        /// <summary>
        /// Windows
        /// </summary>
        public static String DocBase = @"E:\WorkSpace2018\FDDC2018";

        /// <summary>
        /// Mac
        /// </summary>
        //public static String DocBase = @"/Users/hu/Desktop/FDDCTraing";

        /// <summary>
        /// 这个模式下，有问题的数据会输出，正式比赛的时候设置为False，降低召回率！
        /// </summary>
        public static bool IsDebugMode = false;
        /// <summary>
        /// 多线程模式
        /// </summary>
        public static bool IsMultiThreadMode = true;

        static void Main(string[] args)
        {
            Logger = new StreamWriter("Log.log");
            //全局编码    
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //公司全称简称曾用名字典   
            CompanyNameLogic.LoadCompanyName(@"Resources" + Path.DirectorySeparatorChar + "FDDC_announcements_company_name_20180531.json");
            //增减持公告日期的读入
            StockChange.ImportPublishTime();
            //结巴分词的地名修正词典
            PosNS.ImportNS(@"Resources" + Path.DirectorySeparatorChar + "ns.dict");
            CIRecord = new StreamWriter("CI.log");
            //预处理
            Traning();
            Evaluator = new StreamWriter("Evaluator.log");
            Score = new StreamWriter(@"Result" + Path.DirectorySeparatorChar + "Score" + Path.DirectorySeparatorChar + "score" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
            EntityProperty.Logger = Logger;
            Extract();
            CIRecord.Close();
            Score.Close();
            Evaluator.Close();
            Logger.Close();
        }

        private static void Traning()
        {
            Training = new StreamWriter("Training.log");
            TraningDataset.InitContract();
            TraningDataset.InitStockChange();
            TraningDataset.InitIncreaseStock();
            ContractTraning.Train();
            StockChangeTraning.Traning();
            IncreaseStockTraning.Training(100);
            Training.Close();
        }

        private static void GetBatchFile()
        {
            //地名修正词典的获取
            PosNS.ExtractNsFromDP();
            //PDFMiner:PDF转TXTbatch
            PDFToTXT.GetPdf2TxtBatchFile();
            //TXT整理
            PDFToTXT.FormatTxtFile();
            //LTP:XML生成Batch
            PDFToTXT.GetLTPXMLBatchFile();
        }

        private static void Extract()
        {
            var IsRunContract = false;
            var IsRunContract_TEST = false;
            var ContractPath_TRAIN = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_train_20180518" + Path.DirectorySeparatorChar + "重大合同";
            var ContractPath_TEST = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_test_b_20180708" + Path.DirectorySeparatorChar + "重大合同";

            var IsRunStockChange = true;
            var IsRunStockChange_TEST = true;
            var StockChangePath_TRAIN = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_train_20180518" + Path.DirectorySeparatorChar + "增减持";
            var StockChangePath_TEST = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_test_b_20180708" + Path.DirectorySeparatorChar + "增减持";

            //复赛中删除
            var IsRunIncreaseStock = false;
            var IsRunIncreaseStock_TEST = false;
            var IncreaseStockPath_TRAIN = DocBase + Path.DirectorySeparatorChar + @"FDDC_announcements_round1_train_20180518" + Path.DirectorySeparatorChar + "定增";
            var IncreaseStockPath_TEST = DocBase + Path.DirectorySeparatorChar + @"FDDC_announcements_round1_test_b_20180708" + Path.DirectorySeparatorChar + "定增";

            //复赛中新增
            var IsRunReorganization = false;
            var IsRunReorganization_TEST = false;
            var ReorganizationPath_TRAIN = DocBase + Path.DirectorySeparatorChar + @"复赛新增类型训练数据-20180712" + Path.DirectorySeparatorChar + "资产重组";
            var ReorganizationPath_TEST = DocBase + Path.DirectorySeparatorChar + @"复赛新增类型测试数据-20180712" + Path.DirectorySeparatorChar + "资产重组";


            if (IsRunContract)
            {
                //合同处理
                Console.WriteLine("Start To Extract Info Contract TRAIN");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "hetong_train.txt", false, utf8WithoutBom);
                var Contract_Result = Run<Contract>(ContractPath_TRAIN, ResultCSV);
                Evaluate.EvaluateContract(Contract_Result.Select((x) => (ContractRec)x).ToList());
                Console.WriteLine("Complete Extract Info Contract");
            }
            if (IsRunContract_TEST)
            {
                Console.WriteLine("Start To Extract Info Contract TEST");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "hetong.txt", false, utf8WithoutBom);
                var Contract_Result = Run<Contract>(ContractPath_TEST, ResultCSV);
                Console.WriteLine("Complete Extract Info Contract");
            }

            //资产重组
            if (IsRunReorganization)
            {
                Console.WriteLine("Start To Extract Info Contract TRAIN");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "hetong_train.txt", false, utf8WithoutBom);
                var Contract_Result = Run<Reorganization>(ContractPath_TRAIN, ResultCSV);
                Evaluate.EvaluateReorganization(Contract_Result.Select((x) => (ReorganizationRec)x).ToList());
                Console.WriteLine("Complete Extract Info Contract");
            }
            if (IsRunReorganization_TEST)
            {
                Console.WriteLine("Start To Extract Info Contract TEST");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "hetong.txt", false, utf8WithoutBom);
                var Contract_Result = Run<Contract>(ContractPath_TEST, ResultCSV);
                Console.WriteLine("Complete Extract Info Contract");
            }

            //增减持
            if (IsRunStockChange)
            {
                Console.WriteLine("Start To Extract Info StockChange TRAIN");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "zengjianchi_train.txt", false, utf8WithoutBom);
                var StockChange_Result = Run<StockChange>(StockChangePath_TRAIN, ResultCSV);
                Evaluate.EvaluateStockChange(StockChange_Result.Select((x) => (StockChangeRec)x).ToList());
                Console.WriteLine("Complete Extract Info StockChange");
            }
            if (IsRunStockChange_TEST)
            {
                Console.WriteLine("Start To Extract Info StockChange TEST");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "zengjianchi.txt", false, utf8WithoutBom);
                var StockChange_Result = Run<StockChange>(StockChangePath_TEST, ResultCSV);
                Console.WriteLine("Complete Extract Info StockChange");
            }

            //定增
            if (IsRunIncreaseStock)
            {
                Console.WriteLine("Start To Extract Info IncreaseStock TRAIN");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "dingzeng_train.txt", false, utf8WithoutBom);
                var Increase_Result = Run<IncreaseStock>(IncreaseStockPath_TRAIN, ResultCSV);
                Evaluate.EvaluateIncreaseStock(Increase_Result.Select((x) => (IncreaseStockRec)x).ToList());
                Console.WriteLine("Complete Extract Info IncreaseStock");
            }
            if (IsRunIncreaseStock_TEST)
            {
                Console.WriteLine("Start To Extract Info IncreaseStock TEST");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "dingzeng.txt", false, utf8WithoutBom);
                var Increase_Result = Run<IncreaseStock>(IncreaseStockPath_TEST, ResultCSV);
                Console.WriteLine("Complete Extract Info IncreaseStock");
            }
        }

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T">公告类型</typeparam>
        /// <typeparam name="S">记录类型</typeparam>
        public static List<RecordBase> Run<T>(string path, StreamWriter ResultCSV) where T : AnnouceDocument, new()
        {
            var Contract_Result = new List<RecordBase>();
            if (IsMultiThreadMode)
            {
                var Bag = new ConcurrentBag<RecordBase>();    //线程安全版本
                Parallel.ForEach(System.IO.Directory.GetFiles(path + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar), (filename) =>
                {
                    var contract = new T();
                    contract.Init(filename);
                    foreach (var item in contract.Extract())
                    {
                        Bag.Add(item);
                    }
                });
                Contract_Result = Bag.ToList();
                Contract_Result.Sort((x, y) => { return x.id.CompareTo(y.id); });
                ResultCSV.WriteLine(Contract_Result.First().CSVTitle());
                foreach (var item in Contract_Result)
                {
                    ResultCSV.WriteLine(item.ConvertToString());
                }
            }
            else
            {
                foreach (var filename in System.IO.Directory.GetFiles(path + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar))
                {
                    var contract = new T();
                    contract.Init(filename);
                    foreach (var item in contract.Extract())
                    {
                        if (Contract_Result.Count == 0)
                        {
                            ResultCSV.WriteLine(item.CSVTitle());
                        }
                        Contract_Result.Add(item);
                        ResultCSV.WriteLine(item.ConvertToString());
                    }
                }
            }
            ResultCSV.Close();
            return Contract_Result;
        }

        /// <summary>
        /// 快速测试区
        /// </summary>
        private static void UT()
        {
            var ContractPath_TRAIN = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_train_20180518\\round1_train_20180518" + Path.DirectorySeparatorChar + "重大合同";
            foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + Path.DirectorySeparatorChar + "srl" + Path.DirectorySeparatorChar))
            {
                var Srllist = LTPTrainingSRL.AnlayzeSRL(filename);
                var fi = new FileInfo(filename);
                if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("Name：" + fi.Name);
                foreach (var m in Srllist)
                {
                    if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("SRL：" + m);
                }
            }
        }
    }
}
