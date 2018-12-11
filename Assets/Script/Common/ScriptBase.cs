using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrueSync;

//[RequireComponent(typeof(CustomTransform))]
public class ScriptBase : MonoBehaviour
{
    private AssetManager mAssetManager;
    protected AssetManager _AssetManager
    {
        get
        {
            if (mAssetManager == null)
                mAssetManager = Facade.Instance.GetManager<AssetManager>(FacadeConfig.ChildSystem_Asset);
            return mAssetManager;
        }
    }

    private ViewManager mViewManager;
    protected ViewManager _ViewManager
    {
        get
        {
            if (mViewManager == null)
                mViewManager = Facade.Instance.GetManager<ViewManager>(FacadeConfig.ChildSystem_View);
            return mViewManager;
        }
    }

    private UdpReciveManager mUdpReciveManager;
    protected UdpReciveManager _UdpReciveManager
    {
        get
        {
            if (mUdpReciveManager == null)
                mUdpReciveManager = Facade.Instance.GetManager<UdpReciveManager>(FacadeConfig.ChildSystem_UdpRecive);
            return mUdpReciveManager;
        }
    }

    private UdpSendManager mUdpSendManager;
    protected UdpSendManager _UdpSendManager
    {
        get
        {
            if (mUdpSendManager == null)
                mUdpSendManager = Facade.Instance.GetManager<UdpSendManager>(FacadeConfig.ChildSystem_UdpSend);
            return mUdpSendManager;
        }
    }

    private UnityUdpSocket mUnityUdpSocket;
    protected UnityUdpSocket _UnityUdpSocket
    {
        get
        {
            if (mUnityUdpSocket == null)
                mUnityUdpSocket = Facade.Instance.GetManager<UnityUdpSocket>(FacadeConfig.ChildSystem_UdpSocket);
            return mUnityUdpSocket;
        }
    }

    private TcpReciveManager mTcpReciveManager;
    protected TcpReciveManager _TcpReciveManager
    {
        get
        {
            if (mTcpReciveManager == null)
                mTcpReciveManager = Facade.Instance.GetManager<TcpReciveManager>(FacadeConfig.ChildSystem_TcpRecive);
            return mTcpReciveManager;
        }
    }

    private TcpSendManager mTcpSendManager;
    protected TcpSendManager _TcpSendManager
    {
        get
        {
            if (mTcpSendManager == null)
                mTcpSendManager = Facade.Instance.GetManager<TcpSendManager>(FacadeConfig.ChildSystem_TcpSend);
            return mTcpSendManager;
        }
    }

    private UnityTcpSocket mUnityTcpSocket;
    protected UnityTcpSocket _UnityTcpSocket
    {
        get
        {
            if (mUnityTcpSocket == null)
                mUnityTcpSocket = Facade.Instance.GetManager<UnityTcpSocket>(FacadeConfig.ChildSystem_TcpSocket);
            return mUnityTcpSocket;
        }
    }

    private DaoBiaoManager mDaoBiaoManager;
    protected DaoBiaoManager _DaoBiaoManager
    {
        get
        {
            if (mDaoBiaoManager == null)
                mDaoBiaoManager = Facade.Instance.GetManager<DaoBiaoManager>(FacadeConfig.ChildSystem_DaoBiao);
            return mDaoBiaoManager;
        }
    }

    private HUDFPS mHUDFPS;
    protected HUDFPS _HUDFPS
    {
        get
        {
            if (mHUDFPS == null)
                mHUDFPS = Facade.Instance.GetManager<HUDFPS>(FacadeConfig.ChildSystem_HUDFPS);
            return mHUDFPS;
        }
    }
}
