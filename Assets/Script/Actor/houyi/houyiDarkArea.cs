using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using Google.Protobuf;
using TrueSync;

public class houyiDarkArea : TrueSyncBehaviour
{
	private int SelfTick = 0;
	
	void Awake()
	{
	}

	public override void OnSyncedUpdate()
	{
		SelfTick += 1;
		if (SelfTick >= 100) {
			TrueSyncManager.SyncedDestroy(gameObject);
		}
    }

	#region TrueSyncBehaviour操作相关

	public void OnSyncedTriggerEnter(TSCollision other)
	{
		//Debug.LogErrorFormat("houyiDarkArea====>Trigger  Enter==>{0}", other.gameObject.name);
	}

	public void OnSyncedTriggerStay(TSCollision other)
	{
		if (SelfTick % 10 != 0) return;
		int otherLayerMask = (int)Math.Pow(2, other.gameObject.layer);
		//Debug.LogErrorFormat("houyiDarkArea====>Trigger  Stay==>{0},{1}", otherLayerMask, other.gameObject.name);
		if (otherLayerMask == LayerMask.GetMask("Role"))
		{
			Actor mActor = other.gameObject.GetComponent<Actor>();
			//Debug.LogErrorFormat("houyiDarkArea====>TriggerEnter==>自己阵营:{0},对方阵营={1}", OwnerCamp, mActor.OwnerCamp);
			if (OwnerCamp != mActor.OwnerCamp)
			{
				mActor.mActorAttr.Hp -= 5;
			}
		}
		else if (otherLayerMask == LayerMask.GetMask("Monsters"))
		{
			YeGuaiAI mYeGuaiAI = other.gameObject.GetComponent<YeGuaiAI>();
			mYeGuaiAI.AddHp(-5, OwnerID);
		}
	}

	public void OnSyncedTriggerExit(TSCollision other)
	{
		//Debug.LogErrorFormat("houyiDarkArea====>Trigger  Exit==>{0}", other.gameObject.name);
	}

	#endregion TrueSyncBehaviour操作相关
}