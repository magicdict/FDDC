using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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

            var IsRunStockChange = false;
            var IsRunStockChange_TEST = false;
            var StockChangePath_TRAIN = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_train_20180518" + Path.DirectorySeparatorChar + "增减持";
            var StockChangePath_TEST = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_test_b_20180708" + Path.DirectorySeparatorChar + "增减持";

            var IsRunIncreaseStock = false;
            var IsRunIncreaseStock_TEST = false;
            var IncreaseStockPath_TRAIN = DocBase + Path.DirectorySeparatorChar + @"FDDC_announcements_round1_train_20180518" + Path.DirectorySeparatorChar + "定增";
            var IncreaseStockPath_TEST = DocBase + Path.DirectorySeparatorChar + @"FDDC_announcements_round1_test_b_20180708" + Path.DirectorySeparatorChar + "定增";

            var IsRunReorganization = false;
            var IsRunReorganization_TEST = false;
            var ReorganizationPath_TRAIN = DocBase + Path.DirectorySeparatorChar + @"FDDC_announcements_round2_train_20180715" + Path.DirectorySeparatorChar + "资产重组";
            var ReorganizationPath_TEST = DocBase + Path.DirectorySeparatorChar + @"FDDC_announcements_round2_test_a_20180715" + Path.DirectorySeparatorChar + "资产重组";


            if (IsRunContract)
            {
                //合同处理
                Console.WriteLine("Start To Extract Info Contract TRAIN");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "hetong_train.txt", false, utf8WithoutBom);
                ResultCSV.WriteLine("公告id\t甲方\t乙方\t项目名称\t合同名称\t合同金额上限\t合同金额下限\t联合体成员");
                var Contract_Result = new List<struContract>();
                if (IsMultiThreadMode)
                {
                    var Bag = new ConcurrentBag<struContract>();    //线程安全版本
                    Parallel.ForEach(System.IO.Directory.GetFiles(ContractPath_TRAIN + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar), (filename) =>
                    {
                        var contract = new Contract();
                        foreach (var item in contract.Extract())
                        {
                            Bag.Add(item);
                        }
                    });
                    Contract_Result = Bag.ToList();
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
                        var contract = new Contract();
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
                        var contract = new Contract();
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
                        var contract = new Contract();
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


            if (IsRunReorganization)
            {
                //资产重组
                Console.WriteLine("Start To Extract Info Contract TRAIN");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "hetong_train.txt", false, utf8WithoutBom);
                ResultCSV.WriteLine("公告id\t甲方\t乙方\t项目名称\t合同名称\t合同金额上限\t合同金额下限\t联合体成员");
                var Contract_Result = new List<struContract>();
                if (IsMultiThreadMode)
                {
                    var Bag = new ConcurrentBag<struContract>();    //线程安全版本
                    Parallel.ForEach(System.IO.Directory.GetFiles(ReorganizationPath_TRAIN + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar), (filename) =>
                    {
                        var contract = new Contract();
                        foreach (var item in contract.Extract())
                        {
                            Bag.Add(item);
                        }
                    });
                    Contract_Result = Bag.ToList();
                    Contract_Result.Sort((x, y) => { return x.id.CompareTo(y.id); });
                    foreach (var item in Contract_Result)
                    {
                        ResultCSV.WriteLine(item.ConvertToString(item));
                    }
                }
                else
                {
                    foreach (var filename in System.IO.Directory.GetFiles(ReorganizationPath_TRAIN + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar))
                    {
                        var contract = new Contract();
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
            if (IsRunReorganization_TEST)
            {
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "hetong.txt", false, utf8WithoutBom);
                ResultCSV.WriteLine("公告id\t甲方\t乙方\t项目名称\t合同名称\t合同金额上限\t合同金额下限\t联合体成员");
                var Contract_Result = new List<struContract>();
                Console.WriteLine("Start To Extract Info Contract TEST");
                if (IsMultiThreadMode)
                {
                    Parallel.ForEach(System.IO.Directory.GetFiles(ReorganizationPath_TEST + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar), (filename) =>
                    {
                        var contract = new Contract();
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
                    foreach (var filename in System.IO.Directory.GetFiles(ReorganizationPath_TEST + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar))
                    {
                        var contract = new Contract();
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
                    var stockchange = new StockChange();
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
                    var stockchange = new StockChange();
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
                        var increasestock = new IncreaseStock();
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
                        var increasestock = new IncreaseStock();
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
                    var increasestock = new IncreaseStock();
                    foreach (var item in increasestock.Extract())
                    {
                        ResultCSV.WriteLine(item.ConvertToString(item));
                    }
                }
                ResultCSV.Close();
                Console.WriteLine("Complete Extract Info IncreaseStock");
            }
        }

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T">公告类型</typeparam>
        /// <typeparam name="S">记录类型</typeparam>
        public static void Run<T>(string path) where T : AnnouceDocument, new()
        {
            //合同处理
            Console.WriteLine("Start To Extract Info Contract TRAIN");
            StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "hetong_train.txt", false, utf8WithoutBom);
            ResultCSV.WriteLine("公告id\t甲方\t乙方\t项目名称\t合同名称\t合同金额上限\t合同金额下限\t联合体成员");
            var Contract_Result = new List<IRecord>();
            if (IsMultiThreadMode)
            {
                var Bag = new ConcurrentBag<IRecord>();    //线程安全版本
                Parallel.ForEach(System.IO.Directory.GetFiles(path + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar), (filename) =>
                {
                    var contract = new T();
                    foreach (var item in contract.Extract())
                    {
                        Bag.Add(item);
                    }
                });
                Contract_Result = Bag.ToList();
                //Contract_Result.Sort((x, y) => { return x.id.CompareTo(y.id); });
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
                    foreach (var item in contract.Extract())
                    {
                        Contract_Result.Add(item);
                        ResultCSV.WriteLine(item.ConvertToString());
                    }
                }
            }

            ResultCSV.Close();
            //Evaluate.EvaluateContract(Contract_Result);
            Console.WriteLine("Complete Extract Info Contract");
        }



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
