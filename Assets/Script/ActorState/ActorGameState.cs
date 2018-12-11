using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActorStateType
{
    Idle,
    Move,
    Skill_1,
    Skill_2,
    Skill_3,
    Skill_4,
}

public abstract class ActorState
{
    public abstract ActorStateType StateType { get; }
    public abstract void TryTransState(ActorStateType tStateType);
    public abstract void Enter(params object[] param);
    public abstract void OnUpdate();
    public abstract void Exit(ActorState NextGameState);
}
//====================================================================================================
//只是提供一些基础的通用的ActorGameState
public class GameState_Idle_Normal : ActorState
{
    private Actor mActor;

    public override ActorStateType StateType
    {
        get
        {
            return ActorStateType.Idle;
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
                mActor.ActorAnimation.Play("idle");
            }
        }
    }

    public override void Exit(ActorState NextGameState)
    {
        mActor = null;
    }

    public override void OnUpdate()
    {

    }
}

public class GameState_Move_Normal : ActorState
{
    private Actor mActor;

    public override ActorStateType StateType
    {
        get
        {
            return ActorStateType.Move;
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
                mActor.ActorAnimation.Play("run");
            }
            mActor.IsMove = true;
        }
    }

    public override void Exit(ActorState NextGameState)
    {
        mActor.IsMove = false;
        mActor = null;
    }

    public override void OnUpdate()
    {
        if (mActor != null && mActor.ActorObj != null)
        {
            mActor.Move();
        }
    }
}