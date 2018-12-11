using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ClientGame.Net;
using Google.Protobuf;

namespace TrueSync {
    /// <summary>
    ///@brief Manages creation of player prefabs and lockstep execution.
    ///DV 管理player预制构件的创建和驱动帧同步。 
    /// </summary>
    [AddComponentMenu("")]
    public class TrueSyncManager : ScriptBase {
        //=================原来GameManager的功能===========================================================================================
        private string FRS2C_Host;
        private uint FRS2C_Port;
        private uint guanqia;
        private uint teamid;
        private uint fightroomid;
        private int randomseed;
        private int fps;
        private uint playeridx;
        private string hellokey;

        bool _isLoginGameServer = false;
        bool _isGameStarted = false;
        public bool isGameStarted
        {
            get
            {
                return _isGameStarted;
            }
        }
        private Dictionary<uint, Actor> mActorDic = new Dictionary<uint, Actor>();

        private Transform mActorParent;

        private FrameData mFrameData;
        private LockStep mLockStep;

        public void OnBattleStart()
        {
            _UnityUdpSocket.RegisterServerEvent(PrepGameStart);
            _UnityUdpSocket.RegisterHandler(MsgID.S2CFightStart, OnS2CFightLoadingStart);
            _UnityUdpSocket.RegisterHandler(MsgID.S2CFightEnd, OnS2CFightEnd);
            _UnityUdpSocket.ConnectToServer(FRS2C_Host, (int)FRS2C_Port);
        }
        /// <summary>
        /// 网络连接成功，登录操作
        /// </summary>
        /// <param name="eType"></param>
        public void PrepGameStart(NetworkEvent eType)
        {
            Debug.LogFormat("eType={0}", eType);
            if (eType == NetworkEvent.connected)
            {

                PB_C2SLoginHello msg = new PB_C2SLoginHello();
                msg.Fightroomid = fightroomid;
                msg.Hellokey = hellokey;
                Debug.LogFormat("PrepGameStart send {0} to serve {1} {2}", MsgID.C2SLoginHello, msg.Fightroomid, msg.Hellokey);
                _UnityUdpSocket.Send(MsgID.C2SLoginHello, msg);
                _isLoginGameServer = true;
                //假装加载完毕
                PB_C2SFightLoadingEnd loadEnd = new PB_C2SFightLoadingEnd();
                _UnityUdpSocket.Send(MsgID.C2SFightLoadingEnd, loadEnd);

            }
            else if (eType == NetworkEvent.connectFail)
            {
                Debug.LogErrorFormat("连接战斗服务器失败");
            }
            else
            {

            }
        }

        /// <summary>
        /// 所有人已客户端资源加载完成,服务器通知一起战斗开始
        /// </summary>
        /// <param name="pack"></param>
        public void OnS2CFightLoadingStart(KcpNetPack pack)
        {
            _isGameStarted = true;
            _UnityUdpSocket.UnregisterHandler(MsgID.S2CFightStart, OnS2CFightLoadingStart);
            lockstep.CheckGameStart();
            Debug.LogFormat("Battle Started!");
        }

        /// <summary>
        /// 战斗失败/胜利后退出战斗,及结算信息
        /// </summary>
        void OnS2CFightEnd(KcpNetPack pack)
        {
            FightEnd();
        }
        public void FightEnd()
        {
            _UnityUdpSocket.DisconnectServer();
            _UnityUdpSocket.UnregisterServerEvent(PrepGameStart);
            _UnityUdpSocket.UnregisterHandler(MsgID.S2CFightEnd, OnS2CFightEnd);

            FRS2C_Port = 0;
            FRS2C_Host = string.Empty;
            guanqia = 0;
            teamid = 0;
            fightroomid = 0;
            playeridx = 0;
            hellokey = string.Empty;
            _isLoginGameServer = false;
        }
        //====================================================
        public void AddActor(uint tUserid, Actor tActor)
        {
            mActorDic[tUserid] = tActor;
        }

        public void RemoveActor(uint tUserid)
        {
            Destroy(mActorDic[tUserid].gameObject);
            mActorDic.Remove(tUserid);
        }

        public Actor GetActor(uint tUserid)
        {
            Actor actor = null;
            mActorDic.TryGetValue(tUserid, out actor);
            return actor;
        }
        //====================================================
        public void AddOneFrame(uint frameindex, List<PB_PlayerFrame> list)
        {
            mFrameData.AddOneFrame(frameindex, list);
        }

        public bool LockFrameTurn(ref List<PB_PlayerFrame> list)
        {
            return mFrameData.LockFrameTurn(ref list);
        }

        public void SetFaseForward(int tValue)
        {
            mLockStep.SetFaseForward(tValue);
        }
        //====================================================
        void OnDestroy()
        {
            instance = null;
            _UnityUdpSocket.Close();
        }

        //=================原来TrueSyncManager的功能===========================================================================================
        private const float JitterTimeFactor = 0.001f; //为了避免浮动点数比较造成误差

        private const string serverSettingsAssetFile = "TrueSyncGlobalConfig";

        /// <summary>
        ///DV 行为初始化 ,第一次更新 ,已起动 
        ///启动状态, 在Update决定什么时候运行lockstep.RunSimulation(true);
        /// </summary>
        private enum StartState { BEHAVIOR_INITIALIZED, FIRST_UPDATE, STARTED };//

        private StartState startState;

        ///// <summary> 
        /////@brief Player prefabs to be instantiated in each machine.
        /////DV 在每个机器中实例化Player预设列表 
        ///// </summary>
        //public GameObject[] playerPrefabs;

