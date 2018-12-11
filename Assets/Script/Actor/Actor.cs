using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Google.Protobuf;
using TrueSync;

public class ActorAttr
{
    public FP Speed { set; get; }
    private FP _hp;
    public FP Hp { 
        set
        {
            if (value>HpMax) 
                _hp = HpMax;
            else
                _hp = value;
        }
        get {
            return _hp;
        }
    }
    private FP _hpmax;
    public FP HpMax
    {
        set
        {
            if (_hpmax > value)
            {
                _hp += value - _hpmax;
                _hpmax = value;
            }
            else if (_hpmax < value)
            {
                _hpmax = value;
                if (_hp > _hpmax)
                    _hp = _hpmax;
            }
        }
        get
        {
            return _hpmax;
        }
    }
}

public abstract class Actor : TrueSyncBehaviour
{
    public bool IsETCControl { set; get; }

    /// <summary>
    /// 这个Actor身上所有的属性
    /// </summary>
    public ActorAttr mActorAttr = new ActorAttr();

    /// <summary>
    /// 这个Actor身上所有的Buff
    /// </summary>
    public ActorBuffManager mActorBuffManager = new ActorBuffManager();

    protected ActorState CurState { set; get; }
    /// <summary>
    /// 所有的状态
    /// </summary>
    protected Dictionary<ActorStateType, ActorState> mStateMachineDic = new Dictionary<ActorStateType, ActorState>();
    /// <summary>
    /// 限制状态跳转字典
    /// </summary>
    protected Dictionary<ActorStateType, List<ActorStateType>> mTransShieldDic = new Dictionary<ActorStateType, List<ActorStateType>>();

    /// <summary>
    /// 初始化状态机
    /// </summary>
    protected abstract void InitStateMachine();
    /// <summary>
    /// 初始化当前状态
    /// </summary>
    protected abstract void InitCurState();
    /// <summary>
    /// 状态机跳转限制
    /// </summary>
    protected abstract void InitStateTransLimit();

    /// <summary>
    /// 初始化可能被频繁用到的预制体(因为常用,让他一直在内存里)
    /// </summary>
    protected abstract void InitWillUsedPrefabs();

    public abstract void PlayerInputHandle(PB_ClientInput input);
    public abstract void PlayerInputHandle_MoveStart();
    public abstract void PlayerInputHandle_MoveAngle(int inputAngleX, int inputAngleY);
    public abstract void PlayerInputHandle_MoveEnd();
    public abstract void PlayerInputHandle_KeyUp(int inputKey);
    public abstract void PlayerInputHandle_KeyDown(int inputKey);
    public abstract void PlayerInputHandle_KeyAngle(int inputKey);
    public abstract void PlayerInputHandle_ClickXY(int inputPosX, int inputPosY);

    public void TransState(ActorStateType tStateType, params object[] param)
    {
        if (CurState == null)
            return;
        if (tStateType == CurState.StateType)
            return;
        else
        {
            ActorState mState = null;
            if (mStateMachineDic.TryGetValue(tStateType, out mState))
            {
                List<ActorStateType> shieldList = null;
                if (mTransShieldDic.TryGetValue(CurState.StateType, out shieldList))
                    if (shieldList.Contains(tStateType))
                        return;
                CurState.Exit(mState);
                CurState = mState;
                CurState.Enter(this, param);
            }
        }
    }

    public uint Id { set; get; }

    /// <summary>
    /// 模型
    /// </summary>
    private GameObject mActorObj;
    public GameObject ActorObj
    {
        get
        {
            if (mActorObj == null)
                mActorObj = transform.Find("rotate/actor").gameObject;
            return mActorObj;
        }
    }

    /***
    private Animator mActorAnimator;
    /// <summary>
    /// 动画状态机控件(多个动画之间相互切换，并且有一个动画控制器，俗称动画状态机)
    /// </summary>
    public Animator ActorAnimator
    {
        get
        {
            if (mActorAnimator == null)
                mActorAnimator = ActorObj.GetComponent<Animator>();
            return mActorAnimator;
        }
    }
    ***/
    private Animation mActorAnimation;
    /// <summary>
    /// 动画播放控件(控制一个动画的播放)
    /// </summary>
    public Animation ActorAnimation
    {
        get
        {
            if (mActorAnimation == null)
                mActorAnimation = ActorObj.GetComponent<Animation>();
            return mActorAnimation;
        }
    }

