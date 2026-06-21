using PurrNet;
using UnityEngine;
using System;

public abstract class ContactInteractable : NetworkBehaviour
{
    public enum InteractionResultAction
    {
        None,
        Destroy
    }
    
    // Event that gets emitted when the object is destroyed
    public event Action<GameObject> OnDestroyed;
    
    public abstract InteractionResultAction OnInteractedWith(GameObject sender);

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