using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("UI �������")]
    public GameObject preparePanel;  // ׼������ A
    public GameObject lobbyPanel;    // ������� B

    void Start()
    {
        // ����ʱֻ��ʾ A������ B
        preparePanel.SetActive(true);
        lobbyPanel.SetActive(false);
    }

    void Update()
    {
        if (preparePanel.activeSelf && Input.anyKeyDown)
        {
            // �л��� Lobby
            preparePanel.SetActive(false);
            lobbyPanel.SetActive(true);
        }
    }
}
