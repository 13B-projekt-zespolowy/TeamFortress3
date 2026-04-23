using System;
using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

enum MovementType
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

    private InputAction lookAction;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction crouchAction;

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
        crouchAction = InputSystem.actions.FindAction("Crouch");

        jumpAction.performed += OnJumpActionPerformed;
        crouchAction.performed += _ => StartCrouching();
        crouchAction.canceled += _ => StopCrouching();

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
        Vector2 lookDelta = lookAction.ReadValue<Vector2>();
        rotationX -= lookDelta.y * cameraSensitivity * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, -90, 90);
        transform.Rotate(Vector3.up, lookDelta.x * cameraSensitivity * Time.deltaTime);
        playerCamera.transform.localEulerAngles = Vector3.right * rotationX;
    }

    void Motion()
    {
        movementType = crouchPressed ? MovementType.Crouching : MovementType.Walking;
        walkMotion = Vector3.zero;
        moveInput = moveAction.ReadValue<Vector2>();

        walkMotion += transform.right * moveInput.x;
        walkMotion += transform.forward * moveInput.y;

        if (walkMotion.magnitude > 1) walkMotion.Normalize();

        jumpVelocity += gravity * Time.deltaTime;

        if (controller.isGrounded && jumpVelocity.y < 0)
        {
            jumpVelocity.y = -5f;
        }

        float movementSpeed = movementType == MovementType.Crouching ? crouchSpeed : walkSpeed;
        controller.Move((walkMotion * movementSpeed + jumpVelocity) * Time.deltaTime);
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