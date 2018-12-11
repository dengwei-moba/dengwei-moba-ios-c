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

public class NetTcpSocket
{
    public interface IListener
    {
        void OnEvent(NetworkEvent nEvent);
    }

    //enum EncryptType
    //{
    //    NOT_ENCRYPT = 0,
    //    WHOLE_ENCRYPT,
    //    HEAD_ENCRYPT,
    //    TAIL_ENCRYPT
    //};

    public Socket m_Socket = null;
    public string m_Host = null;
    public int m_Port;
    public int m_TryTimes;

    private const int m_nBufferSize = 4096;
    private byte[] m_buffer = new byte[m_nBufferSize];

    private NetPack curPack = null;
    protected Queue<NetPack> _recvPacks = new Queue<NetPack>();

    private IListener listener;
    private Queue<NetworkEvent> netEventList = new Queue<NetworkEvent>();

    public string name { get; private set; }

    public bool isConnected
    {
        get
        {
            if (m_Socket == null)
                return false;
            return m_Socket.Connected;
        }
    }

    public bool isConnectting
    {
        get;
        private set;
    }

    public NetTcpSocket(IListener lsn)
    {
        listener = lsn;
    }

    public void ConnectTCP(string sHost, int iPort)
    {
        m_Host = sHost;
        m_Port = iPort;
        m_TryTimes = 0;

        if (m_Socket == null)
        {
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_Socket.NoDelay = true;

            StartConnect();
        }
        else
        {
            Debug.LogErrorFormat("socket != null ! ");
        }
    }

    public void Disconnect()
    {
        Debug.LogErrorFormat("Disconnect");
        if (m_Socket != null)
        {
            try
            {
                if (m_Socket.Connected)
                    m_Socket.Disconnect(true);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("{0}", e);
            }
            finally
            {
                m_Socket.Close();
                m_Socket = null;
                //listener.OnEvent(NetworkEvent.connectFail, this);
                lock (netEventList)
                {
                    netEventList.Enqueue(NetworkEvent.disconnected);
                }
            }
        }
    }

    public void SendPack(NetPack pack)
    {
        var data = pack.Assembly();
        SendPack(data.Bytes, data.BufSize);
    }

    public void SendPack(byte[] bytes, int size = 0)
    {
        if (m_Socket == null)
        {
            Debug.LogErrorFormat("m_Socket == null");
            return;
        }

        if (!m_Socket.Connected)
        {
            Debug.LogErrorFormat("m_Socket.Connected == false");
            return;
        }

        if (size == 0)
            size = bytes.Length;

        try
        {
            int totalBytesSend = m_Socket.Send(bytes, size, SocketFlags.None);
            if (totalBytesSend < 1)
            {
                Debug.LogErrorFormat("send pack faild");
                throw new Exception();
            }
            else
            {

            }
        }
        catch (System.Exception ex)
        {
            Debug.LogErrorFormat("{0}", ex);
            return;
        }
    }

    public Queue<NetPack> SwitchPacks()
    {
        Queue<NetPack> target = null;
        lock (_recvPacks)
        {
            if (_recvPacks.Count <= 0)
            {
                target = null;
            }
            else
            {
                target = new Queue<NetPack>();
                while (_recvPacks.Count > 0)
                {
                    target.Enqueue(_recvPacks.Dequeue());
                }
            }
        }

        return target;
    }

    /// <summary>
    /// 更新Socket内部逻辑
    /// Socket是异步的，会涉及到多线程访问
    /// 在多线程中触发了事件后，存入队列中在Update中弹出解决多线程问题
    /// </summary>
    public void OnUpdate()
    {
        lock (netEventList)
        {
            while (netEventList.Count > 0)
            {
                var e = netEventList.Dequeue();
                listener.OnEvent(e);
            }
        }
    }

    void ConnectCallback(IAsyncResult result)
    {
        bool bConnectError = false;
        try
        {
            isConnectting = false;
            m_Socket.EndConnect(result);
        }
        catch (SocketException e)
        {
            Debug.LogErrorFormat("{0}", e);
            bConnectError = true;
        }

        //连接失败，尝试多次连接
        if (bConnectError || !m_Socket.Connected)
        {
            if (m_TryTimes < 10)
            {
                StartConnect();
            }
            else
            {
                Debug.LogErrorFormat("[失败]第{0}次连接{1}:{2} ", m_TryTimes, m_Host, m_Port);
                //listener.OnEvent(NetworkEvent.connectFail, this);
                lock (netEventList)
                {
                    netEventList.Enqueue(NetworkEvent.connectFail);
                }
            }
        }
        else
        {
            Debug.LogFormat("[成功]第{0}次连接{1}:{2} ", m_TryTimes, m_Host, m_Port);
            //listener.OnEvent(NetworkEvent.connected, this);
            lock (netEventList)
            {
                netEventList.Enqueue(NetworkEvent.connected);
            }
            AsyncRead();
        }
    }

    private void StartConnect()
    {
        m_TryTimes += 1;
        Debug.LogFormat("第{0}次连接{1}:{2} ", m_TryTimes, m_Host, m_Port);
        isConnectting = true;
        m_Socket.BeginConnect(m_Host, m_Port, new AsyncCallback(ConnectCallback), null);
    }

    private void AsyncRead()
    {
        try
        {
            m_Socket.BeginReceive(m_buffer, 0, m_nBufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
        }
        catch (SocketException e)
        {
            Debug.LogErrorFormat("{0}", e);
            Disconnect();
        }
    }

    private List<NetPack> DecodeBuffer(byte[] buffer, int bufSize)
    {
        List<NetPack> packs = new List<NetPack>();
        try
        {
            int offset = 0;
            int restCount = bufSize;// 剩余缓存大小

            while (restCount > 0)
            {
                if (curPack == null)
                {
                    curPack = new NetPack();
                }

                if (curPack.FillPack(buffer, ref offset, ref restCount))
                {
                    CBuffer cBuffer = curPack.Assembly();
                    packs.Add(curPack);
                    curPack = null;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogErrorFormat("Decode pack error. {1}", ex.ToString());
        }
        return packs;
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        int m_totalBytesRead = 0;
        try
        {
            m_totalBytesRead = m_Socket.EndReceive(result);
            if (m_totalBytesRead <= 0)
            {
                Disconnect();
                return;
            }
        }
        catch (SocketException e)
        {
            Debug.LogErrorFormat("{0}", e);
            //这里输出错误日志，方便客户端记录日志
            Debug.LogException(e);
            Disconnect();
            return;
        }

        List<NetPack> packs = DecodeBuffer(m_buffer, m_totalBytesRead);
        lock (_recvPacks)
        {
            for (int i = 0; i < packs.Count; ++i)
                _recvPacks.Enqueue(packs[i]);
        }
        Array.Clear(m_buffer, 0, m_nBufferSize);
        AsyncRead();
    }
}
