using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ChatManager : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionReference openChatAction;
    [SerializeField] private InputActionReference sendMessageAction;
    [SerializeField] private InputActionReference cancelChatAction;

    public GameObject chatPanel;
    public TMP_InputField chatInput;
    public GameObject messagePrefab;
    public Transform messageArea;

    public float showDuration = 4f;

    private float hideTimer = 0f;
    private bool isTyping = false;

    private void Awake()
    {
        openChatAction.action.performed += _ => OpenChat();
        sendMessageAction.action.performed += _ => SendMessageToChat();
        cancelChatAction.action.performed += _ => CancelChat();
    }

    private void Start()
    {
        chatInput.gameObject.SetActive(false);

        if (chatPanel != null)
            chatPanel.SetActive(false);
    }

    private void Update()
    {
        if (isTyping)
            return;

        if (chatPanel != null && chatPanel.activeSelf)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
            {
                chatPanel.SetActive(false);
            }
        }
    }

    public void OpenChat()
    {
        InputManager.Instance.SwitchInputMode(InputMode.Ui);

        isTyping = true;
        chatPanel.SetActive(true);
        chatInput.gameObject.SetActive(true);
        chatInput.ActivateInputField();
    }

    public void SendMessageToChat()
    {
        InputManager.Instance.SwitchInputMode(InputMode.Gameplay);

        if (!string.IsNullOrWhiteSpace(chatInput.text))
        {
            GameObject newMessage = Instantiate(messagePrefab, messageArea);
            TextMeshProUGUI textComponent = newMessage.GetComponent<TextMeshProUGUI>();
            textComponent.text = "<b><color=red>Player1</color>:</b> " + chatInput.text;
        }

        ShowChatTemporarily();
        CloseChatUI();
    }

    private void CancelChat()
    {
        InputManager.Instance.SwitchInputMode(InputMode.Gameplay);

        ShowChatTemporarily();
        CloseChatUI();
    }

    private void CloseChatUI()
    {
        isTyping = false;
        chatInput.text = "";
        chatInput.gameObject.SetActive(false);
    }

    public void ShowChatTemporarily()
    {
        chatPanel.SetActive(true);
        hideTimer = showDuration;
    }
}