using UnityEngine;

[DisallowMultipleComponent]
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public bool dontDestroyOnLoad;

    private static volatile T _instance;
    private static readonly object SyncRoot = new System.Object();

    public static T Instance
    {
        get
        {
            Initialize();
            return _instance;
        }
    }

    public static bool IsInitialized => _instance != null;

    public static void Initialize()
    {
        if (_instance != null)
        {
            return;
        }

        lock (SyncRoot)
        {
            _instance = FindObjectOfType<T>();

            if (_instance == null)
            {
                var go = new GameObject(typeof(T).FullName);
                _instance = go.AddComponent<T>();
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            Debug.LogError(GetType().Name + " Singleton class is already created.");
        }

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(this);
        }

        OnAwake();
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    protected virtual void OnAwake()
    {
    }
}