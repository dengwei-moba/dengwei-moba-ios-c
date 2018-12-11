using System;
using System.Collections.Generic;

namespace TrueSync
{
    /// <summary>
    /// DV 帧同步托管行为类
    /// 帧同步行为管理器，实现接口 ITrueSyncBehaviourGamePlay, ITrueSyncBehaviour, ITrueSyncBehaviourCallbacks
    /// 主要是包装了TrueSyncBehaviour/ITrueSyncBehaviour, 实现的接口方法直接掉TrueSyncBehaviour的放方法，TrueSyncBehaviour的OnSyncedStartLocalPlayer 方法该行为是本地用户时才调
    /// 后面就是一些全局静态方法，用管理处理列表的事件
    /// </summary>
	public class TrueSyncManagedBehaviour : ITrueSyncBehaviourGamePlay, ITrueSyncBehaviourCallbacks
	{
		public ITrueSyncBehaviour trueSyncBehavior;

		[AddTracking]
        public bool disabled;// true时，不会参与帧更新

		public TSPlayerInfo owner;

		public TrueSyncManagedBehaviour(ITrueSyncBehaviour trueSyncBehavior)
		{
			StateTracker.AddTracking(this);
			StateTracker.AddTracking(trueSyncBehavior);
			this.trueSyncBehavior = trueSyncBehavior;
		}

        #region ITrueSyncBehaviourGamePlay 接口方法
        public void OnPreSyncedUpdate()
		{
			bool flag = this.trueSyncBehavior is ITrueSyncBehaviourGamePlay;
			if (flag)
			{
				((ITrueSyncBehaviourGamePlay)this.trueSyncBehavior).OnPreSyncedUpdate();
			}
		}

		public void OnSyncedUpdate()
		{
			bool flag = this.trueSyncBehavior is ITrueSyncBehaviourGamePlay;
			if (flag)
			{
				((ITrueSyncBehaviourGamePlay)this.trueSyncBehavior).OnSyncedUpdate();
			}
		}
        #endregion ITrueSyncBehaviourGamePlay 接口方法

        #region 生命周期方法
        // 开始同步
        public static void OnGameStarted(List<TrueSyncManagedBehaviour> generalBehaviours, Dictionary<byte, List<TrueSyncManagedBehaviour>> behaviorsByPlayer)
        {
            int i = 0;
            int count = generalBehaviours.Count;
            while (i < count)
            {
                generalBehaviours[i].OnSyncedStart();
                i++;
            }
            Dictionary<byte, List<TrueSyncManagedBehaviour>>.Enumerator enumerator = behaviorsByPlayer.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<byte, List<TrueSyncManagedBehaviour>> current = enumerator.Current;
                List<TrueSyncManagedBehaviour> value = current.Value;
                int j = 0;
                int count2 = value.Count;
                while (j < count2)
                {
                    value[j].OnSyncedStart();
                    j++;
                }
            }
        }

