using PurrNet;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the core game logic including player spawning, respawning, team balancing, and session management.
/// Handles network synchronization for player-related operations.
/// </summary>
public class GameManager : NetworkBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private GameObject sceneCamera;
    //[SerializeField] private Transform spawnPointsRoot;
    [SerializeField] private Transform redSpawnPointsRoot;
    [SerializeField] private Transform blueSpawnPointsRoot;
    [SerializeField] private float respawnTime = 5f;

    public static GameManager Instance;
    private Dictionary<PlayerID, PlayerSession> sessions = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Despawn();
        else
            Instance = this;
    }

    /// <summary>
    /// Spawns a new player for the specified player connection.
    /// Assigns a balanced team and initializes player components.
    /// </summary>
    /// <param name="player">The PlayerID of the player to spawn.</param>
    /// <param name="conn">The PlayerConnection instance.</param>
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

        Team assignedTeam = GetBalancedTeam();

        Transform spawnPoint = GetSpawnPoint(assignedTeam);
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

            if (obj.TryGetComponent(out PlayerTeam pTeam))
                pTeam.InitializeTeam(assignedTeam);

            if (obj.TryGetComponent(out PlayerHealth health))
                health.Initialize(playerClass.maxHealth);
        }
    }

    /// <summary>
    /// Starts the respawn countdown for a player.
    /// Server-only operation.
    /// </summary>
    /// <param name="player">The PlayerID of the player to respawn.</param>
    public void StartRespawnCountdown(PlayerID player)
    {
        if (!isServer) return;

        if (sessions.TryGetValue(player, out PlayerSession session))
            session.connection.respawnTimer.StartTimer(respawnTime);
    }

    /// <summary>
    /// Respawns a player at an appropriate spawn point based on their team.
    /// Server-only operation.
    /// </summary>
    /// <param name="player">The PlayerID of the player to respawn.</param>
    public void RespawnPlayer(PlayerID player)
    {
        if (!isServer) return;

        if (sessions.TryGetValue(player, out PlayerSession session) && session.playerObject != null)
        {
            NetworkIdentity playerObject = session.playerObject;

            Team team = Team.Red;
            if (playerObject.TryGetComponent(out PlayerTeam pt)) 
                team = pt.Team;

            Transform spawnPoint = GetSpawnPoint(team);
            Vector3 spawnPos = (spawnPoint) ? spawnPoint.position : Vector3.zero;
            Quaternion spawnRot = (spawnPoint) ? spawnPoint.rotation : Quaternion.identity;

            RespawnPlayerRpc(playerObject, spawnPos, spawnRot);
        }
    }

    /// <summary>
    /// Removes a player from the game session and despawns their object.
    /// </summary>
    /// <param name="player">The PlayerID of the player to remove.</param>
    public void RemovePlayer(PlayerID player)
    {
        if (sessions.TryGetValue(player, out PlayerSession session))
        {
            if (session.playerObject.TryGetComponent<PlayerFlagCarry>(out PlayerFlagCarry flagCarry)) 
                if (flagCarry.carriedFlag != null) flagCarry.carriedFlag.ReturnToBase();

            if (session.playerObject != null)
                session.playerObject.Despawn();

            sessions.Remove(player);
        }
    }

    /// <summary>
    /// Gets the scene camera GameObject.
    /// </summary>
    /// <returns>The scene camera GameObject.</returns>
    public GameObject GetSceneCamera() => sceneCamera;

    /// <summary>
    /// RPC that respawns a player at the specified position and rotation.
    /// Restores health, ammo, and reactivates the player object.
    /// </summary>
    /// <param name="playerObject">The NetworkIdentity of the player object.</param>
    /// <param name="position">The spawn position.</param>
    /// <param name="rotation">The spawn rotation.</param>
    [ObserversRpc]
    private void RespawnPlayerRpc(NetworkIdentity playerObject, Vector3 position, Quaternion rotation)
    {
        if (!playerObject.TryGetComponent(out PlayerHealth health) || !playerObject.TryGetComponent(out PlayerShooter shooter)) return;

        // RESET VELOCITY
        if (playerObject.TryGetComponent(out PlayerController controller))
            controller.ResetVelocity();

        // SET POSITION
        Transform playerTransform = playerObject.transform;
        playerTransform.position = position;
        playerTransform.rotation = rotation;
        
        // RESET STATS
        health.RefillHealth();
        shooter.RefillAmmo();

        // SPAWN
        playerObject.gameObject.SetActive(true);
        if (playerObject.TryGetComponent(out PlayerVisuals visuals)) visuals.SwitchWeapon(0);
    }

    /// <summary>
    /// Determines the team with fewer players and assigns the new player to that team for balance.
    /// </summary>
    /// <returns>The team with fewer active players.</returns>
    private Team GetBalancedTeam()
    {
        int red = 0, blue = 0;
        foreach (var session in sessions.Values)
        {
            if (session.playerObject != null && session.playerObject.TryGetComponent(out PlayerTeam pt))
            {
                if (pt.Team == Team.Red) 
                    red++;
                else 
                    blue++;
            }
        }
        return red <= blue ? Team.Red : Team.Blue;
    }

    /// <summary>
    /// Gets a random spawn point for the specified team.
    /// </summary>
    /// <param name="team">The team to get a spawn point for.</param>
    /// <returns>A Transform of the spawn point, or null if none exist.</returns>
    private Transform GetSpawnPoint(Team team)
    {
        Transform root = team == Team.Red ? redSpawnPointsRoot : blueSpawnPointsRoot;
        return root ? root.GetChild(Random.Range(0, root.childCount)) : null;
    }

    /*private Transform GetSpawnPoint()
    {
        return spawnPointsRoot ? spawnPointsRoot.GetChild(Random.Range(0, spawnPointsRoot.childCount)) : null;
    }*/
}

/// <summary>
/// Represents a player session containing their connection and player object.
/// </summary>
class PlayerSession
{
    public PlayerConnection connection;
    public NetworkIdentity playerObject;
}
