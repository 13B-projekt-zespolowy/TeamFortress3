using System.Collections;
using PurrLobby;
using PurrNet;
using PurrNet.Logging;
using PurrNet.Steam;
using Steamworks;
using UnityEngine;

public class ConnectionStarter : MonoBehaviour
{
    private NetworkManager networkManager;
    private LobbyDataHolder lobbyDataHolder;
    
    private void Awake()
    {
        if (!TryGetComponent(out networkManager))
        {
            PurrLogger.LogError($"Failed to get {nameof(NetworkManager)} component.", this);
            return;
        }
        
        lobbyDataHolder = FindFirstObjectByType<LobbyDataHolder>();

        if (!lobbyDataHolder)
        {
            PurrLogger.LogError($"Failed to get {nameof(LobbyDataHolder)} component.", this);
            return;
        }
    }
    
    private void Start()
    {
        if (!networkManager)
        {
            PurrLogger.LogError($"Failed to start connection. {nameof(NetworkManager)} is null!", this);
            return;
        }
        
        if (!lobbyDataHolder)
        {
            PurrLogger.LogError($"Failed to start connection. {nameof(LobbyDataHolder)} is null!", this);
            return;
        }

        if (!lobbyDataHolder.CurrentLobby.IsValid)
        {
            PurrLogger.LogError($"Failed to start connection. Lobby is invalid!", this);
            return;
        }

        if (networkManager.transport is SteamTransport)
        {
            var steamTransport = networkManager.transport as SteamTransport;
            steamTransport.peerToPeer = true;
            steamTransport.dedicatedServer = false;
            if (ulong.TryParse(lobbyDataHolder.CurrentLobby.LobbyId, out var parsedId))
            {
                var steamLobbyId = new CSteamID(parsedId);
                Debug.Log($"Lobby Owner's SteamID: {SteamMatchmaking.GetLobbyOwner(steamLobbyId)}");
                if (Steamworks.SteamMatchmaking.GetLobbyGameServer(steamLobbyId,
                                                                   out var gameServerIP,
                                                                   out var gameServerPort,
                                                                   out var gameServerSteamID))
                {
                    Debug.Log($"gameServerIP: {gameServerIP}");
                    Debug.Log($"gameServerPort: {gameServerPort}");
                    Debug.Log($"gameServerSteamID: {gameServerSteamID}");
                    steamTransport.address = gameServerSteamID.ToString();
                }
            }
        }

        if (lobbyDataHolder.CurrentLobby.IsOwner)
            networkManager.StartServer();

        StartCoroutine(StartClient());
    }

    private IEnumerator StartClient()
    {
        // Brief delay to ensure server is fully listening before client connects.
        yield return new WaitForSeconds(1f);
        networkManager.StartClient();
    }
}
