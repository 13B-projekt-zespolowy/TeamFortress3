using PurrNet;
using UnityEngine;

/// <summary>
/// Manages the team affiliation of a player in a networked multiplayer environment.
/// The team value is synchronized across the network and can only be set by the server.
/// </summary>
public class PlayerTeam : NetworkBehaviour
{
    [SerializeField] private SyncVar<Team> team;

    /// <summary>
    /// Gets or sets the player's team. Setting this value updates the synchronized network variable.
    /// </summary>
    public Team Team {
        get => team.value;
        set => team.value = value;
    }

    /// <summary>
    /// Initializes the player's team assignment.
    /// This operation is server-only to maintain authority over team assignments.
    /// </summary>
    /// <param name="assignedTeam">The team to assign to the player.</param>
    public void InitializeTeam(Team assignedTeam)
    {
        if (!isServer) return;
        team.value = assignedTeam;
    }
}
