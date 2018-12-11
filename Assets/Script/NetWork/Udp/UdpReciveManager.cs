using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using ClientGame.Net;
using Google.Protobuf;
using System.IO;
using TrueSync;


public class UdpReciveManager: ScriptBase
{
    void Awake()
    {
        _UnityUdpSocket.RegisterHandler(MsgID.S2CFrameInfo, OnS2CFrameInfo);
    }

    /// <summary>
    /// 帧同步信息
    /// </summary>
    public void OnS2CFrameInfo(KcpNetPack pack)
    {
        PB_FrameInfo mFrameInfo = PB_FrameInfo.Parser.ParseFrom(pack.BodyBuffer.Bytes);
        uint frameindex = mFrameInfo.FrameIndex;
        List<PB_PlayerFrame> list =new List<PB_PlayerFrame>();
        for (int i = 0; i < mFrameInfo.Inputs.Count; i++)
        {
            list.Add(mFrameInfo.Inputs[i]);
        }
        TrueSyncManager.Instance.AddOneFrame(frameindex, list);
        
    }

    public void PlayerFrameHandle(List<PB_PlayerFrame> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            PB_PlayerFrame mPlayerFrame = list[i];
            uint userid = mPlayerFrame.Index;
            Actor actor = TrueSyncManager.Instance.GetActor(userid);
            if (actor != null)
            {
                actor.PlayerInputHandle(mPlayerFrame.Input);
            }
        }
    }

}
