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
    public string Profits;

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

    public override string GetKey()
    {
        return Id + ":" + Target.NormalizeKey();
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
        /*        
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
        */
        if (Array.Length == 6)
        {
            c.EvaluateMethod = Array[5];
        }
        return c;
    }

    public override string ConvertToString()
    {
        var record = Id + "\t" +
        Target + "\t" +
        TargetCompany + "\t" +
        TradeCompany + "\t";
        record += Normalizer.NormalizeNumberResult(Price) + "\t";
        record += Normalizer.NormalizeNumberResult(Profits) + "\t";
        record += Normalizer.NormalizeNumberResult(MontherCompanyAsset) + "\t";
        record += Normalizer.NormalizeNumberResult(TargetAsset) + "\t";
        record += EvaluateMethod;
        return record;
    }



    public override string CSVTitle()
    {
        return "";
    }
}