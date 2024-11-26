using TMPro;
using UnityEngine;

public class FPSText : MonoBehaviour
{

    private FPSManager fpsManager;
    private TextMeshProUGUI textMeshProUGUI;

    void Start()
    {
        fpsManager = GameObject.FindGameObjectWithTag("FPSManager").GetComponent<FPSManager>();
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        textMeshProUGUI.text = "" + fpsManager.FrameRate;
    }
}
