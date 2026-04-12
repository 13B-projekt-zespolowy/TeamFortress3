
using UnityEngine;

public class TestContactInteractable : ContactInteractable
{
    public override void OnInteractedWith()
    {
        Debug.Log("Interaction!");
    }
}