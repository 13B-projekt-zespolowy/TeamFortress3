using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ChatManager : MonoBehaviour
{
    public GameObject chatPanel;
    public TMP_InputField chatInput;
    public GameObject messagePrefab;
    public Transform messageArea;
    public PlayerController playerMovement;

    public float showDuration = 4f;

    private float hideTimer = 0f;
    private bool isTyping = false;

    void Start()
    {
        chatInput.gameObject.SetActive(false);
        if (chatPanel != null) chatPanel.SetActive(false);
    }

    void Update()
    {
        if (!isTyping && chatPanel != null && chatPanel.activeSelf)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
            {
                chatPanel.SetActive(false);
            }
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.tKey.wasPressedThisFrame && !isTyping)
            {
                OpenChat();
            }

            if (Keyboard.current.enterKey.wasPressedThisFrame && isTyping)
            {
                SendMessageToChat();
            }

            if (isTyping && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelChat();
            }
        }
    }

    public void OpenChat()
    {
        isTyping = true;
        chatPanel.SetActive(true);
        chatInput.gameObject.SetActive(true);
        chatInput.ActivateInputField();

        if (playerMovement != null) playerMovement.canMove = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SendMessageToChat()
    {
        if (!string.IsNullOrWhiteSpace(chatInput.text))
        {
            GameObject newMessage = Instantiate(messagePrefab, messageArea);
            TextMeshProUGUI textComponent = newMessage.GetComponent<TextMeshProUGUI>();
            textComponent.text = "<b><color=red>Player1</color>:</b> " + chatInput.text;
            ShowChatTemporarily();
        }
        else
        {
            ShowChatTemporarily();
        }

        CloseChatUI();
    }

    private void CancelChat()
    {
        ShowChatTemporarily();
        CloseChatUI();
    }

    private void CloseChatUI()
    {
        isTyping = false;
        chatInput.text = "";
        chatInput.gameObject.SetActive(false);

        if (playerMovement != null) playerMovement.canMove = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowChatTemporarily()
    {
        chatPanel.SetActive(true);
        hideTimer = showDuration;
    }
}