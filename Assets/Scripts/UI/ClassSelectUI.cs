using UnityEngine;
using UnityEngine.InputSystem;

public class ClassSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject sceneCamera;

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

    private void OnToggleMenu(InputAction.CallbackContext context)
    {
        SetMenuState(!uiPanel.activeSelf);
    }

    public void SetMenuState(bool state)
    {
        uiPanel.SetActive(state);
        InputManager.Instance.SwitchInputMode(state ? InputMode.Ui : InputMode.Gameplay);
    }

    public void SelectClass(PlayerClass playerClass)
    {
        if (!PlayerConnection.Local) return;

        if (sceneCamera && PlayerConnection.Local.GetClass() == null)
            sceneCamera.SetActive(false);

        PlayerConnection.Local.ChooseClassServerRpc(playerClass);
        SetMenuState(false);
    }
}