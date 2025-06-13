using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class EndGameUI : MonoBehaviour
{
    public static EndGameUI Instance { get; private set; }

    [Header("结束面板")]
    public GameObject endGamePanel;

    [Header("胜利／失败图片")]
    public Image alliesWinImage;
    public Image axisWinImage;

    [Header("胜利文字")]
    public TextMeshProUGUI resultText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // 初始隐藏
        endGamePanel.SetActive(false);
        alliesWinImage.gameObject.SetActive(false);
        axisWinImage.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 结束时调用，根据 winner 来切 UI
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
