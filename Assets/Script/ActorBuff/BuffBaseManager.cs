using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TrueSync;

//用来配置buff属性(还没有做读取导表,面板上直接配置)
public class BuffBaseManager : ScriptBase
{
    [SerializeField]
    private Dictionary<int, BuffBase> _BuffBaseDict = new Dictionary<int,BuffBase>();

    private static BuffBaseManager _instance;
    public static BuffBaseManager Instance
    {
        get { return _instance; }
    }
    void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        //从导表数据中初始化
        int iRowNum = _DaoBiaoManager.RofBuffTable.RowNum;
        for (int i = 1; i <= iRowNum; i++)
        {
            RofBuffRow mRofBuffRow = _DaoBiaoManager.RofBuffTable.GetDataByRow(i);
            if (mRofBuffRow!=null)
                _BuffBaseDict[mRofBuffRow.ID] = new BuffBase(mRofBuffRow);
        }
    }

    /// <summary>
    /// 执行buff
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="buffID"></param>
    public void DoBuff(Actor mActor, int buffID)
    {
        DoBuff(mActor, GetBuffBase(buffID));
    }

    /// <summary>
    /// 执行buff
    /// </summary>
    /// <param name="mActor"></param>
    /// <param name="buffBase"></param>
    public void DoBuff(Actor mActor, BuffBase buffBase)
    {
        if (buffBase == null) return;
        ActorBuff db = null;

        Debug.Log("DoBuff===>BuffID:" + buffBase.BuffID + ",buffEffectType:" + buffBase.buffEffectType);
        switch (buffBase.buffEffectType)
        {
            case BuffEffectType.AddHp: //增加血量
                if (!IsAdd(mActor, buffBase))
                {
                    db = ActorBuff.Create(buffBase, delegate
                    {
                        mActor.mActorAttr.Hp += buffBase.Num;
                    });

                }
                break;
            case BuffEffectType.AddMaxHp: //增加最大血量
                if (!IsAdd(mActor, buffBase))
                {
                    db = ActorBuff.Create(buffBase, delegate
                    {
                        mActor.mActorAttr.HpMax += buffBase.Num;
                    }, delegate {
                        mActor.mActorAttr.HpMax += buffBase.Num;
                    }, delegate {
                        mActor.mActorAttr.HpMax -= buffBase.Num;
                    });
                }
                break;
            case BuffEffectType.SubHp: //减少血量
                if (!IsAdd(mActor, buffBase))
                { 
                    db = ActorBuff.Create(buffBase, delegate
                    {
                        mActor.mActorAttr.Hp -= buffBase.Num;
                    });
                }
                break;
            case BuffEffectType.SubMaxHp: //减少最大血量
                if (!IsAdd(mActor, buffBase))
                {
                    db = ActorBuff.Create(buffBase, delegate
                    {
                        mActor.mActorAttr.HpMax -= buffBase.Num;
                    }, delegate
                    {
                        mActor.mActorAttr.HpMax -= buffBase.Num;
                    }, delegate
                    {
                        mActor.mActorAttr.HpMax += buffBase.Num;
                    });
                }
                break;
            case BuffEffectType.AddSpeed:
                if (!IsAdd(mActor, buffBase))
                {
                    db = ActorBuff.Create(buffBase, null);
                    db.TempNum = mActor.Speed*buffBase.Num/100;//增减数值是百分比
                    db.OnStart = delegate
                    {
                        Debug.Log("AddSpeed===>OnStart>Speed:" + mActor.Speed + "==>TempNum:" + db.TempNum);
                        mActor.Speed += db.TempNum;
                        if (buffBase.Effect != null) {
                            db.EffectObject = _AssetManager.GetGameObject("prefab/effect/" + buffBase.Effect + "/" + buffBase.Effect + "_prefab");
                            //Effect.name = "Effect_" + buffBase.BuffID;
                            db.EffectObject.transform.position = mActor.AllTSTransform.position.ToVector();
                            db.EffectObject.transform.SetParent(mActor.ActorObj.transform);
                        }
                    };
                    db.OnFinsh = delegate
                    {
                        Debug.Log("AddSpeed===>OnFinsh>Speed:" + mActor.Speed + "==>TempNum:" + db.TempNum);
                        mActor.Speed -= db.TempNum;
                        //GameObject Effect = mActor.ActorObj.transform.FindChild("Effect_" + buffBase.BuffID).gameObject;
                        Destroy(db.EffectObject); 
                        _AssetManager.UnLoadUnUseAsset("prefab/effect/" + buffBase.Effect + "/" + buffBase.Effect + "_prefab");
                    };
                }
                break;
            case BuffEffectType.AddDamageFloated: //浮空
                if (!IsAdd(mActor, buffBase))
                {
                    db = ActorBuff.Create(buffBase, delegate
                    {
                        //if (actor.ActorState != ActorState.DamageRise)
                        //    actor.ActorAttr.DamageRiseAbility = buff.Num;
                        //actor.SetDamageRiseState();
                    });
                }
                break;
            case BuffEffectType.AddFloated:
                if (!IsAdd(mActor, buffBase))
                {
                    db = ActorBuff.Create(buffBase, delegate
                    {
                        //Vector3 moveDir = Vector3.up;
                        //moveDir *= buff.Num;
                        //actor.CharacterController.Move(moveDir*Time.deltaTime);
                    });
                }
                 break;
        }
        if (db != null)
            mActor.mActorBuffManager.AddBuff(db);
    }

    /// <summary>
    /// 玩家是否已经有此buff
    /// </summary>
    /// <param name="mActor"></param>
    /// <param name="buffBase"></param>
    /// <returns></returns>
    private bool IsAdd(Actor mActor,BuffBase buffBase)
    {
        ActorBuff oldBuff = mActor.mActorBuffManager.GetBuffByBaseID(buffBase.BuffID);
        if (oldBuff != null)
        {
            Debug.Log("IsAdd===>BuffID:" + buffBase.BuffID + ",叠加类型:" + buffBase.buffOverlapType);
            switch (buffBase.buffOverlapType)
            {
                case BuffOverlapType.ResetTime:
                    oldBuff.ResetTime();
                    break;
                case BuffOverlapType.AddLayer:
                    oldBuff.AddLayer();
                    break;
                case BuffOverlapType.AddTime:
                    oldBuff.ChangeTotalFrame(oldBuff.GetTotalFrame + buffBase.TotalFrame);
                    break;
                default:
                    break;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获取配置数据
    /// </summary>
    /// <param name="buffID"></param>
    /// <returns></returns>
    public BuffBase GetBuffBase(int buffID)
    {
        if (_BuffBaseDict.ContainsKey(buffID))
            return _BuffBaseDict[buffID];
        return null;
    }
}

/// <summary>
/// buff效果类型
/// </summary>
public enum BuffEffectType
{
    /// <summary>
    /// 恢复HP
    /// </summary>
    AddHp=1,
    /// <summary>
    /// 增加最大血量
    /// </summary>
    AddMaxHp=2,
    /// <summary>
    /// 减血
    /// </summary>
    SubHp=3,
    /// <summary>
    /// 减最大生命值
    /// </summary>
    SubMaxHp=4,
    /// <summary>
    /// 加速
    /// </summary>
    AddSpeed=5,

    /// <summary>
    /// 眩晕
    /// </summary>
    AddVertigo=6,
    /// <summary>
    /// 被击浮空
    /// </summary>
    AddFloated=7,
    /// <summary>
    /// 被击浮空
    /// </summary>
    AddDamageFloated=8,
}

/// <summary>
/// 叠加类型
/// </summary>
public enum BuffOverlapType
{
    None=1,
    /// <summary>
    /// 增加时间
    /// </summary>
    AddTime=2,
    /// <summary>
    /// 堆叠层数
    /// </summary>
    AddLayer=3,
    /// <summary>
    /// 重置时间
    /// </summary>
    ResetTime=4,
}

/// <summary>
/// 关闭类型
/// </summary>
public enum BuffShutDownType
{
    /// <summary>
    /// 关闭所有
    /// </summary>
    All=1,
    /// <summary>
    /// 单层关闭
    /// </summary>
    Layer=2,
}

/// <summary>
/// 执行时重复类型
/// </summary>
public enum BuffRepeatType
{
    /// <summary>
    /// 一次
    /// </summary>
    Once=1,
    /// <summary>
    /// 每次
    /// </summary>
    Loop=2,
}

[System.Serializable]
public class BuffBase       //配置类，这个类用来实现加载buff的配置表信息
{
    /// <summary>
    /// BuffID
    /// </summary>
    public int BuffID;
    /// <summary>
    /// Buff名称
    /// </summary>
    public string BuffName;
    /// <summary>
    /// Buff效果类型
    /// </summary>
    public BuffEffectType buffEffectType;
    /// <summary>
    /// 执行时重复类型
    /// </summary>
    public BuffRepeatType buffRepeatType = BuffRepeatType.Loop;
    /// <summary>
    /// 叠加类型
    /// </summary>
    public BuffOverlapType buffOverlapType = BuffOverlapType.AddLayer;
    /// <summary>
    /// 消除类型
    /// </summary>
    public BuffShutDownType buffShutDownType = BuffShutDownType.All;
    /// <summary>
    /// 如果是堆叠层数，表示最大层数，如果是时间，表示最大时间
    /// </summary>
    public int MaxLimit = 0;
    /// <summary>
    /// 总共持续时间(单位:帧)
    /// </summary>
    public int TotalFrame = 0;
    /// <summary>
    /// 调用间隔时间(单位:帧)
    /// </summary>
    public int CallIntervalFrame = 1;
    /// <summary>
    /// 执行数值 比如加血就是每次加多少
    /// </summary>
    public FP Num;
    /// <summary>
    /// 特效名称
    /// </summary>
    public string Effect;

    public BuffBase(RofBuffRow mRofBuffRow)
    {
        BuffID = mRofBuffRow.ID;
        BuffName = mRofBuffRow.BuffName;
        buffEffectType = (BuffEffectType)mRofBuffRow.BuffEffectType;
        buffRepeatType = (BuffRepeatType)mRofBuffRow.BuffRepeatType;
        buffOverlapType = (BuffOverlapType)mRofBuffRow.BuffOverlapType;
        buffShutDownType = (BuffShutDownType)mRofBuffRow.BuffShutDownType;
        MaxLimit = mRofBuffRow.MaxLimit;
        TotalFrame = mRofBuffRow.TotalFrame;
        CallIntervalFrame = mRofBuffRow.CallIntervalFrame;
        Num = mRofBuffRow.Num;
        Effect = mRofBuffRow.Effect;
    }
}
