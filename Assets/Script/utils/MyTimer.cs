using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrueSync;

class MyTimer
{
	public Action CompletedEvent;
	private string _flag;
	public string flag { get { return _flag; } }
	//private int _TotalTick;
	//private int _CompletedTick;
	//private bool _SafeTick = true;
	//private int CurrentTick { get { return _SafeTick ? TrueSyncManager.LastSafeTick : TrueSyncManager.Ticks; } }// 获得当前帧
	public MyTimer(int Tick, string flag, Action completedEvent)
	{
		//_TotalTick = Tick;
		//_CompletedTick = Tick + CurrentTick;
		_flag = flag;
		CompletedEvent = completedEvent;
	}

	//public int LeftTick { get { return (_CompletedTick - CurrentTick); } }
}

//实现轮盘定时器
public class MyTimerDriver : TrueSyncBehaviour
{
	#region 单例
	private static MyTimerDriver _instance;
	public static MyTimerDriver Instance
	{
		get
		{
			if (null == _instance)
			{
				_instance = FindObjectOfType<MyTimerDriver>() ?? new GameObject("MyTimerEntity").AddComponent<MyTimerDriver>();
			}
			return _instance;
		}
	}
	private void Awake()
	{
		_instance = this;
	}
	#endregion

	private Dictionary<int, List<MyTimer>> AllTimerDict = new Dictionary<int, List<MyTimer>>();		//所有定时器:<帧,定时器列表>
	private bool _SafeTick = true;
	private int CurrentTick { get { return _SafeTick ? TrueSyncManager.LastSafeTick : TrueSyncManager.Ticks; } }// 获得当前帧

	/// <summary>
	/// 注册定时器
	/// </summary>
	/// <param name="Tick">定时时长(单位:帧)</param>
	/// <param name="flag">定时器标识符</param>
	/// <param name="completedEvent">结束动作</param>
	public bool SetTimer(int Tick, string flag, Action completedEvent)
	{
		int ExecutTick = CurrentTick + Tick;//将要执行的帧
		if (!AllTimerDict.ContainsKey(ExecutTick))
		{
			List<MyTimer> myTimerList = new List<MyTimer>();
			myTimerList.Add(new MyTimer(Tick, flag, completedEvent));
			AllTimerDict[ExecutTick] = myTimerList;
		}
		else {
			List<MyTimer> myTimerList = AllTimerDict[ExecutTick];
			foreach (var myTimer in myTimerList) {
				if (myTimer.flag.Equals(flag)) {
					//同标记的只能添加一个
					return false;
				}
			}
			myTimerList.Add(new MyTimer(Tick, flag, completedEvent));
		}
		return true;
	}

	/// <summary>
	/// 删除定时器(少做删除操作,暂时用的全遍历方法)
	/// </summary>
	public bool DelTimer(string flag)
	{
		foreach (KeyValuePair<int, List<MyTimer>> kv in AllTimerDict)
		{
			List<MyTimer> myTimerList = kv.Value;
			foreach (var myTimer in myTimerList)
			{
				if (myTimer.flag.Equals(flag))
				{
					myTimerList.Remove(myTimer);
					return true;
				}
			}
		}
		return false;//没有这个定时器
	}

	public override void OnSyncedUpdate()
	{
		if (AllTimerDict.ContainsKey(CurrentTick))
		{
			foreach (var myTimer in AllTimerDict[CurrentTick])
			{
				//Debug.LogErrorFormat("myTimer.flag====>{0}", myTimer.flag);
				myTimer.CompletedEvent();
			}
			AllTimerDict.Remove(CurrentTick);
		}
	}
}