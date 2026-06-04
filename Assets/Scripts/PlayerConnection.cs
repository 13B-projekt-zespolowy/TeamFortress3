using PurrNet;

public class PlayerConnection : NetworkBehaviour
{
    public static PlayerConnection Local;
    private SyncVar<PlayerClass> selectedClass = new();

    public SyncTimer respawnTimer = new();

    [ServerRpc]
    public void ChooseClassServerRpc(PlayerClass chosenClass)
    {
        if (chosenClass == null) return;

        selectedClass.value = chosenClass;
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

    public PlayerClass GetClass() => selectedClass.value;

    private void Respawn() => GameManager.Instance.RespawnPlayer((PlayerID)owner);
}