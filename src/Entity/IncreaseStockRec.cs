public class IncreaseStockRec: RecordBase
{

    //增发对象
    public string PublishTarget;

    //增发数量
    public string IncreaseNumber;

    //增发金额
    public string IncreaseMoney;

    //锁定期（一般原则：定价36个月，竞价12个月）
    //但是这里牵涉到不同对象不同锁定期的可能性
    public string FreezeYear;

    //认购方式（现金股票）
    public string BuyMethod;
    public override string GetKey()
    {
        return Id + ":" + PublishTarget.NormalizeKey();
    }
    public static IncreaseStockRec ConvertFromString(string str)
    {
        var Array = str.Split("\t");
        var c = new IncreaseStockRec();
        c.Id = Array[0];
        c.PublishTarget = Array[1];
        if (Array.Length > 2)
        {
            c.IncreaseNumber = Array[2];
        }
        if (Array.Length > 3)
        {
            c.IncreaseMoney = Array[3];
        }
        if (Array.Length > 4)
        {
            c.FreezeYear = Array[4];
        }
        if (Array.Length > 5)
        {
            c.BuyMethod = Array[5];
        }
        return c;
    }


    public override string ConvertToString()
    {
        var record = Id + "\t" +
        PublishTarget + "\t";
        record += Normalizer.NormalizeNumberResult(IncreaseNumber) + "\t";
        record += Normalizer.NormalizeNumberResult(IncreaseMoney) + "\t";
        record += FreezeYear + "\t" + BuyMethod;
        return record;
    }


    public override string CSVTitle()
    {
        return "公告id\t增发对象\t增发数量\t增发金额\t锁定期\t认购方式";
    }
}