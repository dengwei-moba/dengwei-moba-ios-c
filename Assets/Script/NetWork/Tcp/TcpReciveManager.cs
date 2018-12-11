using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using ClientGame.Net;
using Google.Protobuf;

public class TcpReciveManager : ScriptBase
{
    void Awake()
    {
        _UnityTcpSocket.RegisterHandler(MsgID.S2CMatch, OnS2CMatch);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CAccount, OnS2CAccount);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CScene, OnS2CScene);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CCharge, OnS2CCharge);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CBag, OnS2CBag);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CCombat, OnS2CCombat);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CPanel, OnS2CPanel);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CCommon, OnS2CCommon);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CTask, OnS2CTask);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CNotify, OnS2CNotify);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CNpc, OnS2CNpc);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CHuodong, OnS2CHuodong);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CSummon, OnS2CSummon);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CChannel, OnS2CChannel);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CFriend, OnS2CFriend);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CAlliance, OnS2CAlliance);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CMail, OnS2CMail);
        _UnityTcpSocket.RegisterHandler(MsgID.Gs2CShop, OnS2CShop);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CHuodongOther, OnS2CHuodongOther);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CStore, OnS2CStore);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CRank, OnS2CRank);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CActivity, OnS2CActivity);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CEquip, OnS2CEquip);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CLuckyshop, OnS2CLuckyshop);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CPrompt, OnS2CPrompt);
        _UnityTcpSocket.RegisterHandler(MsgID.S2CExchange, OnS2CExchange);
    }

    public void OnS2CMatch(NetPack pack)
    {
        Debug.LogFormat("匹配系统MatchMsgID,CMD={0}, ", pack.CMD);
        //MatchManager mMatchManager = GameObject.Find("MatchView").GetComponent<MatchManager>();
        MatchManager mMatchManager = FindObjectOfType<MatchManager>();
        switch ((MatchMsgID)pack.CMD)
        {
            case MatchMsgID.Fms2Gs2CMatchAdd:
                PB_MatchTeamAdd_FMS2GS2C mMatchTeamAdd = PB_MatchTeamAdd_FMS2GS2C.Parser.ParseFrom(pack.BodyBuffer.Bytes);
                if (mMatchTeamAdd.Ret) {
                    NetData.Instance.Set(MsgID.S2CMatch, (uint)MatchMsgID.Fms2Gs2CMatchAdd, mMatchTeamAdd);
                }
                mMatchManager.ShowMatching(mMatchTeamAdd.Ret);
                Debug.LogFormat("OnS2CMatch,请求加入匹配队列结果:mMatchTeamAdd={0}", mMatchTeamAdd.ToString());
                break;
            case MatchMsgID.Fms2Gs2CMatchDel:
                PB_MatchTeamDel_FMS2GS2C mMatchTeamDel = PB_MatchTeamDel_FMS2GS2C.Parser.ParseFrom(pack.BodyBuffer.Bytes);
                Debug.LogFormat("OnS2CMatch,请求取消匹配队列结果:mMatchTeamDel={0}", mMatchTeamDel.ToString());
                if (mMatchTeamDel.Ret) {
                    mMatchManager.ShowMatching(false);
                }
                break;
            case MatchMsgID.Fms2Gs2CMatchStart:
                PB_MatchTeamReady_FMS2GS2C mMatchTeamReady = PB_MatchTeamReady_FMS2GS2C.Parser.ParseFrom(pack.BodyBuffer.Bytes);
                mMatchManager.ShowMatched(true, mMatchTeamReady.Lasttime);
                foreach (PB_MatchTeamMemberInfo OnePlayersinfo in mMatchTeamReady.Playersinfo)
                {
                    if (OnePlayersinfo.Pid != NetData.Instance.PlayerID) continue;
                    if (OnePlayersinfo.Ready)
                    {
                        mMatchManager.SureFightBtn.interactable = false;
                        mMatchManager.sSureFightBtnText.text = "已准备";
                    }
                }
                Debug.LogFormat("OnS2CMatch,推送匹配成功,及相关信息(多次推送当作刷新信息):mMatchTeamReady={0}", mMatchTeamReady.ToString());
                break;
            case MatchMsgID.Fms2Gs2CMatchFail:
                PB_MatchTeamFail_FMS2GS2C mMatchTeamFail = PB_MatchTeamFail_FMS2GS2C.Parser.ParseFrom(pack.BodyBuffer.Bytes);
                Debug.LogFormat("OnS2CMatch,推送战斗准备失败(重新进入匹配队列):mMatchTeamFail={0}", mMatchTeamFail.ToString());
                if (mMatchTeamFail.Notmatchpidlist.Contains(NetData.Instance.PlayerID))
                {
                    mMatchManager.ShowMatched(false, 0);
                    mMatchManager.ShowMatching(false);
                }
                else {
                    mMatchManager.ShowMatched(false, 0);
                    mMatchManager.ShowMatching(true);
                }
                break;
            case MatchMsgID.Fms2Gs2CMatchFight:
                PB_MatchTeamFight_FMS2GS2C mMatchTeamFight = PB_MatchTeamFight_FMS2GS2C.Parser.ParseFrom(pack.BodyBuffer.Bytes);
                Debug.LogFormat("OnS2CMatch,推送战斗准备成功,开始战斗,及相关信息:mMatchTeamFight={0}", mMatchTeamFight.ToString());
                NetData.Instance.Set(MsgID.S2CMatch, (uint)MatchMsgID.Fms2Gs2CMatchFight, mMatchTeamFight);
                LoadingManager.LoadSceneAsync(SceneConfig.Fight);
                break;
        }
    }

    public void OnS2CAccount(NetPack pack)
    {
        //Debug.LogFormat("登录部分AccountMsgID,pack.CMD={0}, ", pack.CMD);
        switch((AccountMsgID)pack.CMD){
            case AccountMsgID.S2CNetLoginError:
                PB_NetError mNetError = PB_NetError.Parser.ParseFrom(pack.BodyBuffer.Bytes);
                Debug.LogFormat("OnS2CAccount,登录错误:mNetError={0}", mNetError.ToString());
                break;
            case AccountMsgID.S2CPlayerQuickLoginKey:
                PB_S2CPlayerQuickLoginKey mS2CPlayerQuickLoginKey = PB_S2CPlayerQuickLoginKey.Parser.ParseFrom(pack.BodyBuffer.Bytes);
                NetData.Instance.Set(MsgID.S2CAccount, (uint)AccountMsgID.S2CPlayerQuickLoginKey, mS2CPlayerQuickLoginKey);
                Debug.LogFormat("OnS2CAccount,瞬连密钥:QuickKey={0}", mS2CPlayerQuickLoginKey.ToString());
                break;
            case AccountMsgID.S2CPlayerCreate:
                PB_S2CPlayerCreate mS2CPlayerCreate = PB_S2CPlayerCreate.Parser.ParseFrom(pack.BodyBuffer.Bytes);
                if (mS2CPlayerCreate.Hasplayer == 1)
                {
                    Debug.LogFormat("OnS2CAccount,有角色:AccessToken={0} ", mS2CPlayerCreate.ToString());
                }
                else {
                    Debug.LogFormat("OnS2CAccount,没角色,需要创建角色:AccessToken={0} ", mS2CPlayerCreate.ToString());
                    //_ViewManager.ClearView();
                    //_ViewManager.LoadView("prefab/ui/notifyview_prefab");
                    LoginManager mLoginManager = FindObjectOfType<LoginManager>();
                    mLoginManager.Close();
                    _ViewManager.LoadView("prefab/ui/registerview_prefab");
                }
                break;
            case AccountMsgID.S2CPlayerReplaceLogin:
                Debug.LogFormat("OnS2CAccount,角色被顶号登陆(在其他地方登陆) ");
                break;
            case AccountMsgID.S2CPlayerLogin:
                PB_S2CPlayerLogin mS2CPlayerLogin = PB_S2CPlayerLogin.Parser.ParseFrom(pack.BodyBuffer.Bytes);
                Debug.LogFormat("OnS2CAccount,角色登录:Login={0}", mS2CPlayerLogin.Login);
                if (mS2CPlayerLogin.Login) {
                    RegisterManager mRegisterManager = FindObjectOfType<RegisterManager>();
                    if (mRegisterManager != null) 
                        mRegisterManager.Close();
                    else {
                        LoginManager mLoginManager = FindObjectOfType<LoginManager>();
                        mLoginManager.Close();
                    }
                    _ViewManager.LoadView("prefab/ui/matchview_prefab");
                }
                break;
            case AccountMsgID.S2CPlayerInfo:
                PB_S2CPlayerInfo mS2CPlayerInfo = PB_S2CPlayerInfo.Parser.ParseFrom(pack.BodyBuffer.Bytes);
                NetData.Instance.Set(MsgID.S2CAccount, (uint)AccountMsgID.S2CPlayerInfo, mS2CPlayerInfo);
                Debug.LogFormat("OnS2CAccount,玩家基础属性:mS2CPlayerInfo={0} ", mS2CPlayerInfo.ToString());
                break;
            case AccountMsgID.S2CPlayerUpgrade:
                Debug.LogFormat("OnS2CAccount,玩家升级");
                break;
        }
    }

    public void OnS2CScene(NetPack pack)
    {
        Debug.LogFormat("地图协议相关SceneMsgID,CMD={0}, ", pack.CMD);
    }
    public void OnS2CCharge(NetPack pack)
    {
        Debug.LogFormat("充值系统ChargeMsgID,CMD={0}, ", pack.CMD);
    }
    public void OnS2CBag(NetPack pack)
    {
        Debug.LogFormat("背包部分BagMsgID,CMD={0}, ", pack.CMD);
    }
    public void OnS2CCombat(NetPack pack)
    {
        Debug.LogFormat("战斗部分CombatMsgID,CMD={0}, ", pack.CMD);
    }
    public void OnS2CPanel(NetPack pack)
    {
        Debug.LogFormat("人物面板PanelMsgID,CMD={0}, ", pack.CMD);
    }
    public void OnS2CCommon(NetPack pack)
    {
        Debug.LogFormat("获取其他玩家数据显示CommomMsgID,CMD={0}, ", pack.CMD);
    }
    public void OnS2CTask(NetPack pack)
    {
        Debug.LogFormat("任务相关TaskMsgID,CMD={0}, ", pack.CMD);
    }
    public void OnS2CNotify(NetPack pack)
    {
        //Debug.LogFormat("提示通知/对白框相关系统消息协议NotifyMsgID,CMD={0}, ", pack.CMD);
        switch ((NotifyMsgID)pack.CMD)
        {
            case NotifyMsgID.S2CNotifyNarmal:
                PB_NotifyNarmal mNotifyNarmal = PB_NotifyNarmal.Parser.ParseFrom(pack.BodyBuffer.Bytes);
                Debug.LogFormat("OnS2CNotify,普通悬浮提示:mNotifyNarmal={0}", mNotifyNarmal.ToString());
                NotifyManager mNotifyManager = FindObjectOfType<NotifyManager>();
                mNotifyManager.AddNotify(mNotifyNarmal.Msg);
                break;
        }
    }
    public void OnS2CNpc(NetPack pack)
    {
        Debug.LogFormat("NPC系统消息协议NpcMsgID,CMD={0}, ", pack.CMD);
    }
    public void OnS2CHuodong(NetPack pack)
    {
        Debug.LogFormat("活动相关协议HuodongMsgID,CMD={0}, ", pack.CMD);
    }
    public void OnS2CSummon(NetPack pack)
    {
        Debug.LogFormat("summon相关SummonMsgID,CMD={0}, ", pack.CMD);
    }
    public void OnS2CChannel(NetPack pack)
    {
        Debug.LogFormat("聊天系统ChannelMsgID,CMD={0}, ", pack.CMD);
    }
    public void OnS2CFriend(NetPack pack)
    {
        Debug.LogFormat("好友系统FriendMsgID,CMD={0}, ", pack.CMD);
    }
    public void OnS2CAlliance(NetPack pack)
    {
        Debug.LogFormat("帮派,CMD={0}, ", pack.CMD);
    }
    public void OnS2CMail(NetPack pack)
    {
        Debug.LogFormat("邮件MailMsgID,CMD={0}, ", pack.CMD);
    }
    public void OnS2CShop(NetPack pack)
    {
        Debug.LogFormat("商店,CMD={0}, ", pack.CMD);
    }
    public void OnS2CHuodongOther(NetPack pack)
    {
        Debug.LogFormat("和之前活动导表不同的活动HuodongMsgID,CMD={0}, ", pack.CMD);
    }
    public void OnS2CStore(NetPack pack)
    {
        Debug.LogFormat("商城StoreMsgID,CMD={0}, ", pack.CMD);
    }
    public void OnS2CRank(NetPack pack)
    {
        Debug.LogFormat("排行榜,CMD={0}, ", pack.CMD);
    }
    public void OnS2CActivity(NetPack pack)
    {
        Debug.LogFormat("活动Activity,CMD={0}, ", pack.CMD);
    }
    public void OnS2CEquip(NetPack pack)
    {
        Debug.LogFormat("装备,CMD={0}, ", pack.CMD);
    }
    public void OnS2CLuckyshop(NetPack pack)
    {
        Debug.LogFormat("幸运商店,CMD={0}, ", pack.CMD);
    }
    public void OnS2CPrompt(NetPack pack)
    {
        Debug.LogFormat("红点功能,CMD={0}, ", pack.CMD);
    }
    public void OnS2CExchange(NetPack pack)
    {
        Debug.LogFormat("金币兑换水晶,CMD={0}, ", pack.CMD);
    }
}