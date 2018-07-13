public class Reorganization : AnnouceDocument
{
    public struct struReorganization
    {
        /// <summary>
        /// 公告id
        /// </summary>
        public string id;

        /// <summary>
        /// 交易标的
        /// </summary>
        public string Target;
        /// <summary>
        /// 标的公司
        /// </summary>
        public string TargetCompany;
        /// <summary>
        /// 交易对方
        /// </summary>
        public string TradeCompany;
        /// <summary>
        /// 交易标的作价
        /// </summary>
        public string Price;
        /// <summary>
        /// 标的承诺归母净利润
        /// </summary>
        public string profits;

        /// <summary>
        /// 标的归母净资产
        /// </summary>
        public string MontherCompanyAsset;

        /// <summary>
        /// 标的净资产
        /// </summary>
        public string TargetAsset;

        /// <summary>
        /// 评估方法
        /// </summary>
        public string EvaluateMethod;

        public string GetKey()
        {
            return id + ":" + Target.NormalizeKey();
        }


        public static struReorganization ConvertFromString(string str)
        {
            var Array = str.Split("\t");
            var c = new struReorganization();
            c.id = Array[0];
            c.Target = Array[1];
            c.TargetCompany = Array[2];
            if (Array.Length > 3)
            {
                c.TradeCompany = Array[3];
            }
            if (Array.Length > 4)
            {
                c.Price = Array[4];
            }
            if (Array.Length > 5)
            {
                c.profits = Array[5];
            }
            if (Array.Length > 6)
            {
                c.MontherCompanyAsset = Array[6];
            }
            if (Array.Length > 7)
            {
                c.TargetAsset = Array[7];
            }
            if (Array.Length == 9)
            {
                c.EvaluateMethod = Array[8];
            }
            return c;
        }

        public string ConvertToString(struReorganization reorganization)
        {
            var record = reorganization.id + "\t" +
            reorganization.Target + "\t" +
            reorganization.TargetCompany + "\t" +
            reorganization.TradeCompany + "\t";
            record += Normalizer.NormalizeNumberResult(reorganization.Price) + "\t";
            record += Normalizer.NormalizeNumberResult(reorganization.profits) + "\t";
            record += Normalizer.NormalizeNumberResult(reorganization.MontherCompanyAsset) + "\t";
            record += Normalizer.NormalizeNumberResult(reorganization.TargetAsset) + "\t";
            record += reorganization.EvaluateMethod;
            return record;
        }

    }
}