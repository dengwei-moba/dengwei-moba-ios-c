using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LitJson;
using Google.Protobuf;
using TrueSync;

public class LockStep : ScriptBase
{
    private float mLogicTempTime = 0;
    void Update()
    {
        mLogicTempTime += Time.deltaTime;
        if (mLogicTempTime > LockStepConfig.mRenderFrameUpdateTime)
        {
            for (int i = 0; i < mFastForwardSpeed; i++)
            {
                GameTurn();
                mLogicTempTime = mLogicTempTime - LockStepConfig.mRenderFrameUpdateTime;
            }
        }
    }

    private int mFastForwardSpeed = 1;
    public void SetFaseForward(int tValue)
    {
        mFastForwardSpeed = Math.Min(tValue,10);
        //Debug.Log("SetFaseForward=============>" + mFastForwardSpeed + ",tValue=" + tValue);
    }

    void GameTurn()
    {
        List<PB_PlayerFrame> list = null;
        if (TrueSyncManager.Instance.LockFrameTurn(ref list))
        {
            if (list != null)
                _UdpReciveManager.PlayerFrameHandle(list);
            //Debug.Log("处理网络包间隔===================================================>" + mLogicTempTime + ",mFastForwardSpeed==" + mFastForwardSpeed);
            TrueSyncManager.Instance.OnUpdate();
        }
    }
}
