# 开放域实体抽取泛用工具

更新时间 2018年8月8日 By 带着兔子去旅行

开发这个工具的起源是天池大数据竞赛，FDDC2018金融算法挑战赛02－A股上市公司公告信息抽取。这个比赛是针对金融公告开展的信息抽取比赛。在参赛过程中，萌生出一个念头，是否能够开发出一个泛用的信息抽取工具呢？

信息抽取是NLP里的一个实用内容。该工具的目标是打造一个泛用的自动信息抽取工具。使得没有任何基础的用户，可以通过简单的步骤提取文档（PDF，HTML，TXT）中的信息。该工具使用C#(.Net Core)开发，所以可以跨平台运行。（Python在做大的工程的时候有诸多不便，所以没有使用python语言）

工具原理采用的是开放域实体抽取的方法:
使用各种方法尽可能抽取实体，然后对于候选内容进行置信度分析打分。

![开放域实体抽取的方法](/img/开放域实体抽取的主要方法.png "开放域实体抽取的方法")

## 基本环境

* .NetCore2.1
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
* dp（存放LTP的DP结果XML目录，暂时没有使用到）
* ner（存放LTP的NER结果XML目录）
* srl（存放LTP的SRL结果XML目录，暂时没有使用到）

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

### 表格

对于大量表格中的关键字，工具也提供了表格统计的功能。主要是统计一下该关键字的表头标题信息。
同时由于表格中的原始数据可能需要通过参照表格标题才能进行比对的情况，这里支持变换器。

```csharp
    /// <summary>
    /// 增发对象训练
    /// </summary>
    public static void TrainingIncreaseTarget()
    {
        var TargetTool = new TableAnlayzeTool();
        var IncreaseNumberTool = new TableAnlayzeTool();
        IncreaseNumberTool.Transform = NumberUtility.NormalizerStockNumber;
        var IncreaseMoneyTool = new TableAnlayzeTool();
        IncreaseMoneyTool.Transform =  MoneyUtility.Format;
        TraningDataset.InitIncreaseStock();
        var PreviewId = String.Empty;
        var PreviewRoot = new HTMLEngine.MyRootHtmlNode();
        foreach (var increase in TraningDataset.IncreaseStockList)
        {
            if (!PreviewId.Equals(increase.id))
            {
                var htmlfile = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\定增\html\" + increase.id + ".html";
                PreviewRoot = new HTMLEngine().Anlayze(htmlfile, "");
                PreviewId = increase.id;
            }
            TargetTool.PutTrainingItem(PreviewRoot, increase.PublishTarget);
            IncreaseNumberTool.PutTrainingItem(PreviewRoot, increase.IncreaseNumber);
            IncreaseMoneyTool.PutTrainingItem(PreviewRoot, increase.IncreaseMoney);
        }
        TargetTool.WriteTop(10);
    }

增发对象
17%	00237	发行对象
16%	00223	发行对象名称
11%	00156	股东名称
09%	00132	认购对象
07%	00096	投资者名称
06%	00085	名称
04%	00061	认购对象名称
04%	00055	获配投资者名称
02%	00035	询价对象名称
02%	00029	配售对象名称
增发数量
30%	00370	获配股数（股）
19%	00234	配售股数（股）
13%	00158	认购股数（股）
10%	00126	持股数量（股）
03%	00045	认购数量（股）
02%	00028	持股总数（股）
02%	00024	配售数量（股）
01%	00019	持股数（股）
01%	00015	获配数量（股）
00%	00011	总股本比例
00%	00011	获配股数(万股)
00%	00011	认购股数（万股）
增发金额
35%	00257	获配金额（元）
21%	00155	认购金额（元）
17%	00125	配售金额（元）
08%	00062	配售金额(元）
02%	00018	认购金额（万元）
02%	00017	认购金额（人民币元）
01%	00014	发行前
01%	00014	申购金额（万元）
01%	00011	获配金额(元）
01%	00008	追加认购金额（元）
```

