using UnityEngine;

public enum Abilities
{
    NONE,
    REWIND,
    TIME_FREEZE
}


public class AbilityController : MonoBehaviour
{

    private GlobalStates globalStates;
    private GameObject abilitySource;
    public GameObject RewindPrefab;
    public GameObject TimeFreezePrefab;

    private void Start()
    {
        globalStates = GlobalStates.Instance;
        abilitySource = GameObject.FindGameObjectWithTag("AbilitySource");
        InitializeFirstEquippedAbilitiy();
        InitializeSecondEquippedAbilitiy();
    }

    private void InitializeFirstEquippedAbilitiy()
    {
        switch (globalStates.FirstAbility)
        {
            case Abilities.REWIND:
                Instantiate(RewindPrefab, abilitySource.transform);
                break;
            case Abilities.TIME_FREEZE:
                Instantiate(TimeFreezePrefab, abilitySource.transform);
                break;
            default:
                break;
        }
    }

    private void InitializeSecondEquippedAbilitiy()
    {
        if (globalStates.FirstAbility == globalStates.SecondAbility)
        {
            Debug.Log("Can't have two of the same abilities at the same time...");
            return;
        }

        switch (globalStates.SecondAbility)
        {
            case Abilities.REWIND:
                Instantiate(RewindPrefab, abilitySource.transform);
                break;
            case Abilities.TIME_FREEZE:
                Instantiate(TimeFreezePrefab, abilitySource.transform);
                break;
            default:
                break;
        }
    }

}
