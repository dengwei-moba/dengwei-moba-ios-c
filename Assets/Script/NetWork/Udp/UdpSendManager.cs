using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Google.Protobuf;
using System.IO;

using System;
using System.Net;

public class UdpSendManager : ScriptBase
{
    private float mLsatChangeDirTime = 0;
    public static float _ChangeDirIntervalTime = 0.05f;    //变方向采样的发送间隔
    private PB_C2SClientInput ChangeDirInputs = new PB_C2SClientInput();

    public void SendStartMove()
    {
        PB_C2SClientInput mClientInputs = new PB_C2SClientInput();
        PB_ClientInput oneInput = new PB_ClientInput();
        oneInput.InputType = InputType.MoveStart;
        mClientInputs.Inputs.Add(oneInput);
        _UnityUdpSocket.Send(MsgID.C2SInputInfo, mClientInputs);
        //Debug.Log("SendStartMove=====");
    }

    public void SendChangeAngle(int tAngle_X,int tAngle_Y)
    {
        mLsatChangeDirTime += Time.deltaTime;
        if (mLsatChangeDirTime > _ChangeDirIntervalTime)
        {
            PB_ClientInput oneInput = new PB_ClientInput();
            oneInput.InputType = InputType.MoveAngle;
            oneInput.AngleX = tAngle_X;
            oneInput.AngleY = tAngle_Y;
            ChangeDirInputs.Inputs.Add(oneInput);
            _UnityUdpSocket.Send(MsgID.C2SInputInfo, ChangeDirInputs);
            //Debug.Log("SendChangeDir=======" + tAngle);
            ChangeDirInputs = new PB_C2SClientInput();
            mLsatChangeDirTime = 0;
        }
        else {
            PB_ClientInput oneInput = new PB_ClientInput();
            oneInput.InputType = InputType.MoveAngle;
            oneInput.AngleX = tAngle_X;
            oneInput.AngleY = tAngle_Y;
            ChangeDirInputs.Inputs.Add(oneInput);
        }
    }

    public void SendEndMove()
    {
        PB_C2SClientInput mClientInputs = new PB_C2SClientInput();
        PB_ClientInput oneInput = new PB_ClientInput();
        oneInput.InputType = InputType.MoveEnd;
        mClientInputs.Inputs.Add(oneInput);
        _UnityUdpSocket.Send(MsgID.C2SInputInfo, mClientInputs);
        //Debug.Log("SendEndMove=====");
    }

    public void SendInputSkill(int inputKey, InputType inputType)
    {
        PB_C2SClientInput mClientInputs = new PB_C2SClientInput();
        PB_ClientInput oneInput = new PB_ClientInput();
        oneInput.InputType = inputType;
        oneInput.Key = inputKey;
        mClientInputs.Inputs.Add(oneInput);
        _UnityUdpSocket.Send(MsgID.C2SInputInfo, mClientInputs);
        //Debug.Log("SendSkill_1=====");
    }

}
