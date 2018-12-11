using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using TrueSync;

//挂在Actor身上,管理这个PlayerActor身上所有的buff
//最主要功能： 
//1.添加buff 
//2.执行buff 
//3.buff移除buff 
public class ActorBuffManager {
    [SerializeField]
    private List<ActorBuff> _BuffList = new List<ActorBuff>();

    public void OnUpdate()
    {
        for (int i = _BuffList.Count - 1; i >= 0; i--)
        {
            _BuffList[i].OnFrame();
            if (_BuffList[i].IsFinsh)
            {
                _BuffList[i].CloseBuff();
                _BuffList.Remove(_BuffList[i]);
            }
        }
    }

    /// <summary>
    /// 添加buff
    /// </summary>
    /// <param name="buffData"></param>
    public void AddBuff(ActorBuff buffData)
    {
        if (!_BuffList.Contains(buffData))
        {
            _BuffList.Add(buffData);
            buffData.StartBuff();
        }
    }

    /// <summary>
    /// 移除buff
    /// </summary>
    /// <param name="buffDataID"></param>
    public void RemoveBuff(int buffDataID)
    {
        ActorBuff bd = GetBuff(buffDataID);
        if(bd!=null)
            bd.CloseBuff();
    }

    /// <summary>
    /// 移除buff
    /// </summary>
    /// <param name="buffData"></param>
    public void RemoveBuff(ActorBuff buffData)
    {
        if (_BuffList.Contains(buffData))
        {
            buffData.CloseBuff();
        }
    }

    /// <summary>
    /// 获取buff
    /// </summary>
    /// <param name="buffDataID"></param>
    /// <returns></returns>
    public ActorBuff GetBuff(int buffDataID)
    {
        for (int i = 0; i < _BuffList.Count; i++)
        {
            if (_BuffList[i].buffDataID == buffDataID)
                return _BuffList[i];
        }
        return null;
    }

    /// <summary>
    /// 获取buff
    /// </summary>
    /// <param name="buffBaseID"></param>
    /// <returns></returns>
    public ActorBuff GetBuffByBaseID(int buffBaseID)
    {
        for (int i = 0; i < _BuffList.Count; i++)
        {
            if (_BuffList[i].BuffID == buffBaseID)
                return _BuffList[i];
        }
        return null;
    }

    /// <summary>
    /// 获取buff
    /// </summary>
    /// <param name="buffEffectType"></param>
    /// <returns></returns>
    public ActorBuff[] GetBuff(BuffEffectType buffEffectType)
    {
        List<ActorBuff> buffdatas = new List<ActorBuff>();
        for (int i = 0; i < _BuffList.Count; i++)
        {
            if (_BuffList[i].buffEffectType == buffEffectType)
                buffdatas.Add(_BuffList[i]);
        }
        return buffdatas.ToArray();
    }

    /// <summary>
    /// 执行buff
    /// </summary>
    /// <param name="buffID"></param>
    public void DoBuff(Actor mActor, int buffID)
    {
        BuffBaseManager.Instance.DoBuff(mActor, buffID);
    }

}