        public static TrueSyncConfig _TrueSyncGlobalConfig;
        /// <summary>
        ///DV 同步全局配置 
        /// </summary>
        public static TrueSyncConfig TrueSyncGlobalConfig {
            get {
                if (_TrueSyncGlobalConfig == null) {
                    _TrueSyncGlobalConfig = (TrueSyncConfig) Resources.Load(serverSettingsAssetFile, typeof(TrueSyncConfig));
                }

                return _TrueSyncGlobalConfig;
            }
        }

        public static TrueSyncConfig TrueSyncCustomConfig = null;

        public TrueSyncConfig customConfig;

        private Dictionary<int, List<GameObject>> gameOjectsSafeMap = new Dictionary<int, List<GameObject>>();

        /// <summary>
        ///@brief Instance of the lockstep engine.
        ///DV lockstep引擎的实例。 锁帧
        /// </summary>
        private AbstractLockstep lockstep;

        /// <summary>
        ///DV 帧同步一帧的时间
        /// </summary>
        private FP lockedTimeStep;

        /// <summary>
        ///@brief A list of {@link TrueSyncBehaviour} not linked to any player.
        ///DV 没有链接到任何player的{@link TrueSyncBehaviour}列表。 
        ///非哪个玩家拥有的行为，叫做普通行为
        /// </summary>
        private List<TrueSyncManagedBehaviour> generalBehaviours = new List<TrueSyncManagedBehaviour>();

        /// <summary>
        ///@brief A dictionary holding a list of {@link TrueSyncBehaviour} belonging to each player.
        ///DV 持有属于每个player的{@link TrueSyncBehaviour}的字典 
        ///对应玩家所拥有的行为
        /// </summary>
        private Dictionary<byte, List<TrueSyncManagedBehaviour>> behaviorsByPlayer;

        /// <summary>
        ///@brief The coroutine scheduler.
        ///DV 协程调度 
        /// </summary>
        private CoroutineScheduler scheduler;

        /// <summary>
        ///@brief List of {@link TrueSyncBehaviour} that should be included next update.
        ///DV {@link TrueSyncBehaviour}的列表，应该包含在下一个更新中。
        ///一个临时缓存池，用在后面注册行为的时候占存。
        ///在帧更新的时候OnStepUpdate调用CheckQueuedBehaviours。分配玩家拥有者和调OnSyncedStart。然后就清理该列表
        /// </summary>
        private List<TrueSyncManagedBehaviour> queuedBehaviours = new List<TrueSyncManagedBehaviour>();

        /// <summary>
        ///DV 保存了所有行为的字典 
        /// </summary>
        private Dictionary<ITrueSyncBehaviour, TrueSyncManagedBehaviour> mapBehaviorToManagedBehavior = new Dictionary<ITrueSyncBehaviour, TrueSyncManagedBehaviour>();

        // 时间
        private FP time = 0;

        /// <summary>
        ///@brief Returns the deltaTime between two simulation calls.
        ///DV 返回两个模拟调用之间的DeltTime 
        ///帧时间
        /// </summary>
        public static FP DeltaTime {
            get {
                if (instance == null) {
                    return 0;
                }

                return instance.lockedTimeStep;
            }
        }

        /// <summary>
        ///@brief Returns the time elapsed since the beginning of the simulation.
        ///DV 返回从模拟开始以来所经过的时间。 
        /// </summary>
        public static FP Time {
            get {
                if (instance == null || instance.lockstep == null) {
                    return 0;
                }

                return instance.time;
            }
        }

        /// <summary>
        ///@brief Returns the number of the last simulated tick.
        ///DV 返回最后模拟tick的数量 
        ///帧
        /// </summary>
        public static int Ticks {
            get {
                if (instance == null || instance.lockstep == null) {
                    return 0;
                }

                return instance.lockstep.Ticks;
            }
        }

        /// <summary>
        ///@brief Returns the last safe simulated tick.
        ///DV 返回最后安全模拟tick 
        /// </summary>
        public static int LastSafeTick {
            get {
                if (instance == null || instance.lockstep == null) {
                    return 0;
                }

                return instance.lockstep.LastSafeTick;
            }
        }

        ///// <summary> 
        /////@brief Returns the simulated gravity.
        /////DV 返回模拟重力 
        ///// </summary>
        //public static TSVector Gravity {
        //    get {
        //        if (instance == null) {
        //            return TSVector.zero;
        //        }

        //        return instance.ActiveConfig.gravity3D;
        //    }
        //}

        ///// <summary> 
        /////@brief Returns the list of players connected.
        /////DV 返回已连接的玩家列表。 
        ///// </summary>
        //public static List<TSPlayerInfo> Players {
        //    get {
        //        if (instance == null || instance.lockstep == null) {
        //            return null;
        //        }

        //        List<TSPlayerInfo> allPlayers = new List<TSPlayerInfo>();
        //        foreach (TSPlayer tsp in instance.lockstep.Players.Values) {
        //            if (!tsp.dropped) {
        //                allPlayers.Add(tsp.playerInfo);
        //            }
        //        }

        //        return allPlayers;
        //    }
        //}

        ///// <summary> 
        /////@brief Returns the local player.
        /////DV 本地玩家
        ///// </summary>
        //public static TSPlayerInfo LocalPlayer {
        //    get {
        //        if (instance == null || instance.lockstep == null) {
        //            return null;
        //        }

        //        return instance.lockstep.LocalPlayer.playerInfo;
        //    }
        //}

        /// <summary> 
        ///@brief Returns the active {@link TrueSyncConfig} used by the {@link TrueSyncManager}.
        /// </summary>
        public static TrueSyncConfig Config {
            get {
                if (instance == null) {
                    return null;
                }

                return instance.ActiveConfig;
            }
        }

        private static TrueSyncManager instance;

        public static TrueSyncManager Instance {
            get {
                return instance;
            }       
        }

