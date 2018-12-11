using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegisterManager : ViewBase
{
    private InputField mNameInput;
    private Button mRegisterBtn;

    public override void Awake()
    {
        base.Awake();
        Transform canvasTrans = transform.Find("Canvas");
        mNameInput = canvasTrans.Find("NameInput").GetComponent<InputField>();
        mRegisterBtn = canvasTrans.Find("RegisterBtn").GetComponent<Button>();
        mRegisterBtn.onClick.AddListener(OnRegister);
    }

    void OnRegister()
    {
        if (!string.IsNullOrEmpty(mNameInput.text))
        {
            _TcpSendManager.C2SPlayerCreate(mNameInput.text);
        }
    }
}
