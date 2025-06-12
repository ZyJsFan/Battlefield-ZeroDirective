using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 游戏开始后显示子区面板并生成进度条
        onGameStartHandler = () =>
        {
            subRegionPanel.SetActive(true);
            BuildSubRegionRows();
        };
        GameFlowManager.OnGameStartEvent += onGameStartHandler;

        // 大区占领完成时提示
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
        // —— DEBUG START ——  
        Debug.Log($"[UI] BuildSubRegionRows() called");
        var mgr = GameFlowManager.Instance;
        Debug.Log($"[UI] GameFlowManager.Instance = {mgr}");
        if (mgr == null)
        {
            Debug.LogError("[UI] GameFlowManager.Instance is null! 确保场景中挂了 GameFlowManager 并且已 Awake");
            return;
        }

        var areas = mgr.largeAreas;
        Debug.Log($"[UI] largeAreas = {areas} (length = {(areas != null ? areas.Length.ToString() : "null")})");
        if (areas == null || areas.Length == 0)
        {
            Debug.LogError("[UI] largeAreas 数组为空！请在 Inspector 里配置 GameFlowManager.largeAreas");
            return;
        }

        // 如果你已改为 public 属性读取，请改用 mgr.CurrentLargeIndex
        int idx = 0;
        try { idx = mgr.currentLargeIndex; }
        catch { Debug.LogError("[UI] 无法读取 mgr.currentLargeIndex，请改为 public 或通过属性访问"); }
        Debug.Log($"[UI] currentLargeIndex = {idx}");
        if (idx < 0 || idx >= areas.Length)
        {
            Debug.LogError($"[UI] currentLargeIndex 越界 (0~{areas.Length - 1})");
            return;
        }

        var la = areas[idx];
        Debug.Log($"[UI] Selected LargeArea = {la} (name = {la?.name})");
        if (la == null)
        {
            Debug.LogError("[UI] mgr.largeAreas[idx] 是 null！");
            return;
        }
        Debug.Log($"[UI] subRegions = {la.subRegions} (length = {(la.subRegions != null ? la.subRegions.Length.ToString() : "null")})");
        if (la.subRegions == null || la.subRegions.Length == 0)
        {
            Debug.LogError("[UI] LargeArea.subRegions 未配置！");
            return;
        }

        Debug.Log($"[UI] contentParent = {contentParent}, rowPrefab = {rowPrefab}");
        if (contentParent == null) Debug.LogError("[UI] contentParent 未赋值！");
        if (rowPrefab == null) Debug.LogError("[UI] rowPrefab 未赋值！");
        // —— DEBUG END ——  

        // 清空旧行
        foreach (Transform t in contentParent) Destroy(t.gameObject);
        sliders.Clear();


        // 实例化一行，用来 debug
        var go1 = Instantiate(rowPrefab, contentParent);
        go1.name = "DEBUG_ROW";

        // 打印所有子节点名称
        Debug.Log(">>> DEBUG 子节点列表 of rowPrefab:");
        for (int i = 0; i < go1.transform.childCount; i++)
        {
            Debug.Log($"    Child {i}: {go1.transform.GetChild(i).name}");
        }

        // 检查 Label 节点
        var labelTf = go1.transform.Find("Label");
        if (labelTf == null)
        {
            Debug.LogError(">>> 找不到名为 'Label' 的子节点，检查预制体层级或名称是否一致");
            return;
        }
        else Debug.Log("Label 找到，检测组件…");

        var txt = labelTf.GetComponent<TextMeshProUGUI>();
        if (txt == null)
        {
            Debug.LogError(">>> 'Label' 上没有 Text 组件，可能需要改用 TextMeshProUGUI 或者添加 Text");
            return;
        }

        // 检查 Slider 节点
        var sliderTf = go1.transform.Find("Slider");
        if (sliderTf == null)
        {
            Debug.LogError(">>> 找不到名为 'Slider' 的子节点");
            return;
        }
        else Debug.Log("Slider 找到，检测组件…");

        var sld = sliderTf.GetComponent<Slider>();
        if (sld == null)
        {
            Debug.LogError(">>> 'Slider' 上没有 Slider 组件");
            return;
        }

        // 如果能走到这里，就把 go 销毁，下面再用正常逻辑
        Destroy(go1);


        currentAreaName = la.name;
        // 为每个小区建一行
        foreach (var reg in la.subRegions)
        {
            var go = Instantiate(rowPrefab, contentParent);
            go.name = $"Row_{reg.name}";
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
        if (areaName != currentAreaName) return;

        subRegionPanel.SetActive(false);

        var localFaction = FindObjectOfType<GetReady>().faction;
        if (localFaction == Faction.Allies)
            promptText.text = $"干得漂亮！已占领 {areaName}";
        else
            promptText.text = $"该区域已失手：{areaName}，下一区域重新组织防御";

        promptText.gameObject.SetActive(true);
        Invoke(nameof(HidePrompt), 3f);
    }

    void HidePrompt()
    {
        promptText.gameObject.SetActive(false);
    }
}