        private TrueSyncConfig ActiveConfig {
            get {
                if (TrueSyncCustomConfig != null) {
                    customConfig = TrueSyncCustomConfig;
                    TrueSyncCustomConfig = null;
                }

                if (customConfig != null) {
                    return customConfig;
                }

                return TrueSyncGlobalConfig;
            }
        }
        //=================================================================================================================
        void Awake() {
            TrueSyncConfig currentConfig = ActiveConfig;
            lockedTimeStep = currentConfig.lockedTimeStep;

            // 初始化状态跟踪
            StateTracker.Init(currentConfig.rollbackWindow);
            // 初始化随机数
            TSRandom.Init(randomseed);

            // 初始化物理管理器
            if (currentConfig.physics2DEnabled || currentConfig.physics3DEnabled) {
                PhysicsManager.New(currentConfig);
                PhysicsManager.instance.LockedTimeStep = lockedTimeStep;
                PhysicsManager.instance.Init();
            }
            // 跟踪 时间
            StateTracker.AddTracking(this, "time");
        }

        void Start() {
            instance = this;
            Application.runInBackground = true;

            ICommunicator communicator = null;
            //初始化通信
            //if (!PhotonNetwork.connected || !PhotonNetwork.inRoom) {
            //    Debug.LogWarning("You are not connected to Photon. TrueSync will start in offline mode.");
            //} else {
            //    communicator = new PhotonTrueSyncCommunicator(PhotonNetwork.networkingPeer);
            //}

            TrueSyncConfig activeConfig = ActiveConfig;
            //创建lockstep
            lockstep = AbstractLockstep.NewInstance(
                lockedTimeStep.AsFloat(),
                communicator,
                PhysicsManager.instance,
                activeConfig.syncWindow,
                activeConfig.panicWindow,
                activeConfig.rollbackWindow,
                OnGameStarted,
                OnGamePaused,
                OnGameUnPaused,
                OnGameEnded,
                OnPlayerDisconnection,
                OnStepUpdate,
                ProvideInputData
            );
            //==========================================================================
            mFrameData = new FrameData();
            mLockStep = gameObject.AddComponent<LockStep>();

            PB_MatchTeamFight_FMS2GS2C mMatchTeamFight = (PB_MatchTeamFight_FMS2GS2C)NetData.Instance.Query(MsgID.S2CMatch, (uint)MatchMsgID.Fms2Gs2CMatchFight);
            FRS2C_Host = mMatchTeamFight.Frs2Chost;
            FRS2C_Port = mMatchTeamFight.Frs2Cport;
            guanqia = mMatchTeamFight.Guanqia;
            teamid = mMatchTeamFight.Teamid;
            fightroomid = mMatchTeamFight.Fightroomid;
            randomseed = mMatchTeamFight.Seed;
            fps = mMatchTeamFight.Fps;
            foreach (PB_FightPlayerInfo mFightPlayerInfo in mMatchTeamFight.Playersdata)
            {
                GameObject actorobj;
                Actor actor;
                if (mFightPlayerInfo.ChooseHero == 1)
                {
                    actorobj = _AssetManager.GetGameObject("prefab/hero/yase/yase_prefab");
                    actor = actorobj.GetComponent<PlayerActor_yase>();
                }
                else
                {
                    actorobj = _AssetManager.GetGameObject("prefab/hero/houyi/houyi_prefab");
                    actor = actorobj.GetComponent<PlayerActor_houyi>();
                }
                //actor.ownerIndex =(int) mFightPlayerInfo.Playeridx;
                if (mFightPlayerInfo.Pid == NetData.Instance.PlayerID)
                {
                    actor.IsETCControl = true;
                    playeridx = mFightPlayerInfo.Playeridx;
                    hellokey = mFightPlayerInfo.Hellokey;
                    //actor.localOwner = new TSPlayerInfo((byte)mFightPlayerInfo.Pid, mFightPlayerInfo.Name);
                }
                else
                {
                    actor.IsETCControl = false;
                    //actor.owner = new TSPlayerInfo((byte)mFightPlayerInfo.Pid, mFightPlayerInfo.Name);
                }
                if (mActorParent == null)
                    mActorParent = GameObject.Find("ActorParent").transform;
                actor.transform.parent = mActorParent;
                //actor.Position = new CustomVector3(0, 0, 0);
                actor.Speed = (FP)0.1f;
                actor.RotateTSTransform.LookAt(TSVector.left);
                actor.AllTSTransform.LookAt(TSVector.left);
                actor.Angle = new TSVector();
                actor.Id = mFightPlayerInfo.Playeridx;
                AddActor(mFightPlayerInfo.Playeridx, actor);
            }
            OnBattleStart();
            //==========================================================================
            //检测是否是录像模式, 如果是就加载录像
            //if (ReplayRecord.replayMode == ReplayMode.LOAD_REPLAY) {
            //    ReplayPicker.replayToLoad.Load();

            //    ReplayRecord replayRecord = ReplayRecord.replayToLoad;
            //    if (replayRecord == null) {
            //        Debug.LogError("Replay Record can't be loaded");
            //        gameObject.SetActive(false);
            //        return;
            //    } else {
            //        lockstep.ReplayRecord = replayRecord;
            //    }
            //}

            //如果配置了显示TrueSyncStats，那就初始化
            if (activeConfig.showStats) {
                this.gameObject.AddComponent<TrueSyncStats>().Lockstep = lockstep;
            }

            //创建协程调度
            scheduler = new CoroutineScheduler(lockstep);

            //非录像模式下 初始化帧的玩家列表
            if (ReplayRecord.replayMode != ReplayMode.LOAD_REPLAY) {
                lockstep.AddPlayer(0, "Local_Player", true);
                //if (communicator == null) {
                //    lockstep.AddPlayer(0, "Local_Player", true);
                //} else {
                //    List<PhotonPlayer> players = new List<PhotonPlayer>(PhotonNetwork.playerList);
                //    players.Sort(UnityUtils.playerComparer);

                //    for (int index = 0, length = players.Count; index < length; index++) {
                //        PhotonPlayer p = players[index];
                //        lockstep.AddPlayer((byte) p.ID, p.NickName, p.IsLocal);
                //    }
                //}
            }

            //初始化场景中现有的帧同步行为
            TrueSyncBehaviour[] behavioursArray = FindObjectsOfType<TrueSyncBehaviour>();
            for (int index = 0, length = behavioursArray.Length; index < length; index++) {
                generalBehaviours.Add(NewManagedBehavior(behavioursArray[index]));
            }

            //实例化玩家预设playerPrefabs和同步其行为的拥有者
            initBehaviors();
            //初始化行为拥有者，并分配给对于玩家。没有继承TrueSyncBehaviour的就继续放到普通行为列表
            initGeneralBehaviors(generalBehaviours, false);

            //添加物理对象移除监听
            PhysicsManager.instance.OnRemoveBody(OnRemovedRigidBody);

            //设置启动状态
            startState = StartState.BEHAVIOR_INITIALIZED;
        }