        // 游戏暂停
        public static void OnGamePaused(List<TrueSyncManagedBehaviour> generalBehaviours, Dictionary<byte, List<TrueSyncManagedBehaviour>> behaviorsByPlayer)
        {
            int i = 0;
            int count = generalBehaviours.Count;
            while (i < count)
            {
                generalBehaviours[i].OnGamePaused();
                i++;
            }
            Dictionary<byte, List<TrueSyncManagedBehaviour>>.Enumerator enumerator = behaviorsByPlayer.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<byte, List<TrueSyncManagedBehaviour>> current = enumerator.Current;
                List<TrueSyncManagedBehaviour> value = current.Value;
                int j = 0;
                int count2 = value.Count;
                while (j < count2)
                {
                    value[j].OnGamePaused();
                    j++;
                }
            }
        }

        // 取消暂停
        public static void OnGameUnPaused(List<TrueSyncManagedBehaviour> generalBehaviours, Dictionary<byte, List<TrueSyncManagedBehaviour>> behaviorsByPlayer)
        {
            int i = 0;
            int count = generalBehaviours.Count;
            while (i < count)
            {
                generalBehaviours[i].OnGameUnPaused();
                i++;
            }
            Dictionary<byte, List<TrueSyncManagedBehaviour>>.Enumerator enumerator = behaviorsByPlayer.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<byte, List<TrueSyncManagedBehaviour>> current = enumerator.Current;
                List<TrueSyncManagedBehaviour> value = current.Value;
                int j = 0;
                int count2 = value.Count;
                while (j < count2)
                {
                    value[j].OnGameUnPaused();
                    j++;
                }
            }
        }

        // 游戏结束
        public static void OnGameEnded(List<TrueSyncManagedBehaviour> generalBehaviours, Dictionary<byte, List<TrueSyncManagedBehaviour>> behaviorsByPlayer)
        {
            int i = 0;
            int count = generalBehaviours.Count;
            while (i < count)
            {
                generalBehaviours[i].OnGameEnded();
                i++;
            }
            Dictionary<byte, List<TrueSyncManagedBehaviour>>.Enumerator enumerator = behaviorsByPlayer.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<byte, List<TrueSyncManagedBehaviour>> current = enumerator.Current;
                List<TrueSyncManagedBehaviour> value = current.Value;
                int j = 0;
                int count2 = value.Count;
                while (j < count2)
                {
                    value[j].OnGameEnded();
                    j++;
                }
            }
        }

        // 玩家断开连接
        public static void OnPlayerDisconnection(List<TrueSyncManagedBehaviour> generalBehaviours, Dictionary<byte, List<TrueSyncManagedBehaviour>> behaviorsByPlayer, byte playerId)
        {
            int i = 0;
            int count = generalBehaviours.Count;
            while (i < count)
            {
                generalBehaviours[i].OnPlayerDisconnection((int)playerId);
                i++;
            }
            Dictionary<byte, List<TrueSyncManagedBehaviour>>.Enumerator enumerator = behaviorsByPlayer.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<byte, List<TrueSyncManagedBehaviour>> current = enumerator.Current;
                List<TrueSyncManagedBehaviour> value = current.Value;
                int j = 0;
                int count2 = value.Count;
                while (j < count2)
                {
                    value[j].OnPlayerDisconnection((int)playerId);
                    j++;
                }
            }
        }
        #endregion 生命周期方法

        #region ITrueSyncBehaviourCallbacks 接口方法
        // 开始同步
        public void OnSyncedStart()
		{	
            bool flag = this.trueSyncBehavior is ITrueSyncBehaviourCallbacks;
            if (flag)
            {
                ((ITrueSyncBehaviourCallbacks)this.trueSyncBehavior).OnSyncedStart();
            }
		}

        // 游戏暂停
		public void OnGamePaused()
		{
			bool flag = this.trueSyncBehavior is ITrueSyncBehaviourCallbacks;
			if (flag)
			{
				((ITrueSyncBehaviourCallbacks)this.trueSyncBehavior).OnGamePaused();
			}
		}

        // 取消暂停
		public void OnGameUnPaused()
		{
			bool flag = this.trueSyncBehavior is ITrueSyncBehaviourCallbacks;
			if (flag)
			{
				((ITrueSyncBehaviourCallbacks)this.trueSyncBehavior).OnGameUnPaused();
			}
		}

        // 游戏结束
		public void OnGameEnded()
		{
			bool flag = this.trueSyncBehavior is ITrueSyncBehaviourCallbacks;
			if (flag)
			{
				((ITrueSyncBehaviourCallbacks)this.trueSyncBehavior).OnGameEnded();
			}
		}

        // 玩家断开连接
		public void OnPlayerDisconnection(int playerId)
		{
			bool flag = this.trueSyncBehavior is ITrueSyncBehaviourCallbacks;
			if (flag)
			{
				((ITrueSyncBehaviourCallbacks)this.trueSyncBehavior).OnPlayerDisconnection(playerId);
			}
		}
        #endregion ITrueSyncBehaviourCallbacks 接口方法
    }
}
