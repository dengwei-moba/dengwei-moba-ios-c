using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using Google.Protobuf;
using Werewolf.StatusIndicators.Components;

public class GameState_Skill_1_yanse : ActorState
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
        if (mActor != null && mActor.ActorObj != null)
        {
            if (mActor.ActorAnimation != null)
            {
                mActor.ActorAnimation.wrapMode = WrapMode.Loop;
                mActor.ActorAnimation.Play("skill2");
            }
        }
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
        }
    }
}

public class PlayerActor_yase : PlayerActor
{
    protected override void InitStateMachine()
    {
        mStateMachineDic[ActorStateType.Idle] = new GameState_Idle_Normal();
        mStateMachineDic[ActorStateType.Move] = new GameState_Move_Normal();
        mStateMachineDic[ActorStateType.Skill_1] = new GameState_Skill_1_yanse();
    }
	//================================技能实现效果相关===========================================
	public override void Skill_1(params object[] param)  //超速移动
	{
		//AllTSTransform.Translate(Angle * Speed * 2);
		//this.Move();
		mActorBuffManager.DoBuff(this, 10002);
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
		Debug.LogErrorFormat("MoveCallBack_Skill_3==========>{0}", tVec2);
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
            _UdpSendManager.SendInputSkill(4, InputType.KeyUp);
    }
	protected override void onDown_Skill_4()
    {
        splatManager.SelectRangeIndicator("Range");
        if (SkillControlType_4 == SkillControlType.Button_KeyDown)
            _UdpSendManager.SendInputSkill(4, InputType.KeyDown);
    }
	
}
