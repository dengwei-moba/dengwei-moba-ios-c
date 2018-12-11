using System;

namespace TrueSync
{
    /// <summary>
    /// DV 接口 玩家操作帧同步行为, 继承自ITrueSyncBehaviour
    /// </summary>
	public interface ITrueSyncBehaviourGamePlay : ITrueSyncBehaviour
	{
        /// <summary>
        /// DV 同步 预读取玩家操作
        /// </summary>
		void OnPreSyncedUpdate();

        /// <summary>
        /// DV 同步 读取玩家操作
        /// </summary>
		void OnSyncedUpdate();
	}
}