        // 创建ITrueSyncBehaviour的TrueSyncManagedBehaviour
        private TrueSyncManagedBehaviour NewManagedBehavior(ITrueSyncBehaviour trueSyncBehavior) {
            TrueSyncManagedBehaviour result = new TrueSyncManagedBehaviour(trueSyncBehavior);
            mapBehaviorToManagedBehavior[trueSyncBehavior] = result;

            return result;
        }

        // 初始化玩家预设和他们的同步行为。行为设置拥有者，并其添加到对应玩家的行为字典里behaviorsByPlayer
        private void initBehaviors() {
            behaviorsByPlayer = new Dictionary<byte, List<TrueSyncManagedBehaviour>>();

            //var playersEnum = lockstep.Players.GetEnumerator();
            //while (playersEnum.MoveNext()) {
            //    TSPlayer p = playersEnum.Current.Value;

            //    List<TrueSyncManagedBehaviour> behaviorsInstatiated = new List<TrueSyncManagedBehaviour>();

            //    for (int index = 0, length = playerPrefabs.Length; index < length; index++) {
            //        GameObject prefab = playerPrefabs[index];

            //        GameObject prefabInst = Instantiate(prefab);
            //        InitializeGameObject(prefabInst, prefabInst.transform.position.ToTSVector(), prefabInst.transform.rotation.ToTSQuaternion());

            //        TrueSyncBehaviour[] behaviours = prefabInst.GetComponentsInChildren<TrueSyncBehaviour>();
            //        for (int index2 = 0, length2 = behaviours.Length; index2 < length2; index2++) {
            //            TrueSyncBehaviour behaviour = behaviours[index2];
            //            behaviour.owner = p.playerInfo;

            //            TrueSyncManagedBehaviour tsmb = NewManagedBehavior(behaviour);
            //            tsmb.owner = behaviour.owner;

            //            behaviorsInstatiated.Add(tsmb);
            //        }
            //    }

            //    behaviorsByPlayer.Add(p.ID, behaviorsInstatiated);
            //}
        }

        // 对行为列表分配拥有者， 在Start(), CheckQueuedBehaviours()里调用
        private void initGeneralBehaviors(IEnumerable<TrueSyncManagedBehaviour> behaviours, bool realOwnerId) {
            List<TSPlayer> playersList = new List<TSPlayer>(lockstep.Players.Values);
            List<TrueSyncManagedBehaviour> itemsToRemove = new List<TrueSyncManagedBehaviour>();

            var behavioursEnum = behaviours.GetEnumerator();
            while (behavioursEnum.MoveNext()) {
                TrueSyncManagedBehaviour tsmb = behavioursEnum.Current;

                if (!(tsmb.trueSyncBehavior is TrueSyncBehaviour)) {
                    continue;
                }

                TrueSyncBehaviour bh = (TrueSyncBehaviour)tsmb.trueSyncBehavior;

                if (realOwnerId) {
                    bh.ownerIndex = bh.owner.Id;
                } else {
                    if (bh.ownerIndex >= 0 && bh.ownerIndex < playersList.Count) {
                        bh.ownerIndex = playersList[bh.ownerIndex].ID;
                    }
                }

                if (behaviorsByPlayer.ContainsKey((byte)bh.ownerIndex)) {
                    bh.owner = lockstep.Players[(byte)bh.ownerIndex].playerInfo;

                    behaviorsByPlayer[(byte)bh.ownerIndex].Add(tsmb);
                    itemsToRemove.Add(tsmb);
                } else {
                    bh.ownerIndex = -1;
                }

                tsmb.owner = bh.owner;
            }

            for (int index = 0, length = itemsToRemove.Count; index < length; index++) {
                generalBehaviours.Remove(itemsToRemove[index]);
            }
        }

        // 将注册行为占存列表列queuedBehaviours的行为调initGeneralBehaviors分配拥有者。调OnSyncedStart方法
        private void CheckQueuedBehaviours() {
            if (queuedBehaviours.Count > 0) {
                generalBehaviours.AddRange(queuedBehaviours);
                initGeneralBehaviors(queuedBehaviours, true);

                for (int index = 0, length = queuedBehaviours.Count; index < length; index++) {
                    TrueSyncManagedBehaviour tsmb = queuedBehaviours[index];
                    tsmb.OnSyncedStart();
                }

                queuedBehaviours.Clear();
            }
        }

        // 只做一件事，检测启动状态，如果是第一次启动就调lockstep.RunSimulation(true);
        void Update() {
            if (lockstep != null && startState != StartState.STARTED) {
                if (startState == StartState.BEHAVIOR_INITIALIZED) {
                    startState = StartState.FIRST_UPDATE;
                } else if (startState == StartState.FIRST_UPDATE) {
                    lockstep.RunSimulation(true);
                    startState = StartState.STARTED;
                }
            }
        }

