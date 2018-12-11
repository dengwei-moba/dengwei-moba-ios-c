using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
//引入库
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ClientGame.Net;
using Google.Protobuf;

namespace ClientGame.Net
{
    public class NetUdpSocket
    {
        //==========Rcp设定数据==================
        public int BASE_DATA_CACHE = 3;           //基础数据冗余包数量
        private int MIN_DATA_CACHE = 2;           //最小缓存冗余包数量(服务器为1,因为服务器一直在持续发,但客户端没有)
        public int MAX_DATA_CACHE = 9;            //最大缓存冗余包数量(建议小于等于BEST_MTU除以平均每帧数据长度)
        public int UP_CACHE_FS = 1;               //增加冗余包数量的帧阈值(每出现N个包重收/重发才达到,则增加冗余包数量)
        public int DOWN_CACHE_FS = 30;            //减少冗余包数量的帧阈值(连续N个包无重收达到,降低冗余包数量)
        private int BEST_MTU = 548;               //UDP最理想MTU值
        public int CHECK_TIME_OUT = 300;          //超时重查时间(毫秒)
        public int SYN_ACK_TIME = 200;            //超时同步ACK状态时间(毫秒)
        public int MUST_SYN_ACK_TIME = 33*3/2;    //必定同步ACK状态时间(毫秒)(因为服务器一直在持续发,但客户端没有,所以客户端不发任何东西时候,还是要单独向服务器同步ACK信息)
        public int CONNECTED_FAIL_TIME = 1500;    //链接失败时间(毫秒)
        public int RE_RECV_NOT_ACK_TIME = 3;      //重复N次后,可以再次提升CACHE值
        public int RE_SEND_NOT_ACK_TIME = 3;      //重复N次后,可以再次提升CACHE值
        public int RE_RECV_ACK_TIME = 1;          //重复N次后,可以再次请求
        public int RE_SEND_ACK_TIME = 3;          //重复N次后,可以再次回复
        public int MAX_DIS_CONNECTED_FS_NUM =30*6;//对方最新的发送序号超出正在接收处理的包序号N个后,判定数据无效断线(必须大于MAX_DATA_CACHE)
        public bool OBTAIN_PING = true;           //true:测试获取Ping值
        public int PING_CHECK_TIME = 1000;        //Ping间隔时间(毫秒)
        public int MAX_CONNECT_TIME = 10;         //尝试链接最大次数
        //==========Rcp相关数据==================
        private KcpNetPack curPack = null;
        private KcpNetPack curPack2 = null;
        private Dictionary<int, KcpNetPack> _recvPacks = new Dictionary<int, KcpNetPack>();   //所有的接收包缓存
        private int _recvIdx = 0;               //正在接收处理的包序号
        private int _lastReRecvIdx = -1;        //请求重发防重复
        private int _reRecvTime = 0;            //重复RE_RECV_ACK_TIME次后,可以再次请求
        private int _recvAckIdx = -1;           //我方已确认接收成功的Ack序号
        private Dictionary<int, byte[]> _sendPacks = new Dictionary<int, byte[]>();     //所有的发送包缓存
        private int _sendIdx = -1;              //正在发送处理的包序号
        private int _lastReSendIdx = -1;        //回复重发防重复
        private int _reSendTime = 0;            //重复RE_SEND_ACK_TIME次后,可以再次回复
        private int _sendAckIdx = -1;           //对方已确认接收成功的Ack序号
        private int _sendNewIdx = -1;           //对方最新的发送序号
        private int _lastSendAckIdx = 0;        //上次清理的发送包起始序号
        private int _lastRecvNotAckIdx = -1;    //提升CACHE值防重复
        private int _lastRecvNotAckTime = 0;    //重复RE_RECV_NOT_ACK_TIME次后,可以再次提升CACHE值
        private int _lastSendNotAckIdx = -1;    //提升CACHE值防重复
        private int _lastSendNotAckTime = 0;    //重复RE_SEND_NOT_ACK_TIME次后,可以再次提升CACHE值
        private int _upCacheFSTime = 0;         //已经正确增加冗余包数量的帧次数
        private int _downCacheFSTime = 0;       //已经正确减少冗余包数量的帧次数
        private int _range = 3;                 //发送的一帧中冗余包数量
        private int _lastCheckTime = 0;         //超时重查时间
        private int _lastCheckFailTime = 0;     //链接失败时间
        private int _lastSendTime = 0;          //上次发送数据时间(服务器一直在持续发,但客户端没有)
        private bool _isSynAck = false;         //false:本次超时还未同步ACK状态
        private int _tryConnectTime = 0;        //尝试链接次数

