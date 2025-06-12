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
        Debug.Log("[UI] BuildSubRegionRows called");
        var mgr = GameFlowManager.Instance;
        if (mgr == null || mgr.largeAreas == null || mgr.largeAreas.Length == 0)
        {
            Debug.LogError("[UI] largeAreas 未配置或为空");
            return;
        }
        int idx = mgr.currentLargeIndex;
        Debug.Log($"[UI] currentLargeIndex = {idx}");
        if (idx < 0 || idx >= mgr.largeAreas.Length)
        {
            Debug.LogError("[UI] currentLargeIndex 越界");
            return;
        }

        var la = mgr.largeAreas[idx];
        if (la == null || la.subRegions == null || la.subRegions.Length == 0)
        {
            Debug.LogError("[UI] subRegions 未配置或为空");
            return;
        }

        foreach (Transform t in contentParent) Destroy(t.gameObject);
        sliders.Clear();

        currentAreaName = la.name;
        Debug.Log($"[UI] Building rows for LargeArea {currentAreaName} with {la.subRegions.Length} subRegions");
        foreach (var reg in la.subRegions)
        {
            Debug.Log($"[UI] SubRegion {reg.name} initial progress={reg.Progress}");
            var go = Instantiate(rowPrefab, contentParent);
            go.name = $"Row_{reg.name}";
            var label = go.transform.Find("Label").GetComponent<TextMeshProUGUI>();
            var slider = go.transform.Find("Slider").GetComponent<Slider>();

            label.text = reg.name;
            slider.minValue = 0;
            slider.maxValue = reg.maxProgress;
            slider.value = reg.Progress;
            slider.interactable = false;

            sliders.Add(slider);
            reg.OnProgressChanged += (v) =>
            {
                Debug.Log($"[UI] OnProgressChanged {reg.name} -> {v}");
                slider.value = v;
            };
        }
    }

    void OnLargeAreaCaptured(string areaName)
    {
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
