# -*- coding: utf-8 -*-
syntax = "proto3"
import os
# os.system('protoc -I=./pbfile --python_out=./pyproto pbfile/p.proto ')
# print "pro to py finish"
# os.system('protoc -I=./pbfile --csharp_out=./csproto pbfile/*.proto')
# print "pro to c# finish"

def file_name2(file_dir):
    L=[]
    for root, dirs, files in os.walk(file_dir):
        for file in files:
            if os.path.splitext(file)[1] == '.proto':
                L.append(file)
    return L 
if __name__=="__main__":
    print "生成服务端proto开始"
    filelist =  file_name2("./pbfile/")
    for file in filelist:
        print "file==>",file
        os.system('protoc.exe -I=./pbfile --python_out=./pyproto pbfile/%s'%file)
        os.system('protoc.exe -I=./pbfile --csharp_out=./csproto pbfile/%s'%file)
    print "生成服务端proto结束"