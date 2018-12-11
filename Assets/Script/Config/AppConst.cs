using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppConst
{
    /// <summary>
    /// 加载资源根目录
    /// </summary>
    public static string LoadRes_Root_Path
    {
        get
        {
            return Res_Original_Path;
#if UNITY_IPHONE && !UNITY_EDITOR
            return Application.persistentDataPath + "/AssetBundle/";
#elif UNITY_ANDROID && !UNITY_EDITOR
            return Application.persistentDataPath + "/AssetBundle/";
#elif UNITY_STANDALONE_WIN && !UNITY_EDITOR
            return Application.dataPath + "/StreamingAssets/AssetBundle/";
#endif
            return Application.dataPath + "/StreamingAssets/AssetBundle/";
        }
    }

    /// <summary>
    /// 游戏未安装前的资源目录
    /// </summary>
    public static string Res_Original_Path
    {
        get
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            return Application.dataPath + "/Raw/AssetBundle/";
#elif UNITY_ANDROID && !UNITY_EDITOR
            return "jar:file://" + Application.dataPath + "!/assets/AssetBundle/";
#elif UNITY_STANDALONE_WIN && !UNITY_EDITOR
            return Application.dataPath + "/StreamingAssets/AssetBundle/";
#endif
            return Application.dataPath + "/StreamingAssets/AssetBundle/";
        }
    }

    /// <summary>
    /// 用www加载路径
    /// </summary>
    public static string WWW_Url
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return "jar:file://" + Application.dataPath + "!/assets/AssetBundle/";
#elif UNITY_IPHONE && !UNITY_EDITOR
	        return	Application.dataPath + "/Raw/AssetBundle/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
            return "file://" + Application.dataPath + "/StreamingAssets/AssetBundle/";
#endif
        }
    }

    #region 资源路径配置
    public static string mHeroPath = "prefab/hero/";
    public static string mScene_Path = "scene/";
    public static string mDaoBiao_Path = "daobiao/binarydata/";
    #endregion 资源路径配置
}
