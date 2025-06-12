using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using Mirror;

public class LargeAreaUI : MonoBehaviour
{
    public static LargeAreaUI Instance { get; private set; }

    [Header("UI 引用")]
    public GameObject subRegionPanel;
    public GameObject rowPrefab;
    public RectTransform contentParent;
    public Text promptText;

    private List<Slider> sliders = new();
    private string currentAreaName;
    private Action onGameStartHandler;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        Debug.Log("[UI] Awake: 订阅 OnGameStartEvent & OnLargeAreaCapturedUI");
        onGameStartHandler = () =>
        {
            Debug.Log("[UI] OnGameStartEvent: 显示面板并构建行");
            subRegionPanel.SetActive(true);
            BuildSubRegionRows();
        };
        GameFlowManager.OnGameStartEvent += onGameStartHandler;
        GameFlowManager.OnLargeAreaCapturedUI += OnLargeAreaCaptured;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        GameFlowManager.OnGameStartEvent -= onGameStartHandler;
        GameFlowManager.OnLargeAreaCapturedUI -= OnLargeAreaCaptured;
    }

    void BuildSubRegionRows()
    {
        Debug.Log("[UI] BuildSubRegionRows ENTERED");
        if (subRegionPanel == null || rowPrefab == null || contentParent == null)
            Debug.LogError("[UI] BuildSubRegionRows: 有未绑定的引用");
        var mgr = GameFlowManager.Instance;
        if (mgr == null || mgr.largeAreas == null || mgr.largeAreas.Length == 0) return;
        int idx = mgr.currentLargeIndex;
        if (idx < 0 || idx >= mgr.largeAreas.Length) return;
        var la = mgr.largeAreas[idx];
        if (la == null || la.subRegions == null || la.subRegions.Length == 0) return;

        // 清空旧行
        foreach (Transform t in contentParent) Destroy(t.gameObject);
        sliders.Clear();
        currentAreaName = la.name;

        // 手动布局参数
        float rowHeight = 30f;      // 每行固定高度
        float spacing = 10f;       // 行间距
        float startX = 245f;      // 水平偏移
        float startY = -60f;      // 顶部第一行 y 坐标（锚点参照上边缘）

        // 生成新行并手动排版
        for (int i = 0; i < la.subRegions.Length; i++)
        {
            var reg = la.subRegions[i];
            var go = Instantiate(rowPrefab, contentParent);
            go.name = $"Row_{reg.name}";

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);

            float y = startY - i * (rowHeight + spacing);
            rt.anchoredPosition = new Vector2(startX, y);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, rowHeight);

            var label = go.transform.Find("Label").GetComponent<TextMeshProUGUI>();
            var slider = go.transform.Find("Slider").GetComponent<Slider>();

            label.text = reg.name;
            slider.minValue = 0;
            slider.maxValue = reg.maxProgress;
            slider.value = 0;
            slider.interactable = false;

            sliders.Add(slider);
            reg.OnProgressChanged += v => slider.value = v;
        }
    }

    void OnLargeAreaCaptured(string areaName)
    {
        Debug.Log($"[UI] OnLargeAreaCaptured({areaName}), currentAreaName={currentAreaName}");
        Debug.Log($"[UI] subRegionPanel={(subRegionPanel == null ? "NULL" : "OK")}, promptText={(promptText == null ? "NULL" : "OK")}");
        Debug.Log($"[UI] OnLargeAreaCaptured({areaName}), currentAreaName={currentAreaName}");
        if (areaName != currentAreaName) return;

        subRegionPanel.SetActive(false);

        var netId = NetworkClient.connection?.identity;
        if (netId == null) { Debug.LogError("[UI] 找不到本地 NetworkIdentity"); return; }
        var ready = netId.GetComponent<GetReady>();
        if (ready == null) { Debug.LogError("[UI] 本地玩家没有 GetReady"); return; }

        var faction = ready.faction;
        Debug.Log($"[UI] 本地阵营 = {faction}");
        promptText.text = faction == Faction.Allies
            ? $"干得漂亮！已占领 {areaName}"
            : $"该区域已失手：{areaName}，下一区域重新组织防御";

        promptText.gameObject.SetActive(true);
        Invoke(nameof(HidePrompt), 3f);
    }

    void HidePrompt()
    {
        Debug.Log("[UI] HidePrompt: 重新显示面板并刷新");
        promptText.gameObject.SetActive(false);
        subRegionPanel.SetActive(true);
        BuildSubRegionRows();
    }
}
