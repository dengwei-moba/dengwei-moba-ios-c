using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum ViewTypeEnum
{
    main = 0,
    common = 1,
    loading = 2,
    message = 3,
    loadingcircle = 4,
}

public enum ViewAnimationEnum
{
    none = 0,
    fadeIn = 1,
    fadeOut = 2,
    scaleIn = 3,
    scaleOut = 4,
}

public class ViewAttr : ScriptBase
{ 
    public ViewTypeEnum mViewType;
    public ViewAnimationEnum mInAnimation;
    public ViewAnimationEnum mOutAnimation;
    public Transform mAnimationTargetTrans;

    /// <summary>
    /// 需要显示的所有Canvas和RendererAttr
    /// </summary>
    public List<Transform> mRendererLayerList = new List<Transform>();

    public void Open()
    {
        if (mAnimationTargetTrans == null)
            return;
        switch (mInAnimation)
        {
            case ViewAnimationEnum.fadeIn:
                //...
                break;
            case ViewAnimationEnum.scaleIn:
                mAnimationTargetTrans.DOLocalMove(Vector3.zero, 0.15f).From();
                break;
            default:
                break;
        }
    }

    public void Close()
    {
        if (mAnimationTargetTrans == null)
            return;
        switch (mOutAnimation)
        {
            case ViewAnimationEnum.fadeOut:
                //...
                Destroy(gameObject);
                break;
            case ViewAnimationEnum.scaleOut:
                mAnimationTargetTrans.DOLocalMove(Vector3.zero, 0.15f).onComplete = () => { Destroy(gameObject); };
                break;
            default:
                Destroy(gameObject);
                break;
        }
    }

    public void AddRenderer(RendererAttr tRendererAttr)
    {
        if (!mRendererLayerList.Contains(tRendererAttr.transform))
            mRendererLayerList.Add(tRendererAttr.transform);
        _ViewManager.UpdateView(this);
    }

    public void RemoveRenderer(RendererAttr tRendererAttr)
    {
        if (mRendererLayerList.Contains(tRendererAttr.transform))
            mRendererLayerList.Remove(tRendererAttr.transform);
        _ViewManager.UpdateView(this);
    }

    void OnDestroy()
    {
        _ViewManager.UnLoadView(this);
    }
}
