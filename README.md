# FDDC2018金融算法挑战赛02－A股上市公司公告信息抽取

## 基本环境

* .NetCore2.0
* LTP组件：哈工大LTP3.3.2版
* PDF转TXT工具 pdfminer

## 前期准备

* 使用pdfminer将PDF文件转化为Txt文件
* 使用哈工大LTP工具，将Txt文件转换为NER，DP，SRL的XML文件

## 训练

* 分析待提取信息自身的特征
* 分析待提取信息周围语境的特征（LTP工具）
* 构建置信度体系

## 抽取

采用各种方法抽取数据，务必使得所有数据都抽取出来。根据训练结果从候选值里面获得置信度最大的数据。抽取手段如下：

* 具有明确先导词
* NER实体标识
* 具体语境

## 抽取工具

EntityProperty对象属性如下：

* PropertyName：属性名称
* PropertyType：属性类型（数字，金额，字符，日期）
* MaxLength：最大长度
* MinLength：最小长度
* MaxLengthCheckPreprocess：最大长度判定前预处理器（不改变抽取内容）
* LeadingColonKeyWordList：先导词（包含"："）
* LeadingColonKeyWordCandidatePreprocess：先导词预处理器（**改变抽取内容**）
* QuotationTrailingWordList:引号和书名号中的词语
* DpKeyWordList：句法依存环境
* ExternalStartEndStringFeature：普通的开始结尾词判定
* CandidatePreprocess:一般候选词预处理器（**改变抽取内容**）
* ExcludeContainsWordList：不能包含词语列表
* ExcludeEqualsWordList：不能等于词语列表
* Confidence：置信度对象

```csharp
    /// <summary>
    /// 获得合同名
    /// </summary>
    /// <returns></returns>
    string GetContractName()
    {
        var e = new EntityProperty();
        e.PropertyName = "合同名称";
        e.PropertyType = EntityProperty.enmType.Normal;
        e.MaxLength = ContractTraning.MaxContractNameLength;
        e.MinLength = 5;
        /* 训练模式下 
        e.LeadingColonKeyWordList = ContractTraning.ContractNameLeadingDict
                                    .Where((x) => { return x.Value >= 40; })    //阈值40%以上
                                    .Select((x) => { return x.Key + "："; }).ToArray();
        */
        e.LeadingColonKeyWordList =  new string[] { "合同名称：" };
        e.QuotationTrailingWordList = new string[] { "协议书", "合同书", "确认书", "合同", "协议" };
        e.QuotationTrailingWordList_IsSkipBracket = true;   //暂时只能选True
        var KeyList = new List<ExtractPropertyByDP.DPKeyWord>();
        KeyList.Add(new ExtractPropertyByDP.DPKeyWord()
        {
            StartWord = new string[] { "签署", "签订" },    //通过SRL训练获得
            StartDPValue = new string[] { LTPTrainingDP.核心关系, LTPTrainingDP.定中关系, LTPTrainingDP.并列关系 },
            EndWord = new string[] { "补充协议", "合同书", "合同", "协议书", "协议", },
            EndDPValue = new string[] { LTPTrainingDP.核心关系, LTPTrainingDP.定中关系, LTPTrainingDP.并列关系, LTPTrainingDP.动宾关系, LTPTrainingDP.主谓关系 }
        });
        e.DpKeyWordList = KeyList;

        var StartArray = new string[] { "签署了", "签订了" };   //通过语境训练获得
        var EndArray = new string[] { "合同" };
        e.ExternalStartEndStringFeature = Utility.GetStartEndStringArray(StartArray, EndArray);
        e.ExternalStartEndStringFeatureCandidatePreprocess = (x) => { return x + "合同"; };
        e.MaxLengthCheckPreprocess = str =>
        {
            return EntityWordAnlayzeTool.TrimEnglish(str);
        };
        //最高级别的置信度，特殊处理器
        e.LeadingColonKeyWordCandidatePreprocess = str =>
        {
            var c = Normalizer.ClearTrailing(TrimJianCheng(str));
            return c;
        };

        e.CandidatePreprocess = str =>
        {
            var c = Normalizer.ClearTrailing(TrimJianCheng(str));
            var RightQMarkIdx = c.IndexOf("”");
            if (!(RightQMarkIdx != -1 && RightQMarkIdx != c.Length - 1))
            {
                //对于"XXX"合同，有右边引号，但不是最后的时候，不用做
                c = c.TrimStart("“".ToCharArray());
            }
            c = c.TrimStart("《".ToCharArray());
            c = c.TrimEnd("》".ToCharArray()).TrimEnd("”".ToCharArray());
            return c;
        };
        e.ExcludeContainsWordList = new string[] { "日常经营重大合同" };
        //下面这个列表的根据不足
        e.ExcludeEqualsWordList = new string[] { "合同", "重大合同", "项目合同", "终止协议", "经营合同", "特别重大合同", "相关项目合同" };
        e.Extract(this);

        //是否所有的候选词里面包括（测试集无法使用）
        var contractlist = TraningDataset.ContractList.Where((x) => { return x.id == this.Id; });
        if (contractlist.Count() > 0)
        {
            var contract = contractlist.First();
            var contractname = contract.ContractName;
            if (!String.IsNullOrEmpty(contractname))
            {
                e.CheckIsCandidateContainsTarget(contractname);
            }
        }
        //置信度
        e.Confidence = ContractTraning.ContractES.GetStardardCI();
        return e.EvaluateCI();
    }
```
