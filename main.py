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
def pdfTotxt(fileDir,txtDir,nerDir):
    files = os.listdir(fileDir + 'pdf/')
    if not os.path.exists(txtDir):
        os.mkdir(txtDir)
    if not os.path.exists(nerDir):
        os.mkdir(nerDir)
    replace = re.compile(r'\.pdf', re.I)

    for file in files:
        filePath = fileDir + 'pdf/' + file
        outPath = txtDir + re.sub(replace, '', file) + '.txt'
        outNerPath = nerDir + re.sub(replace, '', file) + '.xml'
        try:
            if not os.path.exists(outPath):
                print(filePath, outPath)
                os.chdir("/home/118_4/code")
                os.system("python pdf2txt.py " + filePath + " > " + outPath)
                Reformat(outPath)
                os.chdir("/home/118_4/ltp-3.4.0/bin")
                os.system(" ./ltp_test --last-stage ner --input " + outPath +
                          " > " + outNerPath)
        except Exception as e:
            print("Exception:", e)

if not os.path.exists("/home/118_4/temp"):
        os.mkdir("/home/118_4/temp")
if not os.path.exists("/home/118_4/temp/hetong"):
        os.mkdir("/home/118_4/temp/hetong")
if not os.path.exists("/home/118_4/temp/zengjianchi"):
        os.mkdir("/home/118_4/temp/zengjianchi")
pdfTotxt('/home/data/hetong/','/home/118_4/temp/hetong/txt/','/home/118_4/temp/hetong/ner/')
pdfTotxt('/home/data/zengjianchi/','/home/118_4/temp/zengjianchi/txt/','/home/118_4/temp/zengjianchi/ner/')
os.chdir("/home/118_4/code")
os.system("dotnet run")
