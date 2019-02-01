using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using Google.Protobuf;
using TrueSync;

//用来控制整个游戏进程,怎么玩,游戏规则等
public class GameProcessManager_Dota : TrueSyncBehaviour
{
	public static int MONSTER_ID = -1;
	public static int SOLDIERS_ID = -100000;

	ETCButton FightEndBtn;		//模拟游戏结束的按钮
	public GameObject[] WillUsedPrefabs;
	public Dictionary<int, Actor> mMonsterActorDic = new Dictionary<int, Actor>();
	public Dictionary<int, Actor> mSoldiersActorDic = new Dictionary<int, Actor>();

	private static GameProcessManager_Dota _instance;
	public static GameProcessManager_Dota Instance
	{
		get
		{
			return _instance;
		}
	}

	private int _MonsterID = MONSTER_ID;
	private int NewMonsterID()
	{
		_MonsterID -= 1;
		return _MonsterID;
	}
	private int _SoldiersID = SOLDIERS_ID;
	private int NewSoldiersID()
	{
		_SoldiersID -= 1;
		return _SoldiersID;
	}
	public Actor GetGuaiActor(int iOwnerID)
	{
		if (iOwnerID >= 0) return null;
		if (iOwnerID >= SOLDIERS_ID) return GetMonsterActor(iOwnerID);
		return GetSoldiersActor(iOwnerID);
	}
	//====================================================
	public void AddMonsterActor(int iOwnerID, Actor tActor)
	{
		mMonsterActorDic[iOwnerID] = tActor;
	}

	public void RemoveMonsterActor(int iOwnerID)
	{
		//Destroy(mMonsterActorDic[iOwnerID].gameObject);
		mMonsterActorDic.Remove(iOwnerID);
	}

	public Actor GetMonsterActor(int iOwnerID)
	{
		Actor actor = null;
		mMonsterActorDic.TryGetValue(iOwnerID, out actor);
		return actor;
	}

	public List<Actor> GetMonsterActorList()
	{
		List<Actor> actors = new List<Actor>();
		foreach (KeyValuePair<int, Actor> kv in mMonsterActorDic)
		{
			actors.Add(kv.Value);
		}
		return actors;
	}
	//==================
	public void AddSoldiersActor(int iOwnerID, Actor tActor)
	{
		mSoldiersActorDic[iOwnerID] = tActor;
	}

	public void RemoveSoldiersActor(int iOwnerID)
	{
		//Destroy(mSoldiersActorDic[iOwnerID].gameObject);
		mSoldiersActorDic.Remove(iOwnerID);
	}

	public Actor GetSoldiersActor(int iOwnerID)
	{
		Actor actor = null;
		mSoldiersActorDic.TryGetValue(iOwnerID, out actor);
		return actor;
	}

	public List<Actor> GetEnemySoldiersActorList(GameCamp OwnerCamp)
	{
		if (OwnerCamp == GameCamp.BLUE)
			return GetFriendSoldiersActorList(GameCamp.RED);
		else
			return GetFriendSoldiersActorList(GameCamp.BLUE);
	}

	public List<Actor> GetFriendSoldiersActorList(GameCamp OwnerCamp)
	{
		List<Actor> actors = new List<Actor>();
		foreach (KeyValuePair<int, Actor> kv in mSoldiersActorDic)
		{
			if (kv.Value.OwnerCamp == OwnerCamp) actors.Add(kv.Value);
		}
		return actors;
	}

	public List<Actor> GetAllSoldiersActorList()
	{
		List<Actor> actors = new List<Actor>();
		foreach (KeyValuePair<int, Actor> kv in mSoldiersActorDic)
		{
			actors.Add(kv.Value);
		}
		return actors;
	}
	//====================================================

	void Awake()
	{
		FightEndBtn = GameObject.Find("FightEndBtn").GetComponent<ETCButton>();
		FightEndBtn.onUp.AddListener(onUp_FightEnd);
		InitWillUsedPrefabs();
		_instance = this;
	}

