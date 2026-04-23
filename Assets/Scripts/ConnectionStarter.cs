using System.Collections;
using PurrLobby;
using PurrNet;
using PurrNet.Steam;
using PurrNet.Transports;
using PurrNet.Utils;
using Steamworks;
using UnityEngine;

[RequireComponent(typeof(NetworkManager), typeof(UDPTransport), typeof(SteamTransport))]
public class ConnectionStarter : MonoBehaviour
{
    [Header("These only apply when not connecting through Steam.")]
    public StartFlags udpServerStartFlags = StartFlags.ServerBuild | StartFlags.Editor;
    public StartFlags udpClientStartFlags = StartFlags.ClientBuild | StartFlags.Editor | StartFlags.Clone;

    private NetworkManager networkManager;
    // We assume that if a lobby data holder was found, we should connect through steam and through UDP otherwise.
    private LobbyDataHolder lobbyDataHolder; 

    private SteamTransport steamTransport;
    private UDPTransport udpTransport;
    
    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
        lobbyDataHolder = FindFirstObjectByType<LobbyDataHolder>();

        steamTransport = GetComponent<SteamTransport>();
        steamTransport.enabled = false;
        udpTransport = GetComponent<UDPTransport>();
        udpTransport.enabled = false;
    }
    
    private void Start()
    {
        var startServer = false;
        var startClient = false;

        if (lobbyDataHolder)
        {
            // Connect through steam.

            steamTransport.enabled = true;
            networkManager.transport = steamTransport;
            steamTransport.peerToPeer = true;
            steamTransport.dedicatedServer = false;

            if (!lobbyDataHolder)
            {
                Debug.LogError($"Failed to start connection. {nameof(LobbyDataHolder)} is null!", this);
                return;
            }

            if (!lobbyDataHolder.CurrentLobby.IsValid)
            {
                Debug.LogError($"Failed to start connection. Lobby is invalid!", this);
                return;
            }

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

            startServer = lobbyDataHolder.CurrentLobby.IsOwner;
            startClient = true;
        }
        else
        {
            // Connect through UDP.

            udpTransport.enabled = true;
            networkManager.transport = udpTransport;

            startServer = ShouldStart(udpServerStartFlags);
            startClient = ShouldStart(udpClientStartFlags);
        }

        if (startServer)
            networkManager.StartServer();

        if (startClient)
            StartCoroutine(StartClient());
    }

    private IEnumerator StartClient()
    {
        // Brief delay to ensure server is fully listening before client connects.
        yield return new WaitForSeconds(1f);
        networkManager.StartClient();
    }

    private static bool ShouldStart(StartFlags flags)
    {
        return (flags.HasFlag(StartFlags.Editor) && ApplicationContext.isMainEditor) ||
               (flags.HasFlag(StartFlags.Clone) && ApplicationContext.isClone) ||
               (flags.HasFlag(StartFlags.ClientBuild) && ApplicationContext.isClientBuild) ||
               (flags.HasFlag(StartFlags.ServerBuild) && ApplicationContext.isServerBuild);
    }
}
