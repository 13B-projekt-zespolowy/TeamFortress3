using PurrNet;
using UnityEngine;

/// <summary>
/// Spawns network objects at the spawner's position with a configurable respawn delay.
/// The spawned object must have a ContactInteractable component to handle destruction events.
/// </summary>
public class ItemSpawner : NetworkBehaviour
{
    /// <summary>
    /// Object to spawn (note: object needs to have a ContactInteractable component).
    /// </summary>
    [SerializeField] private NetworkIdentity spawnObject;



    /// <summary>
    /// Object to show when there is no current item
    /// </summary>
    [SerializeField] private GameObject placeholderObject;

    /// <summary>
    /// Time after which the object respawns after being destroyed.
    /// </summary>
    [SerializeField] private float respawnTime;

    private NetworkIdentity instance;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        Spawn();
    }

    /// <summary>
    /// Called when the spawned instance is destroyed.
    /// Starts the respawn timer.
    /// </summary>
    /// <param name="object">The destroyed GameObject.</param>
    private void OnInstanceDestroyed(GameObject @object)
    {
        placeholderObject.SetActive(true);
        Invoke(nameof(Spawn), respawnTime);
    }

    /// <summary>
    /// Spawns a new instance of the spawnObject at the spawner's transform.
    /// Server-only operation.
    /// </summary>
    [ServerRpc]
    private void Spawn()
    {
        placeholderObject.SetActive(false);
        instance = Instantiate(spawnObject, transform);
        instance.GetComponent<ContactInteractable>().OnDestroyed += OnInstanceDestroyed;
    }
}
