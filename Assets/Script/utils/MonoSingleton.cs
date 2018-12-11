using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    protected static T m_Instance = null;
    public static T Instance
    {
        get
        {
            // Instance requiered for the first time, we look for it
            if (m_Instance == null)
            {
                m_Instance = GameObject.FindObjectOfType(typeof(T)) as T;

                if (m_Instance == null)
                {
                    Debug.LogErrorFormat("{0} is not found", typeof(T));
                }
                else
                {
                    m_Instance.Init();
                }

            }
            return m_Instance;
        }
    }

    /// <summary>
    /// 检查单例的实体是否存在；【也许已经释放了呢】
    /// </summary>
    public static bool IsInstance
    {
        get { return m_Instance != null; }
    }

    public virtual void Destroy()
    {
        GameObject.Destroy(this.gameObject);
        m_Instance = null;
    }

    // If no other monobehaviour request the instance in an awake function
    // executing before this one, no need to search the object.
    /*
	private void Awake()
	{
		if( m_Instance == null )
		{
			m_Instance = this as T;
			m_Instance.Init();
		}
	}
    */

    // This function is called when the instance is used the first time
    // Put all the initializations you need here, as you would do in Awake
    public virtual void Init() { }

    // Make sure the instance isn't referenced anymore when the user quit, just in case.
    private void OnApplicationQuit()
    {
        m_Instance = null;
    }
}