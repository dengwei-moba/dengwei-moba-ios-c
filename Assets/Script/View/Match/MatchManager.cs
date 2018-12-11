using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchManager : ViewBase
{
    private Button OneSelfBtn;
    private Button OneVOneBtn;
    private Button TwoVTwoBtn;
    private Button ThreeVThreeBtn;

    public GameObject mMatching;
    public GameObject mMatched;
    public Button SureFightBtn;
    public Text sSureFightBtnText;
    private Text sSucMatchText;
    private uint _ReadyLasttime;

    public override void Awake()
    {
        base.Awake();
        Transform canvasTrans = transform.Find("Canvas");
        OneSelfBtn = canvasTrans.Find("OneSelfBtn").GetComponent<Button>();
        OneVOneBtn = canvasTrans.Find("OneVOneBtn").GetComponent<Button>();
        TwoVTwoBtn = canvasTrans.Find("TwoVTwoBtn").GetComponent<Button>();
        ThreeVThreeBtn = canvasTrans.Find("ThreeVThreeBtn").GetComponent<Button>();
        OneSelfBtn.onClick.AddListener(() => { OneVOneBtnFunc(OneSelfBtn); });
        OneVOneBtn.onClick.AddListener(() => { OneVOneBtnFunc(OneVOneBtn); });
        TwoVTwoBtn.onClick.AddListener(() => { OneVOneBtnFunc(TwoVTwoBtn); });
        ThreeVThreeBtn.onClick.AddListener(() => { OneVOneBtnFunc(ThreeVThreeBtn); });
    }
    
    void OneVOneBtnFunc(Button sender)
    {
        if (sender == OneSelfBtn)
        {
            _TcpSendManager.C2SAddMatch(1001);
        }
        else if (sender == OneVOneBtn)
        {
            _TcpSendManager.C2SAddMatch(1002);
        }
        else if (sender == TwoVTwoBtn)
        {
            _TcpSendManager.C2SAddMatch(1003);
        }
        else if (sender == ThreeVThreeBtn)
        {
            _TcpSendManager.C2SAddMatch(1004);
        }
    }

    public void ShowMatching(bool bShow) {
        mMatching.SetActive(bShow);
        if (bShow) {
            Button DelMatchBtn = mMatching.transform.Find("DelMatch").GetComponent<Button>();
            DelMatchBtn.onClick.AddListener(() => { OnClickDelMatch(); });
        }
    }
    public void ShowMatched(bool bShow, uint Lasttime)
    {
        mMatched.SetActive(bShow);
        if (bShow)
        {
            SureFightBtn = mMatched.transform.Find("SureFightBtn").GetComponent<Button>();
            SureFightBtn.onClick.AddListener(() => { OnClickSureFight(); });
            sSureFightBtnText = SureFightBtn.transform.Find("SureFightBtnText").GetComponent<Text>();
            sSucMatchText = mMatched.transform.Find("SucMatchText").GetComponent<Text>();

            _ReadyLasttime = Lasttime;
            sSucMatchText.text = "匹配成功!请" + Lasttime + "秒内准备战斗";
            CancelInvoke();
            Invoke("LastTimer", 1.0f);
        }
        else {
            if (SureFightBtn != null) SureFightBtn.interactable = true;
            if (sSureFightBtnText != null) sSureFightBtnText.text = "确认准备";
        }
    }

    void LastTimer()
    {
        if (_ReadyLasttime <= 0) return;
        _ReadyLasttime--;
        sSucMatchText.text = "匹配成功!请" + _ReadyLasttime + "秒内准备战斗";
        Invoke("LastTimer", 1.0f);
    }

    void OnClickDelMatch()
    {
        _TcpSendManager.C2SDelMatch();
    }

    void OnClickSureFight()
    {
        _TcpSendManager.C2SSureMatch();
    }
}
