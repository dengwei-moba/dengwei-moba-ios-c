using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System;
using System.Text;
using ClientGame.Net;
using Google.Protobuf;

public enum NetworkEvent
{
    none,
    connected,      //连接服务器成功
    connectFail,    //连接服务器失败
    disconnected,   //客户端主动断开
    sendWaitForResponse,
    sendResponsed,
}

public class UnityUdpSocket : ScriptBase, NetUdpSocket.IListener
{
    public delegate void OnServerEvent(NetworkEvent eType);
    private OnServerEvent onServerEvent;
    public delegate void Handle(KcpNetPack pack);
    private Dictionary<MsgID, Handle> msgHandle;
    bool isOpen = false;
    private NetUdpSocket kcpsocket;

    void Awake()
    {
        msgHandle = new Dictionary<MsgID, Handle>();
    }

    public void ConnectToServer(string host, int iport)
    {
        if (kcpsocket == null)
        {
            kcpsocket = new NetUdpSocket(this);
            kcpsocket.ConnectKCP(host, iport);
            Debug.LogErrorFormat("ConnectToServer, host={0},iport={1}", host, iport);
        }
        else if (kcpsocket.isConnectting)
        {
            Debug.LogErrorFormat("kcpsocket is connectting, not start!!! host={0},iport={1}", host, iport);
        }
        else
        {
            Debug.LogErrorFormat("DisConnectToServer, host={0},iport={1},Try Again", host, iport);
            kcpsocket.Disconnect("ConnectToServer");
            kcpsocket.ConnectKCP(host, iport);
        }
    }

    public void Close()
    {
        isOpen = false;
        DisconnectServer();
    }

    public void DisconnectServer()
    {
        if (kcpsocket != null && kcpsocket.isConnectting == false)
        {
            kcpsocket.Disconnect("DisconnectServer");
            kcpsocket = null;
            GC.Collect();
        }
        else
        {
            Debug.LogErrorFormat("kcpsocket is connectting, not DisconnectServer!!!");
        }
    }

    public void Update()
    {
        if (kcpsocket != null)
        {
            kcpsocket.OnUpdate();
            var packs = kcpsocket.SwitchPacks();
            if (packs != null)
            {
                while (packs.Count > 0)
                {
                    var pack = packs.Dequeue();
                    HandleMsg(pack);
                }
            }
        }
    }

    #region Send 发送数据到服务器
    public void Send<T>(MsgID msgID, T content) where T : Google.Protobuf.IMessage
    {
        KcpNetPack pack = KcpNetPack.SerializeToPack(content, (ushort)msgID);
        Send(pack);
    }

    public void Send(KcpNetPack pack)
    {
        if (kcpsocket != null)
            kcpsocket.SendPack(pack);
    }
    #endregion
    //===============================================================================
    public void RegisterServerEvent(OnServerEvent callback)
    {
        onServerEvent += callback;
    }

    public void UnregisterServerEvent(OnServerEvent callback)
    {
        if (onServerEvent != null)
            onServerEvent -= callback;
    }

    void NetUdpSocket.IListener.OnEvent(NetworkEvent nEvent)
    {
        Debug.LogFormat("Udp OnEvent={0}", nEvent);
        if (onServerEvent != null)
        {
            onServerEvent(nEvent);
        }
    }
    //========================================
    public void RegisterHandler(MsgID messageId, Handle handler)
    {
        if (!msgHandle.ContainsKey(messageId))
            msgHandle[messageId] = handler;
        else
            msgHandle[messageId] += handler;
    }

    public void UnregisterHandler(MsgID messageId, Handle handler)
    {
        if (msgHandle.ContainsKey(messageId))
        {
            msgHandle[messageId] -= handler;
        }
        else
        {
            Debug.LogErrorFormat("Unable to find handler : {0}", messageId);
        }
    }

    public bool HandleMsg(KcpNetPack pack)
    {
        try
        {
            Handle handler = null;
            MsgID msgID = (MsgID)pack.HeadID;
            //Debug.LogFormat("msg.pack.id = {0}", pack.MessageID);
            if (msgHandle.TryGetValue(msgID, out handler))
            {
                if (handler != null)
                {
                    handler(pack);
                    pack.BodyBuffer.Dispose();
                    pack.HeaderBuffer.Dispose();
                    return true;
                }
                else
                {
                    Debug.LogWarningFormat("Error -- find message handler = null with MessageID: {0}", msgID);
                }
            }
            else
            {
                if (msgID != MsgID.HeartbeatId)
                {
                    //TODO:主要是因为在PVP时，有可能出错一方发广播消息，另一方还没有注册此消息，引发错误
                    //后面改回来，规范流程
                    Debug.LogErrorFormat("Error -- Can't find message handler with MessageID: {0}", msgID);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
        return false;
    }
    //===============================================================================
}