[Serializable]
public class ActorBuff
{
    /// <summary>
    /// 缓存栈
    /// </summary>
    private static Stack<ActorBuff> poolCache = new Stack<ActorBuff>();
    /// <summary>
    /// BuffData下一个ID
    /// </summary>
    public static int buffIndex { get; private set; }
    /// <summary>
    /// ID
    /// </summary>
    public int buffDataID;
    /// <summary>
    /// 配置表ID
    /// </summary>
    public int BuffID;
    /// <summary>
    /// buff类型
    /// </summary>
    public BuffEffectType buffEffectType;
    /// <summary>
    /// 叠加类型
    /// </summary>
    public BuffOverlapType buffOverlapType = BuffOverlapType.AddLayer;
    /// <summary>
    /// 执行次数
    /// </summary>
    public BuffRepeatType buffRepeatType = BuffRepeatType.Loop;
    /// <summary>
    /// 关闭类型
    /// </summary>
    public BuffShutDownType buffShutDownType = BuffShutDownType.All;
    /// <summary>
    /// 最大限制
    /// </summary>
    public int MaxLimit;
    /// <summary>
    /// 当前数据
    /// </summary>
    [SerializeField]
    private int _Limit;
    public int GetLimit { get { return _Limit; } }
    /// <summary>
    /// 设置的总共持续时间(单位:帧)
    /// </summary>
    [SerializeField]
    private int TotalFrame;
    public int GetTotalFrame { get { return TotalFrame; } }
    /// <summary>
    /// 当前时间(单位:帧)
    /// </summary>
    [SerializeField]
    private int _CurFrame;
    /// <summary>
    /// 事件参数
    /// </summary>
    public object Data;
    /// <summary>
    /// 设置的间隔时间(单位:帧)
    /// </summary>
    public int CallIntervalFrame { get; set; }
    /// <summary>
    /// 当前间隔时间(单位:帧)
    /// </summary>
    private int _CurCallIntervalFrame { get; set; }
    /// <summary>
    /// 执行次数
    /// </summary>
    [SerializeField]
    private int index = 0;
    /// <summary>
    /// 用来寄存实际增减的数值
    /// </summary>
    public FP TempNum;
    /// <summary>
    /// 根据 CallIntervalFrame 间隔 调用 结束时会调用一次 会传递 Data数据
    /// </summary>
    public Action<object> OnCallBackParam;
    /// <summary>
    ///   /// <summary>
    /// 根据 CallIntervalFrame 间隔 调用 结束时会调用一次 会传递 Data数据 int 次数
    /// </summary>
    /// </summary>
    public Action<object, int> OnCallBackParamIndex;
    /// <summary>
    /// 根据 CallIntervalFrame 间隔 调用 结束时会调用一次
    /// </summary>
    public Action OnCallBack;
    /// <summary>
    /// 根据 CallIntervalFrame 间隔 调用 结束时会调用一次 int 次数
    /// </summary>
    public Action<int> OnCallBackIndex;

    /// <summary>
    /// 当改变时间
    /// </summary>
    public Action<ActorBuff> OnChagneTotalFrame;
    /// <summary>
    /// 当添加层
    /// </summary>
    public Action<ActorBuff> OnAddLayer;
    /// <summary>
    /// 当删除层
    /// </summary>
    public Action<ActorBuff> OnSubLayer;
    /// <summary>
    /// 开始调用
    /// </summary>
    public Action OnStart;
    /// <summary>
    /// 结束调用
    /// </summary>
    public Action OnFinsh;
    [SerializeField]
    private bool _isFinsh;

    /// <summary>
    /// 记录本buff生成的各类特效Obj
    /// </summary>
    public GameObject EffectObject;

    /// <summary>
    /// 构造方法
    /// </summary>
    private ActorBuff() {
        buffDataID = buffIndex++;
        CallIntervalFrame = 1;
        TotalFrame = 0;
    }

    private ActorBuff(int totalFrame, Action onCallBack)
    {

        TotalFrame = totalFrame;
        OnCallBack = onCallBack;
        buffDataID = buffIndex++;
    }

    /// <summary>
    /// 重置时间
    /// </summary>
    public void ResetTime()
    {
        _CurFrame = 0;
    }

    /// <summary>
    /// 修改 时间
    /// </summary>
    /// <param name="time"></param>
    public void ChangeTotalFrame(int totalFrame)
    {
        TotalFrame = totalFrame;
        if (TotalFrame >= MaxLimit)
            TotalFrame = MaxLimit;
        if (OnChagneTotalFrame != null)
            OnChagneTotalFrame(this);
    }

    /// <summary>
    /// 加一层
    /// </summary>
    public void AddLayer()
    {
        _Limit++;
        _CurFrame = 0;
        if (_Limit > MaxLimit)
        {
            _Limit = MaxLimit;
            return;
        }
        if (OnAddLayer != null)
            OnAddLayer(this);
    }

    /// <summary>
    /// 减一层
    /// </summary>
    public void SubLayer()
    {
        _Limit--;
        if (OnSubLayer != null)
            OnSubLayer(this);
    }

    /// <summary>
    /// 开始Buff
    /// </summary>
    public void StartBuff()
    {
        //ChangeLimit(MaxLimit);
        _isFinsh = false;
        if (OnStart != null)
            OnStart();
    }

