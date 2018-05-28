using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using 金融数据整理大赛;
using static HTMLEngine;

public class Contract
{
    public struct struContract
    {
        //公告id
        public string id;

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
        public string COntractMoneyDownLimit;

        //联合体成员
        public string UnionMember;

        public string GetKey()
        {
            return id + ":" + JiaFang + ":" + YiFang;
        }
    }


    internal static struContract ConvertFromString(string str)
    {
        var Array = str.Split("\t");
        var c = new struContract();
        c.id = Array[0];
        c.JiaFang = Array[1];
        c.YiFang = Array[2];
        c.ProjectName = Array[3];
        if (Array.Length > 5)
        {
            c.ContractName = Array[4];
        }
        if (Array.Length > 6)
        {
            c.ContractMoneyUpLimit = Array[5];
            c.COntractMoneyDownLimit = Array[6];
        }
        if (Array.Length == 8)
        {
            c.UnionMember = Array[7];
        }
        return c;
    }

    internal static string ConvertToString(struContract contract)
    {
        return contract.id + "," +
               contract.JiaFang + "," +
               contract.YiFang + "," +
               contract.ProjectName + "," +
               contract.ContractName + "," +
               contract.ContractMoneyUpLimit + "," +
               contract.COntractMoneyDownLimit + "," +
               contract.UnionMember;
    }

    public static int CorrectKey = 0;


    public static void Extract(string htmlFileName)
    {
        //模式1：只有一个主合同
        //模式2：只有多个子合同
        //模式3：有一个主合同以及多个子合同
        var fi = new System.IO.FileInfo(htmlFileName);
        Program.Logger.WriteLine("Start FileName:[" + fi.Name + "]");
        var node = HTMLEngine.Anlayze(htmlFileName);
        var Id = fi.Name.Replace(".html", "");
        Program.Logger.WriteLine("公告ID:" + Id);
        //主合同的抽取
        ExtractSingle(node, Id);
        //子合同的抽取
        ExtractMulti(node, Id);
    }


    static struContract ExtractSingle(MyRootHtmlNode node, String Id)
    {
        var contract = new struContract();
        //公告ID
        contract.id = Id;
        //甲方
        contract.JiaFang = GetJiaFang(node);
        //乙方
        contract.YiFang = GetYiFang(node);
        //合同
        contract.ContractName = GetContractName(node);
        return contract;
    }



    //是否为多个合同或者多个工程
    static List<struContract> ExtractMulti(MyRootHtmlNode node, String Id)
    {
        var contractlist = new List<struContract>();
        //寻找一下列表项目
        foreach (var lst in node.DetailItemList.Values)
        {
            //能否提取到合同，工程，如果可以的话，直接提取了
            foreach (var content in lst)
            {

            }
        }
        return contractlist;
    }



    static string GetJiaFang(MyRootHtmlNode node)
    {
        var Extractor = new ExtractProperty();
        //这些关键字后面
        Extractor.LeadingWordList = new string[] { "招标人：", "业主方：", "业主：", "甲方：" };
        Extractor.Extract(node);
        foreach (var item in Extractor.CandidateWord)
        {
            Program.Logger.WriteLine("甲方候补词：[" + item + "]");
        }

        //招标
        Extractor = new ExtractProperty();
        var StartArray = new string[] { "业主", "收到", "接到" };
        var EndArray = new string[] { "发来", "发出", "的中标" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(node);
        foreach (var item in Extractor.CandidateWord)
        {
            Program.Logger.WriteLine("甲方候补词：[" + item + "]");
        }

        //合同
        Extractor = new ExtractProperty();
        StartArray = new string[] { "与" };
        EndArray = new string[] { "签署", "签订" };
        Extractor.StartEndFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        Extractor.Extract(node);
        foreach (var item in Extractor.CandidateWord)
        {
            Program.Logger.WriteLine("甲方候补词：[" + item + "]");
        }
        return "";
    }



    static string GetContractName(MyRootHtmlNode root)
    {
        var Extractor = new ExtractProperty();
        var MarkFeature = new ExtractProperty.struMarkFeature();
        MarkFeature.MarkStartWith = "《";
        MarkFeature.MarkEndWith = "》";
        MarkFeature.InnerEndWith = "合同";
        Extractor.MarkFeature = new ExtractProperty.struMarkFeature[] { MarkFeature };
        Extractor.Extract(root);
        foreach (var item in Extractor.CandidateWord)
        {
            Program.Logger.WriteLine("合同候补词：[" + item + "]");
        }
        return "";
    }

    static string GetProjectName(String strContent)
    {
        return "";
    }


    static string GetYiFang(HTMLEngine.MyHtmlNode node)
    {
        //乙方:"有限公司"
        return "";
    }



    #region  Money
    static string GetMoney(string Content)
    {
        var Money = "";
        var KeyWordList = new string[] { "中标金额", "中标价", "合同金额" };
        return Money;
    }
    static string GetMoney(HTMLEngine.MyHtmlNode node)
    {
        //金额
        var Money = "";
        var KeyWordList = new string[] { "中标金额", "中标价", "合同金额" };
        return "";
    }
    #endregion
}