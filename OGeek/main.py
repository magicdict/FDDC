import pandas as pd
import numpy as np

# 内部文件
import config
import dataprocess
import createModels

# 从CSV读取数据
dataframe = pd.read_table(
    config.training_csv,
    header=0,
)

print("Feature Start")

# 将类型进行标签化整理
dataframe = dataprocess.importcsvdata(config.training_csv)
word2vec = dataprocess.get_word2vec(dataframe)
# 特征和标签划分
dataset = dataframe.values
# 只选择类别和编辑距离乘积两个特征 标题变成向量
X = np.hstack((dataset[:, 1:2], dataset[:, 3:4], word2vec))  #hstack 两个矩阵横向合并
y = dataset[:, 4]
print("Data End")

print("training Start")

#保存Model(注:save文件夹要预先建立，否则会报错)
svc_model = createModels.get_svc_model(X,y)
keras_mode = createModels.get_keras_cnn_model(X,y)

# 读取测试集
dataframe = dataprocess.importcsvdata(config.test_csv)
# 特征和标签划分
dataset = dataframe.values
word2vec = dataprocess.get_word2vec(dataframe)
# 只选择类别和编辑距离乘积两个特征
X = np.hstack((dataset[:, 1:2], dataset[:, 3:4], word2vec))  #hstack 两个矩阵横向合并
y = svc_model.predict(X)
y = keras_mode.predict(X)
print("training End")
