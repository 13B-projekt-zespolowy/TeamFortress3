using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MovementType
{
    Walking,
    Crouching
}

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float cameraSensitivity = 20.0f;
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float jumpForceVertical = 10.0f;
    [SerializeField] private float jumpForceHorizontal = 1.0f;
    [SerializeField] private Vector3 gravity = Vector3.up * -40.0f; 

    [Header("Camera Settings")]
    [SerializeField] private float baseCameraHeight = 0.5f;
    [SerializeField] private float crouchCameraHeight = 0f;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference crouchAction;

    [HideInInspector] public bool canMove = true;

    private Vector2 moveInput = Vector2.zero;
    private float cameraHeight;
    private MovementType movementType = MovementType.Walking;

    private bool crouchPressed = false;
    private float rotationX = 0.0f;
    private Vector3 jumpVelocity = Vector3.zero;
    private Vector3 walkMotion = Vector3.zero;

    private CharacterController controller;
    private Camera playerCamera;

    private void Awake()
    {
        playerCamera = transform.Find("PlayerCamera").GetComponent<Camera>();
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        jumpAction.action.performed += OnJumpActionPerformed;
        crouchAction.action.performed += _ => StartCrouching();
        crouchAction.action.canceled += _ => StopCrouching();

        cameraHeight = baseCameraHeight;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void StartCrouching()
    {
        if (!canMove) return;
        crouchPressed = true;
        controller.height = 1;
        controller.center = new Vector3(0, -0.5f, 0);
    }

    private void StopCrouching()
    {
        crouchPressed = false;
        controller.height = 2;
        controller.center = new Vector3(0, 0, 0);
    }

    protected override void OnSpawned()
    {
        enabled = isOwner;
        if (!isOwner && playerCamera != null)
        {
            Destroy(playerCamera.gameObject);
        }
    }

    private void OnJumpActionPerformed(InputAction.CallbackContext context)
    {
        if (!canMove || !controller.isGrounded) return;

        jumpVelocity =
            jumpForceVertical * Vector3.up +
            jumpForceHorizontal * moveInput.y * transform.forward +
            jumpForceHorizontal * moveInput.x * transform.right;
    }

    void MouseLook()
    {
        Vector2 lookDelta = lookAction.action.ReadValue<Vector2>();
        rotationX -= lookDelta.y * cameraSensitivity * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, -90, 90);
        transform.Rotate(Vector3.up, lookDelta.x * cameraSensitivity * Time.deltaTime);
        playerCamera.transform.localEulerAngles = Vector3.right * rotationX;
    }

    void Motion()
    {
        movementType = crouchPressed ? MovementType.Crouching : MovementType.Walking;
        walkMotion = Vector3.zero;
        moveInput = moveAction.action.ReadValue<Vector2>();

        walkMotion += transform.right * moveInput.x;
        walkMotion += transform.forward * moveInput.y;
        walkMotion = Vector3.ClampMagnitude(walkMotion, 1.0f);

        jumpVelocity += gravity * Time.deltaTime;

        if (controller.isGrounded)
        {
            jumpVelocity.x = 0.0f;
            jumpVelocity.z = 0.0f;
        }

        if (controller.isGrounded && jumpVelocity.y < 0)
        {
            jumpVelocity.y = -5f;
        }

        float movementSpeed = movementType == MovementType.Crouching ? crouchSpeed : walkSpeed;
        var finalMove = walkMotion * movementSpeed + jumpVelocity;
        controller.Move(finalMove * Time.deltaTime);
    }

    void ApplyOnlyGravity()
    {
        jumpVelocity += gravity * Time.deltaTime;

        if (controller.isGrounded && jumpVelocity.y < 0)
        {
            jumpVelocity.y = -5f;
        }

        controller.Move(jumpVelocity * Time.deltaTime);
    }

    void Update()
    {
        UpdateCameraHeight();

        if (canMove)
        {
            MouseLook();
            Motion();
        }
        else
        {
            ApplyOnlyGravity();
        }
    }

    private void UpdateCameraHeight()
    {
        float targetCameraHeight = movementType == MovementType.Crouching ? crouchCameraHeight : baseCameraHeight;
        Vector3 cameraPos = playerCamera.transform.localPosition;
        cameraHeight = Mathf.Lerp(cameraHeight, targetCameraHeight, Time.deltaTime * 5.0f);
        cameraPos.y = cameraHeight;
        playerCamera.transform.localPosition = cameraPos;
    }
}