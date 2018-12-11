using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LitJson;
using System.IO;
using Google.Protobuf;
using TrueSync;

public class FrameData
{
    private uint mPlayFrameIndex = 1;
    private Dictionary<uint, List<PB_PlayerFrame>> mFrameCatchDic;

    public FrameData()
    {
        mFrameCatchDic = new Dictionary<uint, List<PB_PlayerFrame>>();
        mPlayFrameIndex = 1;
    }

    public void AddOneFrame(uint frameindex, List<PB_PlayerFrame> list)
    {
        lock (mFrameCatchDic)
        {
            if (frameindex >= mPlayFrameIndex)
            {
                mFrameCatchDic[frameindex] = list;

                int speed = (int)(frameindex - mPlayFrameIndex);
                if (speed == 0)
                    speed = 1;
                TrueSyncManager.Instance.SetFaseForward(speed);
            }
        }
    }

    public bool LockFrameTurn(ref List<PB_PlayerFrame> list)
    {
        lock (mFrameCatchDic)
        {
            if (mFrameCatchDic.TryGetValue(mPlayFrameIndex, out list))
            {
                //Debug.Log("执行帧id = " + mPlayFrameIndex);
                mFrameCatchDic.Remove(mPlayFrameIndex);
                mPlayFrameIndex++;
                return true;
            }
            else
                return false;
        }
    }
}
