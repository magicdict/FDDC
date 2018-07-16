   public class StockChangeRec : RecordBase
    {
        //股东全称
        public string HolderFullName;

        //股东简称
        public string HolderShortName;

        //变动截止日期
        public string ChangeEndDate;

        //变动价格
        public string ChangePrice;

        //变动数量
        public string ChangeNumber;

        //变动后持股数
        public string HoldNumberAfterChange;

        //变动后持股比例
        public string HoldPercentAfterChange;

        public override string GetKey()
        {
            return Id + ":" + HolderFullName.NormalizeKey() + ":" + ChangeEndDate;
        }
        public static StockChangeRec ConvertFromString(string str)
        {
            var Array = str.Split("\t");
            var c = new StockChangeRec();
            c.Id = Array[0];
            c.HolderFullName = Array[1];
            c.HolderShortName = Array[2];
            if (Array.Length > 3)
            {
                c.ChangeEndDate = Array[3];
            }
            if (Array.Length > 4)
            {
                c.ChangePrice = Array[4];
            }
            if (Array.Length > 5)
            {
                c.ChangeNumber = Array[5];
            }
            if (Array.Length > 6)
            {
                c.HoldNumberAfterChange = Array[6];
            }
            if (Array.Length == 8)
            {
                c.HoldPercentAfterChange = Array[7];
            }
            return c;
        }

        public override string ConvertToString()
        {
            var record = Id + "\t" +
            HolderFullName + "\t" +
            HolderShortName + "\t" +
            ChangeEndDate + "\t";
            record += Normalizer.NormalizeNumberResult(ChangePrice) + "\t";
            record += Normalizer.NormalizeNumberResult(ChangeNumber) + "\t";
            record += Normalizer.NormalizeNumberResult(HoldNumberAfterChange) + "\t";
            record += Normalizer.NormalizeNumberResult(HoldPercentAfterChange);
            return record;
        }

    public override string CSVTitle()
    {
        return "公告id\t股东全称\t股东简称\t变动截止日期\t变动价格\t变动数量\t变动后持股数\t变动后持股比例";
    }
}