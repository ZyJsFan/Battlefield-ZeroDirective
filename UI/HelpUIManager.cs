using UnityEngine;

public class HelpUIManager : MonoBehaviour
{
    [Tooltip("���� HelpPanel ��Ϸ����")]
    public GameObject helpPanel;

    void Start()
    {
        // ȷ��һ��ʼ����
        if (helpPanel != null)
            helpPanel.SetActive(false);
    }

    void Update()
    {
        // �� F ��ʱ�л���ʾ/����
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
