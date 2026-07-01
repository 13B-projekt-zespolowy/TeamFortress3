using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the class selection UI for players before spawning.
/// Handles input switching, menu visibility, and class selection communication with the server.
/// </summary>
public class ClassSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private InputActionReference toggleAction;

    private void OnEnable()
    {
        toggleAction.action.Enable();
        toggleAction.action.performed += OnToggleMenu;
    }

    private void OnDisable()
    {
        toggleAction.action.performed -= OnToggleMenu;
        toggleAction.action.Disable();
    }

    private void Start()
    {
        SetMenuState(true);
    }

    /// <summary>
    /// Toggles the class selection menu visibility.
    /// </summary>
    /// <param name="context">The input action context.</param>
    private void OnToggleMenu(InputAction.CallbackContext context)
    {
        SetMenuState(!uiPanel.activeSelf);
    }

    /// <summary>
    /// Sets the menu state and switches input mode accordingly.
    /// </summary>
    /// <param name="state">Whether the menu should be visible.</param>
    public void SetMenuState(bool state)
    {
        uiPanel.SetActive(state);
        InputManager.Instance.UiModeLock = state;
        InputManager.Instance.SwitchInputMode(state ? InputMode.Ui : InputMode.Gameplay);
    }

    /// <summary>
    /// Selects a class and sends the selection to the server.
    /// Also deactivates the scene camera for the local player.
    /// </summary>
    /// <param name="playerClass">The selected player class.</param>
    public void SelectClass(PlayerClass playerClass)
    {
        if (!PlayerConnection.Local) return;

        GameObject sceneCamera = GameManager.Instance.GetSceneCamera();
        if (sceneCamera && PlayerConnection.Local.GetClass() == null)
            sceneCamera.SetActive(false);

        PlayerConnection.Local.ChooseClassServerRpc(playerClass);
        SetMenuState(false);
    }
}
