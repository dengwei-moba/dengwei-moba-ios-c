using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

using Google.Protobuf;
using System.Net;

public class TcpSendManager : ScriptBase
{
    public void SendLogin(string name)
    {
        PB_C2SPlayerLogin login = new PB_C2SPlayerLogin();
        login.AccessToken = name;
        login.Platform = "internal";
        login.OS = "test";
        login.GameName = "dv";
        login.SysVer = "1.1.1";
        login.DeviceModel = "ios";
        login.Memory = "123123M";
        login.MAC = "23:23:ew:ew:ew";
        _UnityTcpSocket.Send(MsgID.C2SAccount, (uint)AccountMsgID.C2SPlayerLogin, login);
    }

    public void C2SPlayerCreate(string name)
    {
        PB_C2SPlayerCreate mPlayerCreate = new PB_C2SPlayerCreate();
        mPlayerCreate.Name = name;
        mPlayerCreate.Sex = 1;
        mPlayerCreate.Nation = 1;
        mPlayerCreate.Shape = 50002;
        mPlayerCreate.Icon = 50002;
        _UnityTcpSocket.Send(MsgID.C2SAccount, (uint)AccountMsgID.C2SPlayerCreate, mPlayerCreate);
    }

    public void C2SAddMatch(uint iGuanqia)
    {
        PB_MatchTeamAdd_C2GS2FMS addmatch = new PB_MatchTeamAdd_C2GS2FMS();
        addmatch.Pidlist.Add(NetData.Instance.PlayerID);
        addmatch.Guanqia = iGuanqia;
        _UnityTcpSocket.Send(MsgID.C2SMatch, (uint)MatchMsgID.C2Gs2FmsMatchAdd, addmatch);
    }

    public void C2SDelMatch()
    {
        PB_MatchTeamAdd_FMS2GS2C mMatchTeamAdd = (PB_MatchTeamAdd_FMS2GS2C)NetData.Instance.Query(MsgID.S2CMatch, (uint)MatchMsgID.Fms2Gs2CMatchAdd);
        if (mMatchTeamAdd == null) return;
        PB_MatchTeamDel_C2GS2FMS delmatch = new PB_MatchTeamDel_C2GS2FMS();
        delmatch.Guanqia = mMatchTeamAdd.Guanqia;
        _UnityTcpSocket.Send(MsgID.C2SMatch, (uint)MatchMsgID.C2Gs2FmsMatchDel, delmatch);
    }

    public void C2SSureMatch()
    {
        PB_MatchTeamAdd_FMS2GS2C mMatchTeamAdd = (PB_MatchTeamAdd_FMS2GS2C)NetData.Instance.Query(MsgID.S2CMatch, (uint)MatchMsgID.Fms2Gs2CMatchAdd);
        if (mMatchTeamAdd == null) return;
        PB_MatchTeamReady_C2GS2FMS surematch = new PB_MatchTeamReady_C2GS2FMS();
        surematch.Guanqia = mMatchTeamAdd.Guanqia;
        _UnityTcpSocket.Send(MsgID.C2SMatch, (uint)MatchMsgID.C2Gs2FmsMatchReady, surematch);
    }
}