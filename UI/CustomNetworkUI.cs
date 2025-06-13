using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class CustomNetworkUI : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public TMP_InputField addressInput;

    // ���� ���� ���� 
    [Header("Canvas �л�")]
    public GameObject mainCanvas;  // ���� MainCanvas������ PreparePanel��LobbyPanel��
    public GameObject baseCanvas;  // ���� base Canvas�������Ϸ UI��

    void Start()
    {
        // ��ʼ��ʱ��ȷ�� baseUI ���أ��˵� UI �ɼ�
        mainCanvas.SetActive(true);
        baseCanvas.SetActive(false);

        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
    }

    void OnHostClicked()
    {
        // ���� Host
        NetworkManager.singleton.StartHost();

        // ���� �������л� Canvas ���� 
        mainCanvas.SetActive(false);
        baseCanvas.SetActive(true);
    }

    void OnClientClicked()
    {
        // ��������Ϊ�գ�Ĭ���� localhost
        string addr = addressInput.text.Trim();
        if (string.IsNullOrEmpty(addr))
        {
            Debug.Log("[CustomNetworkUI] address empty, defaulting to localhost");
            addr = "localhost";
        }

        NetworkManager.singleton.networkAddress = addr;
        NetworkManager.singleton.StartClient();

        mainCanvas.SetActive(false);
        baseCanvas.SetActive(true);
    }
}
