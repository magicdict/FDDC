using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using static Contract;
using static IncreaseStock;
using static StockChange;
using JiebaNet.Segmenter;
using JiebaNet.Segmenter.PosSeg;
using System.Threading.Tasks;

namespace FDDC
{
    class Program
    {
        public static Encoding utf8WithoutBom = new System.Text.UTF8Encoding(false);
        public static StreamWriter Training = new StreamWriter("Training.log");
        public static StreamWriter Logger = new StreamWriter("Log.log");
        public static StreamWriter Evaluator = new StreamWriter("Evaluator.log");
        public static StreamWriter Score = new StreamWriter(@"Result" + Path.DirectorySeparatorChar + "Score" + Path.DirectorySeparatorChar + "score" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
        //WIN
        public static String DocBase = @"E:\WorkSpace2018\FDDC2018";
        //MAC
        //public static String DocBase = @"/Users/hu/Desktop/FDDCTraing";

        /// <summary>
        /// 这个模式下，有问题的数据会输出，正式比赛的时候设置为False，降低召回率！
        /// </summary>
        public static bool IsDebugMode = false;
        /// <summary>
        /// 多线程模式
        /// </summary>
        public static bool IsMultiThreadMode = false;

        static void Main(string[] args)
        {
            //全局编码    
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //初始化   
            CompanyNameLogic.LoadCompanyName(@"Resources" + Path.DirectorySeparatorChar + "FDDC_announcements_company_name_20180531.json");
            //增减持公告日期的读入
            ImportPublishTime();
            //预处理
            //PDFMiner:PDF转TXTbatch
            //PDFToTXT.GetPdf2TxtBatchFile(); return;
            //TXT整理
            //PDFToTXT.FormatTxtFile(); return;
            //LTP:XML生成Batch
            //PDFToTXT.GetLTPXMLBatchFile(); return;            
            //LTP.Anlayze(@"E:\WorkSpace2018\FDDC2018\1021332.xml"); return;
            if (IsDebugMode) WordUtility.DictNSAdjust = new string[] { };    //调试模式下，去掉地名调整字典
            TraningDataset.InitStockChange();
            TraningDataset.InitContract();
            TraningDataset.InitIncreaseStock();
            //通过训练获得各种字段的最大长度，便于抽取的时候做置信度检查
            ContractTraning.TraningMaxLenth();
            ContractTraning.EntityWordPerperty();
            //ContractTraning.GetListLeadWords();
            //警告：可能所有的Segmenter使用的是共用的词典！
            //下面的训练将把关键字加入到词典中，引发一些问题
            //ContractTraning.AnlayzeEntitySurroundWordsLTP(); Training.Close(); return;
            Training.Close();
            
            //new StockChange(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持\html\300393.html").Extract();
            //return;
            
            Extract();
            Logger.Close();
            Score.Close();
            Evaluator.Close();
        }

        /// <summary>
        /// 增减持公告发布日期
        /// </summary>
        /// <typeparam name="String"></typeparam>
        /// <typeparam name="String"></typeparam>
        /// <returns></returns>
        public static Dictionary<String, String> PublishTime = new Dictionary<String, String>();


        private static void Extract()
        {
            var IsRunContract = false;
            var IsRunContract_TEST = false;
            var ContractPath_TRAIN = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_train_20180518" + Path.DirectorySeparatorChar + "round1_train_20180518" + Path.DirectorySeparatorChar + "重大合同";
            var ContractPath_TEST = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_test_a_20180605" + Path.DirectorySeparatorChar + "重大合同";

            var IsRunStockChange = true;
            var IsRunStockChange_TEST = false;
            var StockChangePath_TRAIN = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_train_20180518" + Path.DirectorySeparatorChar + "round1_train_20180518" + Path.DirectorySeparatorChar + "增减持";
            var StockChangePath_TEST = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_test_a_20180605" + Path.DirectorySeparatorChar + "增减持";

            var IsRunIncreaseStock = false;
            var IsRunIncreaseStock_TEST = false;
            var IncreaseStockPath_TRAIN = DocBase + Path.DirectorySeparatorChar + @"FDDC_announcements_round1_train_20180518" + Path.DirectorySeparatorChar + "round1_train_20180518" + Path.DirectorySeparatorChar + "定增";
            var IncreaseStockPath_TEST = DocBase + Path.DirectorySeparatorChar + @"FDDC_announcements_round1_test_a_20180605" + Path.DirectorySeparatorChar + "定增";

            if (IsRunContract)
            {
                //合同处理
                Console.WriteLine("Start To Extract Info Contract TRAIN");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "hetong_train.txt", false, utf8WithoutBom);
                ResultCSV.WriteLine("公告id\t甲方\t乙方\t项目名称\t合同名称\t合同金额上限\t合同金额下限\t联合体成员");
                var Contract_Result = new List<struContract>();

                if (IsMultiThreadMode)
                {
                    Parallel.ForEach(System.IO.Directory.GetFiles(ContractPath_TRAIN + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar), (filename) =>
                    {
                        var contract = new Contract(filename);
                        foreach (var item in contract.Extract())
                        {
                            Contract_Result.Add(item);
                        }
                    });
                    Contract_Result.Sort((x, y) => { return x.id.CompareTo(y.id); });
                    foreach (var item in Contract_Result)
                    {
                        ResultCSV.WriteLine(item.ConvertToString(item));
                    }
                }
                else
                {
                    foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar))
                    {
                        var contract = new Contract(filename);
                        foreach (var item in contract.Extract())
                        {
                            Contract_Result.Add(item);
                            ResultCSV.WriteLine(item.ConvertToString(item));
                        }
                    }
                }

