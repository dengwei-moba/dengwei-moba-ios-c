using System.Collections;
using System.Text;
using System;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

public class Datapack 
{
	public static Datapack m_UnpackInst = null;
	public static Datapack m_PackInst = null;

	public byte[] m_Buff = new byte[4096];
	public int m_Size = 0;
	public int m_UnpackPos = 0;

	public static Datapack GetUnpackInst()
	{
		if (m_UnpackInst == null)
			m_UnpackInst = new Datapack();
		return m_UnpackInst;
	}

	public static Datapack GetPackInst()
	{
		if (m_PackInst == null)
			m_PackInst = new Datapack();
		return m_PackInst;
	}

	public static int CheckUnpack(byte[] bytes, int iBuffSize, int iMaxSize)
	{
		if (iBuffSize < 4)
			return 0;
		// int iSize = bytes[0];
		int iSize = Bytes2Int(bytes, 0);
		if (iSize <= 0 || iSize > iMaxSize)
			return -1;
		if (iBuffSize < iSize)
			return 0;
		return iSize;
	}

	public static int CheckUnpack(byte[] bytes, int iBuffSize)
	{
		return CheckUnpack(bytes, iBuffSize, 256);
	}

	static long Bytes2Long(byte[] bytes, int iStartIndex)
	{
		return (long)(((long)bytes[iStartIndex + 0] << 56) + ((long)bytes[iStartIndex + 1] << 48) +
			((long)bytes[iStartIndex + 2] << 40) + ((long)bytes[iStartIndex + 3] << 32) +
		              ((long)bytes[iStartIndex + 4] << 24) + ((long)bytes[iStartIndex + 5] << 16) + 
		              ((long)bytes[iStartIndex + 6] << 8) + (long)bytes[iStartIndex + 7]);
	}

	static int Bytes2Int(byte[] bytes, int iStartIndex)
	{
		return (int)((int)(bytes[iStartIndex + 0] << 24) + ((int)bytes[iStartIndex + 1] << 16) + 
		             ((int)bytes[iStartIndex + 2] << 8) + (int)bytes[iStartIndex + 3]);
	}

	static short Bytes2Short(byte[] bytes, int iStartIndex)
	{
		return (short)(((short)bytes[iStartIndex + 0] << 8) + (short)bytes[iStartIndex + 1]);
	}

	static void Long2Bytes(byte[] bytes, int iStartIndex, long l)
	{
		bytes[iStartIndex + 0] = (byte)(l >> 56);
		bytes[iStartIndex + 1] = (byte)(l >> 48);
		bytes[iStartIndex + 2] = (byte)(l >> 40);
		bytes[iStartIndex + 3] = (byte)(l >> 32);
		bytes[iStartIndex + 4] = (byte)(l >> 24);
		bytes[iStartIndex + 5] = (byte)(l >> 16);
		bytes[iStartIndex + 6] = (byte)(l >> 8);
		bytes[iStartIndex + 7] = (byte)l;
	}

	static void Int2Bytes(byte[] bytes, int iStartIndex, int i)
	{
		bytes[iStartIndex + 0] = (byte)(i >> 24);
		bytes[iStartIndex + 1] = (byte)(i >> 16);
		bytes[iStartIndex + 2] = (byte)(i >> 8);
		bytes[iStartIndex + 3] = (byte)i;
	}

	static void Short2Bytes(byte[] bytes, int iStartIndex, short s)
	{
		bytes[iStartIndex + 0] = (byte)(s >> 8);
		bytes[iStartIndex + 1] = (byte)s;
	}

	static public void ClearAllPacketData()
	{
		Datapack obj = GetUnpackInst();
		if (obj != null)
			obj.ClearPacketData();
		obj = GetPackInst();
		if (obj != null)
			obj.ClearPacketData();
	}

	public void ClearPacketData()
	{
		m_Buff = new byte[4096];
		m_Size = 0;
		m_UnpackPos = 0;
	}
	
	public byte[] GetPackData()
	{
		byte[] packData = new byte[m_Size];
		Array.Copy(m_Buff, packData, m_Size);
		return packData;
	}

	public void SetPackData(byte[] bytes, int iStartIndex, int iLength)
	{
		if (iLength == 0)
			iLength = bytes.Length;
		if (m_Buff == null || m_Buff.Length < iLength)
			m_Buff = new byte[iLength];
		Array.Copy(bytes, iStartIndex, m_Buff, 0, iLength);
		m_Size = iLength;
	}

	public void SetPackData(byte[] bytes)
	{
		SetPackData(bytes, 0, 0);
	}

	public void SetPackData(string s, string sCharset)
	{
		SetPackData(Encoding.GetEncoding(sCharset).GetBytes(s));
	}

	public void SetPackData(string s)
	{
		SetPackData(s, "utf-8");
	}

	public void SetForUnpack(byte[] bytes, int iStartIndex, int iLength)
	{
		SetPackData(bytes, iStartIndex, iLength);
		m_UnpackPos = 4;
	}

	public void SetForUnpack(byte[] bytes)
	{
		SetForUnpack(bytes, 0, 0);
	}

	public void SetForUnpack(string s, string sCharset)
	{
		SetPackData(s, sCharset);
		m_UnpackPos = 4;
	}

