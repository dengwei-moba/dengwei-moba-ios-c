using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class NotifyManager : ViewBase
{
    private float _LastShowTime = 0;
    private static float _ShowIntervalTime = 0.1f;
    private static int MaxShowCount = 10;
    public List<GameObject> mNormalNotifyObjList = new List<GameObject>(MaxShowCount);
    private Queue<string> mWaitShowMsgQueue = new Queue<string>();
    private List<int> mShowIdxList = new List<int>(MaxShowCount);
    public override void Awake()
    {
        base.Awake();
    }

    public void AddNotify(string sMsg)
    {
        lock (mWaitShowMsgQueue)
        {
            mWaitShowMsgQueue.Enqueue(sMsg);
        }
        //TryShowNormalNotifyObj();
    }

    void Update()
    {
        _LastShowTime += Time.deltaTime;
        if (_LastShowTime < _ShowIntervalTime) return;
        TryShowNormalNotifyObj();

    }
    private void TryShowNormalNotifyObj()
    {
        if (mWaitShowMsgQueue.Count <= 0) return;
        _LastShowTime = 0;
        GameObject mOneNormalNotifyObj = TryGetOneNormalNotifyObj();
        if (mOneNormalNotifyObj == null) return;
        string sShowMsg;
        lock (mWaitShowMsgQueue)
        {
            sShowMsg = mWaitShowMsgQueue.Dequeue();
        }
        Text sText = mOneNormalNotifyObj.GetComponent<Text>();
        sText.text = sShowMsg;
        mOneNormalNotifyObj.SetActive(true);
        mOneNormalNotifyObj.transform.DOLocalMoveY(100, 1.0f).onComplete = () => { OnOneNormalNotifyObjDestroy(mOneNormalNotifyObj); };
        
    }

    private GameObject TryGetOneNormalNotifyObj() {
        foreach (GameObject mOneNormalNotifyObj in mNormalNotifyObjList) {
            if (mShowIdxList.Contains(mOneNormalNotifyObj.GetInstanceID())) continue;
            mShowIdxList.Add(mOneNormalNotifyObj.GetInstanceID());
            return mOneNormalNotifyObj;
        }
        return null;
    }

    void OnOneNormalNotifyObjDestroy(GameObject mOneNormalNotifyObj)
    {
        mOneNormalNotifyObj.transform.DOLocalMoveY(-100, 0.01f);
        //mOneNormalNotifyObj.transform.DOComplete();
        mOneNormalNotifyObj.SetActive(false);
        if (mShowIdxList.Contains(mOneNormalNotifyObj.GetInstanceID())) mShowIdxList.Remove(mOneNormalNotifyObj.GetInstanceID());
        //TryShowNormalNotifyObj();
    }

}
