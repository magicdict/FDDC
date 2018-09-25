import pandas as pd
from sklearn.preprocessing import LabelEncoder  # 枚举标签转数字
from keras_preprocessing import text
from keras.preprocessing import sequence
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

def importcsvdata(filename):
    dataframe = pd.read_table(
        filename,
        header=0,
    )
    label = LabelEncoder()
    label.fit(list(dataframe["tag"].values))
    dataframe["tag"] = label.transform(list(dataframe["tag"].values))
    dataframe["query_prediction"] = list(
        map(get_query_prediction_value, dataframe["query_prediction"],
            dataframe["title"]))
    return dataframe

def get_word2vec(dataframe):    
    #词向量
    # jieba分词
    wordList = dataframe["title"].apply(lambda x:list(jieba.cut(x)))
    words_dict =[]
    texts = []
    stoplist = []
    # 去掉停用词
    for words in wordList:
        line = [word for word in words if word not in stoplist]
        words_dict.extend([word for word in line])
        texts.append(line)
    maxlen = 0
    for line in texts:
        if maxlen < len(line):
            maxlen = len(line)
    max_words=50000
    # 利用keras的Tokenizer进行onehot，并调整未等长数组
    tokenizer = text.Tokenizer(num_words=max_words)
    tokenizer.fit_on_texts(texts)
    data_w = tokenizer.texts_to_sequences(texts)    
    word2vec = sequence.pad_sequences(data_w, maxlen=maxlen)
    return word2vec
