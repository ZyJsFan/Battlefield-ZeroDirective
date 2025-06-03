using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;




public class DeploymentController : MonoBehaviour
{

    public static DeploymentController instance;

    [Header("���� UI Ԫ��")]
    public GameObject panel;              // ���� ScrollView ���ڵ㣨�������ϣ�
    public GameObject itemButtonPrefab;   // ��λ��ť Prefab����Ŀ���ϣ�
    public Transform contentPanel;        // ScrollView��Viewport��Content���������ϣ�

    [Header("�˾��ɲ��� Prefabs")]
    public List<GameObject> allyPrefabs;  // �� Inspector һ�����Ϻ������˾���λ Prefab
    [Header("���Ĺ��ɲ��� Prefabs")]
    public List<GameObject> axisPrefabs;  // �� Inspector һ�����Ϻ��������Ĺ���λ Prefab

    private List<GameObject> myPrefabs;   // ���ݱ�����Ӫ��ָ̬�� allyPrefabs �� axisPrefabs
    private List<Button> itemButtons = new List<Button>();
    private int selectedIndex = 0;
    private bool uiActive = false;
    private GetReady localPlayer;

    void Awake()
    {
        instance = this;
        // һ��ʼ������壬���õ���Ӫ���� Show/Build
        panel.SetActive(false);
    }

    void OnEnable()
    {
        // ���ģ�����������õ���Ӫ�󣬾ͻᴥ������ص�
        GetReady.OnLocalFactionReady += OnLocalFactionReady;
    }

    void OnDisable()
    {
        GetReady.OnLocalFactionReady -= OnLocalFactionReady;
    }

    // ֻ�е������Ӫȷ���������꣩������ű�����һ��
    private void OnLocalFactionReady(Faction f)
    {
        // �õ��������ʵ��
        localPlayer = NetworkClient.connection.identity.GetComponent<GetReady>();

        // ������Ӫѡ����ȷ�� Prefab �б�
        myPrefabs = (f == Faction.Allies) ? allyPrefabs : axisPrefabs;

        // ���ɰ�ť�� Content �һ�����Show�ſɼ�
        BuildUI();
    }

    private void Update()
    {
        if (localPlayer == null) return;  // �ȱ������ͬ��û���������κ���

        // Q �����л�����ģʽ
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!uiActive && SelectionProcessor.Instance.CurrentlySelected.Count == 0)
                ShowUI();
            else if (uiActive)
            {
                TrySpawnSelectedPrefab();
                HideUI();
            }
        }

        // UI ��Ծʱ�����Ҽ��л�����
        if (uiActive)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                SelectIndex((selectedIndex - 1 + myPrefabs.Count) % myPrefabs.Count);
            if (Input.GetKeyDown(KeyCode.RightArrow))
                SelectIndex((selectedIndex + 1) % myPrefabs.Count);
        }
    }

    // ������ť�б�
    private void BuildUI()
    {
        // ������ϴ�����
        foreach (var btn in itemButtons)
            Destroy(btn.gameObject);
        itemButtons.Clear();

        // ��һ�� myPrefabs ��� Prefab ����һ����ť
        for (int i = 0; i < myPrefabs.Count; i++)
        {
            var btnGO = Instantiate(itemButtonPrefab, contentPanel);
            var btn = btnGO.GetComponent<Button>();
            int idx = i;
            btn.onClick.AddListener(() => SelectIndex(idx));

            // ��ѡ����� itemButtonPrefab ���� Image�����ó� myPrefabs[idx] ��ͼ��
            // btnGO.GetComponent<Image>().sprite = someIconList[idx];

            itemButtons.Add(btn);
        }

        // Ĭ�ϸ����� 0 ��
        if (myPrefabs.Count > 0)
            SelectIndex(0);
    }

    private void SelectIndex(int idx)
    {
        selectedIndex = idx;
        // ���°�ť������������ Outline
        for (int i = 0; i < itemButtons.Count; i++)
        {
            var outline = itemButtons[i].GetComponent<Outline>();
            if (outline != null)
                outline.enabled = (i == selectedIndex);
        }
    }

    private void ShowUI()
    {
        panel.SetActive(true);
        uiActive = true;
    }

    private void HideUI()
    {
        panel.SetActive(false);
        uiActive = false;
    }

    // ���� Spawn ����
    private void TrySpawnSelectedPrefab()
    {
        if (localPlayer == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 1000f, LayerMask.GetMask("Ground")))
        {
            // ���� GetReady �е� RequestSpawn ����
            localPlayer.RequestSpawn(selectedIndex, hit.point);
        }
    }
}


