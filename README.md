# FDDC2018金融算法挑战赛02－A股上市公司公告信息抽取

更新时间 2018年7月10日 By 带着兔子去旅行


信息抽取是NLP里的一个实用内容。该工具的目标是打造一个泛用的自动信息抽取工具。使得没有任何基础的用户，可以通过简单的步骤提取文档（PDF，HTML，TXT）中的信息。该工具使用C#(.Net Core)开发，所以可以跨平台运行。（Python在做大的工程的时候有诸多不便，所以没有使用python语言）

## 基本环境

* .NetCore2.0
* LTP组件：哈工大LTP3.3.2版
* PDF转TXT工具 pdfminer
* 分词系统：结巴分词

ltp工具：哈工大LTP工具（ltp.ai）提供的ltp工具，最新版为3.3.4.该工具在windows，max，centos上，srl的训练可能无法正常完成。（dp，ner阶段没有问题）所以这里使用了3.3.2版本。ltp工具的SRL结果中包含了DP和NER的内容，但是暂时保留DP和NER中间XML文件。

pdfminer：请注意处理中文的时候需要额外的步骤，具体方法不再赘述。部分PDF可能无法正确转换，原因CaseByCase。

结巴分词：某些地名，例如"大连"，会被误判。这里使用地名辅助字典的方式做纠正。ltp工具没有这个问题。ltp工具和结巴分词功能虽然重复，但是暂时还不能移除结巴分词。

## 前期准备

* 使用pdfminer将PDF文件转化为Txt文件
* 使用哈工大LTP工具，将Txt文件转换为NER，DP，SRL的XML文件

期待文件夹结构

* html（存放HTML文件目录）
* pdf（存放PDF文件目录）
* txt（存放TXT文件目录）
* dp（存放LTP的DP结果XML目录）
* ner（存放LTP的NER结果XML目录）
* srl（存放LTP的SRL结果XML目录）

## 训练（词语统计）

* 分析待提取信息自身的特征
* 分析待提取信息周围语境的特征（LTP工具）
* 构建置信度体系

### 词语自身属性

* 长度
* 包含词数
* 首词词性（POS）
* 词尾

### 语境

* 该关键字在 ：（中文冒号）之后的场景下，：（中文冒号）前面的内容
* 包含该关键字的句子中，该关键字的前置动词
* 包含该关键字的句子中，该关键字是否在角色标识中存在

训练结果例：

```CSharp
协议书(5.180388%)[56]
协议(11.84089%)[128]
合同(58.55689%)[633]
合同书(2.960222%)[32]
买卖合同(3.792784%)[41]
承包合同(12.0259%)[130]
意向书(0.2775208%)[3]
补充协议(1.110083%)[12]
项目(0.2775208%)[3]
书(0.9250694%)[10]
议案(0.2775208%)[3]
)(0.8325624%)[9]
```

(更多规则持续加入中,同时对于相关度低的规则也会剔除)

这里暂时使用频率最高的前5位作为抽取依据。同时为了保证正确率，部分特征的占比必须超过某个阈值。
以下是中文冒号的一个例子，要求前导词占比在40%以上。
（例如前导词A可以正确抽取10个关键字，前导词B可以抽取5个关键字，前导词C可以抽取15个关键字。则前导词A的占比为33%）

```csharp
        e.LeadingColonKeyWordList = ContractTraning.ContractNameLeadingDict
        .Where((x) => { return x.Value >= 40; })    //阈值40%以上
        .Select((x) => { return x.Key + "："; }).ToArray();
```

## 抽取

采用各种方法抽取数据，务必使得所有数据都抽取出来。根据训练结果从候选值里面获得置信度最大的数据。抽取手段如下：

* 具有明确先导词
* NER实体标识
* 具体语境

### 表格抽取工具（内容系）

代码内置表头规则系的表抽取工具，对于表格可以设定如下抽取规则：

* Content:匹配内容
* IsContentEq:内容匹配规则（包含或者相等）

```csharp
    /// <summary>
    /// 表抽取规则（内容系）
    /// </summary>
    public struct TableSearchContentRule
    {
        /// <summary>
        /// 匹配内容
        /// </summary>
        public List<String> Content;
        /// <summary>
        /// 是否相等模式
        /// </summary>
        public bool IsContentEq;
    }
```

下面是一个表格抽取的例子：

```csharp
        var rule = new TableSearchContentRule();
        rule.Content = new string[] { "集中竞价交易", "竞价交易", "大宗交易", "约定式购回" }.ToList();
        rule.IsContentEq = true;
        var result = HTMLTable.GetMultiRowsByContentRule(root,rule);
```

### 表格抽取工具（表头规则系）

代码内置表头规则系的表抽取工具，对于表格可以设定如下抽取规则：

* SuperTitle：层叠表头的情况下，父表头文字
* IsSuperTitleEq：父表头文字匹配规则（包含或者相等）
* Title：表头文字
* IsTitleEq：表头文字匹配规则（包含或者相等）
* IsRequire：在行单位抽取时，该项目是否为必须项目
* ExcludeTitle：表标题不能包含的文字
* Normalize：抽取内容预处理器

```csharp
  /// <summary>
    /// 表抽取规则
    /// </summary>
    public struct TableSearchTitleRule
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
        var result = HTMLTable.GetMultiInfoByTitleRules(root, Rules, false);
```

### EntityProperty对象

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

## 鸣谢

感谢阿里巴巴组委会提供标注好的金融数据。

感谢组委会@通联数据_梅洁,@梅童的及时答疑。

感谢微信好友 邓少冬 潘昭鸣 NLP宋老师 的帮助和指导