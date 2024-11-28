using UnityEngine;

/// <summary>
/// SINGLETON FOR GLOBAL VARIABLES
/// GlobalStates s = GlobalStates.Instance;
/// </summary>
public class GlobalStates : MonoBehaviour
{

    public static GlobalStates Instance { get; private set; }
    public int Credits = 0;
    public int CurrentLevel = 1;
    public int ScaledTime = 0;
    public float RealTime = 0f;
    public Abilities FirstAbility;
    public Abilities SecondAbility;   

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
            Destroy(this); //on loading a new scene, object wont be destroyed
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        Time.timeScale = 1f;
    }

    private void UpdateTime()
    {
        ScaledTime = (int)Time.time;
        RealTime = Time.realtimeSinceStartup;
    }

    private void Update()
    {
        UpdateTime();
    }

}
