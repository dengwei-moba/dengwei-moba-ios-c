using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using UnityEngine;
using ClientGame.Net;
using Google.Protobuf;
using System.Collections.Generic;

public class UnityTcpSocket : ScriptBase, NetTcpSocket.IListener
{
    public delegate void OnServerEvent(NetworkEvent eType);
    private OnServerEvent onServerEvent;
    public delegate void Handle(NetPack pack);
    private Dictionary<MsgID, Handle> msgHandle;
    bool isOpen = false;
    private NetTcpSocket tcpsocket;

    void Awake()
    {
        msgHandle = new Dictionary<MsgID, Handle>();
    }

    public void ConnectToServer(string host, int iport)
    {
        if (tcpsocket == null)
        {
            tcpsocket = new NetTcpSocket(this);
            tcpsocket.ConnectTCP(host, iport);
            Debug.LogFormat("ConnectToServer, host={0},iport={1}", host, iport);
        }
        else if (tcpsocket.isConnectting)
        {
            Debug.LogErrorFormat("tcpsocket is connectting, not start!!! host={0},iport={1}", host, iport);
        }
        else
        {
            Debug.LogErrorFormat("DisConnectToServer, host={0},iport={1},Try Again", host, iport);
            tcpsocket.Disconnect();
            tcpsocket.ConnectTCP(host, iport);
        }
    }

    public void Close()
    {
        isOpen = false;
        DisconnectServer();
    }

    public void DisconnectServer()
    {
        if (tcpsocket != null && tcpsocket.isConnectting == false)
        {
            tcpsocket.Disconnect();
            tcpsocket = null;
        }
        else
        {
            Debug.LogErrorFormat("tcpsocket is connectting, not DisconnectServer!!!");
        }
    }

    public void Update()
    {
        if (tcpsocket != null)
        {
            tcpsocket.OnUpdate();
            var packs = tcpsocket.SwitchPacks();
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
    public void Send<T>(MsgID msgID, uint cmd, T content) where T : Google.Protobuf.IMessage
    {
        NetPack pack = NetPack.SerializeToPack(content, (uint) msgID, cmd);
        Send(pack);
    }

    public void Send(NetPack pack)
    {
        if (tcpsocket != null)
            tcpsocket.SendPack(pack);
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

    void NetTcpSocket.IListener.OnEvent(NetworkEvent nEvent)
    {
        Debug.LogFormat("Tcp OnEvent={0}", nEvent);
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

    public bool HandleMsg(NetPack pack)
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