    /// <summary>
    /// 执行一帧
    /// </summary>
    public void OnFrame()
    {
        _CurFrame += 1;
        //判断时间(单位:帧)是否结束
        if (_CurFrame >= TotalFrame)
        {
            ///调用事件
            CallBack();
            //判断结束类型 为层方式
            if (buffShutDownType == BuffShutDownType.Layer)
            {
                SubLayer();
                //判读层数小于1 则结束
                if (_Limit <= 0)
                {
                    _isFinsh = true;
                    return;
                }
                //重置时间
                _CurCallIntervalFrame = 0;
                _CurFrame = 0;
                return;
            }
            _isFinsh = true;
            return;
        }

        //如果是按间隔时间(单位:帧)调用
        if (CallIntervalFrame > 0)
        {
            _CurCallIntervalFrame += 1;
            if (_CurCallIntervalFrame >= CallIntervalFrame)
            {
                _CurCallIntervalFrame = 0;
                CallBack();
            }
            return;
        }
        ///调用回调
        CallBack();
    }

    /// <summary>
    /// 获取当前执行时间(单位:帧)
    /// </summary>
    public float GetCurFrame
    {
        get { return _CurFrame; }
    }
    /// <summary>
    /// 是否结束
    /// </summary>
    public bool IsFinsh
    {
        get { return _isFinsh; }
    }

    /// <summary>
    /// 调用回调
    /// </summary>
    private void CallBack()
    {
        //次数增加
        index++;
        //判断buff执行次数 
        if (buffRepeatType == BuffRepeatType.Once)
        {
            if (index > 1) { index = 2; return; }
        }

        if (OnCallBack != null)
            OnCallBack();
        if (OnCallBackIndex != null)
            OnCallBackIndex(index);
        if (OnCallBackParam != null)
            OnCallBackParam(Data);
        if (OnCallBackParamIndex != null)
            OnCallBackParamIndex(Data, index);
    }

    /// <summary>
    /// 关闭buff
    /// </summary>
    public void CloseBuff()
    {
        if (OnFinsh != null)
            OnFinsh();
        Clear();
    }

    public void Clear()
    {
        _Limit = 0;
        BuffID = -1;
        index = 0;
        TotalFrame = 0;
        _CurFrame = 0;
        Data = null;
        CallIntervalFrame = 0;
        _CurCallIntervalFrame = 0;
        OnCallBackParam = null;
        OnCallBack = null;
        OnStart = null;
        OnFinsh = null;
        _isFinsh = false;
        Push(this);
    }

    /// <summary>
    /// 创建BuffData
    /// </summary>
    /// <returns></returns>
    public static ActorBuff Create()
    {
        if (poolCache.Count < 1)
            return new ActorBuff();
        ActorBuff buffData = poolCache.Pop();
        return buffData;
    }

    public static ActorBuff Create(BuffBase buffBase,Action onCallBack)
    {
       return Create(buffBase,onCallBack,null,null);
    }

    public static ActorBuff Create(BuffBase buffBase, Action onCallBack,Action<ActorBuff> addLayerAcion,Action<ActorBuff> subLayerAction)
    {
        ActorBuff db = Pop();
        db.buffRepeatType = buffBase.buffRepeatType;
        db.BuffID = buffBase.BuffID;
        db.CallIntervalFrame = buffBase.CallIntervalFrame;
        db.TotalFrame = buffBase.TotalFrame;
        db.buffOverlapType = buffBase.buffOverlapType;
        db.buffShutDownType = buffBase.buffShutDownType;
        db.buffEffectType = buffBase.buffEffectType;
        db.MaxLimit = buffBase.MaxLimit;
        db.OnCallBack = onCallBack;
        db.OnAddLayer = addLayerAcion;
        db.OnSubLayer = subLayerAction;
        db._Limit = 1;
        return db;
    }

    /// <summary>
    /// 弹出
    /// </summary>
    /// <returns></returns>
    private static ActorBuff Pop()
    {
        if (poolCache.Count < 1)
        {
            ActorBuff bd = new ActorBuff();
            return bd;
        }
        ActorBuff buffData = poolCache.Pop();
        return buffData;
    }

    /// <summary>
    /// 压入
    /// </summary>
    /// <param name="buffData"></param>
    private static void Push(ActorBuff buffData)
    {
        poolCache.Push(buffData);
    }

}
