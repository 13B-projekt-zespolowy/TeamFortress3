using PurrNet;
using UnityEngine;

public abstract class ContactInteractable : NetworkBehaviour
{
    public enum OnEndInteractAction
    {
        None,
        Destroy
    }
    public OnEndInteractAction onEndInteractAction; 
    
    public abstract void OnInteractedWith();

    [ServerRpc]
    public void HandleInteraction()
    {
        OnInteractedWith();
        switch(onEndInteractAction)
        {
            case OnEndInteractAction.None:
                return;
            case OnEndInteractAction.Destroy:
                Destroy(gameObject);
                return;
        }
    }

}