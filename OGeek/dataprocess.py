import os
#os.environ['KERAS_BACKEND']='tensorflow'

import pandas as pd
from sklearn.preprocessing import LabelEncoder  # 枚举标签转数字
from keras_preprocessing import text
from keras.preprocessing import sequence
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.feature_extraction.text import TfidfTransformer

import json
import config
import utility
import jieba


# 计算编辑距离
def get_query_prediction_value(query_prediction, title):
    dict = json.loads(s=query_prediction)
    diff = 0.0
    for k, v in dict.items():
        diff = diff + float(v) * utility.levenshtein(k, title)
    #print(title + ":" + str(diff))
    return diff


def importcsvdata(filename, nrows):
    dataframe = pd.read_table(filename, header=0, nrows=nrows)
    # 将类型进行标签化整理
    label = LabelEncoder()
    label.fit(list(dataframe["tag"].values))
    dataframe["tag"] = label.transform(list(dataframe["tag"].values))
    dataframe["query_prediction"] = list(
        map(get_query_prediction_value, dataframe["query_prediction"],
            dataframe["title"]))
    return dataframe


def get_words_tfidf(dataframe):
    #语料
    corpus = dataframe["title"].apply(lambda x: " ".join(list(jieba.cut(x))))
    #该类会将文本中的词语转换为词频矩阵，矩阵元素a[i][j] 表示j词在i类文本下的词频
    vectorizer = CountVectorizer()
    #该类会统计每个词语的tf-idf权值
    transformer = TfidfTransformer()
    #第一个fit_transform是计算tf-idf，第二个fit_transform是将文本转为词频矩阵
    tfidf = transformer.fit_transform(vectorizer.fit_transform(corpus))
    word = vectorizer.get_feature_names()  #获取词袋模型中的所有词语
    #将tf-idf矩阵抽取出来，元素a[i][j]表示j词在i类文本中的tf-idf权重
    weight = tfidf.toarray()
    #打印每类文本的tf-idf词语权重，第一个for遍历所有文本，第二个for便利某一类文本下的词语权重
    for i in range(len(weight)):
        print(u"-------这里输出第", i, u"类文本的词语tf-idf权重------")
        for j in range(len(word)):
            print(word[j], weight[i][j])


def get_word2vec(dataframe):
    #词向量
    # jieba分词
    wordList = dataframe["title"].apply(lambda x: list(jieba.cut(x)))
    #words_dict =[]
    texts = []
    stoplist = []
    # 去掉停用词
    for words in wordList:
        line = [word for word in words if word not in stoplist]
        #words_dict.extend([word for word in line])
        texts.append(line)
    maxlen = 0
    for line in texts:
        if maxlen < len(line):
            maxlen = len(line)
    max_words = 50000
    # 利用keras的Tokenizer进行onehot，并调整未等长数组
    tokenizer = text.Tokenizer(num_words=max_words)
    tokenizer.fit_on_texts(texts)
    data_w = tokenizer.texts_to_sequences(texts)
    word2vec = sequence.pad_sequences(data_w, maxlen=maxlen)
    return word2vec
