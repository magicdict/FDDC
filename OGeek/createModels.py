import numpy as np
from keras.models import Sequential
from keras.layers import Dense, Dropout
import pickle  #pickle模块
from sklearn.svm import SVC  #SVM分类器
import config

def get_keras_cnn_model(x_train,y_train):
    #设计模型，通过add的方式叠起来
    #注意输入时，初始网络一定要给定输入的特征维度input_dim或者input_shape数据类型
    #activition激活函数既可以在Dense网路设置里，又可以单独添加
    model = Sequential()
    dim = x_train.shape[1]
    model.add(Dense(64, input_dim=dim, activation='relu'))
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
    #score = model.evaluate(x_test, y_test, batch_size=128)
    #print(score) 
    with open(config.keras_model_file, 'wb') as f:
        pickle.dump(model, f)
    return model

def get_svc_model(x_train,y_train):
    model = SVC(kernel='rbf')
    model.fit(x_train, y_train.astype("int"))  
    with open(config.svc_model_file, 'wb') as f:
        pickle.dump(model, f)
    return model     
