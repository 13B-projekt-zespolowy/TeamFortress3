using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages input modes and action maps for the game.
/// Supports switching between gameplay and UI input modes with cursor state management.
/// Singleton pattern ensures global access to input controls.
/// </summary>
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

    /// <summary>
    /// Switches the active input mode and updates cursor visibility/lock state accordingly.
    /// </summary>
    /// <param name="mode">The input mode to switch to (Gameplay or UI).</param>
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

/// <summary>
/// Defines the available input modes for the game.
/// </summary>
public enum InputMode
{
    /// <summary>Gameplay mode with cursor locked and hidden.</summary>
    Gameplay,
    /// <summary>UI mode with cursor visible and unlocked.</summary>
    Ui,
}
