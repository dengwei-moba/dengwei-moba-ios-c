using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewRoot
{
    private int curDepth = 0;
    private int maxDepth = 0;
    private List<ViewAttr> mViewList = new List<ViewAttr>();

    public ViewRoot(int tCurDepth, int tMaxDepth)
    {
        curDepth = tCurDepth;
        maxDepth = tMaxDepth;
    }

    public void AddChild(ViewAttr tViewAttr)
    {
        if (mViewList.Contains(tViewAttr))
            return;
        mViewList.Add(tViewAttr);
        AutoSetOrder(tViewAttr);
    }

    public void RemoveChild(ViewAttr tViewAttr)
    {
        if (!mViewList.Contains(tViewAttr))
            return;
        int index = mViewList.IndexOf(tViewAttr);
        for (int i = 0; i < tViewAttr.mRendererLayerList.Count; i++)
        {
            Transform trans = tViewAttr.mRendererLayerList[i];
            Canvas panel = trans.GetComponent<Canvas>();
            if (panel != null)
            {
                curDepth = panel.sortingOrder;
                break;
            }
            else
            {
                RendererAttr attr = trans.GetComponent<RendererAttr>();
                if (attr != null)
                {
                    if (attr.mRenderer.Length > 0)
                    {
                        curDepth = attr.mRenderer[0].sortingOrder;
                        break;
                    }
                }
            }
        }
        for (int i = index + 1; i < mViewList.Count; i++)
        {
            AutoSetOrder(mViewList[i]);
        }
        mViewList.Remove(tViewAttr);
    }

    public void UpdateChild(ViewAttr tViewAttr)
    {
        if (!mViewList.Contains(tViewAttr))
            return;
        int index = mViewList.IndexOf(tViewAttr);
        for (int i = 0; i < tViewAttr.mRendererLayerList.Count; i++)
        {
            Transform trans = tViewAttr.mRendererLayerList[i];
            Canvas panel = trans.GetComponent<Canvas>();
            if (panel != null)
            {
                curDepth = panel.sortingOrder;
                break;
            }
            else
            {
                RendererAttr attr = trans.GetComponent<RendererAttr>();
                if (attr != null)
                {
                    if (attr.mRenderer.Length > 0)
                    {
                        curDepth = attr.mRenderer[0].sortingOrder;
                        break;
                    }
                }
            }
        }
        for (int i = index; i < mViewList.Count; i++)
        {
            AutoSetOrder(mViewList[i]);
        }
    }

    void AutoSetOrder(ViewAttr tViewAttr)
    {
        List<Transform> mRendererLayerList = tViewAttr.mRendererLayerList;
        for (int i = 0; i < mRendererLayerList.Count; i++)
        {
            Transform trans = mRendererLayerList[i];
            Canvas panel = trans.GetComponent<Canvas>();
            if (panel != null)
            {
                panel.sortingOrder = curDepth;
                curDepth++;
            }
            else
            {
                RendererAttr attr = trans.GetComponent<RendererAttr>();
                if (attr != null)
                {
                    Renderer[] mRenderer = attr.mRenderer;
                    for (int j = 0; j < mRenderer.Length; j++)
                    {
                        mRenderer[j].sortingOrder = curDepth;
                        curDepth++;
                    }
                }
                if (panel == null && attr == null)
                {
                    Debug.LogError(string.Format("物体:{0}的ViewAttr组件的mRendererLayerList列表中只能添加带Canvas或者RenderAttr组件的Transform，",
                        "物体{1}没有添加这两个组件中任何一个", tViewAttr.gameObject.name, trans.gameObject.name));
                }
            }
        }
    }
}

public class ViewManager : ScriptBase
{
    private Transform mViewRootTrans;

    private Dictionary<ViewTypeEnum, ViewRoot> mViewRootDic = new Dictionary<ViewTypeEnum, ViewRoot>();
    private List<GameObject> mViewObjs = new List<GameObject>();

    void Awake()
    {
        mViewRootTrans = GameObject.Find("Facade/UIRoot").transform;

        mViewRootDic = new Dictionary<ViewTypeEnum, ViewRoot>();
        for (ViewTypeEnum i = ViewTypeEnum.main; i < ViewTypeEnum.loadingcircle; i++)
        {
            int baseIndex = (int)i;
            mViewRootDic[i] = new ViewRoot(baseIndex * 1000, (baseIndex + 1) * 1000);
        }
    }

    public GameObject LoadView(string tPath, Transform tParent = null)
    {
        GameObject viewObj = _AssetManager.GetGameObject(tPath);
        Transform viewTrans = viewObj.transform;
        if (tParent != null)
            viewTrans.SetParent(tParent);
        else
            viewTrans.SetParent(mViewRootTrans);
        viewTrans.localScale = Vector3.one;
        viewTrans.localPosition = Vector3.zero;

        mViewObjs.Add(viewObj);

        ViewBase mViewBase = viewObj.GetComponent<ViewBase>();
        mViewBase.RecordLoadPath(tPath);
        ViewAttr mViewAttr = viewObj.GetComponent<ViewAttr>();
        ViewRoot mViewRoot = mViewRootDic[mViewAttr.mViewType];
        mViewRoot.AddChild(mViewAttr);
        return viewObj;
    }
    public void UnLoadView(ViewAttr tViewAttr)
    {
        if (!mViewObjs.Contains(tViewAttr.gameObject))
            return;
        mViewObjs.Remove(tViewAttr.gameObject);
        ViewRoot mViewRoot = mViewRootDic[tViewAttr.mViewType];
        mViewRoot.RemoveChild(tViewAttr);
    }

    public void UpdateView(ViewAttr tViewAttr)
    {
        if (!mViewObjs.Contains(tViewAttr.gameObject))
            return;
        ViewRoot mViewRoot = mViewRootDic[tViewAttr.mViewType];
        mViewRoot.UpdateChild(tViewAttr);
    }
    public void ClearView()
    {
        for (int i = 0; i < mViewObjs.Count; i++)
        {
            GameObject.Destroy(mViewObjs[i].gameObject);
        }
        mViewObjs.Clear();
    }
}