除了统计标题之外，还可以通过某个标题下面出现的内容。
下面的例子是看一下增减持方式有哪些：

```csharp
    /// <summary>
    /// 增减持训练
    /// </summary>
    /// <param name="TraningCnt">训练条数</param>
    public static void Traning(int TraningCnt = int.MaxValue)
    {
        var ChangeMethodTool = new TableAnlayzeTool();
        var PreviewId = String.Empty;
        var PreviewRoot = new HTMLEngine.MyRootHtmlNode();
        int Cnt = 0;
        foreach (var stockchange in TraningDataset.StockChangeList)
        {
            if (!PreviewId.Equals(stockchange.id))
            {
                var htmlfile = Program.DocBase + @"\FDDC_announcements_round1_train_20180518\增减持\html\" + stockchange.id + ".html";
                PreviewRoot = new HTMLEngine().Anlayze(htmlfile, "");
                PreviewId = stockchange.id;
                Cnt++; if (Cnt == TraningCnt) break;
            }
            ChangeMethodTool.PutValueTrainingItem(PreviewRoot, new string[]{"减持方式","增持方式"}.ToList());
        }
        Program.Training.WriteLine("增减持方式");
        ChangeMethodTool.WriteTop(10);
    }

增减持方式
33%	09277	集中竞价交易
24%	06771	集中竞价
21%	05940	大宗交易
08%	02468	竞价交易
01%	00464	集中竞价减持
01%	00365	减持方式
01%	00303	<null>
01%	00289	二级市场竞价
00%	00258	合计
00%	00196	竞价减持
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
* struRegularExpressFeature：正则表达式特征检索条件
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

### 简单关键字抽取

对于一些及其简单的关键字抽取，例如，出现"现金认购"，则将认购方法标记为"现金"，则可以使用KeyWordMap属性即可。

```csharp
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
        if (!Program.IsMultiThreadMode) Program.Logger.WriteLine("评估方式:" + string.Join(Utility.SplitChar, p.WordMapResult));
        return string.Join(Utility.SplitChar, p.WordMapResult);
    }
```

## 实体位置体系

在寻在实体的时候，尽可能的将找到的实体及其位置进行记录，下面的结构体则是一个实体的记录。

```csharp
    /// <summary>
    /// 位置和值
    /// </summary>
    public struct LocAndValue<T>
    {
        /// <summary>
        /// HTML整体位置
        /// </summary>
        public int Loc;
        /// <summary>
        /// 开始位置
        /// </summary>
        public int StartIdx;
        /// <summary>
        /// 值
        /// </summary>
        public T Value;
        /// <summary>
        /// 类型
        /// </summary>
        public string Type;
    }
```

下面则是一个实体位置的应用。公司里面放着所有公司实体的位置，标的则放着 公司 + 数字百分比 + “股权”等 字样的实体。通过位置信息，则可以将“公司”和“标的”成对发现。

```csharp
    /// <summary>
    /// 从释义表中抽取
    /// </summary>
    /// <returns></returns>
    List<(string Target, string Comany)> getTargetListFromReplaceTable()
    {

        var ReplaceCompany = new List<String>();
        foreach (var c in companynamelist)
        {
            if (c.positionId == -1)
            {
                //释义表
                if (!String.IsNullOrEmpty(c.secFullName)) ReplaceCompany.Add(c.secFullName);
                if (!String.IsNullOrEmpty(c.secShortName)) ReplaceCompany.Add(c.secShortName);
            }
        }

        var TargetAndCompanyList = new List<(string Target, string Comany)>();
        var targetRegular = new ExtractProperyBase.struRegularExpressFeature()
        {
            LeadingWordList = ReplaceCompany,
            RegularExpress = RegularTool.PercentExpress,
            TrailingWordList = new string[] { "的股权", "股权", "的权益", "权益" }.ToList()
        };
        var targetLoc = ExtractPropertyByHTML.FindRegularExpressLoc(targetRegular, root);
        targetLoc.AddRange(ExtractPropertyByHTML.FindWordLoc("资产及负债", root));
        targetLoc.AddRange(ExtractPropertyByHTML.FindWordLoc("业务及相关资产负债", root));

        foreach (var item in ReplacementDict)
        {
            var keys = item.Key.Split(Utility.SplitChar);
            var keys2 = item.Key.Split(new char[] { '／', '/' });
            if (keys.Length == 1 && keys2.Length > 1)
            {
                keys = keys2;
            }
            var values = item.Value.Split(Utility.SplitChar);
            var values2 = item.Value.Split("；");
            if (values.Length == 1 && values2.Length > 1)
            {
                values = values2;
            }
            foreach (var key in keys)
            {
                if (key == "交易标的")
                {
                    foreach (var value in values)
                    {
                        var targetAndcompany = value.Trim();
                        //将公司名称和交易标的划分开来
                        ExtractPropertyByHTML.RegularExFinder(0, value, targetRegular, "|");
                    }
                }
            }
        }
        return TargetAndCompanyList;
    }