        private int _lastPingTime = 0;          //Ping发出时间
        private bool _canNextPing = true;       //防重复
        private int _pingIdx = 0;               //Ping测试序号
        public int Ping = 0;                    //Ping值
        private int _quickConnect = 0;          //
        //=======================================================================
        Socket socket; //目标socket
        EndPoint serverEnd; //服务端
        IPEndPoint ipEnd; //服务端端口
        private byte[] recvData = new byte[1024];
        private byte[] sendData = new byte[1024];
        private int recvLen;
        Thread connectThread; //连接线程

        public interface IListener
        {
            void OnEvent(NetworkEvent nEvent);
        }
        private IListener listener;
        private Queue<NetworkEvent> netEventList = new Queue<NetworkEvent>();
        public bool isConnectting = false;
        public bool isConnected = false;

        public NetUdpSocket(IListener lsn)
        {
            listener = lsn;
        }

        private HUDFPS mHUDFPS;
        public void ConnectKCP(string host, int iport)
        {
            mHUDFPS = Facade.Instance.GetManager<HUDFPS>(FacadeConfig.ChildSystem_HUDFPS);
            _range = BASE_DATA_CACHE;
            ipEnd = new IPEndPoint(IPAddress.Parse(host), iport);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //socket.Connect(ipEnd);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            serverEnd = (EndPoint)sender;
            Kcp_Connect msg = new Kcp_Connect();
            msg.Quickconnect = _quickConnect;
            KcpNetPack pack = KcpNetPack.SerializeToPack(msg, (ushort)Kcp_MsgID.Connected);
            SetHadCheck("Connect");
            var data = pack.Assembly();
            socket.SendTo(data.Bytes, data.Length, SocketFlags.None, ipEnd);
            isConnectting = true;
            isConnected = false;
            _lastCheckFailTime = System.Environment.TickCount;
            ConnectReceiveThread_Start();
        }

        public void SocketSend(string sendStr)
        {
            if (!isConnected || isConnectting) return;
            sendData = new byte[1024];
            sendData = Encoding.ASCII.GetBytes(sendStr);
            socket.SendTo(sendData, sendData.Length, SocketFlags.None, ipEnd);
            _lastSendTime = System.Environment.TickCount;
        }
        public void SocketSend(byte[] sendData, int iLength)
        {
            if (!isConnected || isConnectting) return;
            socket.SendTo(sendData, iLength, SocketFlags.None, ipEnd);
            _lastSendTime = System.Environment.TickCount;
        }
        public void SocketSend(KcpNetPack pack)
        {
            if (!isConnected || isConnectting) return;
            var data = pack.Assembly();
            socket.SendTo(data.Bytes, data.Length, SocketFlags.None, ipEnd);
            _lastSendTime = System.Environment.TickCount;
        }

        void SocketReceive()
        {
            //接收数据方案1:ReceiveFrom
            while (true)
            {
                recvData = new byte[1024];
                //获取客户端，获取服务端端数据，用引用给服务端赋值，实际上服务端已经定义好并不需要赋值
                recvLen = socket.ReceiveFrom(recvData, ref serverEnd);
                Input(recvData, recvLen);
                //string recvStr=Encoding.ASCII.GetString(recvData,0,recvLen);
                //Debug.Log("message1 from: " + serverEnd.ToString() + "===>" + recvLen + "===>" + recvStr); //打印服务端信息
            }
            //接收数据方案2:BeginReceiveFrom
            //recvData = new byte[1024];
            //socket.BeginReceiveFrom(recvData, 0, recvData.Length, SocketFlags.None, ref serverEnd,new AsyncCallback(ReciveCallback), null);
        }

