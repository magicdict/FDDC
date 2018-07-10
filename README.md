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

### 表格抽取工具（表头规则系）

代码内置的表抽取工具，对于表格可以设定如下抽取规则：

* SuperTitle：层叠表头的情况下，父表头文字
* IsSuperTitleEq：父表头文字匹配规则（包含或者相等）
* Title：表头文字
* IsTitleEq：表头文字匹配规则（包含或者相等）
* IsRequire：在行单位抽取时，该项目是否为必须项目
* ExcludeTitle：表标题不能包含的文字
* Normalize：抽取内容预处理器

_以行内容为依据的表格抽取工具开发中..._

```csharp
  /// <summary>
    /// 表抽取规则
    /// </summary>
    public struct TableSearchRule
    {
        public string Name;
        /// <summary>
        /// 父标题
        /// </summary>
        public List<String> SuperTitle;
        /// <summary>
        /// 是否必须一致
        /// </summary>
        public bool IsSuperTitleEq;
        /// <summary>
        /// 标题
        /// </summary>
        public List<String> Title;
        /// <summary>
        /// 是否必须一致
        /// </summary>
        public bool IsTitleEq;
        /// <summary>
        /// 是否必须
        /// </summary>
        public bool IsRequire;
        /// <summary>
        /// 表标题不能包含的文字
        /// </summary>
        public List<String> ExcludeTitle;
        /// <summary>
        /// 抽取内容预处理器
        /// </summary>
        public Func<String, String, String> Normalize;
    }
```

下面是一个表格抽取的例子：

| 增持前|（合并表头） |增持后|（合并表头）  |
| ------ | ------ | ------ | ------ |

| 持股数  | 持股比例 |持股数  | 持股比例 |
| ------ | ------ | ------ | ------ |

这里我们想抽取持股比例和持股数，但是希望抽取的是增持后的部分，所以需要使用SuperTitle的规则了。

```csharp
        var HoldList = new List<struHoldAfter>();
        var StockHolderRule = new TableSearchRule();
        StockHolderRule.Name = "股东全称";
        StockHolderRule.Title = new string[] { "股东名称", "名称", "增持主体", "增持人", "减持主体", "减持人" }.ToList();
        StockHolderRule.IsTitleEq = true;
        StockHolderRule.IsRequire = true;

        var HoldNumberAfterChangeRule = new TableSearchRule();
        HoldNumberAfterChangeRule.Name = "变动后持股数";
        HoldNumberAfterChangeRule.IsRequire = true;
        HoldNumberAfterChangeRule.SuperTitle = new string[] { "减持后", "增持后" }.ToList();
        HoldNumberAfterChangeRule.IsSuperTitleEq = false;
        HoldNumberAfterChangeRule.Title = new string[] {
             "持股股数","持股股数",
             "持股数量","持股数量",
             "持股总数","持股总数","股数"
        }.ToList();
        HoldNumberAfterChangeRule.IsTitleEq = false;

        var HoldPercentAfterChangeRule = new TableSearchRule();
        HoldPercentAfterChangeRule.Name = "变动后持股数比例";
        HoldPercentAfterChangeRule.IsRequire = true;
        HoldPercentAfterChangeRule.SuperTitle = HoldNumberAfterChangeRule.SuperTitle;
        HoldPercentAfterChangeRule.IsSuperTitleEq = false;
        HoldPercentAfterChangeRule.Title = new string[] { "比例" }.ToList();
        HoldPercentAfterChangeRule.IsTitleEq = false;

        var Rules = new List<TableSearchRule>();
        Rules.Add(StockHolderRule);
        Rules.Add(HoldNumberAfterChangeRule);
        Rules.Add(HoldPercentAfterChangeRule);
        var result = HTMLTable.GetMultiInfo(root, Rules, false);
```


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
