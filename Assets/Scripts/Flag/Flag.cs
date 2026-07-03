using PurrNet;
using UnityEngine;

/// <summary>
/// Represents a flag in a multiplayer capture-the-flag game mode.
/// Handles pickup, drop, return-to-base mechanics, and player collision interactions.
/// Only server-authoritative operations are permitted.
/// </summary>
public class Flag : NetworkBehaviour
{
    [SerializeField] private Team team;
    [SerializeField] private Transform basePosition;
    [SerializeField] private GameTimer gameTimer;

    private PlayerFlagCarry carrier;

    private bool IsCarried => carrier != null;
    public Team Team => team;

    /// <summary>
    /// Initializes the flag position at its base when the server starts.
    /// </summary>
    private void Awake()
    {
        if (!isServer)
            return;

        transform.position = basePosition.position;
    }

    /// <summary>
    /// Picks up the flag by attaching it to a player.
    /// </summary>
    /// <param name="player">The player carrying the flag.</param>
    public void Pickup(PlayerFlagCarry player)
    {
        if (!isServer)
            return;

        carrier = player;
        carrier.carriedFlag = this;
        transform.SetParent(player.holdPoint);
        transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Drops the flag at a specified world position.
    /// </summary>
    /// <param name="pos">The world position where the flag should be dropped.</param>
    public void Drop(Vector3 pos)
    {
        if (!isServer)
            return;

        if (carrier != null)
            carrier.carriedFlag = null;
        carrier = null;
        transform.SetParent(null);
        transform.position = pos;
    }

    /// <summary>
    /// Returns the flag to its base position and clears any carrier reference.
    /// </summary>
    public void ReturnToBase()
    {
        if (!isServer)
            return;

        if (carrier != null)
            carrier.carriedFlag = null;
        carrier = null;
        transform.SetParent(null);
        transform.position = basePosition.position;
    }

    /// <summary>
    /// Handles trigger collision with players.
    /// - If a player from the same team touches the flag, it returns to base.
    /// - If an enemy player touches the flag and it's not carried, the player picks it up.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer)
            return;

        if (!gameTimer.IsRunning) return;

        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerFlagCarry>();
            var playerTeam = other.GetComponent<PlayerTeam>();

            if (playerTeam.Team == team)
            {
                ReturnToBase();
                return;
            }

            if (!IsCarried && player.carriedFlag == null)
                Pickup(player);
        }
    }
}
