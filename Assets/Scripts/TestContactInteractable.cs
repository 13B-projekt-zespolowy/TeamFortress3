
using UnityEngine;

public class TestContactInteractable : ContactInteractable
{
    public override InteractionResultAction OnInteractedWith(GameObject sender)
    {
        Debug.Log("Interaction!");
        return InteractionResultAction.None;
    }
}