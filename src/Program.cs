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

namespace FDDC
{
    class Program
    {
        public static Encoding utf8WithoutBom = new System.Text.UTF8Encoding(false);
        public static StreamWriter Training = new StreamWriter("Training.log");
        public static StreamWriter Logger = new StreamWriter("Log.log");
        public static StreamWriter Score = new StreamWriter(@"Result" + Path.DirectorySeparatorChar + "Score" + Path.DirectorySeparatorChar + "score" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
        //WIN
        public static String DocBase = @"E:\WorkSpace2018\FDDC2018";
        //MAC
        //public static String DocBase = @"/Users/hu/Desktop/FDDCTraing";

        //这个模式下，有问题的数据会输出，正式比赛的时候设置为False，降低召回率！
        public static bool IsDebugMode = false;

        static void Main(string[] args)
        {
            //生成PDF的TXT文件的批处理命令
            //PDFToTXT.GetBatchFile();    
            //初始化   
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            CompanyNameLogic.LoadCompanyName(@"Resources" + Path.DirectorySeparatorChar + "FDDC_announcements_company_name_20180531.json");
            if (IsDebugMode) WordUtility.DictNSAdjust = new string[] { };    //调试模式下，去掉地名调整字典
            TraningDataset.InitStockChange();
            TraningDataset.InitContract();
            ContractTraning.TraningMaxLenth();
            ContractTraning.EntityWordPerperty();
            //ContractTraning.GetListLeadWords();
            //警告：可能所有的Segmenter使用的是共用的词典！
            //下面的训练将把关键字加入到词典中，引发一些问题
            //ContractTraning.AnlayzeEntitySurroundWords();
            TraningDataset.InitIncreaseStock();
            Training.Close();
            UT();
            Extract();
            Logger.Close();
            Score.Close();

        }

        private static void Extract()
        {
            var IsRunContract = false;
            var IsRunContract_TEST = false;
            var ContractPath_TRAIN = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_train_20180518" + Path.DirectorySeparatorChar + "round1_train_20180518" + Path.DirectorySeparatorChar + "重大合同";
            var ContractPath_TEST = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_test_a_20180605" + Path.DirectorySeparatorChar + "重大合同";

            var IsRunStockChange = false;
            var IsRunStockChange_TEST = true;
            var StockChangePath_TRAIN = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_train_20180518" + Path.DirectorySeparatorChar + "round1_train_20180518" + Path.DirectorySeparatorChar + "增减持";
            var StockChangePath_TEST = DocBase + Path.DirectorySeparatorChar + "FDDC_announcements_round1_test_a_20180605" + Path.DirectorySeparatorChar + "增减持";

            var IsRunIncreaseStock = false;
            var IsRunIncreaseStock_TEST = false;
            var IncreaseStockPath_TRAIN = DocBase + Path.DirectorySeparatorChar + @"FDDC_announcements_round1_train_20180518" + Path.DirectorySeparatorChar + "round1_train_20180518" + Path.DirectorySeparatorChar + "定增";
            var IncreaseStockPath_TEST = DocBase + Path.DirectorySeparatorChar + @"FDDC_announcements_round1_test_a_20180605" + Path.DirectorySeparatorChar + "定增";

            if (IsRunContract)
            {
                //合同处理
                //通过训练获得各种字段的最大长度，便于抽取的时候做置信度检查
                Console.WriteLine("Start To Extract Info Contract TRAIN");
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "hetong_train.txt", false, utf8WithoutBom);
                ResultCSV.WriteLine("公告id\t甲方\t乙方\t项目名称\t合同名称\t合同金额上限\t合同金额下限\t联合体成员");
                var StockChange_Result = new List<struContract>();
                foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TRAIN + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar))
                {
                    foreach (var item in Contract.Extract(filename))
                    {
                        StockChange_Result.Add(item);
                        ResultCSV.WriteLine(item.ConvertToString(item));
                    }
                }
                ResultCSV.Close();
                Evaluate.EvaluateContract(StockChange_Result);
                Console.WriteLine("Complete Extract Info Contract");
            }
            if (IsRunContract_TEST)
            {
                StreamWriter ResultCSV = new StreamWriter("Result" + Path.DirectorySeparatorChar + "hetong.txt", false, utf8WithoutBom);
                ResultCSV.WriteLine("公告id\t甲方\t乙方\t项目名称\t合同名称\t合同金额上限\t合同金额下限\t联合体成员");
                Console.WriteLine("Start To Extract Info Contract TEST");
                foreach (var filename in System.IO.Directory.GetFiles(ContractPath_TEST + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar))
                {
                    foreach (var item in Contract.Extract(filename))
                    {
                        ResultCSV.WriteLine(item.ConvertToString(item));
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
                    foreach (var item in StockChange.Extract(filename))
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
                    foreach (var item in StockChange.Extract(filename))
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
                foreach (var filename in System.IO.Directory.GetFiles(IncreaseStockPath_TRAIN + Path.DirectorySeparatorChar + "html" + Path.DirectorySeparatorChar))
                {
                    foreach (var item in IncreaseStock.Extract(filename))
                    {
                        Increase_Result.Add(item);
                        ResultCSV.WriteLine(item.ConvertToString(item));
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
                    foreach (var item in IncreaseStock.Extract(filename))
                    {
                        ResultCSV.WriteLine(item.ConvertToString(item));
                    }
                }
                ResultCSV.Close();
                Console.WriteLine("Complete Extract Info IncreaseStock");
            }
        }

        private static void UT()
        {
            var s0 = "华陆工程（科技）有限责任公司";
            JiebaSegmenter segmenter = new JiebaSegmenter();
            segmenter.AddWord("华陆工程科技有限责任公司");
            segmenter.AddWord("中煤陕西榆林能源化工有限公司");
            PosSegmenter posSeg = new PosSegmenter(segmenter);
            var c = posSeg.Cut(s0);
            s0 = s0.NormalizeTextResult();
            s0 = RegularTool.Trimbrackets(s0);
            //Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1775568.html");
            StockChange.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持\html\300393.html");
        }
    }
}
