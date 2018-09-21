# -*- coding: utf-8 -*-
"""
Created on Mon Jan  8 23:52:46 2018
序贯模型实例
@author: BruceWong
"""
#MLP的二分类：
import numpy as np
from keras.models import Sequential
from keras.layers import Dense, Dropout

# Generate dummy data 生成数据，训练数据和测试数据
#x_train\x_test生成随机的浮点数，x_train为1000行20列；x_test为100行20列
#列数一定要意义对应，相当于特征个数要对应；
#此处的二元分类，可以不需要one_hot编译，np.random.randint可以直接生成0、1编码
x_train = np.random.random((1000, 20))
y_train = np.random.randint(2, size=(1000, 1))
x_test = np.random.random((100, 20))
y_test = np.random.randint(2, size=(100, 1))
#设计模型，通过add的方式叠起来
#注意输入时，初始网络一定要给定输入的特征维度input_dim或者input_shape数据类型
#activition激活函数既可以在Dense网路设置里，又可以单独添加
model = Sequential()
model.add(Dense(64, input_dim=20, activation='relu'))
#Drop防止过拟合的数据处理方式
model.add(Dropout(0.5))
model.add(Dense(64, activation='relu'))
model.add(Dropout(0.5))
model.add(Dense(1, activation='sigmoid'))
#编译模型，定义损失函数、优化函数、绩效评估函数
model.compile(loss='binary_crossentropy',
              optimizer='rmsprop',
              metrics=['accuracy'])
#导入数据进行训练
model.fit(x_train, y_train,
          epochs=20,
          batch_size=128)
#模型评估
score = model.evaluate(x_test, y_test, batch_size=128)
print(score)