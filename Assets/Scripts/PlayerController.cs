using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

enum MovementType { 
    Walking, 
    Sprinting, 
    Crouching // Not implemented
}

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float cameraSensitivity = 20.0f;

    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float runSpeed = 6.0f;

    [SerializeField] private float jumpForce = 20.0f;
    
    [SerializeField] private Vector3 gravity = Vector3.up * -80.0f;


    private MovementType movementType = MovementType.Walking;


    private float baseCameraFov;
    private float targetCameraFov;

    private float rotationX = 0.0f;
    private Vector3 jumpVelocity = Vector3.zero;
    private Vector3 walkMotion = Vector3.zero;

    private CharacterController controller;
    private Camera playerCamera;

    private InputAction lookAction;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    private void Awake()
    {
        playerCamera = transform.Find("PlayerCamera").GetComponent<Camera>();
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        lookAction = InputSystem.actions.FindAction("Look");
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        
        jumpAction.performed += OnJumpActionPerformed;

        baseCameraFov = playerCamera.fieldOfView;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    protected override void OnSpawned()
    {
        enabled = isOwner;

        if (!isOwner)
        {
            Destroy(playerCamera.gameObject);
        }
    }

    private void OnJumpActionPerformed(InputAction.CallbackContext context)
    {
        if (!controller.isGrounded)
        {
            return;
        }
        jumpVelocity = Vector3.up * jumpForce;
    }

    void MouseLook() 
    {
        Vector2 lookDelta = lookAction.ReadValue<Vector2>();
        rotationX -= lookDelta.y * cameraSensitivity * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, -90, 90);
        transform.Rotate(Vector3.up, lookDelta.x * cameraSensitivity * Time.deltaTime);
        playerCamera.transform.localEulerAngles = Vector3.right * rotationX;
    }

    void Motion() 
    {
        movementType =  MovementType.Walking;
        walkMotion = Vector3.zero;

        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        if (moveInput.y > 0 && sprintAction.IsPressed())
        {
          movementType = MovementType.Sprinting;
        }

        walkMotion += transform.right * moveInput.x;
        walkMotion += transform.forward * moveInput.y;
        walkMotion.Normalize();

        jumpVelocity += gravity * Time.deltaTime;

        float movementSpeed = movementType == MovementType.Sprinting ? runSpeed : walkSpeed;

        controller.Move((walkMotion * movementSpeed + jumpVelocity)* Time.deltaTime);
    }

    void Update()
    {
        MouseLook();
        Motion();
        UpdateCameraFov();
    }

    private void UpdateCameraFov()
    { 
        targetCameraFov = baseCameraFov * (movementType == MovementType.Sprinting  && walkMotion.magnitude > 0 ? 1.25f : 1f); 
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetCameraFov, Time.deltaTime * 4.0f);
    }
}
