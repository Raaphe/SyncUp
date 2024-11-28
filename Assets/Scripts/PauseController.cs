using System;
using UnityEngine;

public class PauseController : MonoBehaviour
{

    public PauseController Instance { get; private set; }
    public bool GameIsPaused = false;
    public KeyCode PauseKey = KeyCode.None;
    private PlayerController playerController;
    private AudioManager audioManager;

    public Canvas GameOverlay;
    public Canvas PauseOverlay;
    public Canvas DeathOverlay;

    public event EventHandler OnPauseEnter;
    public event EventHandler OnPauseLeave;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else 
            Destroy(this);
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        if (PauseKey == KeyCode.None) PauseKey = KeyCode.Escape;
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        PauseOverlay.gameObject.SetActive(false);
        DeathOverlay.gameObject.gameObject.SetActive(false);

        OnPauseEnter += (object sender, EventArgs e) => 
        {
            playerController.PlayerCanMove = false;
            GameIsPaused = true;
            Cursor.lockState = CursorLockMode.None;
            PauseOverlay.gameObject.SetActive(true);
            GameOverlay.gameObject.SetActive(false);
            audioManager.StopCapture();
        };

        OnPauseLeave += (object sender, EventArgs e) =>
        {
            playerController.PlayerCanMove = true;
            GameIsPaused = false;
            Cursor.lockState = CursorLockMode.Locked;
            PauseOverlay.gameObject.SetActive(false);
            GameOverlay.gameObject.SetActive(true);
            audioManager.StartCapture();
        };
    }

    public void Pause() 
    {
        Time.timeScale = 0f;
        OnPauseEnter?.Invoke(this, EventArgs.Empty);
    }

    public void UnPause()
    {
        Time.timeScale = 1f;
        OnPauseLeave?.Invoke(this, EventArgs.Empty);
    }

    public void TogglePause()
    {
        if (GameIsPaused)
            UnPause();
        else
            Pause();
    }

    private void Update()
    {
        if (Input.GetKeyDown(PauseKey))
            TogglePause();
    }

    public void ActuateDeathOverlay()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        playerController.PlayerCanMove = false;
        GameOverlay.gameObject.SetActive(false);
        PauseOverlay.gameObject.SetActive(false);
        DeathOverlay.gameObject.SetActive(true);
    }
}
