using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class CustomNetworkUI : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public TMP_InputField addressInput;

    // —— 新增 —— 
    [Header("Canvas 切换")]
    public GameObject mainCanvas;  // 拖入 MainCanvas（包含 PreparePanel＋LobbyPanel）
    public GameObject baseCanvas;  // 拖入 base Canvas（你的游戏 UI）

    void Start()
    {
        // 初始化时，确保 baseUI 隐藏，菜单 UI 可见
        mainCanvas.SetActive(true);
        baseCanvas.SetActive(false);

        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
    }

    void OnHostClicked()
    {
        // 启动 Host
        NetworkManager.singleton.StartHost();

        // —— 新增：切换 Canvas —— 
        mainCanvas.SetActive(false);
        baseCanvas.SetActive(true);
    }

    void OnClientClicked()
    {
        // 如果输入框为空，默认连 localhost
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
