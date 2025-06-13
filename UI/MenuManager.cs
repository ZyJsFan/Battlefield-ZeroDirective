using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("UI 面板引用")]
    public GameObject preparePanel;  // 准备界面 A
    public GameObject lobbyPanel;    // 房间界面 B

    void Start()
    {
        // 启动时只显示 A，隐藏 B
        preparePanel.SetActive(true);
        lobbyPanel.SetActive(false);
    }

    void Update()
    {
        if (preparePanel.activeSelf && Input.anyKeyDown)
        {
            // 切换到 Lobby
            preparePanel.SetActive(false);
            lobbyPanel.SetActive(true);
        }
    }
}
