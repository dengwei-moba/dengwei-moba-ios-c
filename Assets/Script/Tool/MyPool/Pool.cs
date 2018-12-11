using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool
{
    private Dictionary<string, PoolItem> mPoolDic;

    private static Pool mInstance;
    public static Pool Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new Pool();
            }
            return mInstance;
        }
    }

    private Pool()
    {
        mPoolDic = new Dictionary<string, PoolItem>();
    }

    public void Regist(string tName, string tPath)
    {
        if (mPoolDic.ContainsKey(tName))
        {
            Debug.Log("对象池已注册该对象");
        }
        else
        {
            GameObject obj = new GameObject();
            obj.name = tName;
            PoolItem item = obj.AddComponent<PoolItem>();
            item.SetPath(tName,tPath);
            mPoolDic[tName] = item;
        }
    }

    public void UnRegist(string tName)
    {
        if (mPoolDic.ContainsKey(tName))
        {
            mPoolDic.Remove(tName);
        }
    }

    public GameObject Pop(string tName)
    {
        PoolItem item = null;
        if (mPoolDic.TryGetValue(tName, out item))
        {
            return item.Pop();
        }
        else
        {
            Debug.Log("获取对象失败，对象池未注册该对象");
            return null;
        }
    }

    public void Push(string tName, GameObject tObj)
    {
        PoolItem item = null;
        if (mPoolDic.TryGetValue(tName, out item))
        {
            item.Push(tObj);
        }
        else
        {
            Debug.Log("回收对象失败，对象池未注册该对象");
        }
    }
}