	public void SetForUnpack(string s)
	{
		SetForUnpack(s, "utf-8");
	}

	public byte UnpackByte()
	{
		return m_Buff[m_UnpackPos++];
	}

	public char UnpackChar()
	{
		return (char)m_Buff[m_UnpackPos++];
	}

	public short UnpackShort()
	{
		short ret = Bytes2Short(m_Buff, m_UnpackPos);
		m_UnpackPos += 2;
		return ret;
	}

	public ushort UnpackUShort()
	{
		return (ushort)UnpackShort();
	}

	/*public int UnpackInt()
	{
		int ret = Bytes2Int(m_Buff, m_UnpackPos);
		m_UnpackPos += 4;
		return ret;
	}

	public uint UnpackUInt()
	{
		return (uint)UnpackInt();
	}*/

	public long UnpackLong()
	{
		long ret = Bytes2Long(m_Buff, m_UnpackPos);
		m_UnpackPos += 8;
		return ret;
	}

	public ulong UnpackULong()
	{
		return (ulong)UnpackLong();
	}

	public long UnpackInt()
	{
		return UnpackLong();
	}
	
	public ulong UnpackUInt()
	{
		return UnpackULong();
	}

	public string UnpackString(int iCount, string sCharset)
	{
		Encoding e = Encoding.GetEncoding(sCharset);
		if (iCount == 0)
			iCount = m_Size - m_UnpackPos;
		char[] chars = e.GetChars(m_Buff, m_UnpackPos, iCount);
		m_UnpackPos += iCount;
		return new string(chars);
	}

	public string UnpackString(int iLength)
	{
		if(iLength == 0)
			return "";
		return UnpackString(iLength, "utf-8");
	}

	public byte[] UnpackBytes(int iLength)
	{
		if (iLength == 0)
			iLength = m_Size - m_UnpackPos;
		byte[] bs = new byte[iLength];
		Array.Copy(m_Buff, m_UnpackPos, bs, 0, iLength);
		m_UnpackPos += iLength;
		return bs;
	}

	public void PacketPrepare(int iMaxSize)
	{
		if (m_Buff == null || m_Buff.Length < iMaxSize)
			m_Buff = new byte[iMaxSize];
		m_Size = 4;
		Int2Bytes(m_Buff, 0, m_Size);

	}

	public void PacketPrepare()
	{	

		PacketPrepare(256);
	}

	public void PackByte(byte b)
	{
		m_Buff[m_Size++] = b;
		Int2Bytes(m_Buff, 0, m_Size);
	}

	public void PackChar(char c)
	{
		PackByte((byte)c);
	}

	public void PackShort(short s)
	{
		Short2Bytes(m_Buff, m_Size, s);
		m_Size += 2;
		Int2Bytes(m_Buff, 0, m_Size);
	}

	public void PackUShort(ushort us)
	{
		PackShort((short)us);
	}

	/*public void PackInt(int i)
	{
		Int2Bytes(m_Buff, m_Size, i);
		m_Size += 4;
		Int2Bytes(m_Buff, 0, m_Size);
	}

	public void PackUInt(uint ui)
	{
		PackInt((int)ui);
	}*/

	public void PackLong(long l)
	{
		Long2Bytes(m_Buff, m_Size, l);
		m_Size += 8;
		Int2Bytes(m_Buff, 0, m_Size);
	}

	public void PackULong(ulong ul)
	{
		PackLong((long)ul);
	}

	public void PackInt(long l)
	{
		PackLong(l);
	}
	
	public void PackUInt(ulong ul)
	{
		PackULong(ul);
	}

	public void PackString(string s, int iLength, string sCharset)
	{
		Encoding e = Encoding.GetEncoding(sCharset);
		byte[] bytes = e.GetBytes(s);
		if (iLength > bytes.Length || iLength == 0)
			iLength = bytes.Length;
		Array.Copy(bytes, 0, m_Buff, m_Size, iLength);
		m_Size += iLength;
		Int2Bytes(m_Buff, 0, m_Size);
	}

	public void PackString(string s, int iLength, bool packLen = false)
	{
        //是否先打包字符长度
        if (packLen)
        {
        Encoding e = Encoding.GetEncoding("utf-8");
        byte[] bytes = e.GetBytes(s);
        Debug.Log(bytes.Length);
        PackInt(bytes.Length);
        }

		PackString(s, iLength, "utf-8");

	}

	public void PackBytes(byte[] bs, int iStart, int iLength)
	{
		Array.Copy(bs, iStart, m_Buff, m_Size, iLength);
		m_Size += iLength;
		Int2Bytes(m_Buff, 0, m_Size);
	}

	public void PackBytes(byte[] bs, int iLength)
	{
		PackBytes(bs, 0, iLength);
	}

	public void PackBytes(byte[] bs)
	{
		PackBytes(bs, 0, bs.Length);
	}
	
	public static int GetStringBytesAmount(string s, string sCharset)
	{
		Encoding e = Encoding.GetEncoding(sCharset);
		byte[] bytes = e.GetBytes(s);
		return bytes.Length;
	}
	
	public static int GetStringBytesAmount(string s)
	{
		return GetStringBytesAmount(s, "utf-8");
	}
}
