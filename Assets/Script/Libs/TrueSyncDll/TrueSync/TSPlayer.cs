using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrueSync
{
    /// <summary>
    /// DV 帧同步玩家
    /// </summary>
	[Serializable]
	public class TSPlayer
	{
        /// <summary>
        /// 帧同步玩家信息
        /// </summary>
		[SerializeField]
		public TSPlayerInfo playerInfo;

        /// <summary>
        /// 掉线次数
        /// </summary>
		[NonSerialized]
        public int dropCount; 

        /// <summary>
        /// 是否掉线
        /// </summary>
		[NonSerialized]
        public bool dropped;

        /// <summary>
        /// 保存玩家整个战斗的操作同步数据, 他是一个字典SerializableDictionary<int, SyncedData>
        /// </summary>
		[SerializeField]
        internal SerializableDictionaryIntSyncedData controls;

        /// <summary>
        /// 最后一次 同步操作数据的帧, AddData(SyncedData data)
        /// </summary>
        private int lastTick;

		public byte ID
		{
			get
			{
				return this.playerInfo.id;
			}
		}

		internal TSPlayer(byte id, string name)
		{
            // 创建玩家信息
			this.playerInfo = new TSPlayerInfo(id, name);
			this.dropCount = 0;
			this.dropped = false;
            // 创建玩家操作字典存储器
			this.controls = new SerializableDictionaryIntSyncedData();
		}

        #region 公共方法
        /// <summary>
        /// DV 数据是否已准备好
        /// 获取某帧是否有真实同步操作数据
        /// </summary>
        public bool IsDataReady(int tick)
		{
            return true;
			//return this.controls.ContainsKey(tick) && !this.controls[tick].fake;
		}

        /// <summary>
        /// DV 是不是脏数据
        /// 获取某帧是否有模拟同步操作数据, 客户端先行，是客户端预测的操作, 回滚添加的。
        /// </summary>
		public bool IsDataDirty(int tick)
		{
			bool flag = this.controls.ContainsKey(tick);
			return flag && this.controls[tick].dirty;
		}
        
        /// <summary>
        /// DV 获取该帧的同步操作数据
        /// </summary>
        public SyncedData GetData(int tick)
		{
			bool flag = !this.controls.ContainsKey(tick);
			SyncedData result;
			if (flag) // 没有数据
			{
                bool flag2 = this.controls.ContainsKey(tick - 1); // 如果不存在，就查找上一帧是否存在
				SyncedData syncedData;
				if (flag2) // 有数据,把前一个数据复制为当前数据
				{
					syncedData = this.controls[tick - 1].clone();
					syncedData.tick = tick;
				}
				else // 没有数据,获取一个新的数据
				{
					syncedData = SyncedData.pool.GetNew();
					syncedData.Init(this.ID, tick);
				}
				syncedData.fake = true; // 设置为假数据
                this.controls[tick] = syncedData;// 保存到帧字典
				result = syncedData;
			}
			else // 如果存在,就返回该帧数据
			{
				result = this.controls[tick];
			}
			return result;
		}

        /// <summary>
        /// DV 添加存在帧同步数据
        /// </summary>
		public void AddData(SyncedData data)
		{
			int tick = data.tick;
			bool flag = this.controls.ContainsKey(tick);
			if (flag) // 数据已存在，把数据对象回收到池中
			{
				SyncedData.pool.GiveBack(data);
			}
			else // 未有数据
			{
				this.controls[tick] = data;
                this.lastTick = tick;// 设置最后存储的帧
			}
		}

        /// <summary>
        /// DV 添加存在帧同步数据
        /// </summary>
		public void AddData(List<SyncedData> data)
		{
			for (int i = 0; i < data.Count; i++)
			{
				this.AddData(data[i]);
			}
		}

        // 移除数据
		public void RemoveData(int refTick)
		{
			bool flag = this.controls.ContainsKey(refTick);
			if (flag)
			{
				SyncedData.pool.GiveBack(this.controls[refTick]);
				this.controls.Remove(refTick);
			}
		}

        // 更新同步数据
		public void AddDataProjected(int refTick, int window)
		{
			SyncedData syncedData = this.GetData(refTick);
			for (int i = 1; i <= window; i++)
			{
				SyncedData data = this.GetData(refTick + i);
				bool fake = data.fake;
				if (fake)
				{
					SyncedData syncedData2 = syncedData.clone();
					syncedData2.fake = true;
					syncedData2.tick = refTick + i;
					bool flag = this.controls.ContainsKey(syncedData2.tick);
					if (flag)
					{
						SyncedData.pool.GiveBack(this.controls[syncedData2.tick]);
					}
					this.controls[syncedData2.tick] = syncedData2;
				}
				else
				{
					bool dirty = data.dirty;
					if (dirty)
					{
						data.dirty = false;
						syncedData = data;
					}
				}
			}
		}

        // 添加数据
		public void AddDataRollback(List<SyncedData> data)
		{
			for (int i = 0; i < data.Count; i++)
			{
				SyncedData data2 = this.GetData(data[i].tick);
				bool fake = data2.fake;
				if (fake) // 取出的是假数据
				{
					bool flag = data2.EqualsData(data[i]);
					if (!flag) // 两个数据不想等
					{
						data[i].dirty = true; // 设置为脏数据
						SyncedData.pool.GiveBack(this.controls[data[i].tick]); // 回收该位置的数据
						this.controls[data[i].tick] = data[i]; // 放入新数据
						break; // 中断，没有下一个数据要处理？
					}
					data2.fake = false;
					data2.dirty = false;
				}
				SyncedData.pool.GiveBack(data[i]);
			}
		}

		public bool GetSendDataForDrop(byte fromPlayerId, SyncedData[] sendWindowArray)
		{
			bool flag = this.controls.Count == 0;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				this.GetDataFromTick(this.lastTick, sendWindowArray);
				sendWindowArray[0] = sendWindowArray[0].clone();
				sendWindowArray[0].dropFromPlayerId = fromPlayerId;
				sendWindowArray[0].dropPlayer = true;
				result = true;
			}
			return result;
		}

		public void GetSendData(int tick, SyncedData[] sendWindowArray)
		{
			this.GetDataFromTick(tick, sendWindowArray);
		}
        #endregion 公共方法

        #region 私有方法
        private void GetDataFromTick(int tick, SyncedData[] sendWindowArray)
        {
            for (int i = 0; i < sendWindowArray.Length; i++)
            {
                // 以倒序的方式把数据放入数组中
                sendWindowArray[i] = this.GetData(tick - i);
            }
        }
        #endregion 私有方法
    }
}
