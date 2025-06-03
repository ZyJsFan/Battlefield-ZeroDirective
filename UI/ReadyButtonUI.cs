using UnityEngine;
using UnityEngine.UI;

public class ReadyButtonUI : MonoBehaviour
{
    private Button btn;
    private Text btnText;
    private GetReady localPlayer;

    void Awake()
    {
        // 缓存 Button 和它下面的 Text
        btn = GetComponent<Button>();
        btnText = GetComponentInChildren<Text>();
    }

    void OnEnable()
    {
        // 绑定点击
        btn.onClick.AddListener(OnClick);
        // 订阅 “本地玩家就绪” 事件
        GetReady.OnLocalPlayerReady += HandleLocalPlayerReady;
    }

    void OnDisable()
    {
        btn.onClick.RemoveListener(OnClick);
        GetReady.OnLocalPlayerReady -= HandleLocalPlayerReady;
    }

    // 当本地玩家脚本创建完成时，这里会被调用一次
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

            // 更新按钮文字并禁用它
            if (btnText != null) btnText.text = "已准备";
            btn.interactable = false;
        }
        else
        {
            Debug.LogWarning("[ReadyButtonUI] localPlayer is null!");
        }
    }
}