                ResultCSV.Close();
                Evaluate.EvaluateContract(Contract_Result);
                Console.WriteLine("Complete Extract Info Contract");
            }
            if (IsRunContract_TEST)
            {
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "hetong.txt", false, utf8WithoutBom);
                ResultCSV.WriteLine("公告id\t甲方\t乙方\t项目名称\t合同名称\t合同金额上限\t合同金额下限\t联合体成员");
                var Contract_Result = new List<struContract>();
                Console.WriteLine("Start To Extract Info Contract TEST");
                if (IsMultiThreadMode)
                {
                    Parallel.ForEach(System.IO.Directory.GetFiles(ContractPath_TEST + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar), (filename) =>
                    {
                        var contract = new Contract(filename);
                        foreach (var item in contract.Extract())
                        {
                            Contract_Result.Add(item);
                        }
                    });
                    Contract_Result.Sort((x, y) => { return x.id.CompareTo(y.id); });
                    foreach (var item in Contract_Result)
                    {
                        ResultCSV.WriteLine(item.ConvertToString(item));
                    }
                }
                else
                {
                    foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TEST + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar))
                    {
                        var contract = new Contract(filename);
                        foreach (var item in contract.Extract())
                        {
                            Contract_Result.Add(item);
                            ResultCSV.WriteLine(item.ConvertToString(item));
                        }
                    }
                }
                ResultCSV.Close();
                Console.WriteLine("Complete Extract Info Contract");
            }


            if (IsRunStockChange)
            {
                //增减持
                Console.WriteLine("Start To Extract Info StockChange TRAIN");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "zengjianchi_train.txt", false, utf8WithoutBom);
                ResultCSV.WriteLine("公告id\t股东全称\t股东简称\t变动截止日期\t变动价格\t变动数量\t变动后持股数\t变动后持股比例");
                var StockChange_Result = new List<struStockChange>();
                foreach (var filename in System.IO.Directory.GetFiles(StockChangePath_TRAIN + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar))
                {
                    var stockchange = new StockChange(filename);
                    foreach (var item in stockchange.Extract())
                    {
                        StockChange_Result.Add(item);
                        ResultCSV.WriteLine(item.ConvertToString(item));
                    }
                }
                ResultCSV.Close();
                Evaluate.EvaluateStockChange(StockChange_Result);
                Console.WriteLine("Complete Extract Info StockChange");
            }
            if (IsRunStockChange_TEST)
            {
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "zengjianchi.txt", false, utf8WithoutBom);
                ResultCSV.WriteLine("公告id\t股东全称\t股东简称\t变动截止日期\t变动价格\t变动数量\t变动后持股数\t变动后持股比例");
                Console.WriteLine("Start To Extract Info StockChange TEST");
                foreach (var filename in System.IO.Directory.GetFiles(StockChangePath_TEST + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar))
                {
                    var stockchange = new StockChange(filename);
                    foreach (var item in stockchange.Extract())
                    {
                        ResultCSV.WriteLine(item.ConvertToString(item));
                    }
                }
                ResultCSV.Close();
                Console.WriteLine("Complete Extract Info StockChange");
            }

            if (IsRunIncreaseStock)
            {
                //定增
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "dingzeng_train.txt", false, utf8WithoutBom);
                ResultCSV.WriteLine("公告id\t增发对象\t增发数量\t增发金额\t锁定期\t认购方式");
                Console.WriteLine("Start To Extract Info IncreaseStock TRAIN");
                var Increase_Result = new List<struIncreaseStock>();

                if (IsMultiThreadMode)
                {
                    Parallel.ForEach(System.IO.Directory.GetFiles(IncreaseStockPath_TRAIN + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar), (filename) =>
                    {
                        var increasestock = new IncreaseStock(filename);
                        foreach (var item in increasestock.Extract())
                        {
                            Increase_Result.Add(item);
                        }
                    });
                    Increase_Result.Sort((x, y) => { return x.id.CompareTo(y.id); });
                    foreach (var item in Increase_Result)
                    {
                        ResultCSV.WriteLine(item.ConvertToString(item));
                    }
                }
                else
                {
                    foreach (var filename in System.IO.Directory.GetFiles(IncreaseStockPath_TRAIN + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar))
                    {
                        var increasestock = new IncreaseStock(filename);
                        foreach (var item in increasestock.Extract())
                        {
                            Increase_Result.Add(item);
                            ResultCSV.WriteLine(item.ConvertToString(item));
                        }
                    }
                }
                ResultCSV.Close();
                Evaluate.EvaluateIncreaseStock(Increase_Result);
                Console.WriteLine("Complete Extract Info IncreaseStock");
            }

            if (IsRunIncreaseStock_TEST)
            {
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "dingzeng.txt", false, utf8WithoutBom);
                ResultCSV.WriteLine("公告id\t增发对象\t增发数量\t增发金额\t锁定期\t认购方式");
                Console.WriteLine("Start To Extract Info IncreaseStock TEST");
                foreach (var filename in System.IO.Directory.GetFiles(IncreaseStockPath_TEST + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar))
                {
                    var increasestock = new IncreaseStock(filename);
                    foreach (var item in increasestock.Extract())
                    {
                        ResultCSV.WriteLine(item.ConvertToString(item));
                    }
                }
                ResultCSV.Close();
                Console.WriteLine("Complete Extract Info IncreaseStock");
            }
        }

        private static void ImportPublishTime()
        {
            foreach (var csvfilename in System.IO.Directory.GetFiles(DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_public_time_20180629"))
            {
                if (csvfilename.EndsWith(".csv"))
                {
                    var sr = new StreamReader(csvfilename);
                    sr.ReadLine();  //Skip Header
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine().Split(",");
                        PublishTime.Add(line[1], line[0]);
                    }
                }
            }
        }

        private static void UT()
        {
            var ContractPath_TRAIN = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_train_20180518\\round1_train_20180518" + Path.DirectorySeparatorChar + "重大合同";
            foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + Path.DirectorySeparatorChar + "srl" + Path.DirectorySeparatorChar))
            {
                var Srllist = LTP.AnlayzeSRL(filename);
                var fi = new FileInfo(filename);
                if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("Name：" + fi.Name);
                foreach (var m in Srllist)
                {
                    if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("SRL：" + m);
                }
            }
            //var s0 = "公司子公司山东省路桥集团有限公司（简称“路桥集团”）与山东高速建设集团有限公司（简称“建设集团”）就蓬莱西海岸海洋文化旅游产业聚集区区域建设用海工程项目（简称“本项目”）签署了前期工作委托协议（简称“本协议”）。";
            //var BracketList = RegularTool.GetChineseBrackets(s0);
            //var s1 = RegularTool.TrimChineseBrackets(s0);
            //var contract = new Contract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1450.html");
            //var result = contract.Extract();
            //IncreaseStock.Extract(Program.DocBase + @"\FDDC_announcements_round1_test_a_20180605\定增\html\15304036");
            //new Contract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\797495.html").Extract();
        }
    }
}
