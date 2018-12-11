using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AssetCiteData
{
    public AssetBundle p_AssetObj;
    public int p_CiteCount;
    public bool p_LoadFinish;

    static public AssetCiteData Defult()
    {
        AssetCiteData n_Asset;
        n_Asset.p_AssetObj = null;
        n_Asset.p_CiteCount = 0;
        n_Asset.p_LoadFinish = false;
        return n_Asset;
    }
}

public class AssetManager : ScriptBase
{
    private AssetBundle m_MainAssetBundle;
    private AssetBundleManifest m_MainAssetBundleManifest;
    private Dictionary<string, AssetCiteData> d_CiteData = new Dictionary<string, AssetCiteData>();// 缓存对象列表key:文件名
    private Dictionary<string, UnityEngine.Object> d_ObjData = new Dictionary<string, UnityEngine.Object>();//缓存加载对象实例列表key:文件路径 val:LoadAsset


    #region 内部处理
    /// <summary>
    /// AssetBundle的资源主文件
    /// </summary>
    private AssetBundle main_assetbundle
    {
        get
        {
            if (m_MainAssetBundle == null)
            {
                m_MainAssetBundle = AssetBundle.LoadFromFile(AppConst.LoadRes_Root_Path + "AssetBundle");
            }
            return m_MainAssetBundle;
        }
    }
    /// <summary>
    /// 创建所有资源的引用
    /// </summary>
    private AssetBundleManifest main_assetbundle_manifest
    {
        get
        {
            if (m_MainAssetBundleManifest == null)
                m_MainAssetBundleManifest = main_assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            return m_MainAssetBundleManifest;
        }
    }
    /// <summary>
    /// AssetBundle引用对象信息
    /// </summary>
    ///
    private AssetCiteData GetAssetCiteInfo(string path)
    {
        if (d_CiteData.ContainsKey(path))
            return d_CiteData[path];
        return AssetCiteData.Defult();
    }
    /// <summary>
    /// AssetBundle增加对象信息
    /// </summary>
    ///
    private void AddAssetCiteData(string path, AssetCiteData data)
    {
        if (!d_CiteData.ContainsKey(path))
            d_CiteData.Add(path, data);
    }
    /// <summary>
    /// AssetBundle引用对象信息是否存在
    /// </summary>
    ///
    private bool HasAssetCite(string path)
    {
        return d_CiteData.ContainsKey(path);
    }
    /// <summary>
    /// 递归加载资源
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="type">类型</param>
    /// <param name="is_dependencies">是否是依赖资源</param>
    /// <returns></returns>
    private UnityEngine.Object LoadAsset(string path, System.Type type, bool is_dependencies = false)
    {
        path = path.Replace(".", "_");
        AssetBundle ab = null;
        string[] assetbundleDependencies = main_assetbundle_manifest.GetDirectDependencies(path);

        for (int i = 0; i < assetbundleDependencies.Length; i++)
        {
            LoadAsset(assetbundleDependencies[i], null, true);
        }
        if (!HasAssetCite(path))
        {
            //Debug.Log("加载资源路径:" + path);
            ab = AssetBundle.LoadFromFile(AppConst.LoadRes_Root_Path + path);
            if (ab != null)
            {
                AssetCiteData acd = new AssetCiteData();
                acd.p_AssetObj = ab;
                acd.p_CiteCount += 1;
                acd.p_LoadFinish = true;
                AddAssetCiteData(path, acd);
            }
        }
        else
        {
            if (GetAssetCiteInfo(path).p_AssetObj != null)
                ab = GetAssetCiteInfo(path).p_AssetObj;
            SetAssetCiteCount(path, 1);
        }
        UnityEngine.Object retObj = null;
        if (!is_dependencies)
        {

            if (d_ObjData.ContainsKey(path))
                return d_ObjData[path];
            string obj_name = path.Substring(path.LastIndexOf("/") + 1);
            string obj_name_suff = obj_name.Remove(0, obj_name.LastIndexOf('_') + 1);
            obj_name = obj_name.Remove(obj_name.LastIndexOf('_'), obj_name.Length - obj_name.LastIndexOf('_')) + "." + obj_name_suff;
            retObj = ab.LoadAsset(obj_name);
            d_ObjData.Add(path, retObj);
        }
        return retObj;
    }
    /// <summary>
    /// 将加载资源实例化到游戏场景
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns></returns>
    private GameObject LoadGameObj(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
        //Debug.LogError("加载资源:" + path);
        UnityEngine.Object assetObj = this.LoadAsset(path, typeof(GameObject)) as UnityEngine.Object;
        if (assetObj == null)
            return null;
        assetObj = UnityEngine.Object.Instantiate(assetObj);
        assetObj.name = assetObj.name.Replace("(Clone)", "");
        return (GameObject)assetObj;
    }
    #endregion

    #region 对外接口

    /// <summary>
    /// AssetBundle设置引用对象计数
    /// </summary>
    ///
    private void SetAssetCiteCount(string path, int count)
    {
        if (d_CiteData.Count > 0)
        {
            if (d_CiteData.ContainsKey(path))
            {
                AssetCiteData acd = d_CiteData[path];
                acd.p_CiteCount += count;
                d_CiteData[path] = acd;
            }
        }
    }

    /// <summary>
    /// AssetBundle卸载,is_recycled是否立即回收内存
    /// </summary>
    ///
    public void UnLoadUnUseAsset(string path, bool is_dependencies = false)
    {
        path = path.Replace(".", "_");
        SetAssetCiteCount(path, -1);
        AssetCiteData acd = GetAssetCiteInfo(path);
        if (acd.p_AssetObj == null) return;
        if (acd.p_CiteCount == 0)
        {
            acd.p_AssetObj.Unload(true);
            acd.p_AssetObj = null;
            d_CiteData.Remove(path);
            //Debug.Log("卸载资源路径:" + path);
        }
        if (!is_dependencies)
        {
            if (d_ObjData.ContainsKey(path))
            {
                d_ObjData.Remove(path);
            }
        }
        string[] assetbundleDependencies = main_assetbundle_manifest.GetDirectDependencies(path);
        for (int i = 0; i < assetbundleDependencies.Length; i++)
        {
            UnLoadUnUseAsset(assetbundleDependencies[i], true);
        }
    }

    /// <summary>
    /// 加载对象在指定节点下
    /// </summary>
    /// <typeparam name="T">返回泛型对象</typeparam>
    /// <param name="path">路径</param>
    /// <param name="parent">节点</param>
    /// <param name="is_add">是否增加</param>
    /// <returns></returns>
    public T GetGameObjectReturnCompent<T>(string path, Transform parent, bool is_add = false) where T : Component
    {
        GameObject go = LoadGameObj(path);
        go.transform.parent = parent;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        if (go == null)
            return null;
        T comp = (!is_add) ? go.GetComponent<T>() : go.AddComponent<T>();
        if (comp == null)
            return null;
        return comp;
    }
    /// <summary>
    /// 普通的资源加载
    /// </summary>
    /// <param name="path"></param>
    /// <param name="obj_name"></param>
    /// <returns></returns>
    public GameObject GetGameObject(string path, string obj_name = null)
    {
        return LoadGameObj(path);
    }
    /// <summary>
    /// 加载资源并返回组件对象
    /// </summary>
    /// <typeparam name="T">组件对象</typeparam>
    /// <param name="path">路径</param>
    /// <param name="objname">名字</param>
    /// <returns></returns>
    public T GetAsset<T>(string path, string objname = null) where T : class
    {
        return this.LoadAsset(path, typeof(T)) as T;
    }

    #endregion
}
