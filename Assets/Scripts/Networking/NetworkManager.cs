using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject player;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void StartServer()
    {
        Application.targetFrameRate = Constants.TICKS_PER_SECOND;
        QualitySettings.vSyncCount = 0;
        Server.Start(50, 26950);
        DontDestroyOnLoad(gameObject);
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

}
