using UnityEngine;

public class HelpUIManager : MonoBehaviour
{
    [Tooltip("拖入 HelpPanel 游戏对象")]
    public GameObject helpPanel;

    void Start()
    {
        // 确保一开始隐藏
        if (helpPanel != null)
            helpPanel.SetActive(false);
    }

    void Update()
    {
        // 按 F 键时切换显示/隐藏
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (helpPanel != null)
            {
                bool now = !helpPanel.activeSelf;
                helpPanel.SetActive(now);
            }
        }
    }
}