```

LocateProperty.LocateCustomerWord方法用来定位自定义的字符串列表

```csharp
       //在全文中寻找交易对象出现的地方
        var traderLoc = LocateProperty.LocateCustomerWord(root, trades);
        var targetLoc = LocateProperty.LocateCustomerWord(root, targets.Select(x => x.TargetCompany).ToList());
        var TradeLocMap = new Dictionary<int, List<LocateProperty.LocAndValue<String>>>();
        var TargetLocMap = new Dictionary<int, List<LocateProperty.LocAndValue<String>>>();
        //按照段落为依据，进行分组
        foreach (var trloc in traderLoc)
        {
            if (!TradeLocMap.ContainsKey(trloc.Loc))
            {
                TradeLocMap.Add(trloc.Loc, new List<LocateProperty.LocAndValue<String>>());
            }
            TradeLocMap[trloc.Loc].Add(trloc);
        }
        foreach (var trloc in targetLoc)
        {
            if (!TargetLocMap.ContainsKey(trloc.Loc))
            {
                TargetLocMap.Add(trloc.Loc, new List<LocateProperty.LocAndValue<String>>());
            }
            TargetLocMap[trloc.Loc].Add(trloc);
        }
```

NerMap是一个地图，标记这每个段落中的实体信息

```csharp
    public void Anlayze(AnnouceDocument doc)
    {
        var nerlist = new List<LocAndValue<String>>();
        if (doc.Nerlist != null)
        {
            var nh = doc.Nerlist.Where(x => x.Type == enmNerType.Nh).Select(y => y.RawData).Distinct();
            nerlist.AddRange(LocateCustomerWord(doc.root, nh.ToList(), "人名"));

            var ni = doc.Nerlist.Where(x => x.Type == enmNerType.Ni).Select(y => y.RawData).Distinct();
            nerlist.AddRange(LocateCustomerWord(doc.root, ni.ToList(), "机构"));

            var ns = doc.Nerlist.Where(x => x.Type == enmNerType.Ns).Select(y => y.RawData).Distinct();
            nerlist.AddRange(LocateCustomerWord(doc.root, ns.ToList(), "地名"));

        }

        foreach (var paragragh in doc.root.Children)
        {
            foreach (var s in paragragh.Children)
            {
                var p = LocateParagraphInfo(doc, s.PositionId, nerlist);
                if (p.NerList.Count + p.moneylist.Count + p.datelist.Count != 0)
                {
                    if (!ParagraghlocateDict.ContainsKey(s.PositionId)) ParagraghlocateDict.Add(s.PositionId, p);
                }
            }
        }
    }
```

## 参考文献

* [自然语言处理和信息抽取](https://wenku.baidu.com/view/9df6408971fe910ef12df8af.html)

## 鸣谢

* 感谢阿里巴巴组委会提供标注好的金融数据。
* 感谢组委会@通联数据_梅洁,@梅童的及时答疑。
* 感谢微信好友 邓少冬 潘昭鸣 NLP宋老师 的帮助和指导
