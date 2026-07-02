using PurrNet;

/// <summary>
/// Represents a player's network connection and manages class selection, respawn timers, and player spawning.
/// Handles server-authoritative class selection and coordinates with GameManager for player spawning.
/// </summary>
public class PlayerConnection : NetworkBehaviour
{
    public static PlayerConnection Local;
    private SyncVar<PlayerClass> selectedClass = new();
    private SyncVar<Team?> selectedTeam = new();


    public SyncTimer respawnTimer = new();

    /// <summary>
    /// Server RPC for choosing a player class.
    /// Spawns the player after class selection.
    /// </summary>
    /// <param name="chosenClass">The PlayerClass to select.</param>
    [ServerRpc]
    public void ChooseClassServerRpc(PlayerClass chosenClass)
    {
        if (chosenClass == null) return;

        selectedClass.value = chosenClass;
        GameManager.Instance.SpawnPlayer((PlayerID)owner, this);
    }

    /// <summary>
    /// Server RPC for switching a team.
    /// Spawns the player changing team.
    /// </summary>
    /// <param name="team">The Team to select.</param>
    [ServerRpc]
    public void ChooseTeamServerRpc(Team? team)
    {
        selectedTeam.value = team;
        GameManager.Instance.SpawnPlayer((PlayerID)owner, this);
    }




    protected override void OnSpawned()
    {
        if (isOwner)
            Local = this;

        if (isServer)
            respawnTimer.onTimerEnd += Respawn;
    }

    protected override void OnDespawned()
    {
        if (Local == this)
            Local = null;

        if (isServer)
        {
            respawnTimer.onTimerEnd -= Respawn;
            GameManager.Instance.RemovePlayer((PlayerID)owner);
        }
    }

    /// <summary>
    /// Gets the currently selected player class.
    /// </summary>
    /// <returns>The selected PlayerClass, or null if none selected.</returns>
    public PlayerClass GetClass() => selectedClass.value;

    /// <summary>
    /// Gets the currently selected team.
    /// </summary>
    /// <returns>The selected Team, or null if none selected.</returns>
    public Team? GetSelectedTeam() => selectedTeam.value;


    /// <summary>
    /// Called when the respawn timer ends. Triggers player respawn.
    /// </summary>
    private void Respawn() => GameManager.Instance.RespawnPlayer((PlayerID)owner);
}