    private TSTransform mRotateTSTransform;
    public TSTransform RotateTSTransform
    {
        get
        {
            if (mRotateTSTransform == null) {
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
            if (mAllTSTransform == null) {
                mAllTSTransform = GetComponent<TSTransform>();
            }
            return mAllTSTransform;
        }
    }

    public GameObject pengzhuanweizhi;
    public GameObject pengzhuanfaxiangliang1;
    public GameObject pengzhuanfaxiangliang2;
    public GameObject pengzhuanfaxiangliang3;
    void Awake()
    {
        InitStateMachine();
        InitCurState();
        InitStateTransLimit();
        InitWillUsedPrefabs();
        pengzhuanweizhi = _AssetManager.GetGameObject("prefab/effect/bullet/pengzhuanweizhi_prefab");
        pengzhuanfaxiangliang1 = _AssetManager.GetGameObject("prefab/effect/bullet/pengzhuanfaxiangliang_prefab");
        pengzhuanfaxiangliang2 = _AssetManager.GetGameObject("prefab/effect/bullet/pengzhuanfaxiangliang_prefab");
        pengzhuanfaxiangliang3 = _AssetManager.GetGameObject("prefab/effect/bullet/pengzhuanfaxiangliang_prefab");
    }

    public override void OnSyncedUpdate()
    {
        if (CurState != null)
            CurState.OnUpdate();
        mActorBuffManager.OnUpdate();
        AllTSTransform.OnUpdate();
        RotateTSTransform.OnUpdate();
    }

    #region 位置相关
    public bool IsMove { get; set; }

    public FP Speed { 
        set
        {
            mActorAttr.Speed = value;
        }
        get 
        {
            return mActorAttr.Speed;
        }
    }

    public TSVector Angle { set; get; }

    private SortedDictionary<int, TSCollision> _TriggerStayTSCollisions = new SortedDictionary<int, TSCollision>();

    public virtual void Move()
    {
        if (_TriggerStayTSCollisions.Count>0)
        {
            //此算法,不能解决两个阻碍墙夹角少于90的问题
            TSVector AngleOnNormal = Angle;// new TSVector(Angle.x, FP.Zero, Angle.z);
            foreach (KeyValuePair<int, TSCollision> item in _TriggerStayTSCollisions)
            {
                //pengzhuanweizhi.transform.position = item.Value.contacts[0].point.ToVector();
                //pengzhuanfaxiangliang1.transform.position = item.Value.contacts[0].point.ToVector();
                //pengzhuanfaxiangliang2.transform.position = AllTSTransform.position.ToVector();
                //pengzhuanfaxiangliang3.transform.position = item.Value.contacts[0].point.ToVector();
                //pengzhuanfaxiangliang1.transform.rotation = TSQuaternion.LookRotation(item.Value.contacts[0].normal).ToQuaternion();
                TSVector diff = item.Value.contacts[0].point - AllTSTransform.position;
                //pengzhuanfaxiangliang2.transform.rotation = TSQuaternion.LookRotation(diff).ToQuaternion();
                //pengzhuanfaxiangliang2.transform.rotation = TSQuaternion.LookRotation(Angle).ToQuaternion();
                //pengzhuanfaxiangliang1.transform.rotation = TSQuaternion.LookRotation(new TSVector(-z, FP.Zero, x)).ToQuaternion();
                //pengzhuanfaxiangliang3.transform.rotation = TSQuaternion.LookRotation(AngleOnNormal).ToQuaternion();
                FP AngleAndDiff = TSVector2.Angle(new TSVector2(AngleOnNormal.x, AngleOnNormal.z), new TSVector2(diff.x, diff.z));
                //Debug.LogErrorFormat("夹角度{0},控制轮盘的投影{1},控制轮盘{2},Actor与碰撞点向量{3}", AngleAndDiff, AngleOnNormal.ToString(), Angle.ToString(), diff.ToString());
                //Debug.LogErrorFormat("Actor====>Angle==>{0}", TSVector2.Angle(new TSVector2(Angle.x, Angle.z), new TSVector2(item.Value.contacts[0].normal.x, item.Value.contacts[0].normal.z)));
                if (AngleAndDiff < 90)
                {
                    AngleOnNormal = TSVector.Project(AngleOnNormal, new TSVector(-item.Value.contacts[0].normal.z, FP.Zero, item.Value.contacts[0].normal.x));
                    //TmpAngle = AngleOnNormal;//让控制轮盘方向变成修正后的方向AngleOnNormal
                }else{
                    //AngleOnNormal = TmpAngle;//new TSVector(Angle.x, FP.Zero, Angle.z);
                }
                /***
                if (diff.magnitude < (FP)0.9) {
                    TSVector _addTranslation3 = new TSVector(0, 0, 0);
                    if (diff.x < FP.Zero) _addTranslation3.x -= 1;
                    else if (diff.x > FP.Zero) _addTranslation3.x += 1;
                    if (diff.z < FP.Zero) _addTranslation3.z -= 1;
                    else if (diff.z > FP.Zero) _addTranslation3.z += 1;
                    _addTranslation3 = _addTranslation3.normalized;
                    AllTSTransform.position = new TSVector(item.Value.contacts[0].point.x, 0, item.Value.contacts[0].point.z) - _addTranslation3;
                    Debug.LogErrorFormat("修复=================================>{0},{1}", diff.ToString(), _addTranslation3.ToString());
                }
                ***/
            }
            AllTSTransform.Translate(AngleOnNormal * Speed);
        }else{
            AllTSTransform.Translate(Angle * Speed);
        }
    }
    #endregion 位置相关

    #region 技能操作相关
    public virtual void Skill_1()
    {
    }
    public virtual void Skill_2()
    {
    }
    public virtual void Skill_3()
    {
    }
    public virtual void Skill_4()
    {
    }
    #endregion 技能操作相关

    #region TrueSyncBehaviour操作相关
    public override void OnSyncedStart()
    {
        Debug.LogErrorFormat("Actor====>OnSyncedStart");
    }

    /**
    * @brief Tints box's material with gray color when it collides with the ball.
    **/
    public void OnSyncedCollisionEnter(TSCollision other)
    {
        other.gameObject.GetComponent<Renderer>().material.color = Color.gray;
        Debug.LogErrorFormat("Actor====>Collision   Enter==>{0}", other.gameObject.name);
    }

    /**
    * @brief Increases box's local scale by 1% while collision with a ball remains active.
    **/
    public void OnSyncedCollisionStay(TSCollision other)
    {
        //Debug.LogErrorFormat("Actor====>Collision   Stay==>{0}", other.gameObject.name);
    }

    /**
    * @brief Resets changes in box's properties when there is no more collision with the ball.
    **/
    public void OnSyncedCollisionExit(TSCollision other)
    {
        other.gameObject.GetComponent<Renderer>().material.color = Color.blue;
        Debug.LogErrorFormat("Actor====>Collision   Exit==>{0}", other.gameObject.name);
    }

    public void OnSyncedTriggerEnter(TSCollision other)
    {
        Debug.LogErrorFormat("Actor====>Trigger  Enter==>{0}", other.gameObject.name);
        if (other.gameObject.name == "BaseWall")
        {
            //_TriggerStayTSCollision = other;
            _TriggerStayTSCollisions[other.gameObject.GetInstanceID()] = other;
        }
    }

    public void OnSyncedTriggerStay(TSCollision other)
    {
        //Debug.LogErrorFormat("Actor====>Trigger  Stay==>{0}", other.gameObject.name);
        if (other.gameObject.name == "BaseWall")
        {
            //Debug.LogFormat("接触点法线向量:{0},穿透{1}", other.contacts[0].normal.ToString(), other.contacts[0].Penetration);
            //Debug.LogFormat("两个物体间的接触点{0},{1},接触点法线向量{2}", other.contacts[0].point.ToString(), other.contacts[0].point2.ToString(), other.contacts[0].normal.ToString());
            //Debug.LogFormat("Actor====>PositionBack=1=>{0}", AllTSTransform.position.ToString());
            //Debug.LogFormat("Actor====>PositionBack=2=>{0},{1}", other.transform.position.ToString(), AllTSTransform.transform.position.ToString());
            //Debug.LogFormat("Actor====>PositionBack=3=>{0},{1}", other.collider.Shape.BoundingBox.ToString(), tsCollider.Shape.BoundingBox.ToString());
            //Debug.LogFormat("Actor====>PositionBack=4=>{0},{1}", other.collider.bounds.ToString(), tsCollider.bounds.ToString());
            //_TriggerStayTSCollision = other;
            _TriggerStayTSCollisions[other.gameObject.GetInstanceID()] = other;
            //AllTSTransform.PositionBack(other);
        }
    }

    public void OnSyncedTriggerExit(TSCollision other)
    {
        Debug.LogErrorFormat("Actor====>Trigger  Exit==>{0}", other.gameObject.name);
        if (other.gameObject.name == "BaseWall")
        {
            _TriggerStayTSCollisions.Remove(other.gameObject.GetInstanceID());
        }
    }

    #endregion TrueSyncBehaviour操作相关
}