        /// <summary>
        ///@brief Run/Unpause the game simulation.
        ///DV 运行/恢复暂停的 游戏模拟 
        ///在帧同步调暂停后 恢复继续运行
        /// </summary>
        public static void RunSimulation() {
            if (instance != null && instance.lockstep != null) {
                instance.lockstep.RunSimulation(false);
            }
        }

        /// <summary>
        ///@brief Pauses the game simulation.
        ///DV 暂停游戏模拟 
        /// </summary>
        public static void PauseSimulation() {
            if (instance != null && instance.lockstep != null) {
                instance.lockstep.PauseSimulation();
            }
        }

        /// <summary>
        ///@brief End the game simulation.
        ///DV 结束游戏
        /// </summary>
        public static void EndSimulation() {
            if (instance != null && instance.lockstep != null) {
                instance.lockstep.EndSimulation();
            }
        }

        /// <summary>
        ///@brief Update all coroutines created.
        ///DV 更新一次协程， 主要是物理里调用了。默认的协程更新在 帧更新里OnStepUpdate
        /// </summary>
        public static void UpdateCoroutines() {
            if (instance != null && instance.lockstep != null) {
                instance.scheduler.UpdateAllCoroutines();
            }
        }

        /// <summary>
        ///@brief Starts a new coroutine.
        ///
        ///@param coroutine An IEnumerator that represents the coroutine.
        ///DV 添加一个协程
        /// </summary>
        public static void SyncedStartCoroutine(IEnumerator coroutine) {
            if (instance != null && instance.lockstep != null) {
                instance.scheduler.StartCoroutine(coroutine);
            }
        }

        /// <summary>
        ///@brief Instantiate a new prefab in a deterministic way.
        ///DV 以确定性的方式实例化一个新的预制件:实例化一个预设 
        ///@param prefab GameObject's prefab to instantiate.
        ///DV 预制游戏对象的预制实例
        /// </summary>
        public static GameObject SyncedInstantiate(GameObject prefab) {
            return SyncedInstantiate(prefab, prefab.transform.position.ToTSVector(), prefab.transform.rotation.ToTSQuaternion());
        }

        /// <summary>
        ///@brief Instantiates a new prefab in a deterministic way.
        ///
        ///@param prefab GameObject's prefab to instantiate.
        ///@param position Position to place the new GameObject.
        ///@param rotation Rotation to set in the new GameObject.
        /// </summary>
        public static GameObject SyncedInstantiate(GameObject prefab, TSVector position, TSQuaternion rotation) {
            if (instance != null && instance.lockstep != null) {
                //先实例化一个GameObject
                GameObject go = GameObject.Instantiate(prefab, position.ToVector(), rotation.ToQuaternion()) as GameObject;

                if (ReplayRecord.replayMode != ReplayMode.LOAD_REPLAY) {
                    //非录像模式将该对象添加到帧记录里。AddGameObjectOnSafeMap(go);
                    AddGameObjectOnSafeMap(go);
                }

                MonoBehaviour[] monoBehaviours = go.GetComponentsInChildren<MonoBehaviour>();
                for (int index = 0, length = monoBehaviours.Length; index < length; index++) {
                    MonoBehaviour bh = monoBehaviours[index];

                    if (bh is ITrueSyncBehaviour) {
                        //将该对象的帧行为添加到queuedBehaviours，等待帧更新的时候分配拥有者和调度初始化方法
                        instance.queuedBehaviours.Add(instance.NewManagedBehavior((ITrueSyncBehaviour)bh));
                    }
                }

                //该对象上组件的初始化方法（ICollider注册到物理管理器里PhysicsManager, TSTransform, TSTransform2D）。
                InitializeGameObject(go, position, rotation);

                return go;
            }

            return null;
        }

        // 将实例化的GameObject添加到当前的帧+1列表里
        private static void AddGameObjectOnSafeMap(GameObject go) {

            Dictionary<int, List<GameObject>> safeMap = instance.gameOjectsSafeMap;

            int currentTick = TrueSyncManager.Ticks + 1;
            if (!safeMap.ContainsKey(currentTick)) {
                safeMap.Add(currentTick, new List<GameObject>());
            }

            safeMap[currentTick].Add(go);
        }

        // 在帧更新OnStepUpdate的时候掉, 清理销毁掉当前 Ticks + 1里的GameObject。猜测估计是帧回滚的时候把预处理的对象销毁
        private static void CheckGameObjectsSafeMap() {
            Dictionary<int, List<GameObject>> safeMap = instance.gameOjectsSafeMap;

            int currentTick = TrueSyncManager.Ticks + 1;
            if (safeMap.ContainsKey(currentTick)) {
                List<GameObject> gos = safeMap[currentTick];
                for (int i = 0, l = gos.Count; i < l; i++) {
                    GameObject go = gos[i];
                    if (go != null) {
                        Renderer rend = go.GetComponent<Renderer>();
                        if (rend != null) {
                            rend.enabled = false;
                        }

                        GameObject.Destroy(go);
                    }
                }

                gos.Clear();
            }

            safeMap.Remove(TrueSyncManager.LastSafeTick);
        }

