using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using Google.Protobuf;
using TrueSync;
using Werewolf.StatusIndicators.Components;

public class GameState_Skill_1_houyi : ActorState
{
    private Actor mActor;

    public override ActorStateType StateType
    {
        get
        {
            return ActorStateType.Skill_1;
        }
    }

    public override void TryTransState(ActorStateType tStateType)
    {

    }

    public override void Enter(params object[] param)
    {
		mActor = param[0] as Actor;
		//if (mActor != null && mActor.ActorObj != null)
		//{
		//	if (mActor.ActorAnimation != null)
		//	{
		//		mActor.ActorAnimation.wrapMode = WrapMode.Loop;
		//		mActor.ActorAnimation.Play("skill1");
		//	}
		//}
    }

    public override void Exit(ActorState NextGameState)
    {
        mActor = null;
    }

    public override void OnUpdate()
    {
        if (mActor != null && mActor.ActorObj != null)
        {
            mActor.Skill_1();
            mActor.TransState(ActorStateType.Idle);
        }
    }
}

public class GameState_Skill_2_houyi : ActorState
{
	private Actor mActor;
	private int PlayAnimationFrame = 8;
	private int inputAngleX;
	private int inputAngleY;

	public override ActorStateType StateType
	{
		get
		{
			return ActorStateType.Skill_2;
		}
	}

	public override void TryTransState(ActorStateType tStateType)
	{

	}

	public override void Enter(params object[] param)
	{
		mActor = param[0] as Actor;
		object[] param2 = param[1] as object[];
		inputAngleX = (int)(param2[0]);
		inputAngleY = (int)(param2[1]);
		//Debug.LogErrorFormat("Enter==========>{0},{1}", inputAngleX, inputAngleY);
		if (mActor != null && mActor.ActorObj != null)
		{
			//先转身
			//TSVector mTSVector = new TSVector((FP)inputAngleX / 1000, FP.Zero, (FP)inputAngleY / 1000);
			//mActor.RotateTSTransform.rotation = TSQuaternion.LookRotation(mTSVector);
			if (mActor.ActorAnimation != null)
			{
				mActor.ActorAnimation.wrapMode = WrapMode.Loop;
				mActor.ActorAnimation.Play("skill3");
				PlayAnimationFrame = 8;
			}
		}
	}

	public override void Exit(ActorState NextGameState)
	{
		mActor = null;
	}

	public override void OnUpdate()
	{
		PlayAnimationFrame--;
		if (mActor != null && mActor.ActorObj != null && PlayAnimationFrame <= 0)
		{
			mActor.Skill_2(inputAngleX, inputAngleY, PlayAnimationFrame);
			if (PlayAnimationFrame<=0)
				mActor.TransState(ActorStateType.Idle);
		}
	}
}

public class GameState_Skill_3_houyi : ActorState
{
	private Actor mActor;
	private int PlayAnimationFrame = 25;
	private int inputAngleX;
	private int inputAngleY;

	public override ActorStateType StateType
	{
		get
		{
			return ActorStateType.Skill_3;
		}
	}

	public override void TryTransState(ActorStateType tStateType)
	{

	}

	public override void Enter(params object[] param)
	{
		mActor = param[0] as Actor;
		object[] param2 = param[1] as object[];
		inputAngleX = (int)(param2[0]);
		inputAngleY = (int)(param2[1]);
		//Debug.LogErrorFormat("Enter==========>{0},{1}", inputAngleX, inputAngleY);
		if (mActor != null && mActor.ActorObj != null)
		{
			//先转身
			TSVector mTSVector;
			if (inputAngleX == 0 && inputAngleY == 0)
				mTSVector = new TSVector(mActor.Angle.x, FP.Zero, mActor.Angle.z);
			else
				mTSVector = new TSVector((FP)inputAngleX / 1000, FP.Zero, (FP)inputAngleY / 1000);
			mActor.RotateTSTransform.rotation = TSQuaternion.LookRotation(mTSVector);
			if (mActor.ActorAnimation != null)
			{
				mActor.ActorAnimation.wrapMode = WrapMode.Loop;
				mActor.ActorAnimation.Play("skill3");
				PlayAnimationFrame = 25;
			}
		}
	}

	public override void Exit(ActorState NextGameState)
	{
		mActor = null;
	}

	public override void OnUpdate()
	{
		PlayAnimationFrame--;
		if (mActor != null && mActor.ActorObj != null && PlayAnimationFrame<=0)
		{
			mActor.Skill_3(inputAngleX, inputAngleY);
			mActor.TransState(ActorStateType.Idle);
		}
	}
}

