using PurrNet;
using UnityEngine;

public abstract class ContactInteractable : NetworkBehaviour
{
    public enum InteractionResultAction
    {
        None,
        Destroy
    }
    
    public abstract InteractionResultAction OnInteractedWith(GameObject sender);

    [ServerRpc]
    public void HandleInteraction(GameObject sender)
    {
        var action = OnInteractedWith(sender);
        switch(action)
        {
            case InteractionResultAction.None:
                return;
            case InteractionResultAction.Destroy:
                Destroy(gameObject);
                return;
        }
    }

}