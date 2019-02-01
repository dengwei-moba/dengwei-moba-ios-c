using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using Google.Protobuf;
using TrueSync;

public class yeguaiBullet : TrueSyncBehaviour
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

	public Actor mTargetEnemyActor;		//目标敌人

	void Awake()
	{
		Speed = (FP)0.3f;
	}

	public override void OnSyncedUpdate()
	{
		if (mTargetEnemyActor==null) return;
		TSVector DistanceAngle = mTargetEnemyActor.AllTSTransform.position - AllTSTransform.position;
		//Debug.LogErrorFormat("yeguaiBullet====>{0},{1},{2},{3}", mTargetEnemyActor.AllTSTransform.position, AllTSTransform.position, DistanceAngle, DistanceAngle.magnitude);
		
		TSVector Angle = DistanceAngle.normalized;
		AllTSTransform.Translate(Angle * Speed);
		AllTSTransform.OnUpdate();
		//RotateTSTransform.OnUpdate();
		//Debug.LogErrorFormat("yeguaiBullet====>{0},{1},{2}",ownerIndex, Angle, RotateTSTransform.rotation.eulerAngles);
		if (DistanceAngle.magnitude <= 1)
		{
			OnHitEnemyActor();
		}
    }

	private void OnHitEnemyActor(){
		//Debug.LogErrorFormat("OnHitEnemyActor====>");
		mTargetEnemyActor.mActorAttr.Hp -= 3;
		mTargetEnemyActor = null;
		TrueSyncManager.SyncedDestroy(gameObject);
	}
}