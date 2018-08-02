public class ReorganizationRec : RecordBase
{
    /// <summary>
    /// 交易标的
    /// </summary>
    public string Target;
    /// <summary>
    /// 标的公司
    /// </summary>
    public string TargetCompany;

    /// <summary>
    /// 标的公司全称（辅助用，不输出）
    /// </summary>
    public string TargetCompanyFullName;
    /// <summary>
    /// 标的公司简称（辅助用，不输出）
    /// </summary>
    public string TargetCompanyShortName;

    /// <summary>
    /// 交易对方
    /// </summary>
    public string TradeCompany;

    /// <summary>
    /// 标的公司全称（辅助用，不输出）
    /// </summary>
    public string TradeCompanyFullName;
    /// <summary>
    /// 标的公司简称（辅助用，不输出）
    /// </summary>
    public string TradeCompanyShortName;


    /// <summary>
    /// 交易标的作价
    /// </summary>
    public string Price;
    /// <summary>
    /// 评估方法
    /// </summary>
    public string EvaluateMethod;

    public override string GetKey()
    {
        var TargetCompanys = TargetCompany.NormalizeKey().Split("|");
        if (TargetCompanys.Length == 1)
        {
            return Id + ":" + Target.NormalizeKey() + ":" + TargetCompany.NormalizeKey();
        }
        else
        {
            return Id + ":" + Target.NormalizeKey() + ":" + TargetCompanys[0].NormalizeKey() + "|" +
                   Id + ":" + Target.NormalizeKey() + ":" + TargetCompanys[1].NormalizeKey();
        }
    }

    public static ReorganizationRec ConvertFromString(string str)
    {
        var Array = str.Split("\t");
        var c = new ReorganizationRec();
        c.Id = Array[0];
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
        if (Array.Length == 6)
        {
            c.EvaluateMethod = Array[5];
        }
        return c;
    }

    public override string ConvertToString()
    {
        var record = Id + "\t" + Target + "\t" + TargetCompany + "\t" + TradeCompany + "\t";
        record += Normalizer.NormalizeNumberResult(Price) + "\t";
        record += EvaluateMethod;
        return record;
    }

    public override string CSVTitle()
    {
        return "公告id\t交易标的\t标的公司\t交易对方\t交易标的作价\t评估方法";
    }
}