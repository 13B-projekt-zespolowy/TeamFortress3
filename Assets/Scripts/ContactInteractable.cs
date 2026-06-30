using PurrNet;
using UnityEngine;
using System;

/// <summary>
/// Abstract base class for interactable objects that respond to contact-based interactions.
/// Provides server-authoritative interaction handling with network synchronization.
/// Supports destruction-based interaction results with event callbacks.
/// </summary>
public abstract class ContactInteractable : NetworkBehaviour
{
    /// <summary>
    /// Defines the possible results of an interaction.
    /// </summary>
    public enum InteractionResultAction
    {
        /// <summary>No additional action required.</summary>
        None,
        /// <summary>Destroy the interactable object.</summary>
        Destroy
    }
    
    /// <summary>
    /// Event that gets emitted when the object is destroyed through interaction.
    /// </summary>
    public event Action<GameObject> OnDestroyed;
    
    /// <summary>
    /// Called when a player interacts with this object.
    /// Must be implemented by derived classes to define specific interaction behavior.
    /// </summary>
    /// <param name="sender">The GameObject that initiated the interaction.</param>
    /// <returns>The resulting action to take after interaction.</returns>
    public abstract InteractionResultAction OnInteractedWith(GameObject sender);

    /// <summary>
    /// Server RPC that handles interaction requests from clients.
    /// Processes the interaction and handles any resulting actions.
    /// </summary>
    /// <param name="sender">The GameObject that initiated the interaction.</param>
    [ServerRpc]
    public void HandleInteraction(GameObject sender)
    {
        var action = OnInteractedWith(sender);
        switch (action)
        {
            case InteractionResultAction.None:
                return;
            case InteractionResultAction.Destroy:
                // Emit the event before destroying
                OnDestroyed?.Invoke(sender);
                Destroy(gameObject);
                return;
        }
    }
}
