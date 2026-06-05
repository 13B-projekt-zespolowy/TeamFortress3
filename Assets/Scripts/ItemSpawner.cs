using PurrNet;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{

    /**
     <summary>Object to spawn (note: object needs to have a ContactInteractable component)</summary> 
    */
    [SerializeField] private NetworkIdentity spawnObject;


    /**
     <summary>Time after which the objects spawns again</summary> 
    */
    [SerializeField] private float respawnTime;

    private NetworkIdentity instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();
        Spawn();

    }
    
    


    private void OnInstanceDestroyed(GameObject @object)
    {
        Invoke(nameof(Spawn), respawnTime);
    }

    [ServerRpc]
    private void Spawn()
    {
        instance = Instantiate(spawnObject, transform);
        instance.GetComponent<ContactInteractable>().OnDestroyed += OnInstanceDestroyed;
    }
}
