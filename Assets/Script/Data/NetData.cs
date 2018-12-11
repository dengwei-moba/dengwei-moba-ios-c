using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;

public class NetData
{
    private static NetData mInstance;
    public static NetData Instance
    {
        get
        {
            if (mInstance == null)
                mInstance = new NetData();
            return mInstance;
        }
    }

    private Dictionary<MsgID, Dictionary<uint, Google.Protobuf.IMessage>> mNetDict = new Dictionary<MsgID, Dictionary<uint, Google.Protobuf.IMessage>>();
    //public bool Add(MsgID msgID, uint cmd, Google.Protobuf.IMessage T)
    //{
    //    //Google.Protobuf.IMessage 之间的相加
    //    return true;
    //}
    public Google.Protobuf.IMessage Del(MsgID msgID, uint cmd)
    {
        Google.Protobuf.IMessage T = null;
        if (mNetDict.ContainsKey(msgID)) {
            if (mNetDict[msgID].ContainsKey(cmd))
            {
                T = mNetDict[msgID][cmd];
                mNetDict[msgID].Remove(cmd);
            }
        }
        return T;
    }

    public void Set(MsgID msgID, uint cmd, Google.Protobuf.IMessage T)
    {
        if (!mNetDict.ContainsKey(msgID))
        {
            mNetDict[msgID] = new Dictionary<uint, Google.Protobuf.IMessage>();
        }
        mNetDict[msgID][cmd] = T;
    }

    public Google.Protobuf.IMessage Query(MsgID msgID, uint cmd)
    {
        Google.Protobuf.IMessage T = null;
        if (!mNetDict.ContainsKey(msgID))
        {
            mNetDict[msgID] = new Dictionary<uint, Google.Protobuf.IMessage>();
        }
        mNetDict[msgID].TryGetValue(cmd, out T);
        return T;
    }
    public Google.Protobuf.IMessage Query(MsgID msgID, uint cmd, Google.Protobuf.IMessage T)
    {
        Google.Protobuf.IMessage T2 = null;
        if (!mNetDict.ContainsKey(msgID))
        {
            mNetDict[msgID] = new Dictionary<uint, Google.Protobuf.IMessage>();
        }
        if (mNetDict[msgID].TryGetValue(cmd, out T2))
            return T2;
        return T;
    }
    //================================================================================
    //一些辅助快速获取的方法
    public ulong PlayerID
    {
        get
        {
            return ((PB_S2CPlayerInfo)NetData.Instance.Query(MsgID.S2CAccount, (uint)AccountMsgID.S2CPlayerInfo)).ID;
        }
    }
}
