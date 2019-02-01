#大卫的moba-客户端(打包iOS端的版本)

unity版本:Unity 2018.2.17(Mac版本)

使用参考了:
1.Photon TrueSync
2.悠悠仔的基础同步demo(https://gitee.com/youyouzai/moba)

其他插件:
1.特能特效:Magical v1.0b.unitypackage
2.技能指示器:Status Indicators 1.0.unitypackage
3.AI:腾讯behaviac插件

对应服务器不提供所有源代码,但是提供几个关键算法:分配房间,和客户端对应的可靠udp实现,moba游戏逻辑
还提供了一个已经部署好的服务器,可以直接接入运行

注:最大改变是protobuf使用的一个改造的无反射技术的protobuf版本(v3.0.0,可以兼容使用)(https://github.com/zhangzhibin/protobuf3-for-unity-ex)