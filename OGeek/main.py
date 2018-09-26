import pandas as pd
import numpy as np
import os
# 内部文件
import config
import dataprocess
import createModels
import utility

if (config.run_mode == "training"):
    print("Feature Start")
    # 从CSV读取数据
    dataframe = dataprocess.importcsvdata(config.training_csv,None)
    #去重复数据
    dataframe.drop_duplicates()
    #词频-反转文档频率
    #dataprocess.get_words_tfidf(dataframe)

    ent_x = np.array(dataframe["label"].values)
    print("信息熵：" + str(utility.calc_ent(ent_x)))
    ent_y_title_len = np.array(
        list(map(lambda x: len(x), dataframe["title"].values)))
    ent_y_tag = np.array(dataframe["tag"].values)
    print("标题字符长度信息增益：" + str(utility.calc_ent_grap(ent_x, ent_y_title_len)))
    print("类别信息增益：" + str(utility.calc_ent_grap(ent_x, ent_y_tag)))
    g = dataframe.groupby(['tag', 'label'])
    df = pd.DataFrame(g.count()['title'])
    print(df)

    #词向量
    word2vec = dataprocess.get_word2vec(dataframe)
    #词长度
    titlelen = list(map(lambda x: len(x), dataframe["title"].values))
    # 特征和标签划分
    dataset = dataframe.values
    # 只选择类别和编辑距离乘积两个特征 标题变成向量
    X = np.hstack((dataset[:, 1:2], dataset[:, 3:4],
                   word2vec))  #hstack 两个矩阵横向合并
    y = dataset[:, 4].astype('int')
    print("Data End")

    print("training Start")

    #保存Model(注:save文件夹要预先建立，否则会报错)
    if (config.model_type == "svc"):
        model = createModels.get_svc_model(X, y)
    else:
        model = createModels.get_keras_cnn_model(X, y)
else:
    # 读取测试集
    dataframe = dataprocess.importcsvdata(config.test_csv,None)
    # 特征和标签划分
    dataset = dataframe.values
    word2vec = dataprocess.get_word2vec(dataframe)
    # 只选择类别和编辑距离乘积两个特征
    X = np.hstack((dataset[:, 1:2], dataset[:, 3:4],
                   word2vec))  #hstack 两个矩阵横向合并
    if (config.model_type == "svc"):
        svc_model = createModels.load_svc_model()
        y = svc_model.predict(X)
        print("svc score:" + str(svc_model.score(X, y)))
    else:
        # Dropout随机断开激活，所以结果会不一样?
        keras_model = createModels.load_keras_cnn_model()
        y = keras_model.predict(X)
    print("training End")

    r = pd.DataFrame(y)
    r.to_csv(config.result_csv, index=None, header=None)
