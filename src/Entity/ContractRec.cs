
    public class ContractRec : RecordBase
    {
        //甲方
        public string JiaFang;

        //乙方
        public string YiFang;

        //项目名称
        public string ProjectName;

        //合同名称
        public string ContractName;

        //合同金额上限
        public string ContractMoneyUpLimit;

        //合同金额下限
        public string ContractMoneyDownLimit;

        //联合体成员
        public string UnionMember;

        public override string GetKey()
        {
            //去空格转小写
            return Id + ":" + JiaFang.NormalizeKey() + ":" + YiFang.NormalizeKey();
        }
        public static ContractRec ConvertFromString(string str)
        {
            var Array = str.Split("\t");
            var c = new ContractRec();
            c.Id = Array[0];
            c.JiaFang = Array[1];
            c.YiFang = Array[2];
            c.ProjectName = Array[3];
            if (Array.Length > 4)
            {
                c.ContractName = Array[4];
            }
            if (Array.Length > 6)
            {
                c.ContractMoneyUpLimit = Array[5];
                c.ContractMoneyDownLimit = Array[6];
            }
            if (Array.Length == 8)
            {
                c.UnionMember = Array[7];
            }
            return c;
        }

        public override string ConvertToString()
        {
            var record = Id + "\t" +
                         JiaFang + "\t" +
                         YiFang + "\t" +
                         ProjectName + "\t" +
                         ContractName + "\t";
            record += ContractMoneyUpLimit + "\t";
            record += ContractMoneyDownLimit + "\t";
            record += UnionMember;
            return record;
        }

    public override string CSVTitle()
    {
        return "公告id\t甲方\t乙方\t项目名称\t合同名称\t合同金额上限\t合同金额下限\t联合体成员";
    }
}