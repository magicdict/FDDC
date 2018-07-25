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
        //public static String DocBase = @"E:" + Path.DirectorySeparatorChar + "WorkSpace2018" + Path.DirectorySeparatorChar + "FDDC2018";
        /// <summary>
        /// CentOS
        /// </summary>
        public static String DocBase =  @"/home/118_4";
        /// <summary>
        /// MAC
        /// </summary>
        //public static String DocBase = @"/Users/hu/Desktop/FDDC2018";

        /// <summary>
        /// 这个模式下，有问题的数据会输出，正式比赛的时候设置为False，降低召回率！
        /// </summary>
        public static bool IsDebugMode = false;
        /// <summary>
        /// 多线程模式
        /// </summary>
        public static bool IsMultiThreadMode = true;


        /// <summary>
        /// 快速测试区
        /// </summary>
        private static void QuickTestArea()
        {
            var s0 = "天创科技100%的股权和云天化集团直属资产";
            //以 c 以及 和 ，以及 进行切分
            var s1 = Utility.CutByPOSConection(s0);
            var t = new Reorganization();
            t.Init(ReorganizationPath_TRAIN + "/html/23599.html");
            var recs = t.Extract();
        }

        static void Main(string[] args)
        {
            if (Environment.OSVersion.Platform == System.PlatformID.Unix)
            {
                //静态变量已经定下来了，这里改不来了！
                Console.WriteLine("Switch Doc Path To:" + DocBase);
            }
            //日志
            Logger = new StreamWriter("Log.log");
            //实体属性器日志设定
            EntityProperty.Logger = Logger;
            //全局编码    
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            CIRecord = new StreamWriter("CI.log");

            
            //QuickTestArea(); return;

            //PDFToTXT.GetPdf2TxtBatchFile();

            //公司全称简称曾用名字典   
            CompanyNameLogic.LoadCompanyName("Resources" + Path.DirectorySeparatorChar + "FDDC_announcements_company_name_20180531.json");
            //结巴分词的地名修正词典
            PosNS.ImportNS("Resources" + Path.DirectorySeparatorChar + "ns.dict");
            //预处理
            Traning();
            Evaluator = new StreamWriter("Evaluator.log");
            Score = new StreamWriter("Result" + Path.DirectorySeparatorChar + "Score" + Path.DirectorySeparatorChar + "score" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
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
            TraningDataset.InitReorganization();

            ContractTraning.Train();
            //StockChangeTraning.Traning(); 没有实质性的统计
            ReOrganizationTraning.Train();

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

        //重大合同
        public static bool IsRunContract = false;
        public static bool IsRunContract_TEST = false;
        public static string ContractPath_TRAIN = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_train_20180518" + Path.DirectorySeparatorChar + "重大合同";
        public static string ContractPath_TEST = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_test_b_20180708" + Path.DirectorySeparatorChar + "重大合同";

        //增减持
        public static bool IsRunStockChange = false;
        public static bool IsRunStockChange_TEST = false;
        public static string StockChangePath_TRAIN = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_train_20180518" + Path.DirectorySeparatorChar + "增减持";
        public static string StockChangePath_TEST = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_test_b_20180708" + Path.DirectorySeparatorChar + "增减持";

        //资产重组
        public static bool IsRunReorganization = true;
        public static bool IsRunReorganization_TEST = false;
        public static string ReorganizationPath_TRAIN = DocBase + Path.DirectorySeparatorChar + @"复赛新增类型训练数据-20180712" + Path.DirectorySeparatorChar + "资产重组";
        public static string ReorganizationPath_TEST = DocBase + Path.DirectorySeparatorChar + @"复赛新增类型测试数据-20180712" + Path.DirectorySeparatorChar + "资产重组";

        private static void Extract()
        {
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

            if (IsRunStockChange || IsRunStockChange_TEST)
            {
                //增减持公告日期的读入
                StockChange.ImportPublishTime();
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

            //资产重组
            if (IsRunReorganization)
            {
                Console.WriteLine("Start To Extract Info Reorganization TRAIN");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "chongzu_train.txt", false, utf8WithoutBom);
                var Reorganization_Result = Run<Reorganization>(ReorganizationPath_TRAIN, ResultCSV);
                Evaluate.EvaluateReorganization(Reorganization_Result.Select((x) => (ReorganizationRec)x).ToList());
                Console.WriteLine("Complete Extract Info Reorganization");
            }
            if (IsRunReorganization_TEST)
            {
                Console.WriteLine("Start To Extract Info Reorganization TEST");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "chongzu.txt", false, utf8WithoutBom);
                var Reorganization_Result = Run<Reorganization>(ReorganizationPath_TEST, ResultCSV);
                Console.WriteLine("Complete Extract Info Reorganization");
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
            var Announce_Result = new List<RecordBase>();
            if (IsMultiThreadMode)
            {
                var Bag = new ConcurrentBag<RecordBase>();    //线程安全版本
                Parallel.ForEach(System.IO.Directory.GetFiles(path + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar), (filename) =>
                {
                    var announce = new T();
                    announce.Init(filename);
                    foreach (var item in announce.Extract())
                    {
                        Bag.Add(item);
                    }
                });
                Announce_Result = Bag.ToList();
            }
            else
            {
                foreach (var filename in System.IO.Directory.GetFiles(path + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar))
                {
                    var contract = new T();
                    contract.Init(filename);
                    foreach (var item in contract.Extract())
                    {
                        Announce_Result.Add(item);
                    }
                }
            }
            Announce_Result.Sort((x, y) => { return int.Parse(x.Id).CompareTo(int.Parse(y.Id)); });
            ResultCSV.WriteLine(Announce_Result.First().CSVTitle());
            foreach (var item in Announce_Result)
            {
                ResultCSV.WriteLine(item.ConvertToString());
            }
            ResultCSV.Close();
            return Announce_Result;
        }
    }
}
