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
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float jumpForceVertical = 10.0f;
    [SerializeField] private float jumpForceHorizontal = 1.0f;
    [SerializeField] private Vector3 gravity = Vector3.up * -40.0f;
    [SerializeField] private float groundAcceleration = 15f;
    [SerializeField] private float groundDeceleration = 20f;
    [SerializeField] private float airAcceleration = 5f;
    [SerializeField] private float directionChangeMultiplier = 3f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    private float coyoteTimer = 0f;
    private float jumpBufferTimer = 0f;
    private bool pendingJump;


    [Header("Camera Settings")]
    [SerializeField] private float cameraSensitivity = 20.0f;
    [SerializeField] private float baseCameraHeight = 0.5f;
    [SerializeField] private float crouchCameraHeight = 0f;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference crouchAction;

    private Vector2 moveInput = Vector2.zero;
    private float cameraHeight;
    private MovementType movementType = MovementType.Walking;

    private bool crouchPressed = false;
    private float rotationX = 0.0f;
    private Vector3 jumpVelocity = Vector3.zero;
    private Vector3 walkMotion = Vector3.zero;

    private CharacterController controller;
    private Camera playerCamera;

    private Vector3 currentVelocity = Vector3.zero;

    private bool wantsUncrouch = false;

    private bool CanUncrouch()
    {
        float fullHeight = 2f;
        float crouchHeight = 1f;
        float heightDifference = fullHeight - crouchHeight;

        Vector3 bottom = transform.position + controller.center + Vector3.up * (fullHeight / 2f - controller.radius);
        Vector3 top = transform.position + controller.center + Vector3.up * (fullHeight / 2f + heightDifference - controller.radius);

        return !Physics.CheckCapsule(bottom, top, controller.radius - 0.01f, ~LayerMask.GetMask("Player"));
    }

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
    }

    private void StartCrouching()
    {
        wantsUncrouch = false;
        crouchPressed = true;
        controller.height = 1;
        controller.center = new Vector3(0, -0.5f, 0);
    }

    private void StopCrouching()
    {
        wantsUncrouch = true;
    }

    private void HandleUncrouching()
    {
        if (!wantsUncrouch || !CanUncrouch()) return;
        crouchPressed = false;
        controller.height = 2;
        controller.center = new Vector3(0, 0, 0);
        wantsUncrouch = false;
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
        pendingJump = true;
        jumpBufferTimer = jumpBufferTime;
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


        if (controller.isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpBufferTimer > 0)
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (pendingJump && jumpBufferTimer > 0 && coyoteTimer > 0)
        {

            jumpVelocity =
            jumpForceVertical * Vector3.up +
            jumpForceHorizontal * moveInput.y * transform.forward +
            jumpForceHorizontal * moveInput.x * transform.right;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
            pendingJump = false;
        }


        if (!controller.isGrounded)
        {
            jumpVelocity += gravity * Time.deltaTime;
        }

        if (controller.isGrounded && jumpVelocity.y < 0)
        {
            jumpVelocity = Vector3.zero;
        }

        float movementSpeed = movementType == MovementType.Crouching ? crouchSpeed : walkSpeed;



        Vector3 targetVelocity = walkMotion * movementSpeed;

        float dot = Vector3.Dot(currentVelocity.normalized, targetVelocity.normalized);
        float directionMultiplier = dot < 0 ? directionChangeMultiplier : 1f;

        float accel = controller.isGrounded
            ? (targetVelocity.magnitude > 0.01f ? groundAcceleration : groundDeceleration)
            : airAcceleration;

        currentVelocity = Vector3.MoveTowards(
            currentVelocity,
            targetVelocity,
            accel * directionMultiplier * Time.deltaTime
        );

        currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, accel * Time.deltaTime);
        var finalMove = currentVelocity + jumpVelocity;
        controller.Move(finalMove * Time.deltaTime);


    }

    private void Update()
    {
        UpdateCameraHeight();
        MouseLook();
        HandleUncrouching();
        Motion();
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