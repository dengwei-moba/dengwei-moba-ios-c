using System;
using System.Collections.Generic;

namespace TrueSync
{
    /// <summary>
    /// DV 继承ResourcePoolItem 对象池对象接口
    /// 抽象输入数据，主要定义了几个接口。和一个owerID拥有者属性
    /// </summary>
	[Serializable]
	public abstract class InputDataBase : ResourcePoolItem
	{
		public byte ownerID;

		public InputDataBase()
		{
		}

        // 序列化
		public abstract void Serialize(List<byte> bytes);

        // 反序列化:解析
		public abstract void Deserialize(byte[] data, ref int offset);

        // 是否相等
		public abstract bool EqualsData(InputDataBase otherBase);

        // 清理
		public abstract void CleanUp();

        // 拷贝
		public abstract void CopyFrom(InputDataBase fromBase);
	}
}
