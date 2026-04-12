using PurrNet;
using UnityEngine;

[RequireComponent(typeof(Collider))]
class ContactInteractor : NetworkBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out ContactInteractable interactable)) return;
        interactable.HandleInteraction();
    }
}