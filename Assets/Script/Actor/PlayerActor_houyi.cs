using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using Google.Protobuf;
using TrueSync;

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
    }

    protected override void InitWillUsedPrefabs()
    {
        WillUsedPrefabs = new GameObject[3];
        WillUsedPrefabs[0] = _AssetManager.GetGameObject("prefab/effect/bullet/houyibullet_prefab");
    }

    public override void Skill_1()  //闪现
    {
        TSVector rayOrigin = AllTSTransform.position;
        TSVector rayDirection = this.Angle;
        FP maxDistance = Speed * 50;
        //int layerMask = UnityEngine.Physics.DefaultRaycastLayers;
        int layerMask = LayerMask.GetMask("Wall");
        bool bHit=false;//= TSPhysics.Raycast(rayOrigin, rayDirection, hit, maxDistance, layerMask);
        TSRay ray = new TSRay(rayOrigin, rayDirection);
        TSRaycastHit hit = PhysicsWorldManager.instance.Raycast(ray, maxDistance, layerMask);
        if (hit != null)
        {
            if (hit.distance <= maxDistance)
                bHit= true;
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
            else {
                //Debug.LogErrorFormat("IsNearlyZero=={0},{1},", hit2.normal.IsNearlyZero(), hit2.normal.sqrMagnitude);
            }
        }
        AllTSTransform.position = hit.point;
        AllTSTransform.Translate(hit.normal * (FP)1f);//修正一个人物碰撞半径:碰撞法向量的垂直向量

    }
}
