using PurrNet;
using UnityEngine;

/// <summary>
/// Detects trigger collisions and forwards interaction events to ContactInteractable components.
/// Used as a trigger volume on players or objects that can initiate interactions.
/// </summary>
[RequireComponent(typeof(Collider))]
class ContactInteractor : NetworkBehaviour
{
    /// <summary>
    /// Called when another collider enters the trigger volume.
    /// Attempts to find a ContactInteractable on the entering object and triggers interaction.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out ContactInteractable interactable)) return;
        interactable.HandleInteraction(gameObject.transform.parent.gameObject);
    }
}
