using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using TrueSync;


public interface IRofBase
{
    int ReadBody(byte[] rData, int nOffset);
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ROFPathAttribute : System.Attribute
{
    public string AssetName;
    public ROFPathAttribute(string rAssetName)
    {
        this.AssetName = rAssetName;
    }
}

public class RofTable<T> where T : IRofBase, new()
{
    private int             mColNum;
    private int             mRowNum;

    private Dictionary<int, T> mIDMap;
    private Dictionary<int, int> mRowMap;
        
    public int              RowNum  { get { return mRowNum;   } }
    public int              ColNum  { get { return mColNum;   } }

    public RofTable(byte[] rTotalBuffer)
    {
        mIDMap = new Dictionary<int, T>();
        mRowMap = new Dictionary<int, int>();

        int nOffset = 64;

        if (BitConverter.IsLittleEndian) { Array.Reverse(rTotalBuffer, nOffset, 4); }
        mRowNum = (int)BitConverter.ToUInt32(rTotalBuffer, nOffset); nOffset += 4;
        //Debug.Log("mRowNum=>" + mRowNum);
        if (BitConverter.IsLittleEndian) { Array.Reverse(rTotalBuffer, nOffset, 4); }
        mColNum = (int)BitConverter.ToUInt32(rTotalBuffer, nOffset); nOffset += 4;
        //Debug.Log("mColNum=>" + mColNum);
        //解析头
        for (int i = 0; i < mColNum; i++)
        {
            int nNameLen = (int)rTotalBuffer[nOffset];
            nOffset += 1 + nNameLen + 2;
        }

        //解析行
        for (int i = 0; i < mRowNum; i++)
        {
            if (BitConverter.IsLittleEndian) { Array.Reverse(rTotalBuffer, nOffset, 4); }
            int nID = (int)BitConverter.ToUInt32(rTotalBuffer, nOffset);
            //Debug.Log("nID=>" + nID);
            if (BitConverter.IsLittleEndian) { Array.Reverse(rTotalBuffer, nOffset, 4); }

            T rModel = new T();
            nOffset = rModel.ReadBody(rTotalBuffer, nOffset);
            mIDMap.Add(nID, rModel);
            mRowMap.Add(i, nID);
        }
    }
        
    public T GetDataByID(int nID)
    {
        if (mIDMap.ContainsKey(nID) == false)
        {
            return default(T);
        }
        return mIDMap[nID];
    }

    public T GetDataByRow(int nIndex)
    {
        if (mRowMap.ContainsKey(nIndex) == false)
        {
            return default(T);
        }
        int nID = mRowMap[nIndex];
        return mIDMap[nID];
    }
}

public class DaoBiaoManager : ScriptBase
{
    //加配置表往下写
    [ROFPath("RofBuff")]
    public RofTable<RofBuffRow>         RofBuffTable { get; private set; }

    Dictionary<string, byte[]> dic = new Dictionary<string, byte[]>();
    void Start()
    {
        string filepath = "rofbuff_bytes";
        UnityEngine.TextAsset assetObj = (UnityEngine.TextAsset)_AssetManager.GetAsset<UnityEngine.Object>(AppConst.mDaoBiao_Path + filepath);
        if (assetObj != null) { 
            RofBuffTable = new RofTable<RofBuffRow>(assetObj.bytes);
        }
        //StartCoroutine(StartLoadDaoBiao(filepath));
    }

    IEnumerator StartLoadDaoBiao(string filepath)
    {
        AssetBundle mDaoBiaoBundle = AssetBundle.LoadFromFile(AppConst.LoadRes_Root_Path + AppConst.mDaoBiao_Path + filepath);
        string obj_name_suff = filepath.Remove(0, filepath.LastIndexOf('_') + 1);
        string obj_name = filepath.Remove(filepath.LastIndexOf('_'), filepath.Length - filepath.LastIndexOf('_')) + "." + obj_name_suff;
        Debug.Log("===>" + obj_name);
        UnityEngine.TextAsset assetObj = (UnityEngine.TextAsset)mDaoBiaoBundle.LoadAsset(obj_name);
        RofBuffTable = new RofTable<RofBuffRow>(assetObj.bytes);
        Debug.Log("===>" + RofBuffTable.GetDataByID(10001).Num);
        yield return null;
    }
}

