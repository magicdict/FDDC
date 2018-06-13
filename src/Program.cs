using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using static Contract;
using static IncreaseStock;
using static StockChange;

namespace FDDC
{
    class Program
    {

        public static StreamWriter Training = new StreamWriter("Training.log");
        public static StreamWriter Logger = new StreamWriter("Log.log");
        public static StreamWriter Score = new StreamWriter(@"Result\Score\score" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
        public static String DocBase = @"E:\FDDC";
        static void Main(string[] args)
        {
            //生成PDF的TXT文件的批处理命令
            //PDFToTXT.GetBatchFile();    
            //初始化   
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            BussinessLogic.LoadCompanyName(@"Resources\FDDC_announcements_company_name_20180531.json");
            TraningDataset.InitContract();
            TraningDataset.InitStockChange();
            TraningDataset.InitIncreaseStock();
            ContractTraning.TraningMaxLenth();
            ContractTraning.EntityWordPerperty();
            Training.Close();
            UT();
            Extract();
            Logger.Close();
            Score.Close();

        }

        private static void Extract()
        {
            var IsRunContract = true;
            var IsRunContract_TEST = true;
            var ContractPath_TRAIN = DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同";
            var ContractPath_TEST = DocBase + @"\FDDC_announcements_round1_test_a_20180605\重大合同";

            var IsRunStockChange = false;
            var IsRunStockChange_TEST = false;
            var StockChangePath_TRAIN = DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\增减持";
            var StockChangePath_TEST = DocBase + @"\FDDC_announcements_round1_test_a_20180605\增减持";

            var IsRunIncreaseStock = false;
            var IsRunIncreaseStock_TEST = false;
            var IncreaseStockPath_TRAIN = DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\定增";
            var IncreaseStockPath_TEST = DocBase + @"\FDDC_announcements_round1_test_a_20180605\定增";

            if (IsRunContract)
            {
                //合同处理
                //通过训练获得各种字段的最大长度，便于抽取的时候做置信度检查
                Console.WriteLine("Start To Extract Info Contract TRAIN");
                StreamWriter ResultCSV = new StreamWriter("Result\\hetong_train.csv", false, Encoding.GetEncoding("gb2312"));
                ResultCSV.WriteLine("公告id,甲方,乙方,项目名称,合同名称,合同金额上限,合同金额下限,联合体成员");
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
                Evaluate.EvaluateContract(StockChange_Result);
                Console.WriteLine("Complete Extract Info Contract");
            }
            if (IsRunContract_TEST)
            {
                StreamWriter ResultCSV = new StreamWriter("Result\\hetong.csv", false, Encoding.GetEncoding("gb2312"));
                ResultCSV.WriteLine("公告id,甲方,乙方,项目名称,合同名称,合同金额上限,合同金额下限,联合体成员");
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
                StreamWriter ResultCSV = new StreamWriter("Result\\zengjianchi_train.csv", false, Encoding.GetEncoding("gb2312"));
                ResultCSV.WriteLine("公告id,股东全称,股东简称,变动截止日期,变动价格,变动数量,变动后持股数,变动后持股比例");
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
                Evaluate.EvaluateStockChange(StockChange_Result);
                Console.WriteLine("Complete Extract Info StockChange");
            }
            if (IsRunStockChange_TEST)
            {
                StreamWriter ResultCSV = new StreamWriter("Result\\zengjianchi.csv", false, Encoding.GetEncoding("gb2312"));
                ResultCSV.WriteLine("公告id,股东全称,股东简称,变动截止日期,变动价格,变动数量,变动后持股数,变动后持股比例");
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
                ResultCSV.WriteLine("公告id,增发对象,增发数量,增发金额,锁定期,认购方式");
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
                Evaluate.EvaluateIncreaseStock(Increase_Result);
                Console.WriteLine("Complete Extract Info IncreaseStock");
            }

            if (IsRunIncreaseStock_TEST)
            {
                StreamWriter ResultCSV = new StreamWriter("Result\\dingzeng.csv", false, Encoding.GetEncoding("gb2312"));
                ResultCSV.WriteLine("公告id,增发对象,增发数量,增发金额,锁定期,认购方式");
                Console.WriteLine("Start To Extract Info IncreaseStock TEST");
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
        }

        private static void UT()
        {
            //EntityWordAnlayzeTool.ConsoleWritePos("北京金泉广场和摩根中心项目的弱电系统总包工程框架协议》，确立由国电南瑞科技股份有限公司");
            //Console.WriteLine(EntityWordAnlayzeTool.TrimEnglish("CNOOC Iraq Limited（中海油伊拉克有限公司）"));
            //Contract.Extract(Program.DocBase + @"\FDDC_announcements_round1_train_20180518\round1_train_20180518\重大合同\html\1132889.html");
            //ContractTraning.AnlayzeEntitySurroundWords();
            //UT.WordAnlayzeTest();
            //return;
            //ContractTraning.Train();
            //系统分析
            //WordAnlayze.TraningByWordAnlyaze();
            //PropertyWordAnlayze.EntityWordAnlayze();
            //测试区
            //UT.RunWordAnlayze();
            //UT.StockChangeTest();
            //UT.IncreaseStockTest();
            //UT.ContractTest();
            //UT.RegularExpress();
            //UT.JianchengTest();
            //Traning.InitIncreaseStock();
            //WordAnlayze.segmenter.LoadUserDict(@"Resources\dictAdjust.txt");
            //Logger.Close();
        }
    }
}
