using System.Collections;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class TimeFreeze : MonoBehaviour
{
    [Header("Technical Settings")]
    public Camera Camera;

    [Header("Ability Settings")]
    public int AbilityDurationSeconds;

    private PostProcessVolume PostProcessVolume;
    private ColorGrading ColorGrading;

    // Start is called before the first frame update
    void Start()
    {
        AbilityDurationSeconds = 5;
        this.PostProcessVolume = Camera.GetComponent<PostProcessVolume>();
        this.ColorGrading = PostProcessVolume.volumeProfile.GetModule<ColorGrading>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("PRESSED C");
            StartCoroutine(Freeze());

        }
    }

    private IEnumerator Freeze() 
    {
        Debug.Log("Freezing");
        while (PostProcessVolume.weight <= 1)
        {
            PostProcessVolume.weight += 0.01f;
            yield return new WaitForSeconds(0.1f * Time.deltaTime);
        }

        
        

        StartCoroutine(TimingController.Time(TimeType.REALTIME, AbilityDurationSeconds, () =>
        {
            StartCoroutine(UnFreeze());
        }));
    }

    private IEnumerator UnFreeze()
    {
        PostProcessVolume.weight = 1;
        Debug.Log("Un-Freezing");
        while (PostProcessVolume.weight > 0)
        {
            PostProcessVolume.weight -= 0.01f;
            yield return new WaitForSeconds(0.1f * Time.deltaTime);
        }
        PostProcessVolume.weight = 0;
    }

}
