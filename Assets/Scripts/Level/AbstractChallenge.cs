using System;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class Challenge 
{

    public int CreditsReward = 1;
    public string Description = "No description for this challenge";
    public bool Completed = false;
    private readonly GlobalStates globalStates;

    public event EventHandler OnCompletion;

    public Challenge(int creditsReward, string description)
    {
        globalStates = GlobalStates.Instance;
        this.CreditsReward = creditsReward;
        this.Description = description;
        OnCompletion += (object sender, EventArgs e) =>
        {
            Completed = true;
            globalStates.Credits += CreditsReward;
        };
    }

    /// <summary>
    /// Invoke OnCompletion at the right time
    /// </summary>
    public abstract void Check();
    

}
