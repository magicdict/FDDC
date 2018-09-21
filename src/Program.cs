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
using System.Data;

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
        /// 基本Windows
        /// </summary>
        public static String DocBase = @"E:" + Path.DirectorySeparatorChar + "WorkSpace2018" + Path.DirectorySeparatorChar + "FDDC2018";


        /// <summary>
        /// 基本CentOS
        /// </summary>
        //public static String DocBase = @"/home/118_4";

        /// <summary>
        /// 基本MAC
        /// </summary>
        //public static String DocBase = @"/Users/hu/Desktop/FDDC2018";

        //重大合同
        public static bool IsRunContract = false;
        public static bool IsRunContract_TEST = true;
        public static string ContractPath_TRAIN = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_train_20180518" + Path.DirectorySeparatorChar + "重大合同";
        public static string ContractPath_TEST = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_test_b_20180708" + Path.DirectorySeparatorChar + "重大合同";


        //增减持
        public static bool IsRunStockChange = false;
        public static bool IsRunStockChange_TEST = true;
        public static string StockChangePath_TRAIN = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_train_20180518" + Path.DirectorySeparatorChar + "增减持";
        public static string StockChangePath_TEST = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_test_b_20180708" + Path.DirectorySeparatorChar + "增减持";


        //资产重组
        public static bool IsRunReorganization = false;
        public static bool IsRunReorganization_TEST = false;
        public static string ReorganizationPath_TRAIN = DocBase + Path.DirectorySeparatorChar + @"复赛新增类型训练数据-20180712" + Path.DirectorySeparatorChar + "资产重组";
        public static string ReorganizationPath_TEST = DocBase + Path.DirectorySeparatorChar + @"复赛新增类型测试数据-20180712" + Path.DirectorySeparatorChar + "资产重组";

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
            
            var plst = LTPTrainingNER.GetParagraghList(StockChangePath_TEST + "/ner/18877033.xml");
            CompanyNameLogic.GetCompanyNameByNerInfo(plst);            
            return;
            var s0 = "爱康科技向爱康实业、爱康国际、苏州度金、天地国际、钨业研究支付现金购买其合计持有爱康光电100%股权";
            var pos = new PosSegmenter();
            var words = pos.Cut(s0);

            Evaluator = new StreamWriter("Evaluator.log");
            Score = new StreamWriter("Result" + Path.DirectorySeparatorChar + "Score" + Path.DirectorySeparatorChar + "score" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
            //Evaluate.EvaluateReorganizationByFile(@"E:\WorkSpace2018\FDDC2018\FDDC_SRC\Result\chongzu_train.txt");
            //Score.Close();
            //Evaluator.Close();

            //TraningDataset.InitReorganization();
            ReOrganizationTraning.EvaluateMethodList = new string[]{
                "收益法","资产基础法","市场法","市场比较法","估值法","成本法","现金流折现法","现金流折现法","剩余法",
                "内含价值调整法","可比公司市净率法","重置成本法","收益现值法","基础资产法","假设清偿法",
                "成本逼近法","单项资产加和法","成本加和法","基准地价修正法","收益还原法","现金流量法","单项资产加总法","折现现金流量法","基准地价系数修正法"
            }.ToList();
            var t = new Reorganization();
            t.Id = "748379";
            t.HTMLFileName = ReorganizationPath_TEST + "/html/1759374.html";
            //t.TextFileName = ContractPath_TEST + "/txt/128869.txt";
            //t.NerXMLFileName = ContractPath_TEST + "/ner/128869.xml";
            t.Init();
            var recs = t.Extract();
            var s1 = recs[0].ConvertToString();
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
            //结巴分词的地名修正词典
            PosNS.ImportNS("Resources" + Path.DirectorySeparatorChar + "ns.dict");
            CIRecord = new StreamWriter("CI.log");
            QuickTestArea(); return;
            //PDFToTXT.GetPdf2TxtBatchFile();
            //公司全称简称曾用名字典   
            CompanyNameLogic.LoadCompanyName("Resources" + Path.DirectorySeparatorChar + "FDDC_announcements_company_name_20180531.json");
            Evaluator = new StreamWriter("Evaluator.log");
            Score = new StreamWriter("Result" + Path.DirectorySeparatorChar + "Score" + Path.DirectorySeparatorChar + "score" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
            //预处理
            Traning();
            Extract();
            CIRecord.Close();
            Score.Close();
            Evaluator.Close();
            Logger.Close();
        }

        /// <summary>
        /// 最后用抽取
        /// </summary>
        static void Main_FINAL(string[] args)
        {
            Logger = new StreamWriter("Log.log");
            //实体属性器日志设定
            EntityProperty.Logger = Logger;
            //全局编码    
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //结巴分词的地名修正词典
            PosNS.ImportNS("Resources" + Path.DirectorySeparatorChar + "ns.dict");

            if (!Directory.Exists("/home/118_4/submit")) Directory.CreateDirectory("/home/118_4/submit");
            Console.WriteLine("Start To Extract Info Contract TRAIN");
            StreamWriter ResultCSV = new StreamWriter(@"/home/118_4/submit/hetong.txt", false, utf8WithoutBom);
            Run<Contract>(@"/home/data/hetong", @"/home/118_4/temp/hetong", ResultCSV);
            Console.WriteLine("Complete Extract Info Contract");

            Console.WriteLine("Start To Extract Info StockChange TRAIN");
            Console.WriteLine("读取增减持信息：" + "/home/data/zengjianchi/zengjianchi_public.csv");

            var sr = new StreamReader("/home/data/zengjianchi/zengjianchi_public.csv");
            sr.ReadLine();  //Skip Header
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine().Split(",");
                var numbers = RegularTool.GetNumberList(line[0]);
                int year = int.Parse(numbers[0]);
                int month = int.Parse(numbers[1]);
                int day = int.Parse(numbers[2]);
                var AnnouceDate = new DateTime(year, month, day);
                PublishTime.Add(line[1], AnnouceDate);
                //Console.WriteLine("ID:" + line[1] + " Date:" + AnnouceDate.ToString("yyyy-MM-dd"));
            }
            sr.Close();
            Console.WriteLine("读取增减持信息：" + PublishTime.Count);

            ResultCSV = new StreamWriter(@"/home/118_4/submit/zengjianchi.txt", false, utf8WithoutBom);
            Run<StockChange>(@"/home/data/zengjianchi", @"/home/118_4/temp/zengjianchi", ResultCSV);
            Console.WriteLine("Complete Extract Info StockChange");

            Console.WriteLine("Start To Extract Info Reorganization TRAIN");
            //替代训练结果
            Console.WriteLine("加载替代训练结果");
            ReOrganizationTraning.EvaluateMethodList = new string[]{
                "收益法","资产基础法","市场法","市场比较法","估值法","成本法","现金流折现法","现金流折现法","剩余法",
                "内含价值调整法","可比公司市净率法","重置成本法","收益现值法","基础资产法","假设清偿法",
                "成本逼近法","单项资产加和法","成本加和法","基准地价修正法","收益还原法","现金流量法","单项资产加总法","折现现金流量法","基准地价系数修正法"
            }.ToList();
            Console.WriteLine("加载替代训练结果:" + ReOrganizationTraning.EvaluateMethodList.Count);
            ResultCSV = new StreamWriter(@"/home/118_4/submit/chongzu.txt", false, utf8WithoutBom);
            Run<Reorganization>(@"/home/data/chongzu", "", ResultCSV);
            Console.WriteLine("Complete Extract Info Reorganization");

            Logger.Close();
        }

        private static void Traning()
        {
            TraningDataset.InitContract();
            TraningDataset.InitStockChange();
            TraningDataset.InitReorganization();
            Training = new StreamWriter("Training.log");
            //ContractTraning.Train();
            //ReOrganizationTraning.Train();
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
            if (IsRunContract)
            {
                //合同处理
                Console.WriteLine("Start To Extract Info Contract TRAIN");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "hetong_train.txt", false, utf8WithoutBom);
                var Contract_Result = Run<Contract>(ContractPath_TRAIN, ContractPath_TRAIN, ResultCSV);
                Evaluate.EvaluateContract(Contract_Result.Select((x) => (ContractRec)x).ToList());
                Console.WriteLine("Complete Extract Info Contract");
            }
            if (IsRunContract_TEST)
            {
                Console.WriteLine("Start To Extract Info Contract TEST");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "hetong.txt", false, utf8WithoutBom);
                var Contract_Result = Run<Contract>(ContractPath_TEST, ContractPath_TEST, ResultCSV);
                Console.WriteLine("Complete Extract Info Contract");
            }

            if (IsRunStockChange || IsRunStockChange_TEST)
            {
                //增减持公告日期的读入（这里读入的是CSV，本番使用XLSX文件）
                StockChange.ImportPublishTime();
            }

            //增减持
            if (IsRunStockChange)
            {
                Console.WriteLine("Start To Extract Info StockChange TRAIN");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "zengjianchi_train.txt", false, utf8WithoutBom);
                var StockChange_Result = Run<StockChange>(StockChangePath_TRAIN, StockChangePath_TRAIN, ResultCSV);
                Evaluate.EvaluateStockChange(StockChange_Result.Select((x) => (StockChangeRec)x).ToList());
                Console.WriteLine("Complete Extract Info StockChange");
            }
            if (IsRunStockChange_TEST)
            {
                Console.WriteLine("Start To Extract Info StockChange TEST");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "zengjianchi.txt", false, utf8WithoutBom);
                var StockChange_Result = Run<StockChange>(StockChangePath_TEST, StockChangePath_TEST, ResultCSV);
                Console.WriteLine("Complete Extract Info StockChange");
            }

            if (IsRunReorganization || IsRunReorganization_TEST)
            {
                //替代训练结果
                Console.WriteLine("加载替代训练结果");
                ReOrganizationTraning.EvaluateMethodList = new string[]{
                    "收益法","资产基础法","市场法","市场比较法","估值法","成本法","现金流折现法","现金流折现法",
                    "内含价值调整法","可比公司市净率法","重置成本法","收益现值法","基础资产法","假设清偿法",
                    "成本逼近法","单项资产加和法","成本加和法","基准地价修正法","收益还原法","现金流量法","单项资产加总法","折现现金流量法"
                }.ToList();
                Console.WriteLine("加载替代训练结果:" + ReOrganizationTraning.EvaluateMethodList.Count);
            }

            //资产重组
            if (IsRunReorganization)
            {
                Console.WriteLine("Start To Extract Info Reorganization TRAIN");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "chongzu_train.txt", false, utf8WithoutBom);
                var Reorganization_Result = Run<Reorganization>(ReorganizationPath_TRAIN, "", ResultCSV);
                Evaluate.EvaluateReorganization(Reorganization_Result.Select((x) => (ReorganizationRec)x).ToList());
                Console.WriteLine("Complete Extract Info Reorganization");
            }
            if (IsRunReorganization_TEST)
            {
                Console.WriteLine("Start To Extract Info Reorganization TEST");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "chongzu.txt", false, utf8WithoutBom);
                var Reorganization_Result = Run<Reorganization>(ReorganizationPath_TEST, "", ResultCSV);
                Console.WriteLine("Complete Extract Info Reorganization");
            }

        }

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="DataPath"></param>
        /// <typeparam name="T">公告类型</typeparam>
        /// <typeparam name="S">记录类型</typeparam>
        public static List<RecordBase> Run<T>(string DataPath, string TmpPath, StreamWriter ResultCSV) where T : AnnouceDocument, new()
        {
            var Announce_Result = new List<RecordBase>();
            if (IsMultiThreadMode)
            {
                var Bag = new ConcurrentBag<RecordBase>();    //线程安全版本
                Parallel.ForEach(System.IO.Directory.GetFiles(DataPath + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar), (filename) =>
                {
                    try
                    {
                        var announce = new T();
                        var fi = new System.IO.FileInfo(filename);
                        announce.Id = fi.Name.Replace(".html", String.Empty);
                        announce.HTMLFileName = filename;
                        if (!String.IsNullOrEmpty(TmpPath))
                        {
                            announce.TextFileName = TmpPath + Path.DirectorySeparatorChar + "txt" + Path.DirectorySeparatorChar + announce.Id + ".txt";
                            announce.NerXMLFileName = TmpPath + Path.DirectorySeparatorChar + "ner" + Path.DirectorySeparatorChar + announce.Id + ".xml";
                            if (!File.Exists(announce.TextFileName))
                            {
                                Console.WriteLine(announce.Id + "TxtFileNotFound：" + announce.TextFileName);
                            }
                            if (!File.Exists(announce.NerXMLFileName))
                            {
                                Console.WriteLine(announce.Id + "NerXMLFileNotFound：" + announce.NerXMLFileName);
                            }
                        }
                        announce.Init();
                        foreach (var item in announce.Extract())
                        {
                            Bag.Add(item);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });
                Announce_Result = Bag.ToList();
            }
            else
            {
                foreach (var filename in System.IO.Directory.GetFiles(DataPath + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar))
                {
                    var announce = new T();
                    var fi = new System.IO.FileInfo(filename);
                    announce.Id = fi.Name.Replace(".html", String.Empty);
                    announce.HTMLFileName = filename;
                    if (!String.IsNullOrEmpty(TmpPath))
                    {
                        announce.TextFileName = TmpPath + Path.DirectorySeparatorChar + "txt" + Path.DirectorySeparatorChar + announce.Id + ".txt";
                        announce.NerXMLFileName = TmpPath + Path.DirectorySeparatorChar + "ner" + Path.DirectorySeparatorChar + announce.Id + ".xml";
                    }
                    announce.Init();
                    foreach (var item in announce.Extract())
                    {
                        Announce_Result.Add(item);
                    }
                }
            }
            if (IsMultiThreadMode) Announce_Result.Sort((x, y) => { return int.Parse(x.Id).CompareTo(int.Parse(y.Id)); });
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