public class GameState_Skill_4_houyi : ActorState
{
	private Actor mActor;
	private int PlayAnimationFrame = 0;
	private int PlayAnimationFrameInterval = 16;
	private int _TargetID = 0;
	private Actor mTargetActor;

	public override ActorStateType StateType
	{
		get
		{
			return ActorStateType.Skill_4;
		}
	}

	public override void TryTransState(ActorStateType tStateType)
	{

	}

	public override void Enter(params object[] param)
	{
		mActor = param[0] as Actor;
		object[] param2 = param[1] as object[];
		_TargetID = (int)(param2[0]);
		mTargetActor = GameProcessManager_Dota.Instance.GetGuaiActor(_TargetID);
		Debug.LogErrorFormat("Skill_4_houyi Enter==========>{0},{1}", _TargetID, mTargetActor.OwnerID);
	}

	public override void Exit(ActorState NextGameState)
	{
		mActor = null;
		mTargetActor = null;
		PlayAnimationFrame = 0;
		_TargetID = 0;
	}

	public override void OnUpdate()
	{
		PlayAnimationFrame++;
		if (PlayAnimationFrame % PlayAnimationFrameInterval != 1) return;
		if (mActor != null && mActor.ActorObj != null && mTargetActor != null)
		{
			//先转身
			TSVector mTSVector = (mTargetActor.AllTSTransform.position - mActor.AllTSTransform.position).normalized;
			mActor.RotateTSTransform.rotation = TSQuaternion.LookRotation(mTSVector);
			if (mActor.ActorAnimation != null)
			{
				mActor.ActorAnimation.wrapMode = WrapMode.Loop;
				mActor.ActorAnimation.Play("skill1");
			}

			if (mTargetActor.IsDeath)
				mActor.TransState(ActorStateType.Idle);
			else
				mActor.Skill_4(mTargetActor);
		}
		else {
			mActor.TransState(ActorStateType.Idle);
		}
	}
}

public class PlayerActor_houyi : PlayerActor
{
    public static FixedPointF mRenderFrameRate = new FixedPointF(1000, 1000);

    protected override void InitStateMachine()
    {
        mStateMachineDic[ActorStateType.Idle] = new GameState_Idle_Normal();
        mStateMachineDic[ActorStateType.Move] = new GameState_Move_Normal();
        mStateMachineDic[ActorStateType.Skill_1] = new GameState_Skill_1_houyi();
		mStateMachineDic[ActorStateType.Skill_2] = new GameState_Skill_2_houyi();
		mStateMachineDic[ActorStateType.Skill_3] = new GameState_Skill_3_houyi();
		mStateMachineDic[ActorStateType.Skill_4] = new GameState_Skill_4_houyi();
    }

