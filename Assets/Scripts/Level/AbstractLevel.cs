using UnityEngine;

/// <summary>
/// Each Scene (LEVEL) will have a root gameObject with a script extending Level
/// </summary>
public abstract class Level : MonoBehaviour
{

    public string LevelID;
    public Challenge[] challenges;

    private void Update()
    {
        foreach (Challenge challenge in challenges) 
            if (!challenge.Completed) challenge.Check();
    }

}
