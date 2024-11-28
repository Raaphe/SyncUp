using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathInterfaceBehavior : MonoBehaviour
{

    public Button QuitButton;
    public Button RestartButton;
    private PauseController pauseController;

    void Start()
    {
        pauseController = GameObject.Find("PauseController").GetComponent<PauseController>();
        QuitButton.onClick.AddListener(Exit);
    }

    private void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
    }

}
