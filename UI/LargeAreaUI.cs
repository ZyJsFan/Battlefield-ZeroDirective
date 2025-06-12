using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

public class LargeAreaUI : MonoBehaviour
{
    public static LargeAreaUI Instance { get; private set; }

    [Header("UI ����")]
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

        // ��Ϸ��ʼ����ʾ������岢���ɽ�����
        onGameStartHandler = () =>
        {
            subRegionPanel.SetActive(true);
            BuildSubRegionRows();
        };
        GameFlowManager.OnGameStartEvent += onGameStartHandler;

        // ����ռ�����ʱ��ʾ
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
        // ���� DEBUG START ����  
        Debug.Log($"[UI] BuildSubRegionRows() called");
        var mgr = GameFlowManager.Instance;
        Debug.Log($"[UI] GameFlowManager.Instance = {mgr}");
        if (mgr == null)
        {
            Debug.LogError("[UI] GameFlowManager.Instance is null! ȷ�������й��� GameFlowManager ������ Awake");
            return;
        }

        var areas = mgr.largeAreas;
        Debug.Log($"[UI] largeAreas = {areas} (length = {(areas != null ? areas.Length.ToString() : "null")})");
        if (areas == null || areas.Length == 0)
        {
            Debug.LogError("[UI] largeAreas ����Ϊ�գ����� Inspector ������ GameFlowManager.largeAreas");
            return;
        }

        // ������Ѹ�Ϊ public ���Զ�ȡ������� mgr.CurrentLargeIndex
        int idx = 0;
        try { idx = mgr.currentLargeIndex; }
        catch { Debug.LogError("[UI] �޷���ȡ mgr.currentLargeIndex�����Ϊ public ��ͨ�����Է���"); }
        Debug.Log($"[UI] currentLargeIndex = {idx}");
        if (idx < 0 || idx >= areas.Length)
        {
            Debug.LogError($"[UI] currentLargeIndex Խ�� (0~{areas.Length - 1})");
            return;
        }

        var la = areas[idx];
        Debug.Log($"[UI] Selected LargeArea = {la} (name = {la?.name})");
        if (la == null)
        {
            Debug.LogError("[UI] mgr.largeAreas[idx] �� null��");
            return;
        }
        Debug.Log($"[UI] subRegions = {la.subRegions} (length = {(la.subRegions != null ? la.subRegions.Length.ToString() : "null")})");
        if (la.subRegions == null || la.subRegions.Length == 0)
        {
            Debug.LogError("[UI] LargeArea.subRegions δ���ã�");
            return;
        }

        Debug.Log($"[UI] contentParent = {contentParent}, rowPrefab = {rowPrefab}");
        if (contentParent == null) Debug.LogError("[UI] contentParent δ��ֵ��");
        if (rowPrefab == null) Debug.LogError("[UI] rowPrefab δ��ֵ��");
        // ���� DEBUG END ����  

        // ��վ���
        foreach (Transform t in contentParent) Destroy(t.gameObject);
        sliders.Clear();


        // ʵ����һ�У����� debug
        var go1 = Instantiate(rowPrefab, contentParent);
        go1.name = "DEBUG_ROW";

        // ��ӡ�����ӽڵ�����
        Debug.Log(">>> DEBUG �ӽڵ��б� of rowPrefab:");
        for (int i = 0; i < go1.transform.childCount; i++)
        {
            Debug.Log($"    Child {i}: {go1.transform.GetChild(i).name}");
        }

        // ��� Label �ڵ�
        var labelTf = go1.transform.Find("Label");
        if (labelTf == null)
        {
            Debug.LogError(">>> �Ҳ�����Ϊ 'Label' ���ӽڵ㣬���Ԥ����㼶�������Ƿ�һ��");
            return;
        }
        else Debug.Log("Label �ҵ�����������");

        var txt = labelTf.GetComponent<TextMeshProUGUI>();
        if (txt == null)
        {
            Debug.LogError(">>> 'Label' ��û�� Text �����������Ҫ���� TextMeshProUGUI ������� Text");
            return;
        }

        // ��� Slider �ڵ�
        var sliderTf = go1.transform.Find("Slider");
        if (sliderTf == null)
        {
            Debug.LogError(">>> �Ҳ�����Ϊ 'Slider' ���ӽڵ�");
            return;
        }
        else Debug.Log("Slider �ҵ�����������");

        var sld = sliderTf.GetComponent<Slider>();
        if (sld == null)
        {
            Debug.LogError(">>> 'Slider' ��û�� Slider ���");
            return;
        }

        // ������ߵ�����Ͱ� go ���٣��������������߼�
        Destroy(go1);


        currentAreaName = la.name;
        // Ϊÿ��С����һ��
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
            promptText.text = $"�ɵ�Ư������ռ�� {areaName}";
        else
            promptText.text = $"��������ʧ�֣�{areaName}����һ����������֯����";

        promptText.gameObject.SetActive(true);
        Invoke(nameof(HidePrompt), 3f);
    }

    void HidePrompt()
    {
        promptText.gameObject.SetActive(false);
    }
}
