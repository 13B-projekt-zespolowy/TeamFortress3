using PurrNet;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private Transform spawnPointsRoot;

    public static GameManager Instance;
    private Dictionary<PlayerID, PlayerSession> sessions = new();

    private void Awake()
    {
        if(Instance != null && Instance != this)
            Despawn();
        else
            Instance = this;
    }

    public void SpawnPlayer(PlayerID player, PlayerConnection conn)
    {
        if (!sessions.TryGetValue(player, out PlayerSession session))
        {
            session = new PlayerSession { connection = conn };
            sessions[player] = session;
        }

        if (session.playerObject != null)
        {
            session.playerObject.Despawn();
            session.playerObject = null;
        }

        PlayerClass playerClass = session.connection.GetClass();
        if (playerClass == null || playerClass.playerPrefab == null) return;

        Transform spawnPoint = GetSpawnPoint();
        Vector3 spawnPos = (spawnPoint) ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRot = (spawnPoint) ? spawnPoint.rotation : Quaternion.identity;

        GameObject obj = UnityProxy.Instantiate(
            playerClass.playerPrefab,
            spawnPos,
            spawnRot,
            gameObject.scene
        );

        if (obj.TryGetComponent(out NetworkIdentity identity))
        {
            identity.GiveOwnership(player);
            session.playerObject = identity;

            if (obj.TryGetComponent(out PlayerHealth health))
                health.Initialize(playerClass.maxHealth);
        }
    }

    public void RespawnPlayer(PlayerID player, GameObject playerObject)
    {
        if (!isServer) return;

        Transform spawnPoint = GetSpawnPoint();
        Vector3 spawnPos = (spawnPoint) ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRot = (spawnPoint) ? spawnPoint.rotation : Quaternion.identity;

        if (playerObject.TryGetComponent(out PlayerHealth health))
            health.RespawnSnapRpc(spawnPos, spawnRot);

    }

    public void RemovePlayer(PlayerID player)
    {
        if (sessions.TryGetValue(player, out PlayerSession session))
        {
            if (session.playerObject != null)
                session.playerObject.Despawn();

            sessions.Remove(player);
        }
    }

    private Transform GetSpawnPoint()
    {
        return (spawnPointsRoot) ? spawnPointsRoot.GetChild(Random.Range(0, spawnPointsRoot.childCount)) : null;
    }
}

class PlayerSession
{
    public PlayerConnection connection;
    public NetworkIdentity playerObject;
}