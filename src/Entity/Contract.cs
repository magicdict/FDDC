using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using 金融数据整理大赛;

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
        ExtractSingle(node, Id);
        ExtractMulti(node, Id);
    }


    static struContract ExtractSingle(HTMLEngine.MyHtmlNode node, String Id)
    {
        var contract = new struContract();
        //公告ID
        contract.id = Id;
        //甲方
        contract.JiaFang = GetJiaFang(node);
        //乙方
        contract.YiFang = GetYiFang(node);
        //合同金额的提取
        var ContractMoneyUpLimit = GetMoney(node);

        //测试数据集：
        var StandardKey = new List<String>();
        foreach (var c in Traning.GetContractById(contract.id))
        {
            StandardKey.Add(c.GetKey());
            Program.Logger.WriteLine("合同标准主键：" + c.GetKey());
        }
        var MyKey = contract.GetKey();
        Program.Logger.WriteLine("合同提取主键：" + contract.GetKey());
        if (StandardKey.Contains(MyKey))
        {
            CorrectKey++;
        }
        return contract;
    }



    //是否为多个合同或者多个工程
    static List<struContract> ExtractMulti(HTMLEngine.MyHtmlNode node, String Id)
    {
        var contractlist = new List<struContract>();
        //寻找一下列表项目
        foreach (var lst in node.DetailItemList.Values)
        {
            //能否提取到合同，工程，如果可以的话，直接提取了
            foreach (var content in lst)
            {
                if (GetContractName(content) != "")
                {
                    var contract = new struContract();
                    //公告ID
                    contract.id = Id;
                    //乙方
                    contract.YiFang = GetYiFang(node);
                    contract.ContractName = GetContractName(content);
                    contract.ContractMoneyUpLimit = GetMoney(content);
                    contractlist.Add(contract);
                }
            }
        }
        return contractlist;
    }

    static string GetContractName(String strContent)
    {
        Regex r = new Regex(@"(?<=\《)(\S+)(?=\》)");
        if (r.IsMatch(strContent))
        {
            strContent = r.Match(strContent).Value;
            if (strContent.EndsWith("合同"))
            {
                Program.Logger.WriteLine("合同:" + strContent);
                return strContent;
            }
        }
        return "";
    }

    static string GetProjectName(String strContent)
    {
        return "";
    }



    static string GetJiaFang(HTMLEngine.MyHtmlNode node)
    {
        var KeyWordList = new string[] { "招标人：", "业主方：", "业主：", "甲方：" };
        foreach (var keyword in KeyWordList)
        {
            var JiaFang = HTMLEngine.GetValueAfterString(node, keyword);
            if (JiaFang != "")
            {
                if (JiaFang.Contains("（")) JiaFang = Utility.GetStringBefore(JiaFang, "（");
                Program.Logger.WriteLine("甲方:[" + JiaFang + "]");
                return JiaFang;
            }
            JiaFang = HTMLEngine.GetValueFromNextContent(node, keyword);
            if (JiaFang != "")
            {
                if (JiaFang.Contains("（")) JiaFang = Utility.GetStringBefore(JiaFang, "（");
                Program.Logger.WriteLine("甲方：[" + JiaFang + "]");
                return JiaFang;
            }
        }

        var KeyWordListArray = new string[][]
        {
            //招标
            new string[]{"业主", "发来"},
            new string[]{"业主", "发出"},
            new string[]{"收到", "发出"},
            new string[]{"收到", "发来"},
            new string[]{"接到", "发出"},
            new string[]{"接到", "发来"},

            new string[]{"收到", "的中标"},
            new string[]{"接到", "的中标"},

            new string[]{"业主", "对总承包"},      //+1
            new string[]{"确定为", "工程候选人"},   //+1
            //合同
            new string[]{"与", "签署"},
            new string[]{"与", "签订"}
        };
        foreach (var keyword in KeyWordListArray)
        {
            var JiaFang = HTMLEngine.GetValueBetweenString(node, keyword[0], keyword[1]);
            if (JiaFang != "")
            {
                if (JiaFang.Contains("（")) JiaFang = Utility.GetStringBefore(JiaFang, "（");
                Program.Logger.WriteLine("甲方：[" + JiaFang + "]");
                return JiaFang;
            }
        }
        return "";
    }


    static string GetYiFang(HTMLEngine.MyHtmlNode node)
    {
        //乙方
        var YiFang = "";
        var Content = HTMLEngine.searchKeyWord(node, "有限公司");
        for (int i = Content.Count - 1; i > 0; i--)
        {
            string line = Content[i].Content;
            YiFang = Utility.GetStringBefore(line, "有限公司");
            if (!String.IsNullOrEmpty(YiFang))
            {
                YiFang = YiFang + "有限公司";
                if (YiFang.Contains("（")) YiFang = Utility.GetStringBefore(YiFang, "（");
                Program.Logger.WriteLine("乙方：[" + YiFang + "]");
                return YiFang;
            }
        }
        return "";
    }



    #region  Money
    static string GetMoney(string Content)
    {
        var Money = "";
        var KeyWordList = new string[] { "中标金额", "中标价", "合同金额" };
        foreach (var keyword in KeyWordList)
        {
            Money = Utility.SeekMoney(Content, keyword);
            if (!String.IsNullOrEmpty(Money))
            {
                Program.Logger.WriteLine(keyword + "：[" + Money + "]");
                return Money;
            }
        }
        return Money;
    }
    static string GetMoney(HTMLEngine.MyHtmlNode node)
    {
        //金额
        var Money = "";
        var KeyWordList = new string[] { "中标金额", "中标价", "合同金额" };
        foreach (var keyword in KeyWordList)
        {
            var Content = HTMLEngine.searchKeyWord(node, keyword);
            foreach (var line in Content)
            {
                Money = Utility.SeekMoney(line.Content, keyword);
                if (!String.IsNullOrEmpty(Money))
                {
                    Program.Logger.WriteLine(keyword + "：[" + Money + "]");
                    return Money;
                }
            }

            Content = HTMLEngine.searchKeyWordAtTable(node, keyword);
            foreach (var table in Content)
            {
                Money = HTMLEngine.GetValueFromTableNode(table, keyword);
                if (!String.IsNullOrEmpty(Money))
                {
                    Program.Logger.WriteLine(keyword + "(FromTable)：[" + Money + "]");
                    return Money;
                }
            }
        }
        return "";
    }
    #endregion
}