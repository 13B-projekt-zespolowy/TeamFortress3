using PurrNet;
using UnityEngine;

/// <summary>
/// Represents the capture point or home base for a team's flag.
/// Handles scoring when an enemy player enters the base while carrying their team's flag.
/// Only server-authoritative operations are permitted.
/// </summary>
public class FlagBase : NetworkBehaviour
{
    [SerializeField] private Team team;
    public Team Team => team;

    /// <summary>
    /// Handles trigger collision with players entering the base.
    /// - If a player is carrying a flag from a different team, the flag is captured.
    /// - Increases the capturing team's score and returns the flag to its base.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer)
            return;

        if (other.CompareTag("Player"))
        {
            var flag = other.GetComponent<PlayerFlagCarry>().carriedFlag;

            if (flag != null && flag.Team != team)
            {
                ModeManager.Instance.IncreaseScore(team);
                flag.ReturnToBase();
            }
        }
    }
}
