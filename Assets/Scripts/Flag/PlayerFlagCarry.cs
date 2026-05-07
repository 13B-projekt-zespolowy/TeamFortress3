using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFlagCarry : NetworkBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionReference dropFlagAction;

    public Transform holdPoint;

    public Flag carriedFlag;

    private void Awake()
    {
        if (isOwner)
            dropFlagAction.action.performed += _ => Drop();
    }

    [ServerRpc]
    public void Drop()
    {
        if (carriedFlag == null)
            return;
        
        carriedFlag.Drop(transform.position);
        carriedFlag = null;
    }
}