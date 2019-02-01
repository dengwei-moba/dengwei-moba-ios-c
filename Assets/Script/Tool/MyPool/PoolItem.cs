using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrueSync;

public class PoolItem : ScriptBase
{
    private string mName;
    private string mPath;
    private Transform mRootTrans;
    private GameObject mAssetObj;

    private List<GameObject> mGameList = new List<GameObject>();

    internal void SetPath(string tName, string tPath)
    {
        mName = tName;
        mPath = tPath;
        mRootTrans = transform;
    }

    internal GameObject Pop()
    {
        GameObject returnobj = null;
        if (mGameList.Count > 0)
        {
            returnobj = mGameList[mGameList.Count - 1];
            mGameList.RemoveAt(mGameList.Count - 1);
        }
        else
        {
            if (mAssetObj == null)
            {                
                mAssetObj = _AssetManager.GetGameObject(mPath);
                mAssetObj.transform.SetParent(mRootTrans);
                mAssetObj.SetActive(false);
            }
            returnobj = GameObject.Instantiate(mAssetObj);
        }
        returnobj.transform.SetParent(mRootTrans);
        returnobj.SetActive(true);
        return returnobj;
    }

	internal GameObject Pop(TSVector position, TSQuaternion rotation)
	{
		GameObject returnobj = null;
		if (mGameList.Count > 0)
		{
			returnobj = mGameList[mGameList.Count - 1];
			mGameList.RemoveAt(mGameList.Count - 1);
		}
		else
		{
			if (mAssetObj == null)
			{
				mAssetObj = _AssetManager.GetGameObject(mPath);
				mAssetObj.transform.SetParent(mRootTrans);
				mAssetObj.SetActive(false);
			}
			returnobj = TrueSyncManager.SyncedInstantiate(mAssetObj, position, rotation);
		}
		returnobj.transform.SetParent(mRootTrans);
		returnobj.SetActive(true);
		return returnobj;
	}

    internal void Push(GameObject tObj)
    {
		//TrueSyncManager.SyncedDisableBehaviour(tObj);
        tObj.SetActive(false);
        mGameList.Add(tObj);
    }

    private void OnDestroy()
    {
        Pool.Instance.UnRegist(mName);
        _AssetManager.UnLoadUnUseAsset(mPath);
    }
}