        public void ReciveCallback(IAsyncResult asyncResult)
        {
            //信息接收完成
            if (asyncResult.IsCompleted)
            {
                recvLen = socket.EndReceiveFrom(asyncResult, ref serverEnd);
                Input(recvData, recvLen);
                //string recvStr=Encoding.ASCII.GetString(recvData,0,recvLen);
                //Debug.Log("message2 from: " + serverEnd.ToString() + "===>" + recvLen + "===>" + recvStr); //打印服务端信息
                SocketReceive();
            }
        }

        public void Disconnect(string sReason)
        {
            Debug.LogFormat("NetUdpSocket Disconnect:{0}", sReason);
            if (sReason != "Server")
            {
                Kcp_DisConnect msg = new Kcp_DisConnect();
                msg.Type = (int)Kcp_Net_Type.DscNormal;
                KcpNetPack pack = KcpNetPack.SerializeToPack(msg, (ushort)Kcp_MsgID.DisConnected);
                SocketSend(pack);
            }
            isConnected = false;
            isConnectting = false;
            try
            {
                //关闭线程
                ConnectReceiveThread_Stop();
                //最后关闭socket
                if (socket != null)
                    socket.Close();
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Disconnect:{0}", e);
            }
            finally
            {
                lock (netEventList)
                {
                    netEventList.Enqueue(NetworkEvent.disconnected);
                }
            }
            //Debug.Log("继续运行==============================>");
        }

