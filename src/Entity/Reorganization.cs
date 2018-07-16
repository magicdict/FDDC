using System.Collections.Generic;
using FDDC;

public class Reorganization : AnnouceDocument
{
    public override List<RecordBase> Extract()
    {
        var list = new List<RecordBase>();
        var reorgRec = new ReorganizationRec();
        reorgRec.Id = this.Id;
        reorgRec.EvaluateMethod = getEvaluateMethod();
        list.Add(reorgRec);
        return list;
    }

    /// <summary>
    /// 评估方式
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    string getEvaluateMethod()
    {
        var p = new EntityProperty();
        foreach (var method in ReOrganizationTraning.EvaluateMethodList)
        {
            p.KeyWordMap.Add(method, method);
        }
        p.Extract(this);
        if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("评估方式:" + string.Join("、", p.WordMapResult));
        return string.Join("、", p.WordMapResult);
    }
}