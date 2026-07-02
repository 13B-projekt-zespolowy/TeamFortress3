using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages flag carrying functionality for a player.
/// Handles dropping the currently carried flag via input.
/// </summary>
public class PlayerFlagCarry : NetworkBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionReference dropFlagAction;

    /// <summary>
    /// The transform point where the carried flag is attached.
    /// </summary>
    public Transform holdPoint;

    /// <summary>
    /// Reference to the flag currently being carried by this player.
    /// Null if not carrying any flag.
    /// </summary>
    public Flag carriedFlag;

    /// <summary>
    /// Initializes input bindings for the local player.
    /// </summary>
    private void Awake()
    {
        if (isOwner)
            dropFlagAction.action.performed += _ => Drop();
    }

    /// <summary>
    /// Drops the currently carried flag at the player's current position.
    /// Server RPC ensures server-authoritative execution.
    /// </summary>
    [ServerRpc]
    public void Drop()
    {
        if (carriedFlag == null)
            return;
        
        carriedFlag.Drop(transform.position);
        carriedFlag = null;
    }

    protected override void OnDespawned()
    {
        Drop();
    }
}
