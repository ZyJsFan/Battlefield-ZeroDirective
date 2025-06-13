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
    public GameObject subRegionPanel;    // 面板根节点
    public GameObject rowPrefab;         // 子区域行 Prefab
    public RectTransform contentParent;  // 行的父级 RectTransform
    public Text promptText;              // 提示文字

    private List<Slider> sliders = new();
    private Action onGameStartHandler;

    // 客户端 UI 自己维护的索引，初始 0
    private int uiCurrentIndex = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // 1. 游戏开始时，重置 uiIndex 并显示第 0 区
        onGameStartHandler = () =>
        {
            uiCurrentIndex = 0;
            subRegionPanel.SetActive(true);
            BuildSubRegionRows(uiCurrentIndex);
        };
        GameFlowManager.OnGameStartEvent += onGameStartHandler;

        // 2. 大区完成时，弹提示并安排下一区
        GameFlowManager.OnLargeAreaCapturedUI += OnLargeAreaCaptured;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        GameFlowManager.OnGameStartEvent -= onGameStartHandler;
        GameFlowManager.OnLargeAreaCapturedUI -= OnLargeAreaCaptured;
    }

    /// <summary>
    /// 根据 uiCurrentIndex 构建子区域行
    /// </summary>
    public void BuildSubRegionRows(int index)
    {
        Debug.Log($"[UI] BuildSubRegionRows ENTERED idx={index}");

        // 非法索引保护
        var gm = GameFlowManager.Instance;
        if (gm == null || gm.largeAreas == null)
        {
            Debug.LogError("[UI] GameFlowManager.largeAreas 未配置");
            return;
        }
        if (index < 0 || index >= gm.largeAreas.Length)
        {
            Debug.LogError($"[UI] uiCurrentIndex 越界: {index}");
            return;
        }

        var la = gm.largeAreas[index];
        if (la == null || la.subRegions == null || la.subRegions.Length == 0)
        {
            Debug.LogError("[UI] 选中大区或其 subRegions 未配置");
            return;
        }

        // 清空旧行
        foreach (Transform t in contentParent) Destroy(t.gameObject);
        sliders.Clear();

        Debug.Log($"[UI] Building rows for LargeArea {la.name} with {la.subRegions.Length} subRegions");

        // 手动布局
        float rowHeight = 30f, spacing = 10f, startX = 245f, startY = -60f;
        for (int i = 0; i < la.subRegions.Length; i++)
        {
            var reg = la.subRegions[i];
            Debug.Log($"[UI] SubRegion {reg.name} init progress={reg.Progress}");

            // 实例化一行
            var go = Instantiate(rowPrefab, contentParent);
            go.name = $"Row_{reg.name}";
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            float y = startY - i * (rowHeight + spacing);
            rt.anchoredPosition = new Vector2(startX, y);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, rowHeight);

            // Label & Slider
            var label = go.transform.Find("Label").GetComponent<TextMeshProUGUI>();
            var slider = go.transform.Find("Slider").GetComponent<Slider>();
            label.text = reg.name;
            slider.minValue = 0;
            slider.maxValue = reg.maxProgress;
            slider.value = reg.Progress;
            slider.interactable = false;

            sliders.Add(slider);
        }
    }

    /// <summary>
    /// 当服务器通知大区占领完成时调用
    /// </summary>
    void OnLargeAreaCaptured(string areaName)
    {
        Debug.Log($"[UI] OnLargeAreaCaptured({areaName}), uiCurrentIndex={uiCurrentIndex}");

        // 隐藏子区面板
        subRegionPanel.SetActive(false);

        // 读取本地阵营
        var ready = NetworkClient.connection?.identity?.GetComponent<GetReady>();
        var faction = ready != null ? ready.faction : Faction.Allies;

        // 弹提示
        promptText.text = faction == Faction.Allies
            ? $"Great job! {areaName} has been captured."
            : $"Area {areaName} has fallen. Reorganize your defenses for the next sector.";
        promptText.gameObject.SetActive(true);

        // 3 秒后切换到下一区
        Invoke(nameof(HidePrompt), 3f);
    }

    /// <summary>
    /// 隐藏提示后，推进 uiCurrentIndex 并重建面板
    /// </summary>
    void HidePrompt()
    {
        Debug.Log($"[UI] HidePrompt (oldIndex={uiCurrentIndex})");

        promptText.gameObject.SetActive(false);

        // 推进本地 UI 索引（不要碰服务器 currentLargeIndex）
        uiCurrentIndex++;
        Debug.Log($"[UI] uiCurrentIndex -> {uiCurrentIndex}");

        // 重建并显示
        subRegionPanel.SetActive(true);
        BuildSubRegionRows(uiCurrentIndex);
    }

    void Update()
    {
        // 面板打开时，每帧拉最新进度，保证滑条动起来
        if (!subRegionPanel.activeSelf) return;

        var gm = GameFlowManager.Instance;
        if (gm == null || gm.largeAreas == null) return;
        if (uiCurrentIndex < 0 || uiCurrentIndex >= gm.largeAreas.Length) return;

        var la = gm.largeAreas[uiCurrentIndex];
        for (int i = 0; i < sliders.Count && i < la.subRegions.Length; i++)
        {
            sliders[i].value = la.subRegions[i].Progress;
        }
    }
}
