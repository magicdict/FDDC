using System;
using System.Collections.Generic;
using System.Linq;
using FDDC;

public class Reorganization : AnnouceDocument
{
    public override List<RecordBase> Extract()
    {
        var list = new List<RecordBase>();
        var targets = getTargetList();
        var EvaluateMethod = getEvaluateMethod();
        foreach (var item in targets)
        {
            var reorgRec = new ReorganizationRec();
            reorgRec.Id = this.Id;
            reorgRec.Target = item.Target;
            reorgRec.TargetCompany = item.Comany;
            reorgRec.EvaluateMethod = EvaluateMethod;
            list.Add(reorgRec);
        }
        return list;
    }

    /// <summary>
    /// 获得标的
    /// </summary>
    /// <returns></returns>
    List<(string Target, string Comany)> getTargetList()
    {
        var rtn = new List<(string Target, string Comany)>();

        var targetRegular = new ExtractProperyBase.struRegularExpressFeature()
        {
            RegularExpress = RegularTool.PercentExpress,
            TrailingWordList = new string[] { "股权" }.ToList()
        };
        var targetLoc = ExtractPropertyByHTML.FindRegularExpressLoc(targetRegular, root);

        //所有公司名称
        var CompanyList = new List<string>();
        foreach (var companyname in companynamelist)
        {
            //注意，这里的companyname.WordIdx是分词之后的开始位置，不是位置信息！
            if (!CompanyList.Contains(companyname.secFullName))
            {
                if (!string.IsNullOrEmpty(companyname.secFullName)) CompanyList.Add(companyname.secFullName);
            }
            if (!CompanyList.Contains(companyname.secShortName))
            {
                if (!string.IsNullOrEmpty(companyname.secShortName)) CompanyList.Add(companyname.secShortName);
            }
        }

        var targetlist = new List<string>();

        foreach (var companyname in CompanyList)
        {
            var companyLoc = ExtractPropertyByHTML.FindWordLoc(companyname, root);
            foreach (var company in companyLoc)
            {
                foreach (var target in targetLoc)
                {
                    var EndIdx = company.StartIdx + company.Value.Length;
                    if (company.Loc == target.Loc && Math.Abs(target.StartIdx - EndIdx) < 2)
                    {
                        if (!targetlist.Contains(target.Value + ":" + company.Value))
                        {
                            rtn.Add((target.Value, company.Value));
                            targetlist.Add(target.Value + ":" + company.Value);
                        }
                    }
                }
            }
        }

        return rtn;
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
        if (p.WordMapResult == null) return string.Empty;
        if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("评估方式:" + string.Join("、", p.WordMapResult));
        return string.Join("、", p.WordMapResult);
    }
}