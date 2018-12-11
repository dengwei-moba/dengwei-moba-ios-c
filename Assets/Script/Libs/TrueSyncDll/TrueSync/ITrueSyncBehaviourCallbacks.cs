using System;

namespace TrueSync
{
    /// <summary>
    /// DV 接口 回调同步行为, 继承自ITrueSyncBehaviour
    /// </summary>
	public interface ITrueSyncBehaviourCallbacks : ITrueSyncBehaviour
	{
        // 开始
		void OnSyncedStart();

        // 游戏暂停
		void OnGamePaused();

        // 游戏继续
		void OnGameUnPaused();

        // 可以游戏了， 什么时候调用 我也不清楚
		void OnGameEnded();

        // 有玩家离线时调用
		void OnPlayerDisconnection(int playerId);
	}
}
