using System;
using UnityEngine;

namespace TrueSync
{
    /// <summary>
    /// DV 帧同步玩家信息
    /// </summary>
	[Serializable]
	public class TSPlayerInfo
	{
		[SerializeField]
        internal byte id;// 玩家ID

		[SerializeField]
        internal string name;// 玩家名称

		public byte Id
		{
			get
			{
				return this.id;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public TSPlayerInfo(byte id, string name)
		{
			this.id = id;
			this.name = name;
		}
	}
}