	protected void InitWillUsedPrefabs()
	{
		WillUsedPrefabs = new GameObject[3];
		WillUsedPrefabs[0] = _AssetManager.GetGameObject("prefab/monsters/yeguai_prefab");
		WillUsedPrefabs[0].SetActive(false);
	}
	void onUp_FightEnd()
	{
		_UdpSendManager.SendFightEnd();
		Debug.Log("模拟游戏结束");
	}

	public override void OnSyncedStart()
	{
		Debug.LogErrorFormat("GameProcessManager_Dota====>OnSyncedStart");
		MyTimerDriver.Instance.SetTimer(60, "StartGenerateYeGuai", StartGenerateYeGuai);//new Action(StartGenerateYeGuai)
	}

	private void StartGenerateYeGuai() {
		WillUsedPrefabs[0].SetActive(true);//如果带了刚体等组件的,需要先激活再实例化,不然这些组件无法注册到系统中(子弹这种只有trigger的,可以不先激活)
		YeGuaiAI mYeGuaiAI = WillUsedPrefabs[0].gameObject.GetComponent<YeGuaiAI>();
		Debug.LogErrorFormat("StartGenerateYeGuai====>{0}", TrueSyncManager.Ticks);
		mYeGuaiAI.mBackPosition = new TSVector(3, 0, 3);
		GameObject yeguai_prefab1 = TrueSyncManager.SyncedInstantiate(WillUsedPrefabs[0], new TSVector(3, 0, 3), TSQuaternion.identity);
		YeGuaiAI mYeGuaiAI1 = yeguai_prefab1.GetComponent<YeGuaiAI>();
		mYeGuaiAI1.OwnerID = NewMonsterID();
		AddMonsterActor(mYeGuaiAI1.OwnerID,(Actor)mYeGuaiAI1);

		mYeGuaiAI.mBackPosition = new TSVector(-3, 0, -3);
		GameObject yeguai_prefab2 = TrueSyncManager.SyncedInstantiate(WillUsedPrefabs[0], new TSVector(-3, 0, -3), TSQuaternion.identity);
		YeGuaiAI mYeGuaiAI2 = yeguai_prefab2.GetComponent<YeGuaiAI>();
		mYeGuaiAI2.OwnerID = NewMonsterID();
		AddMonsterActor(mYeGuaiAI2.OwnerID, (Actor)mYeGuaiAI2);

		mYeGuaiAI.mBackPosition = new TSVector(-3, 0, 3);
		GameObject yeguai_prefab3 = TrueSyncManager.SyncedInstantiate(WillUsedPrefabs[0], new TSVector(-3, 0, 3), TSQuaternion.identity);
		YeGuaiAI mYeGuaiAI3 = yeguai_prefab3.GetComponent<YeGuaiAI>();
		mYeGuaiAI3.OwnerID = NewMonsterID();
		AddMonsterActor(mYeGuaiAI3.OwnerID, (Actor)mYeGuaiAI3);

		mYeGuaiAI.mBackPosition = new TSVector(3, 0, -3);
		GameObject yeguai_prefab4 = TrueSyncManager.SyncedInstantiate(WillUsedPrefabs[0], new TSVector(3, 0, -3), TSQuaternion.identity);
		YeGuaiAI mYeGuaiAI4 = yeguai_prefab4.GetComponent<YeGuaiAI>();
		mYeGuaiAI4.OwnerID = NewMonsterID();
		AddMonsterActor(mYeGuaiAI4.OwnerID, (Actor)mYeGuaiAI4);

		WillUsedPrefabs[0].SetActive(false);
	}

	public override void OnSyncedUpdate()
	{
		//Debug.LogErrorFormat("====>Ticks={0},LastSafeTick={1},Time={2},DeltaTime={3}", TrueSyncManager.Ticks, TrueSyncManager.LastSafeTick, TrueSyncManager.Time, TrueSyncManager.DeltaTime);
    }

}