        // 调该对象上组件的初始化方法（ICollider注册到物理管理器里PhysicsManager, TSTransform, TSTransform2D）
        private static void InitializeGameObject(GameObject go, TSVector position, TSQuaternion rotation) {
            ICollider[] tsColliders = go.GetComponentsInChildren<ICollider>();
            if (tsColliders != null) {
                for (int index = 0, length = tsColliders.Length; index < length; index++) {
                    PhysicsManager.instance.AddBody(tsColliders[index]);
                }
            }

            TSTransform rootTSTransform = go.GetComponent<TSTransform>();
            if (rootTSTransform != null) {
                rootTSTransform.Initialize();

                rootTSTransform.position = position;
                rootTSTransform.rotation = rotation;
            }

            TSTransform[] tsTransforms = go.GetComponentsInChildren<TSTransform>();
            if (tsTransforms != null) {
                for (int index = 0, length = tsTransforms.Length; index < length; index++) {
                    TSTransform tsTransform = tsTransforms[index];

                    if (tsTransform != rootTSTransform) {
                        tsTransform.Initialize();
                    }
                }
            }

            TSTransform2D rootTSTransform2D = go.GetComponent<TSTransform2D>();
            if (rootTSTransform2D != null) {
                rootTSTransform2D.Initialize();

                rootTSTransform2D.position = new TSVector2(position.x, position.y);
                rootTSTransform2D.rotation = rotation.ToQuaternion().eulerAngles.z;
            }

            TSTransform2D[] tsTransforms2D = go.GetComponentsInChildren<TSTransform2D>();
            if (tsTransforms2D != null) {
                for (int index = 0, length = tsTransforms2D.Length; index < length; index++) {
                    TSTransform2D tsTransform2D = tsTransforms2D[index];

                    if (tsTransform2D != rootTSTransform2D) {
                        tsTransform2D.Initialize();
                    }
                }
            }
        }

        /// <summary>
        ///@brief Instantiates a new prefab in a deterministic way.
        ///
        ///@param prefab GameObject's prefab to instantiate.
        ///@param position Position to place the new GameObject.
        ///@param rotation Rotation to set in the new GameObject.
        /// </summary>
        public static GameObject SyncedInstantiate(GameObject prefab, TSVector2 position, TSQuaternion rotation) {
            return SyncedInstantiate(prefab, new TSVector(position.x, position.y, 0), rotation);
        }

        /// <summary>
        ///@brief Destroys a GameObject in a deterministic way.
        ///DV 以确定的方式销毁游戏对象 :销毁GameObject
        ///The method {@link #DestroyTSRigidBody} is called and attached TrueSyncBehaviors are disabled.
        ///DV 方法被{@link #DestroyTSRigidBody}调用，并附加TrueSyncBehaviors禁用。 
        ///@param rigidBody Instance of a {@link TSRigidBody}
        ///DV 一个刚体实例{@link TSRigidBody}
        /// </summary>
        public static void SyncedDestroy(GameObject gameObject) {
            if (instance != null && instance.lockstep != null) {
                // 第一步调SyncedDisableBehaviour, 停止更新该对象上的ITrueSyncBehaviour
                SyncedDisableBehaviour(gameObject);

                // 第二步调 TSCollider和TSCollider2D 调 DestroyTSRigidBody
                TSCollider[] tsColliders = gameObject.GetComponentsInChildren<TSCollider>();
                if (tsColliders != null) {
                    for (int index = 0, length = tsColliders.Length; index < length; index++) {
                        TSCollider tsCollider = tsColliders[index];
                        DestroyTSRigidBody(tsCollider.gameObject, tsCollider.Body);
                    }
                }

                TSCollider2D[] tsColliders2D = gameObject.GetComponentsInChildren<TSCollider2D>();
                if (tsColliders2D != null) {
                    for (int index = 0, length = tsColliders2D.Length; index < length; index++) {
                        TSCollider2D tsCollider2D = tsColliders2D[index];
                        DestroyTSRigidBody(tsCollider2D.gameObject, tsCollider2D.Body);
                    }
                }
            }
        }

        /// <summary>
        ///@brief Disables 'OnSyncedInput' and 'OnSyncUpdate' calls to every {@link ITrueSyncBehaviour} attached.
        ///DV 禁用对每个{@link ITrueSyncBehaviour}调用的“OnSyncedInput”和“OnSyncUpdate” 
        /// </summary>
        public static void SyncedDisableBehaviour(GameObject gameObject) {
            MonoBehaviour[] monoBehaviours = gameObject.GetComponentsInChildren<MonoBehaviour>();

            for (int index = 0, length = monoBehaviours.Length; index < length; index++) {
                MonoBehaviour tsb = monoBehaviours[index];

                if (tsb is ITrueSyncBehaviour && instance.mapBehaviorToManagedBehavior.ContainsKey((ITrueSyncBehaviour)tsb)) {
                    //将GameObject的ITrueSyncBehaviour的disabled设置为true, 停止对他调帧更新方法 OnSyncedInput，OnSyncUpdate。
                    instance.mapBehaviorToManagedBehavior[(ITrueSyncBehaviour)tsb].disabled = true;
                }
            }
        }

        /// <summary>
        ///@brief The related GameObject is firstly set to be inactive then in a safe moment it will be destroyed.
        ///DV 相关游戏对象首先被设置为不活动，然后在安全时刻它将被销毁。 
        ///@param rigidBody Instance of a {@link TSRigidBody}
        /// </summary>
        private static void DestroyTSRigidBody(GameObject tsColliderGO, IBody body) {
            tsColliderGO.gameObject.SetActive(false);
            instance.lockstep.Destroy(body);// 将物理对象从lockstep销毁
        }

        /// <summary>
        ///@brief Registers an implementation of {@link ITrueSyncBehaviour} to be included in the simulation.
        ///DV 注册包含在仿真中的{@link ITrueSyncBehaviour}的实现 
        ///@param trueSyncBehaviour Instance of an {@link ITrueSyncBehaviour}
        ///注册ITrueSyncBehaviour, 将他添加到queuedBehaviours。在下次CheckQueuedBehaviours的时候，也就是在下次帧更新的时候OnStepUpdate，对他分配拥有者，调OnSyncedStart方法
        /// </summary>
        public static void RegisterITrueSyncBehaviour(ITrueSyncBehaviour trueSyncBehaviour) {
            if (instance != null && instance.lockstep != null) {
                instance.queuedBehaviours.Add(instance.NewManagedBehavior(trueSyncBehaviour));
            }
        }

