using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using Google.Protobuf;
using TrueSync;

public class houyiBullet : TrueSyncBehaviour
{
	private TSTransform mRotateTSTransform;
	public TSTransform RotateTSTransform
	{
		get
		{
			if (mRotateTSTransform == null)
			{
				mRotateTSTransform = transform.Find("rotate").transform.GetComponent<TSTransform>();
			}
			return mRotateTSTransform;
		}
	}

	private TSTransform mAllTSTransform;
	public TSTransform AllTSTransform
	{
		get
		{
			if (mAllTSTransform == null)
			{
				mAllTSTransform = GetComponent<TSTransform>();
			}
			return mAllTSTransform;
		}
	}

	public FP Speed;
	public TSVector Angle;
	void Awake()
	{
		Angle = TSVector.zero;
		Speed = (FP)0.5f;
		//mAllTSTransform = GetComponent<TSTransform>();
	}

	public override void OnSyncedUpdate()
	{
		if (Angle.IsZero()) return;
		AllTSTransform.Translate(Angle * Speed);
		AllTSTransform.OnUpdate();
		RotateTSTransform.OnUpdate();
		//Debug.LogErrorFormat("houyiBullet====>{0},{1},{2}",ownerIndex, Angle, RotateTSTransform.rotation.eulerAngles);
    }

	#region TrueSyncBehaviour操作相关

	public void OnSyncedTriggerEnter(TSCollision other)
	{
		int otherLayerMask = (int)Math.Pow(2, other.gameObject.layer);
		Debug.LogErrorFormat("houyiBullet====>Trigger  Enter==>{0},{1},{2}", otherLayerMask, other.gameObject.name, LayerMask.GetMask("Role"));
		if (otherLayerMask == LayerMask.GetMask("Role"))
		{
			Actor mActor = other.gameObject.GetComponent<Actor>();
			Debug.LogErrorFormat("houyiBullet====>TriggerEnter==>自己阵营:{0},对方阵营={1}", OwnerCamp, mActor.OwnerCamp);
			if (OwnerCamp != mActor.OwnerCamp)
			{
				mActor.mActorAttr.Hp -= 10;
				TrueSyncManager.SyncedDestroy(gameObject);
			}
		}
		else if (otherLayerMask == LayerMask.GetMask("Monsters"))
		{
			YeGuaiAI mYeGuaiAI = other.gameObject.GetComponent<YeGuaiAI>();
			mYeGuaiAI.AddHp(-10, OwnerID);
		}
		else if (otherLayerMask == LayerMask.GetMask("GroundWall"))
		{
			TrueSyncManager.SyncedDestroy(gameObject);
		}
	}

	public void OnSyncedTriggerStay(TSCollision other)
	{
		//Debug.LogErrorFormat("houyiBullet====>Trigger  Stay==>{0}", other.gameObject.name);
	}

	public void OnSyncedTriggerExit(TSCollision other)
	{
		//Debug.LogErrorFormat("houyiBullet====>Trigger  Exit==>{0}", other.gameObject.name);
	}

	#endregion TrueSyncBehaviour操作相关
}