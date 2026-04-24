using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private InputMode initialInputMode = InputMode.Gameplay;
    public InputMode InputMode { get; private set; }

    public static InputManager Instance { get; private set; }

    private InputActionMap gameplayActions;
    private InputActionMap uiActions;
    private InputActionMap alwaysActions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        gameplayActions = inputActions.FindActionMap("Gameplay");
        uiActions = inputActions.FindActionMap("UI");
        alwaysActions = inputActions.FindActionMap("Always");
        SwitchInputMode(initialInputMode);
    }

    public void SwitchInputMode(InputMode mode)
    {
        InputMode = mode;

        switch (InputMode)
        {
            case InputMode.Gameplay:
                uiActions.Disable();
                gameplayActions.Enable();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
            case InputMode.Ui:
                gameplayActions.Disable();
                uiActions.Enable();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        alwaysActions.Enable();
    }
}

public enum InputMode
{
    Gameplay,
    Ui,
}
