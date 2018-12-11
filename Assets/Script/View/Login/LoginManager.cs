using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : ViewBase
{
    private InputField mAccountInput;
    private InputField mIpInput;
    private InputField mPortInput;
    private Button mLoginBtn;

    public override void Awake()
    {
        base.Awake();
        Transform canvasTrans = transform.Find("Canvas");
        mAccountInput = canvasTrans.Find("AccountInput").GetComponent<InputField>();
        mIpInput = canvasTrans.Find("IpInput").GetComponent<InputField>();
        mPortInput = canvasTrans.Find("PortInput").GetComponent<InputField>();
        mLoginBtn = canvasTrans.Find("LoginBtn").GetComponent<Button>();
        mLoginBtn.onClick.AddListener(OnLogin);
    }
    
    void OnLogin()
    {
        if (!string.IsNullOrEmpty(mAccountInput.text))
        {
            string host = "47.107.164.164";
            if (mIpInput.text != "") host = mIpInput.text;
            int port = 10012 + 50 * 101;
            if (mPortInput.text != "") port = int.Parse(mPortInput.text);
            Debug.LogFormat("OnLogin,ip={0}, port={1}", host, port);
            _UnityTcpSocket.ConnectToServer(host, port);
            _UnityTcpSocket.RegisterServerEvent(PrepLoginStart);
        }
    }

    public void PrepLoginStart(NetworkEvent eType)
    {
        Debug.LogFormat("PrepLoginStart eType={0}", eType);
        if (eType == NetworkEvent.connected)
        {
            _TcpSendManager.SendLogin(mAccountInput.text);

        }
    }
}