        /// <summary>
        ///@brief Register a {@link TrueSyncIsReady} delegate to that returns true if the game can proceed or false otherwise.
        ///DV 如果游戏可以继续或错误，则将{@link TrueSyncIsReady}委托注册为返回true。 
        ///@param IsReadyChecker A {@link TrueSyncIsReady} delegate
        ///注册游戏是否继续的委托。 调委托会返回一个bool值。 true游戏可以继续运行。lockstep里会调该方法检测是否可以继续CheckGameIsReady（）
        /// </summary>
        public static void RegisterIsReadyChecker(TrueSyncIsReady IsReadyChecker) {
            if (instance != null && instance.lockstep != null) {
                instance.lockstep.GameIsReady += IsReadyChecker;
            }
        }

        /// <summary>
        ///@brief Removes objets related to a provided player.
        ///DV 移除与所提供的player相关的对象 :移除玩家
        ///@param playerId Target player's id.
        /// </summary>
        public static void RemovePlayer(int playerId) {
            if (instance != null && instance.lockstep != null) {
                List<TrueSyncManagedBehaviour> behaviorsList = instance.behaviorsByPlayer[(byte)playerId];

                for (int index = 0, length = behaviorsList.Count; index < length; index++) {
                    TrueSyncManagedBehaviour tsmb = behaviorsList[index];
                    tsmb.disabled = true;//第一步将该玩家的 行为全部禁止帧更新

                    //第二步将这些行为的GameObject上拥有TSCollider、TSCollider2D的物理全部掉DestroyTSRigidBody
                    TSCollider[] tsColliders = ((TrueSyncBehaviour)tsmb.trueSyncBehavior).gameObject.GetComponentsInChildren<TSCollider>();
                    if (tsColliders != null) {
                        for (int index2 = 0, length2 = tsColliders.Length; index2 < length2; index2++) {
                            TSCollider tsCollider = tsColliders[index2];

                            if (!tsCollider.Body.TSDisabled) {
                                DestroyTSRigidBody(tsCollider.gameObject, tsCollider.Body);
                            }
                        }
                    }

                    TSCollider2D[] tsCollider2Ds = ((TrueSyncBehaviour)tsmb.trueSyncBehavior).gameObject.GetComponentsInChildren<TSCollider2D>();
                    if (tsCollider2Ds != null) {
                        for (int index2 = 0, length2 = tsCollider2Ds.Length; index2 < length2; index2++) {
                            TSCollider2D tsCollider2D = tsCollider2Ds[index2];

                            if (!tsCollider2D.Body.TSDisabled) {
                                DestroyTSRigidBody(tsCollider2D.gameObject, tsCollider2D.Body);
                            }
                        }
                    }
                }
            }
        }

        //private FP tsDeltaTime = 0;

        //void FixedUpdate() {//原来由系统驱动
        //    if (lockstep != null) {
        //        tsDeltaTime += UnityEngine.Time.deltaTime;

        //        if (tsDeltaTime >= (lockedTimeStep - JitterTimeFactor)) {
        //            tsDeltaTime = 0;

        //            instance.scheduler.UpdateAllCoroutines();
        //            lockstep.LockStepUpdate();
        //        }
        //    }
        //}
        public void OnUpdate()//改成由LockStep.GameTurn()中驱动
        {
            if (lockstep != null)
            {
                instance.scheduler.UpdateAllCoroutines();
                lockstep.LockStepUpdate();
            }
        }

        // 里面创建一个输入数据结构 return new InputData();  是在lockstep创建的时候传这个方法给他
        InputDataBase ProvideInputData() {
            return new InputData();
        }

        /// <summary>
        ///DV 帧更新
        ///是在lockstep创建的时候传这个方法给他
        /// </summary>
        void OnStepUpdate(List<InputDataBase> allInputData) {
            // 添加当前时间
            time += lockedTimeStep;

            if (ReplayRecord.replayMode != ReplayMode.LOAD_REPLAY) {
                // 非录像模式， 检测GameObject CheckGameObjectsSafeMap();
                CheckGameObjectsSafeMap();
            }

            TrueSyncInput.SetAllInputs(null);

            // 遍历generalBehaviours普通行为列表，调行为的OnPreSyncedUpdate()。还会调协程更新instance.scheduler.UpdateAllCoroutines();
            for (int index = 0, length = generalBehaviours.Count; index < length; index++) {
                TrueSyncManagedBehaviour bh = generalBehaviours[index];

                if (bh != null && !bh.disabled) {
                    bh.OnPreSyncedUpdate();
                    instance.scheduler.UpdateAllCoroutines();
                }
            }

            // 遍历allInputData,和对应玩家的行为列表behaviorsByPlayer。 调行为的OnPreSyncedUpdate()。还会调协程更新instance.scheduler.UpdateAllCoroutines();
            for (int index = 0, length = allInputData.Count; index < length; index++) {
                InputDataBase playerInputData = allInputData[index];

                if (behaviorsByPlayer.ContainsKey(playerInputData.ownerID)) {
                    List<TrueSyncManagedBehaviour> managedBehavioursByPlayer = behaviorsByPlayer[playerInputData.ownerID];
                    for (int index2 = 0, length2 = managedBehavioursByPlayer.Count; index2 < length2; index2++) {
                        TrueSyncManagedBehaviour bh = managedBehavioursByPlayer[index2];

                        if (bh != null && !bh.disabled) {
                            bh.OnPreSyncedUpdate();
                            instance.scheduler.UpdateAllCoroutines();
                        }
                    }
                }
            }

            TrueSyncInput.SetAllInputs(allInputData);

            TrueSyncInput.CurrentSimulationData = null;
            // 遍历generalBehaviours普通行为列表，调行为的OnSyncedUpdate()。还会调协程更新instance.scheduler.UpdateAllCoroutines();
            for (int index = 0, length = generalBehaviours.Count; index < length; index++) {
                TrueSyncManagedBehaviour bh = generalBehaviours[index];

                if (bh != null && !bh.disabled) {
                    bh.OnSyncedUpdate();
                    instance.scheduler.UpdateAllCoroutines();
                }
            }

            // 遍历allInputData,和对应玩家的行为列表behaviorsByPlayer。 调行为的OnSyncedUpdate()。还会调协程更新instance.scheduler.UpdateAllCoroutines();
            for (int index = 0, length = allInputData.Count; index < length; index++) {
                InputDataBase playerInputData = allInputData[index];

                if (behaviorsByPlayer.ContainsKey(playerInputData.ownerID)) {
                    TrueSyncInput.CurrentSimulationData = (InputData) playerInputData;

                    List<TrueSyncManagedBehaviour> managedBehavioursByPlayer = behaviorsByPlayer[playerInputData.ownerID];
                    for (int index2 = 0, length2 = managedBehavioursByPlayer.Count; index2 < length2; index2++) {
                        TrueSyncManagedBehaviour bh = managedBehavioursByPlayer[index2];

                        if (bh != null && !bh.disabled) {
                            bh.OnSyncedUpdate();
                            instance.scheduler.UpdateAllCoroutines();
                        }
                    }
                }

                TrueSyncInput.CurrentSimulationData = null;
            }

            // 检测占存行为列表CheckQueuedBehaviours（）。给他们分配拥有者和调同步开始方法
            CheckQueuedBehaviours();
        }

