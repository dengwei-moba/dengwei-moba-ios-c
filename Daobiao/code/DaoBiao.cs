using System;
using System.Text;
namespace TrueSync
{
public class RofBuffRow : IRofBase
{
	public int ID { get; private set; }
	public string BuffName { get; private set; }
	public int BuffEffectType { get; private set; }
	public int BuffRepeatType { get; private set; }
	public int BuffOverlapType { get; private set; }
	public int BuffShutDownType { get; private set; }
	public int MaxLimit { get; private set; }
	public int TotalFrame { get; private set; }
	public int CallIntervalFrame { get; private set; }
	public FP Num { get; private set; }
	public string Effect { get; private set; }
	public int ReadBody(byte[] rData, int nOffset)
	{
		if (BitConverter.IsLittleEndian){Array.Reverse(rData, nOffset, 4);}
		ID = (int)BitConverter.ToUInt32(rData, nOffset); nOffset += 4;
		if (BitConverter.IsLittleEndian){Array.Reverse(rData, nOffset, 4);}
		int nBuffNameLen = (int)BitConverter.ToUInt32(rData, nOffset); nOffset += 4;
		BuffName = Encoding.UTF8.GetString(rData, nOffset, nBuffNameLen); nOffset += nBuffNameLen;
		if (BitConverter.IsLittleEndian){Array.Reverse(rData, nOffset, 4);}
		BuffEffectType = (int)BitConverter.ToUInt32(rData, nOffset); nOffset += 4;
		if (BitConverter.IsLittleEndian){Array.Reverse(rData, nOffset, 4);}
		BuffRepeatType = (int)BitConverter.ToUInt32(rData, nOffset); nOffset += 4;
		if (BitConverter.IsLittleEndian){Array.Reverse(rData, nOffset, 4);}
		BuffOverlapType = (int)BitConverter.ToUInt32(rData, nOffset); nOffset += 4;
		if (BitConverter.IsLittleEndian){Array.Reverse(rData, nOffset, 4);}
		BuffShutDownType = (int)BitConverter.ToUInt32(rData, nOffset); nOffset += 4;
		if (BitConverter.IsLittleEndian){Array.Reverse(rData, nOffset, 4);}
		MaxLimit = (int)BitConverter.ToUInt32(rData, nOffset); nOffset += 4;
		if (BitConverter.IsLittleEndian){Array.Reverse(rData, nOffset, 4);}
		TotalFrame = (int)BitConverter.ToUInt32(rData, nOffset); nOffset += 4;
		if (BitConverter.IsLittleEndian){Array.Reverse(rData, nOffset, 4);}
		CallIntervalFrame = (int)BitConverter.ToUInt32(rData, nOffset); nOffset += 4;
		if (BitConverter.IsLittleEndian){Array.Reverse(rData, nOffset, 4);}
		Num = (int)BitConverter.ToUInt32(rData, nOffset); nOffset += 4;
		if (BitConverter.IsLittleEndian){Array.Reverse(rData, nOffset, 4);}
		int nEffectLen = (int)BitConverter.ToUInt32(rData, nOffset); nOffset += 4;
		Effect = Encoding.UTF8.GetString(rData, nOffset, nEffectLen); nOffset += nEffectLen;
		return nOffset;
	}
}
//=======================================================
}
