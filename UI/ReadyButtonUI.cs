using UnityEngine;
using UnityEngine.UI;

public class ReadyButtonUI : MonoBehaviour
{
    private Button btn;
    private Text btnText;
    private GetReady localPlayer;

    void Awake()
    {
        // ���� Button ��������� Text
        btn = GetComponent<Button>();
        btnText = GetComponentInChildren<Text>();
    }

    void OnEnable()
    {
        // �󶨵��
        btn.onClick.AddListener(OnClick);
        // ���� ��������Ҿ����� �¼�
        GetReady.OnLocalPlayerReady += HandleLocalPlayerReady;
    }

    void OnDisable()
    {
        btn.onClick.RemoveListener(OnClick);
        GetReady.OnLocalPlayerReady -= HandleLocalPlayerReady;
    }

    // ��������ҽű��������ʱ������ᱻ����һ��
    private void HandleLocalPlayerReady(GetReady player)
    {
        localPlayer = player;
        Debug.Log($"[ReadyButtonUI] Got localPlayer netId={localPlayer.netId}");
    }

    private void OnClick()
    {
        Debug.Log("[ReadyButtonUI] OnClick fired");
        if (localPlayer != null)
        {
            Debug.Log($"[ReadyButtonUI] localPlayer isLocalPlayer={localPlayer.isLocalPlayer}");
            localPlayer.OnClickReadyButton();

            // ���°�ť���ֲ�������
            if (btnText != null) btnText.text = "��׼��";
            btn.interactable = false;
        }
        else
        {
            Debug.LogWarning("[ReadyButtonUI] localPlayer is null!");
        }
    }
}
