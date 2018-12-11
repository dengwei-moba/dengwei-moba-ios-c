using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ViewAttr))]
public abstract class ViewBase : ScriptBase
{
    public Button mCloseBtn;
    private ViewAttr mViewAttr;
    /// <summary>
    /// 用于收集界面加载的对象路径，以便关闭界面是统一卸载
    /// 层次由低到高添加到队列中
    /// </summary>
    private List<string> mAssetPathList = new List<string>();

    public virtual void Awake()
    {
        mViewAttr = transform.GetComponent<ViewAttr>();
    }

    public virtual void Start()
    {
        mViewAttr.Open();

        if (mCloseBtn != null)
            mCloseBtn.onClick.AddListener(Close);
    }

    public virtual void OnDestroy()
    {
        for (int i = 0; i < mAssetPathList.Count; i++)
            _AssetManager.UnLoadUnUseAsset(mAssetPathList[i]);
    }

    public virtual void Close()
    {
        mViewAttr.Close();
    }    

    public void RecordLoadPath(string tPath)
    {
        mAssetPathList.Add(tPath);
    }

    /// <summary>
    /// 获取资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tPath"></param>
    /// <returns></returns>
    public T GetAsset<T>(string tPath) where T : class
    {
        T mAsset = default(T);
        mAsset = _AssetManager.GetAsset<T>(tPath);
        RecordLoadPath(tPath);
        return mAsset;
    }

    /// <summary>
    /// 获取GameObject
    /// 注意：
    /// 界面加载的对象：一种普通的ui对象，另外一种特效，普通ui对象的层次需要手动设置，特效的话，可能有render属性，
    ///     显示等级同Canvas，为了方便管理层次，规定：所有特效的根物体都要挂RendererAttr脚本，层次从低到高顺序添加特效包含的几个renders
    /// </summary>
    /// <param name="tPath"></param>
    /// <returns></returns>
    public GameObject GetGameObject(string tPath)
    {
        GameObject mObj = _AssetManager.GetGameObject(tPath);
        RendererAttr mRendererAttr = mObj.GetComponent<RendererAttr>();
        if (mRendererAttr != null)
            mViewAttr.AddRenderer(mRendererAttr);
        RecordLoadPath(tPath);
        return mObj;
    }
}
