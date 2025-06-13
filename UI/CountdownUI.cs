using UnityEngine;
using TMPro;

public class CountdownUI : MonoBehaviour
{
    public TextMeshProUGUI timerText;  // Лђеп UnityEngine.UI.Text

    void OnEnable()
    {
        GameFlowManager.OnTimerUpdated += UpdateTimer;
    }

    void OnDisable()
    {
        GameFlowManager.OnTimerUpdated -= UpdateTimer;
    }

    void UpdateTimer(float secondsLeft)
    {
        var m = Mathf.FloorToInt(secondsLeft / 60f);
        var s = Mathf.FloorToInt(secondsLeft % 60f);
        timerText.text = $"{m:00}:{s:00}";
    }
}