    protected override void InitWillUsedPrefabs()
    {
        WillUsedPrefabs = new GameObject[4];
		WillUsedPrefabs[0] = _AssetManager.GetGameObject("prefab/effect/bullet/houyi_pengzhuang_prefab");

		WillUsedPrefabs[1] = _AssetManager.GetGameObject("prefab/effect/bullet/houyibullet_prefab");
		houyiBullet mHouYiBullet = WillUsedPrefabs[1].GetComponent<houyiBullet>();
		mHouYiBullet.OwnerCamp = OwnerCamp;
		mHouYiBullet.OwnerID = OwnerID;
		WillUsedPrefabs[1].SetActive(false);

		WillUsedPrefabs[2] = _AssetManager.GetGameObject("prefab/effect/magical/fx/dark_area_prefab");
		houyiDarkArea mHouYiDarkArea = WillUsedPrefabs[2].GetComponent<houyiDarkArea>();
		mHouYiDarkArea.OwnerCamp = OwnerCamp;
		mHouYiDarkArea.OwnerID = OwnerID;
		WillUsedPrefabs[2].SetActive(false);

		WillUsedPrefabs[3] = _AssetManager.GetGameObject("prefab/effect/bullet/houyibasebullet_prefab");
		houyiBaseBullet mHouYiBaseBullet = WillUsedPrefabs[3].GetComponent<houyiBaseBullet>();
		mHouYiBaseBullet.OwnerCamp = OwnerCamp;
		mHouYiBaseBullet.OwnerID = OwnerID;
		WillUsedPrefabs[3].SetActive(false);
    }
	//================================技能实现效果相关===========================================
	public override void Skill_1(params object[] param)  //闪现
	{
		TSVector rayOrigin = AllTSTransform.position;
		TSVector rayDirection = this.Angle;
		FP maxDistance = Speed * 50;
		//int layerMask = UnityEngine.Physics.DefaultRaycastLayers;
		int layerMask = LayerMask.GetMask("Wall");
		bool bHit = false;//= TSPhysics.Raycast(rayOrigin, rayDirection, hit, maxDistance, layerMask);
		TSRay ray = new TSRay(rayOrigin, rayDirection);
		TSRaycastHit hit = PhysicsWorldManager.instance.Raycast(ray, maxDistance, layerMask);
		if (hit != null)
		{
			if (hit.distance <= maxDistance)
				bHit = true;
		}
		//Debug.LogErrorFormat("闪现1,起点{0},方向{1},距离{2},是否{3}", rayOrigin.ToString(), rayDirection.ToString(), maxDistance.ToString(), bHit);
		if (!bHit)
		{
			AllTSTransform.Translate(Angle * maxDistance);
			return;
		}
		//TrueSyncManager.SyncedInstantiate(WillUsedPrefabs[0], hit.point, RotateTSTransform.rotation);
		//Debug.LogFormat("射线碰撞点1,point={0},transform={1},法向量normal={2},collider={3},distance={4}", hit.point.ToString(), hit.transform.ToString(), hit.normal.ToString(), hit.collider.ToString(), hit.distance.ToString());
		//开始检查反方向的碰撞点法向量是(0,0,0),说明在在内部
		TSVector rayOrigin2 = rayOrigin + (Angle * maxDistance);//rayOrigin+maxDistance的目标坐标
		TSVector rayDirection2 = TSVector.Negate(this.Angle);//逆向量的方向
		TSRay ray2 = new TSRay(rayOrigin2, rayDirection2);
		TSRaycastHit hit2 = PhysicsWorldManager.instance.Raycast(ray2, maxDistance, layerMask);
		bool bHit2 = false;
		if (hit2 != null)
		{
			if (hit2.distance <= maxDistance)
				bHit2 = true;
		}
		//Debug.LogErrorFormat("闪现2,起点{0},方向{1},距离{2},是否{3}", rayOrigin2.ToString(), rayDirection2.ToString(), maxDistance.ToString(), bHit2);
		if (bHit2)
		{
			TrueSyncManager.SyncedInstantiate(WillUsedPrefabs[0], hit2.point, RotateTSTransform.rotation);
			//Debug.LogFormat("射线碰撞点2,point={0},transform={1},法向量normal={2},collider={3},distance={4}", hit2.point.ToString(), hit2.transform.ToString(), hit2.normal.ToString(), hit2.collider.ToString(), hit2.distance.ToString());
			if (!hit2.normal.IsNearlyZero())
			{
				if (hit2.distance > (FP)1f)
				{
					AllTSTransform.Translate(Angle * maxDistance);
					//Debug.LogErrorFormat("超距离{0},", hit2.distance.ToString());
				}
				else
				{
					AllTSTransform.Translate(Angle * maxDistance);
					AllTSTransform.Translate(hit2.normal * ((FP)1f - hit2.distance));//修正一个人物碰撞半径:碰撞法向量的垂直向量
					//Debug.LogErrorFormat("碰撞法向量的垂直向量{0},",(new TSVector(-hit2.normal.z, FP.Zero, hit2.normal.x) * (FP)1f).ToString());
				}
				return;
			}
			else
			{
				//Debug.LogErrorFormat("IsNearlyZero=={0},{1},", hit2.normal.IsNearlyZero(), hit2.normal.sqrMagnitude);
			}
		}
		AllTSTransform.position = hit.point;
		AllTSTransform.Translate(hit.normal * (FP)1f);//修正一个人物碰撞半径:碰撞法向量的垂直向量

	}
	public override void Skill_2(params object[] param)  //范围箭
	{
		int inputAngleX = Convert.ToInt32(param[0]);
		int inputAngleY = Convert.ToInt32(param[1]);
		int PlayAnimationFrame = Convert.ToInt32(param[2]);
		TSVector mTSVector = new TSVector((FP)inputAngleX / 1000, FP.Zero, (FP)inputAngleY / 1000);
		int Range = 9;
		WillUsedPrefabs[2].SetActive(true);
		TrueSyncManager.SyncedInstantiate(WillUsedPrefabs[2], new TSVector(AllTSTransform.position.x, 0, AllTSTransform.position.z) + mTSVector * Range, RotateTSTransform.rotation);
		WillUsedPrefabs[2].SetActive(false);
		/***
		if (PlayAnimationFrame == 0)
		{
			WillUsedPrefabs[2] = _AssetManager.GetGameObject("magical/fx/dark_area_prefab");
			TSTransform dark_area_prefab_AllTSTransform = WillUsedPrefabs[2].GetComponent<TSTransform>();
			dark_area_prefab_AllTSTransform.position = new TSVector(AllTSTransform.position.x, 0, AllTSTransform.position.z);
			
			dark_area_prefab_AllTSTransform.Translate(mTSVector * Range);
			dark_area_prefab_AllTSTransform.OnUpdate();
		}
		else {
			TSTransform dark_area_prefab_AllTSTransform = WillUsedPrefabs[2].GetComponent<TSTransform>();
			int Range = 3;
			dark_area_prefab_AllTSTransform.Translate(mTSVector * Range);
			dark_area_prefab_AllTSTransform.OnUpdate();
		}
		***/
		Debug.LogErrorFormat("Skill_2范围箭==========>{0},{1}", mTSVector, PlayAnimationFrame);
	}
	public override void Skill_3(params object[] param)  //远程箭
	{
		int inputAngleX = Convert.ToInt32(param[0]);
		int inputAngleY = Convert.ToInt32(param[1]);
		TSVector mTSVector;
		if (inputAngleX == 0 && inputAngleY == 0)
			mTSVector = new TSVector(Angle.x, FP.Zero, Angle.z);
		else
			mTSVector = new TSVector((FP)inputAngleX / 1000, FP.Zero, (FP)inputAngleY / 1000);
		//houyiBullet houyibullethouyiBullet = WillUsedPrefabs[1].GetComponent<houyiBullet>();
		//houyibullethouyiBullet.ownerIndex = (int)Id;
		//houyibullethouyiBullet.AllTSTransform.position = new TSVector(AllTSTransform.position.x, 1, AllTSTransform.position.z);
		//houyibullethouyiBullet.RotateTSTransform.rotation = TSQuaternion.LookRotation(mTSVector);
		//houyibullethouyiBullet.Angle = mTSVector;
		Debug.LogErrorFormat("Skill_3远程箭==========>{0},{1},{2},OwnerID={3}", inputAngleX, inputAngleY, mTSVector, OwnerID);
		GameObject realObj = TrueSyncManager.SyncedInstantiate(WillUsedPrefabs[1], new TSVector(AllTSTransform.position.x, 1, AllTSTransform.position.z), TSQuaternion.identity);
		realObj.SetActive(true);
		houyiBullet houyibullethouyiBullet2 = realObj.GetComponent<houyiBullet>();
		houyibullethouyiBullet2.RotateTSTransform.rotation = TSQuaternion.LookRotation(mTSVector);
		houyibullethouyiBullet2.Angle = mTSVector;
		
	}
	public override void Skill_4(params object[] param)  //普攻箭
	{
		Actor mTargetActor = param[0] as Actor;
		GameObject houyibasebullet_prefab = TrueSyncManager.SyncedInstantiate(WillUsedPrefabs[3], new TSVector(AllTSTransform.position.x, 0, AllTSTransform.position.z), TSQuaternion.identity);
		houyibasebullet_prefab.SetActive(true);
		houyiBaseBullet mYeGuaiBaseBullet = houyibasebullet_prefab.GetComponent<houyiBaseBullet>();
		mYeGuaiBaseBullet.mTargetEnemyActor = mTargetActor;
		Debug.LogErrorFormat("Skill_4普攻箭==========>OwnerID={0},TargetOwnerID={1}", OwnerID, mTargetActor.OwnerID);
	}
	//================================技能本地操作相关===========================================
    protected override void InitSkill()
    {
        SkillControlType_1 = SkillControlType.Button_KeyUp;
        SkillControlType_2 = SkillControlType.Joy_Angle;
        SkillControlType_3 = SkillControlType.Joy_Angle;
        SkillControlType_4 = SkillControlType.Button_KeyUp;
        GameObject splat_1 = _AssetManager.GetGameObject("werewolf/statusindicators/prefabs/point/point basic_prefab");
        splat_1.name = "Point";
        splat_1.transform.SetParent(splatManager.transform);
        GameObject splat_2 = _AssetManager.GetGameObject("werewolf/statusindicators/prefabs/linemissile/line missile basic 2_prefab");
        splat_2.name = "Line";
        splat_2.transform.SetParent(splatManager.transform);
        GameObject splat_3 = _AssetManager.GetGameObject("werewolf/statusindicators/prefabs/status/status dots_prefab");
        splat_3.name = "Status";
        splat_3.transform.SetParent(splatManager.transform);
        GameObject splat_4 = _AssetManager.GetGameObject("werewolf/statusindicators/prefabs/range/rangebasic_prefab");
        splat_4.name = "Range";
        splat_4.transform.SetParent(splatManager.transform);
    }
	//=========================================
	protected override void onUp_Skill_1()
	{
		splatManager.CancelAll();
		Debug.LogErrorFormat("onUp_Skill_1==========>{0}", SkillControlType_1);
		if (SkillControlType_1 == SkillControlType.Button_KeyUp)
			_UdpSendManager.SendInputSkill(1, InputType.KeyUp);
	}
	protected override void onDown_Skill_1()
	{
		splatManager.SelectStatusIndicator("Status");
		Debug.LogErrorFormat("onDown_Skill_1==========>{0}", SkillControlType_1);
		if (SkillControlType_1 == SkillControlType.Button_KeyDown)
			_UdpSendManager.SendInputSkill(1, InputType.KeyDown);
	}
	//=========================================
	protected override void StartMoveCallBack_Skill_2()
	{
		image_quxiao.SetActive(true);
		Debug.LogErrorFormat("StartMoveCallBack_Skill_2==========>");
		splatManager.SelectSpellIndicator("Point");
	}
	protected override void MoveCallBack_Skill_2(Vector2 tVec2)
	{
		if (IsInQuXiaoArea(Input.mousePosition))
			image_quxiao2.SetActive(true);
		else
			image_quxiao2.SetActive(false);
		SpellIndicator mCone = splatManager.GetSpellIndicator("Point");
		mCone.JoystickVector = new Vector3(tVec2.x, 0, tVec2.y);
		//Debug.LogErrorFormat("MoveCallBack_Skill_2==========>{0}", tVec2);
	}
	protected override void EndMoveCallBack_Skill_2(Vector2 tVec2)
	{
		image_quxiao.SetActive(false);
		image_quxiao2.SetActive(false);
		SpellIndicator mCone = splatManager.GetSpellIndicator("Point");
		if (!IsInQuXiaoArea(tVec2))
			_UdpSendManager.SendAngleSkill(2, Convert.ToInt32(mCone.JoystickVector.x * 1000), Convert.ToInt32(mCone.JoystickVector.z * 1000));
		splatManager.CancelAll();
		Debug.LogErrorFormat("EndMoveCallBack_Skill_2==========>{0},在取消区域内:{1}", tVec2, IsInQuXiaoArea(tVec2));
	}
	//=========================================
	protected override void StartMoveCallBack_Skill_3()
	{
		image_quxiao.SetActive(true);
		splatManager.SelectSpellIndicator("Line");
		Debug.LogErrorFormat("StartMoveCallBack_Skill_3==========>");
	}
	protected override void MoveCallBack_Skill_3(Vector2 tVec2)
	{
		if (IsInQuXiaoArea(Input.mousePosition))
			image_quxiao2.SetActive(true);
		else
			image_quxiao2.SetActive(false);
		SpellIndicator mCone = splatManager.GetSpellIndicator("Line");
		mCone.JoystickVector = new Vector3(tVec2.x, 0, tVec2.y);
		//Debug.LogErrorFormat("MoveCallBack_Skill_3==========>{0}", tVec2);
	}
	protected override void EndMoveCallBack_Skill_3(Vector2 tVec2)
	{
		image_quxiao.SetActive(false);
		image_quxiao2.SetActive(false);
		SpellIndicator mCone = splatManager.GetSpellIndicator("Line");
		if (!IsInQuXiaoArea(tVec2))
			_UdpSendManager.SendAngleSkill(3, Convert.ToInt32(mCone.JoystickVector.x * 1000), Convert.ToInt32(mCone.JoystickVector.z * 1000));
		splatManager.CancelAll();
		Debug.LogErrorFormat("EndMoveCallBack_Skill_3==========>{0},在取消区域内:{1}", tVec2, IsInQuXiaoArea(tVec2));
	}
	//=========================================
	protected override void onUp_Skill_4()
	{
		splatManager.CancelAll();
		if (SkillControlType_4 == SkillControlType.Button_KeyUp)
		{
			int TargetID = SelectTargetActor(SelectTargetTactics.ENEMY_ALL_NEAREST_DISTANCE,10);
			//Debug.LogErrorFormat("onUp_Skill_4==========>{0}", TargetID);
			_UdpSendManager.SendInputSkill(4, InputType.KeyUp, TargetID);
		}
	}
	protected override void onDown_Skill_4()
	{
		splatManager.SelectRangeIndicator("Range");
		if (SkillControlType_4 == SkillControlType.Button_KeyDown)
			_UdpSendManager.SendInputSkill(4, InputType.KeyDown);
	}
	
}
