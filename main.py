#!/usr/bin/python
#-*- coding: utf-8 -*-

import os
import re
from pdfminer.pdfinterp import PDFResourceManager, PDFPageInterpreter
from pdfminer.pdfpage import PDFPage
from pdfminer.converter import TextConverter
from pdfminer.layout import LAParams
from time import sleep


def Reformat(filepath):
    lines = list()

    # input path
    with open(filepath, 'r', encoding='utf-8') as f:
        for line in f:
            lines.append(line.strip('\n'))
    f.close()

    # save path
    f = open(filepath, 'w', encoding='utf-8')
    tmp = str()
    for item in lines:
        if item == '':
            continue
        if item[-1] == ' ':
            tmp += item
            f.write(tmp + '\n')
            tmp = str()
        else:
            tmp += item
    f.close()


#一个文件夹下的所有pdf文档转换成txt
def pdfTotxt(fileDir):
    files = os.listdir(fileDir + 'pdf/')
    tarDir = fileDir + 'txt/'
    if not os.path.exists(tarDir):
        os.mkdir(tarDir)
    nerDir = fileDir + 'ner/'
    if not os.path.exists(nerDir):
        os.mkdir(nerDir)
    replace = re.compile(r'\.pdf', re.I)

    for file in files:
        filePath = fileDir + 'pdf/' + file
        outPath = tarDir + re.sub(replace, '', file) + '.txt'
        outNerPath = nerDir + re.sub(replace, '', file) + '.xml'
        try:
            if not os.path.exists(outPath):
                print(filePath, outPath)
                os.chdir("/home/118_4/")
                os.system("python pdf2txt.py " + filePath + " > " + outPath)
                Reformat(outPath)
                os.chdir("/home/118_4/ltp-3.4.0/bin")
                os.system(" ./ltp_test --last-stage ner --input " + outPath +
                          " > " + outNerPath)
        except Exception as e:
            print("Exception:", e)

pdfTotxt(u'/home/118_4/FDDC_announcements_round1_test_b_20180708/重大合同/')
pdfTotxt(u'/home/118_4/FDDC_announcements_round1_test_b_20180708/增减持/')
pdfTotxt(u'/home/118_4/FDDC_announcements_round1_train_20180518/增减持/')
os.chdir("/home/118_4/FDDC_SRC")
os.system("dotnet run")
