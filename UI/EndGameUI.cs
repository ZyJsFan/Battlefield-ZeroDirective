using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class EndGameUI : MonoBehaviour
{
    public static EndGameUI Instance { get; private set; }

    [Header("�������")]
    public GameObject endGamePanel;

    [Header("ʤ����ʧ��ͼƬ")]
    public Image alliesWinImage;
    public Image axisWinImage;

    [Header("ʤ������")]
    public TextMeshProUGUI resultText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // ��ʼ����
        endGamePanel.SetActive(false);
        alliesWinImage.gameObject.SetActive(false);
        axisWinImage.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
    }

    /// <summary>
    /// ����ʱ���ã����� winner ���� UI
    /// </summary>
    public void ShowEndScreen(Faction winner)
    {
        endGamePanel.SetActive(true);
        bool alliesWin = winner == Faction.Allies;
        alliesWinImage.gameObject.SetActive(alliesWin);
        axisWinImage.gameObject.SetActive(!alliesWin);


        resultText.text = alliesWin
    ? "The Allies won this battle."
    : "The Axis won this battle.";
        resultText.gameObject.SetActive(true);

        Cursor.visible = true;
        Time.timeScale = 0f;
    }
}