        private void ConnectReceiveThread_Stop()
        {
            try
            {
                //关闭线程
                if (connectThread != null)
                {
                    connectThread.Interrupt();
                    if (connectThread.IsAlive)
                    {
                        connectThread.Abort();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("ConnectReceiveThread_Stop:{0}", e);
            }
        }

        private void ConnectReceiveThread_Start()
        {
            connectThread = new Thread(new ThreadStart(SocketReceive));
            //connectThread.IsBackground = true; 
            connectThread.Start();
        }

        public Queue<KcpNetPack> SwitchPacks()
        {
            int iMaxNum = 10;
            return RecvPacks(iMaxNum);
        }

        public void SendPack(KcpNetPack pack)
        {
            Send(pack);
        }

        public void OnUpdate()
        {
            if (isConnectting)
            {
                if (System.Environment.TickCount - _lastCheckFailTime > (CONNECTED_FAIL_TIME * (_tryConnectTime+1)))
                {
                    _tryConnectTime++;
                    if (_tryConnectTime < MAX_CONNECT_TIME)
                    {
                        ConnectReceiveThread_Stop();//必须先关接收数据线程,再开.不然无法接收数据(待不需要关线程方案)
                        Kcp_Connect msg = new Kcp_Connect();
                        msg.Quickconnect = _quickConnect;
                        KcpNetPack pack = KcpNetPack.SerializeToPack(msg, (ushort)Kcp_MsgID.Connected);
                        SetHadCheck("ReConnect");
                        var data = pack.Assembly();
                        socket.SendTo(data.Bytes, data.Length, SocketFlags.None, ipEnd);
                        _lastCheckFailTime = System.Environment.TickCount;
                        Debug.LogErrorFormat("[链接失败]第{0}次尝试重新连接", _tryConnectTime);
                        ConnectReceiveThread_Start();
                    }
                    else 
                    {
                        lock (netEventList)
                        {
                            netEventList.Enqueue(NetworkEvent.connectFail);
                        }
                        Debug.LogErrorFormat("[失败]第{0}次连接", _tryConnectTime);
                        isConnectting = false;
                        _tryConnectTime = 0;
                    }
                }
            }
            lock (netEventList)
            {
                while (netEventList.Count > 0)
                {
                    var e = netEventList.Dequeue();
                    listener.OnEvent(e);
                }
            }
            Check();
        }

        private void UpdateNetInfo(string netInfo)
        {
            string sNetInfo = "Ping:" + Ping + ",R:" + _range + "," + netInfo;
            mHUDFPS.SetNetInfo(sNetInfo);
        }

        //==========Rcp核心算法======================================================================
        public void Input(byte[] buffer, int bufSize)
        {
            //List<NetPack> packs = DecodeBuffer(recvData, recvLen);
            //for (int i = 0; i < packs.Count; ++i)
            //    Input(packs[i]);
            int offset1 = 0;
            int restCount2 = bufSize;// 剩余缓存大小
            KcpNetPack pack = new KcpNetPack();
            if (!pack.FillPack(buffer, ref offset1, ref restCount2))
            {
                return;
            }

            Kcp_MsgID msgID = (Kcp_MsgID)pack.HeadID;
            if (msgID == Kcp_MsgID.NotAckIndex)
            {
                Kcp_NotAckIndex msg = Kcp_NotAckIndex.Parser.ParseFrom(pack.BodyBuffer.Bytes);
                Debug.Log("回复对方缺失的序号:" + msg.Startindex + "-" + msg.Endindex + ",对方已确认接收成功的Ack序号:" + _sendAckIdx);
                if (_sendAckIdx >= msg.Endindex) return;
                int iStartIndex = Math.Max(msg.Startindex, _sendAckIdx + 1);
                if (iStartIndex > msg.Endindex) return;
                _reSendTime++;
                if (_lastReSendIdx != iStartIndex || _reSendTime > RE_SEND_ACK_TIME)
                {
                    _lastReSendIdx = iStartIndex;
                    _reSendTime = 0;
                    RcpPackSendData(msg.Endindex, msg.Endindex - iStartIndex + 1);
                }
                _lastSendNotAckTime++;
                if (_lastSendNotAckIdx != iStartIndex || _lastSendNotAckTime > RE_SEND_NOT_ACK_TIME)
                {
                    _lastSendNotAckIdx = iStartIndex;
                    _lastSendNotAckTime = 0;
                    UpCacheFSTime();
                }
            }
            else if (msgID == Kcp_MsgID.FsInfoS || msgID == Kcp_MsgID.SynAck)
            {
                Kcp_FsPackDatas msg = Kcp_FsPackDatas.Parser.ParseFrom(pack.BodyBuffer.Bytes);
                if (msg.Ackindex > _sendAckIdx)
                {
                    _sendAckIdx = msg.Ackindex;
                }
                if (msg.Sendindex > _sendNewIdx)
                {
                    _sendNewIdx = msg.Sendindex;
                }
                //if (msgID == Kcp_MsgID.FS_INFO_S)
                //    Debug.Log("最新的已ACK序号确认==>" + _lastSendAckIdx + ",对方已确认接收成功的Ack序号:" + _sendAckIdx + "," + msg.ackindex + ",对方最新的发送序号:" + _sendNewIdx + "," + msg.sendindex);
                //else
                //    Debug.Log("同步ACK状态==========>" + _lastSendAckIdx + ",对方已确认接收成功的Ack序号:" + _sendAckIdx + "," + msg.ackindex + ",对方最新的发送序号:" + _sendNewIdx + "," + msg.sendindex);
                for (int idx = _lastSendAckIdx; idx <= _sendAckIdx; idx++)
                {
                    _sendPacks.Remove(idx);//清理内存
                }
                _lastSendAckIdx = _sendAckIdx;

                int iMinIndex = -1;
                for (int i = 0; i < msg.Fspackdata.Count; i++)
                {
                    Kcp_OneFsPackData fsPackData = msg.Fspackdata[i];
                    //Debug.Log("*************=1==>" + fsPackData.index);
                    if (_recvPacks.ContainsKey(fsPackData.Index)) continue;
                    if (_recvAckIdx >= fsPackData.Index) continue;
                    //Debug.Log("*************=2==>" + fsPackData.index);
                    int offset = 0;
                    int restCount = 1024;// 剩余缓存大小
                    if (curPack2 == null)
                    {
                        curPack2 = new KcpNetPack();
                    }
                    if (curPack2.FillPack(fsPackData.Fsdata.ToByteArray(), ref offset, ref restCount))
                    {
                        if (fsPackData.Index < iMinIndex || iMinIndex == -1)
                            iMinIndex = fsPackData.Index;
                        _recvPacks[fsPackData.Index] = curPack2;
                        curPack2 = null;
                    }
                    else
                    {
                        RcpPackSendData(fsPackData.Index, 2);
                        Debug.LogErrorFormat("Input err: {0} ", fsPackData.Index);
                    }
                }
                CheckPreorderLose(iMinIndex);
                DownCacheFSTime();
            }
            else if (msgID == Kcp_MsgID.Connected || msgID == Kcp_MsgID.ReConnected)  //暂时只有一次握手
            {
                Kcp_Connect msg = Kcp_Connect.Parser.ParseFrom(pack.BodyBuffer.Bytes);
                if (_quickConnect != msg.Quickconnect)  //防重复
                {
                    lock (netEventList)
                    {
                        netEventList.Enqueue(NetworkEvent.connected);
                    }
                    _quickConnect = msg.Quickconnect;
                }
                isConnected = true;
                isConnectting = false;
                _tryConnectTime = 0;
                Debug.Log("链接成功:" + _quickConnect);
            }
            else if (msgID == Kcp_MsgID.ConnectedFail)  //暂时只有一次握手
            {
                lock (netEventList)
                {
                    netEventList.Enqueue(NetworkEvent.connectFail);
                }
                isConnected = false;
                isConnectting = false;
                Debug.Log("链接失败:CONNECTED_FAIL:");
            }
            else if (msgID == Kcp_MsgID.DisConnected)  //暂时只有一次挥手
            {
                Kcp_DisConnect msg = Kcp_DisConnect.Parser.ParseFrom(pack.BodyBuffer.Bytes);
                if (msg.Type == (int)Kcp_Net_Type.DscErrAddr)
                {
                    Debug.Log("链接断开:DSC_ERR_ADDR:" + _quickConnect);
                    isConnectting = true;       //快速重连/身份重新认证/更新IP地址
                }
                else 
                {
                    Disconnect("Server");
                }
                //Debug.Log("继续运行2==============================>");
            }
            else if (msgID == Kcp_MsgID.PingIndex)
            {
                Kcp_Ping msg = Kcp_Ping.Parser.ParseFrom(pack.BodyBuffer.Bytes);
                if (msg.Index == _pingIdx)
                    Ping = System.Environment.TickCount - _lastPingTime;
                UpdateNetInfo("");
                //Debug.Log("Ping==============================>" + Ping);
                _canNextPing = true;
            }
            else
            {
                Debug.LogErrorFormat("Input err2: {0} ", msgID);
                return;
            }
            SetHadCheck("Input");
        }

        private void DownCacheFSTime()
        {
            _downCacheFSTime++;
            if (_downCacheFSTime >= DOWN_CACHE_FS)
            {
                _downCacheFSTime = 0;
                _range--;
                if (_range < MIN_DATA_CACHE) _range = MIN_DATA_CACHE;
                // Debug.Log("减少冗余包数量的帧阈值============================>" + _range);
            }
        }
        private void UpCacheFSTime()
        {
            _upCacheFSTime++;
            _downCacheFSTime = 0;
            if (_upCacheFSTime >= UP_CACHE_FS)
            {
                _upCacheFSTime = 0;
                _range++;
                if (_range > MAX_DATA_CACHE) _range = MAX_DATA_CACHE;
                // Debug.Log("增加冗余包数量的帧阈值============================>" + _range);
            }
        }

        public void Send(KcpNetPack pack)
        {
            _sendIdx++;
            var fsdata = pack.Assembly();
            _sendPacks.Add(_sendIdx, fsdata.Bytes);
            RcpPackSendData(_sendIdx, _range);
            SetHadCheck("Send");
        }

        private void RcpPackSendData(int iSendIdx, int iRange)//包含了iSendIdx的iRange个包
        {
            if (!_sendPacks.ContainsKey(iSendIdx)) return;
            iRange = Math.Min(iRange, iSendIdx - _sendAckIdx);
            if (iRange <= 0) return;
            // Debug.Log("RcpPackSendData=========>" + iSendIdx + ",iRange:" + iRange);
            int iTime = iRange/MAX_DATA_CACHE+1;
            int iStartIdx = iSendIdx - iRange;
            for (int i = 1; i <= iTime; i++)
            {
                int iSendIdx2, iRange2;
                if (iTime == i) 
                {
				    iSendIdx2= iSendIdx;
                    iRange2 = Math.Min(iRange - (i - 1) * MAX_DATA_CACHE, MAX_DATA_CACHE);
				    if (iRange2<=0) return;
			    }else{
				    iSendIdx2= iStartIdx+MAX_DATA_CACHE*i;
				    iRange2 = MAX_DATA_CACHE;
                }
                Kcp_FsPackDatas msg = new Kcp_FsPackDatas();
                msg.Ackindex = _recvAckIdx;
                msg.Sendindex = _sendIdx;
                for (int idx = (iSendIdx2 - iRange2 + 1); idx <= iSendIdx2; idx++)
                {
                    if (idx < 0) continue;
                    if (!_sendPacks.ContainsKey(idx)) continue;
                    // Debug.Log("RcpPackSendData===>" + idx);
                    Kcp_OneFsPackData fsPackData = new Kcp_OneFsPackData();
                    fsPackData.Index = idx;
                    fsPackData.Fsdata = ByteString.CopyFrom(_sendPacks[idx]);
                    msg.Fspackdata.Add(fsPackData);
                }
                //Debug.Log("RcpPackSendData=========>总序号:" + iSendIdx + ",总范围:" + iRange + ",序号" + iSendIdx2 + ",范围:" + iRange2 );
                KcpNetPack pack = KcpNetPack.SerializeToPack(msg, (ushort)Kcp_MsgID.FsInfoS);
                SocketSend(pack);
            }
        }

        private void ReRecvAckIdx(int notAckIdx)
        {
            int iEndNotAckIdx = notAckIdx;
            for (int i = 1; i < MAX_DIS_CONNECTED_FS_NUM; i++)
            {
                if ((notAckIdx + i) > _sendNewIdx) break;           //大于对方最新的发送序号
                if (_recvPacks.ContainsKey(notAckIdx + i)) break;   //已经有数据了
                iEndNotAckIdx++;
            }
            Debug.Log("请求对方重发非ACK序号:" + notAckIdx + "-" + iEndNotAckIdx);
            Kcp_NotAckIndex msg = new Kcp_NotAckIndex();
            msg.Startindex = notAckIdx;
            msg.Endindex = iEndNotAckIdx;

            KcpNetPack pack = KcpNetPack.SerializeToPack(msg, (ushort)Kcp_MsgID.NotAckIndex);
            SocketSend(pack);
            _lastRecvNotAckTime++;
            if (_lastRecvNotAckIdx != notAckIdx || _lastRecvNotAckTime>RE_RECV_NOT_ACK_TIME)
            {
                _lastRecvNotAckIdx = notAckIdx;
                _lastRecvNotAckTime = 0;
                UpCacheFSTime();
            }
        }

        public KcpNetPack Recv()
        {
            if (_recvPacks.ContainsKey(_recvIdx))
            {
                _recvAckIdx = _recvIdx;
                KcpNetPack pack = _recvPacks[_recvIdx];
                _recvPacks.Remove(_recvIdx);//清理内存
                _recvIdx++;
                //SetHadCheck("Recv");
                return pack;
            }
            if (_sendNewIdx - _recvIdx > _range)
            {
                Debug.Log("Recv==>对方最新的发送序号:" + _sendNewIdx + ",正在接收处理的包序号:" + _recvIdx + ",R:" + _range);
                _reRecvTime++;
                if (_lastReRecvIdx != _recvIdx || _reRecvTime > RE_RECV_ACK_TIME)
                {
                    _lastReRecvIdx = _recvIdx;
                    _reRecvTime = 0;
                    ReRecvAckIdx(_recvIdx);
                }
                if (_sendNewIdx - _recvIdx > MAX_DIS_CONNECTED_FS_NUM)
                {
                    Disconnect("Recv");
                }
            }
            return null;
        }

        public Queue<KcpNetPack> RecvPacks(int iMaxNum)
        {
            Queue<KcpNetPack> recvPacks = new Queue<KcpNetPack>();
            int iTime = iMaxNum;
            while (iTime > 0)
            {
                iTime--;
                KcpNetPack pack = Recv();
                if (pack == null) break;
                recvPacks.Enqueue(pack);
            }
            return recvPacks;
        }

        /*public void Flush()
        {
            if (_sendAckIdx < _sendIdx && _sendAckIdx > -1) 
            {
                RcpPackSendData(_sendIdx, _range);
            }
            SetHadCheck();
        }*/

        private void SynAckStatus()
        {
            if ((System.Environment.TickCount - _lastCheckTime > SYN_ACK_TIME && !_isSynAck) || System.Environment.TickCount - _lastSendTime > MUST_SYN_ACK_TIME)
            {
                //Debug.Log("同步Ack");
                _isSynAck = true;
                Kcp_FsPackDatas msg = new Kcp_FsPackDatas();
                msg.Ackindex = _recvAckIdx;
                msg.Sendindex = _sendIdx;
                KcpNetPack pack = KcpNetPack.SerializeToPack(msg, (ushort)Kcp_MsgID.SynAck);
                SocketSend(pack);
            }
        }

        private void TestPing()
        {
            if (OBTAIN_PING)
            {
                if (_canNextPing && System.Environment.TickCount - _lastPingTime > PING_CHECK_TIME)
                {
                    _pingIdx++;
                    Kcp_Ping msg = new Kcp_Ping();
                    msg.Index = _pingIdx;
                    KcpNetPack pack = KcpNetPack.SerializeToPack(msg, (ushort)Kcp_MsgID.PingIndex);
                    SocketSend(pack);
                    _lastPingTime = System.Environment.TickCount;
                    _canNextPing = false;
                }
                else if (!_canNextPing && System.Environment.TickCount - _lastPingTime > PING_CHECK_TIME*3)
                {
                    _canNextPing = true;    //恰好是测试ping丢包时候,这里做个超时重测功能
                }
            }
        }

        public void CheckPreorderLose(int iMinIndex)//客户端/服务端接收到任意包时候,驱动自我检查前序包是否完整
        {
            if (_recvIdx >= iMinIndex) return;
            for (int startindex = _recvIdx; startindex <= iMinIndex; startindex++)
            {
                if (_recvPacks.ContainsKey(startindex)) continue;
                ReRecvAckIdx(startindex);
                Debug.Log("请求对方,前序包不完整==>起始序号:" + startindex + ",(iMinIndex=" + iMinIndex + ")");
                return;//只需要一个起始位置就行
            }

        }

        public void Check()
        {
            if (!isConnected || isConnectting) return;
            TestPing();
            SynAckStatus();
            if (System.Environment.TickCount - _lastCheckTime < CHECK_TIME_OUT) return;
            //检查发送状态
            if (_sendAckIdx < _sendIdx && _sendAckIdx > -1) // 我方有数据,对方尚未完全确认接收
            {
                RcpPackSendData(_sendIdx, _sendIdx - _sendAckIdx);
                Debug.Log("Check我方有数据,对方尚未完全确认接收==>对方已确认接收成功的Ack序号:" + _sendAckIdx + ",正在发送处理的包序号" + _sendIdx);
            }

            //检查接收状态
            if (_sendNewIdx > _recvIdx)    // 对方有数据,我方尚未完全接收
            {
                Debug.Log("Check对方有数据,我方尚未完全接收======>对方最新的发送序号:" + _sendNewIdx + ",正在接收处理的包序号:" + _recvIdx + ",我方已确认接收成功的Ack序号:" + _recvAckIdx );
                ReRecvAckIdx(_recvIdx);
            }

            _isSynAck = false;
            SetHadCheck("Check");
        }

        private void SetHadCheck(string sDebug)
        {
            // Debug.Log(sDebug+"===SetHadCheck============================>" + (System.Environment.TickCount - _lastCheckTime));
            _lastCheckTime = System.Environment.TickCount;

        }
    }
}