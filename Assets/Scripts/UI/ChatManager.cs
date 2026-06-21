using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference openChatAction;
    [SerializeField] private InputActionReference sendMessageAction;
    [SerializeField] private InputActionReference cancelChatAction;

    public GameObject chatPanel;
    public TMP_InputField chatInput;
    public GameObject messagePrefab;
    public Transform messageArea;
    public ScrollRect scrollRect;
    public float showDuration = 4f;

    public bool IsTyping { get; private set; } = false;
    private float hideTimer = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

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
        if (IsTyping)
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

        IsTyping = true;
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

            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
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
        IsTyping = false;
        chatInput.text = "";
        chatInput.gameObject.SetActive(false);
    }

    public void ShowChatTemporarily()
    {
        chatPanel.SetActive(true);
        hideTimer = showDuration;
    }
}