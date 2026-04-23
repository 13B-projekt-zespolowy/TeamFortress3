using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerConnection : NetworkBehaviour
{
    public static PlayerConnection Local;
    private SyncVar<PlayerClass> selectedClass = new();

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
    }

    protected override void OnDespawned()
    {
        if (Local == this)
            Local = null;

        if (isServer)
            GameManager.Instance.RemovePlayer((PlayerID)owner);
    }

    public PlayerClass GetClass() => selectedClass.value;
}