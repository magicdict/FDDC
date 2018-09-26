import numpy as np
import keras
import pickle  #pickle模块
from keras.models import Sequential
from keras.layers import Dense, Dropout
from sklearn.svm import SVC  #SVM分类器
from keras.callbacks import Callback
from sklearn.metrics import confusion_matrix, f1_score, precision_score, recall_score
import config


class Metrics(Callback):
    '''自定义每个周期结束后的动作'''

    def on_train_begin(self, logs={}):
        self.val_f1s = []
        self.val_recalls = []
        self.val_precisions = []
        self.validation_data = []

    def on_epoch_end(self, epoch, logs={}):
        val_predict = (np.asarray(self.model.predict(
            self.validation_data[0]))).round()
        val_targ = self.validation_data[1]
        _val_f1 = f1_score(val_targ, val_predict)
        _val_recall = recall_score(val_targ, val_predict)
        _val_precision = precision_score(val_targ, val_predict)
        self.val_f1s.append(_val_f1)
        self.val_recalls.append(_val_recall)
        self.val_precisions.append(_val_precision)
        print("— val_f1: " + str(_val_f1) + " — val_precision: " +
              str(_val_precision) + " — val_recall: " + str(_val_recall))
        return


def load_keras_cnn_model():
    model = keras.models.load_model(config.keras_model_file)
    return model


def get_keras_cnn_model(x_train, y_train):
    #设计模型，通过add的方式叠起来
    #注意输入时，初始网络一定要给定输入的特征维度input_dim或者input_shape数据类型
    #activition激活函数既可以在Dense网路设置里，又可以单独添加
    model = Sequential()
    dim = x_train.shape[1]
    model.add(Dense(64, input_dim=dim, activation='relu'))
    #Drop防止过拟合的数据处理方式
    #model.add(Dropout(0.1))
    model.add(Dense(64, activation='relu'))
    #model.add(Dropout(0.1))
    model.add(Dense(1, activation='sigmoid'))
    #model.add(Dense(1, activation='softmax'))
    #编译模型，定义损失函数、优化函数、绩效评估函数
    model.compile(
        loss='binary_crossentropy', optimizer='rmsprop', metrics=['accuracy'])
    #导入数据进行训练
    metrics = Metrics()
    model.fit(
        x_train,
        y_train,
        callbacks=[metrics],
        validation_data=(x_train, y_train),
        epochs=20,
        batch_size=128)
    #模型评估
    #score = model.evaluate(x_test, y_test, batch_size=128)
    #print(score)
    model.save(config.keras_model_file)
    #with open(config.keras_model_file, 'wb') as f:
    #    pickle.dump(model, f)
    return model


def load_svc_model():
    with open(config.svc_model_file, 'rb') as f:
        model = pickle.load(f)
    return model


def get_svc_model(x_train, y_train):
    model = SVC(kernel='rbf')
    model.fit(x_train, y_train.astype("int"))
    val_predict = (np.asarray(model.predict(x_train))).round()
    _val_f1 = f1_score(y_train, val_predict)
    print("SVC Score:" + str(_val_f1))
    with open(config.svc_model_file, 'wb') as f:
        pickle.dump(model, f)
    return model