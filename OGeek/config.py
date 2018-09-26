#!/user/bin/env python
# -*- coding:utf-8 -*-

import os
training_csv = os.path.abspath('.') + "/training.csv"
validation_csv = os.path.abspath('.') + "/training.csv"
test_csv = os.path.abspath('.') + "/training.csv"
result_csv = os.path.abspath('.') + "/result.csv"
svc_model_file = os.path.abspath('.') + "/svc_model.pkl"
keras_model_file = os.path.abspath('.') + "/keras_model.pkl"
# svc keras
model_type = "svc"
#model_type = "keras"
# training / predict
run_mode = "training"
#run_mode = "predict"
