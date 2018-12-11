using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Facade : MonoBehaviour
{
    public static Facade Instance;

    private Dictionary<string, Component> mManagerDic = new Dictionary<string, Component>();

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitManager();
    }

    void InitManager()
    {
        AddManager(FacadeConfig.ChildSystem_Asset, gameObject.AddComponent<AssetManager>());
        AddManager(FacadeConfig.ChildSystem_View, gameObject.AddComponent<ViewManager>());

        AddManager(FacadeConfig.ChildSystem_UdpSocket, gameObject.AddComponent<UnityUdpSocket>());
        AddManager(FacadeConfig.ChildSystem_UdpRecive, gameObject.AddComponent<UdpReciveManager>());
        AddManager(FacadeConfig.ChildSystem_UdpSend, gameObject.AddComponent<UdpSendManager>());

        AddManager(FacadeConfig.ChildSystem_TcpSocket, gameObject.AddComponent<UnityTcpSocket>());
        AddManager(FacadeConfig.ChildSystem_TcpRecive, gameObject.AddComponent<TcpReciveManager>());
        AddManager(FacadeConfig.ChildSystem_TcpSend, gameObject.AddComponent<TcpSendManager>());

        AddManager(FacadeConfig.ChildSystem_DaoBiao, gameObject.AddComponent<DaoBiaoManager>());

        AddManager(FacadeConfig.ChildSystem_HUDFPS, gameObject.AddComponent<HUDFPS>());
    }

    void Start()
    {
        GetManager<ViewManager>(FacadeConfig.ChildSystem_View).LoadView("prefab/ui/notifyview_prefab");
        GetManager<ViewManager>(FacadeConfig.ChildSystem_View).LoadView("prefab/ui/loginview_prefab");
    }

    public void AddManager<T>(string tName, T tCom) where T : Component
    {
        mManagerDic[tName] = tCom;
    }

    public T GetManager<T>(string tName) where T : Component
    {
        Component com;
        if (mManagerDic.TryGetValue(tName, out com))
            return com as T;
        else
            return default(T);
    }
}