        /// <summary> 
        ///DV 移除物理对象事件处理 
        ///会移除该对象的GameObject上所有同步行为 调RemoveFromTSMBList
        /// </summary>
        private void OnRemovedRigidBody(IBody body) {
            GameObject go = PhysicsManager.instance.GetGameObject(body);

            if (go != null) {
                List<TrueSyncBehaviour> behavioursToRemove = new List<TrueSyncBehaviour>(go.GetComponentsInChildren<TrueSyncBehaviour>());
                RemoveFromTSMBList(queuedBehaviours, behavioursToRemove);
                RemoveFromTSMBList(generalBehaviours, behavioursToRemove);

                var behaviorsByPlayerEnum = behaviorsByPlayer.GetEnumerator();
                while (behaviorsByPlayerEnum.MoveNext()) {
                    List<TrueSyncManagedBehaviour> listBh = behaviorsByPlayerEnum.Current.Value;
                    RemoveFromTSMBList(listBh, behavioursToRemove);
                }
            }
        }

        /// <summary> 
        ///DV 从tsmbList列表中，移除behaviours 
        /// </summary>
        private void RemoveFromTSMBList(List<TrueSyncManagedBehaviour> tsmbList, List<TrueSyncBehaviour> behaviours) {
            List<TrueSyncManagedBehaviour> toRemove = new List<TrueSyncManagedBehaviour>();
            for (int index = 0, length = tsmbList.Count; index < length; index++) {
                TrueSyncManagedBehaviour tsmb = tsmbList[index];

                if ((tsmb.trueSyncBehavior is TrueSyncBehaviour) && behaviours.Contains((TrueSyncBehaviour)tsmb.trueSyncBehavior)) {
                    toRemove.Add(tsmb);
                }
            }

            for (int index = 0, length = toRemove.Count; index < length; index++) {
                TrueSyncManagedBehaviour tsmb = toRemove[index];
                tsmbList.Remove(tsmb);
            }
        }

        /// <summary> 
        ///@brief Clean up references to be collected by gc.
        ///DV 清除GC所收集的引用 
        /// </summary>
        public static void CleanUp() {
            ResourcePool.CleanUpAll();// 清理对象池
            StateTracker.CleanUp();// 清理状态跟踪
            instance = null;// 去除实例变量引用
        }

        /// <summary> 
        ///DV 玩家离线消息处理
        ///是在lockstep创建的时候传这个方法给他
        /// </summary>
        void OnPlayerDisconnection(byte playerId) {
            TrueSyncManagedBehaviour.OnPlayerDisconnection(generalBehaviours, behaviorsByPlayer, playerId);
        }

        /// <summary> 
        ///DV 游戏开始消息处理
        ///是在lockstep创建的时候传这个方法给他
        /// </summary>
        void OnGameStarted() {
            TrueSyncManagedBehaviour.OnGameStarted(generalBehaviours, behaviorsByPlayer);
            instance.scheduler.UpdateAllCoroutines();

            CheckQueuedBehaviours();
        }

        /// <summary> 
        ///DV 游戏暂停消息处理
        ///是在lockstep创建的时候传这个方法给他
        /// </summary>
        void OnGamePaused() {
            TrueSyncManagedBehaviour.OnGamePaused(generalBehaviours, behaviorsByPlayer);
            instance.scheduler.UpdateAllCoroutines();
        }

        /// <summary> 
        ///DV 游戏继续消息处理
        ///是在lockstep创建的时候传这个方法给他
        /// </summary>
        void OnGameUnPaused() {
            TrueSyncManagedBehaviour.OnGameUnPaused(generalBehaviours, behaviorsByPlayer);
            instance.scheduler.UpdateAllCoroutines();
        }

        /// <summary> 
        ///DV 游戏结束消息处理
        ///是在lockstep创建的时候传这个方法给他
        /// </summary>
        void OnGameEnded() {
            TrueSyncManagedBehaviour.OnGameEnded(generalBehaviours, behaviorsByPlayer);
            instance.scheduler.UpdateAllCoroutines();
        }

        // Unity的消息。退出应用
        void OnApplicationQuit() {
            EndSimulation();
        }

    }

}