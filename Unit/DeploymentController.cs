using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;




public class DeploymentController : MonoBehaviour
{

    public static DeploymentController instance;

    [Header("部署 UI 元素")]
    public GameObject panel;              // 整个 ScrollView 根节点（场景里拖）
    public GameObject itemButtonPrefab;   // 单位按钮 Prefab（项目里拖）
    public Transform contentPanel;        // ScrollView→Viewport→Content（场景里拖）

    [Header("盟军可部署 Prefabs")]
    public List<GameObject> allyPrefabs;  // 在 Inspector 一次性拖好所有盟军单位 Prefab
    [Header("轴心国可部署 Prefabs")]
    public List<GameObject> axisPrefabs;  // 在 Inspector 一次性拖好所有轴心国单位 Prefab

    private List<GameObject> myPrefabs;   // 根据本地阵营动态指向 allyPrefabs 或 axisPrefabs
    private List<Button> itemButtons = new List<Button>();
    private int selectedIndex = 0;
    private bool uiActive = false;
    private GetReady localPlayer;

    void Awake()
    {
        instance = this;
        // 一开始隐藏面板，等拿到阵营后再 Show/Build
        panel.SetActive(false);
    }

    void OnEnable()
    {
        // 订阅：当本地玩家拿到阵营后，就会触发这个回调
        GetReady.OnLocalFactionReady += OnLocalFactionReady;
    }

    void OnDisable()
    {
        GetReady.OnLocalFactionReady -= OnLocalFactionReady;
    }

    // 只有当玩家阵营确定（分配完）后，这里才被调用一次
    private void OnLocalFactionReady(Faction f)
    {
        // 拿到本地玩家实例
        localPlayer = NetworkClient.connection.identity.GetComponent<GetReady>();

        // 根据阵营选用正确的 Prefab 列表
        myPrefabs = (f == Faction.Allies) ? allyPrefabs : axisPrefabs;

        // 生成按钮到 Content 里，一旦面板Show才可见
        BuildUI();
    }

    private void Update()
    {
        if (localPlayer == null) return;  // 等本地玩家同期没到，不做任何事

        // Q 键：切换部署模式
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

        // UI 活跃时，左右键切换高亮
        if (uiActive)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                SelectIndex((selectedIndex - 1 + myPrefabs.Count) % myPrefabs.Count);
            if (Input.GetKeyDown(KeyCode.RightArrow))
                SelectIndex((selectedIndex + 1) % myPrefabs.Count);
        }
    }

    // 构建按钮列表
    private void BuildUI()
    {
        // 先清空上次遗留
        foreach (var btn in itemButtons)
            Destroy(btn.gameObject);
        itemButtons.Clear();

        // 逐一用 myPrefabs 里的 Prefab 生成一个按钮
        for (int i = 0; i < myPrefabs.Count; i++)
        {
            var btnGO = Instantiate(itemButtonPrefab, contentPanel);
            var btn = btnGO.GetComponent<Button>();
            int idx = i;
            btn.onClick.AddListener(() => SelectIndex(idx));

            // 可选：如果 itemButtonPrefab 上有 Image，设置成 myPrefabs[idx] 的图标
            // btnGO.GetComponent<Image>().sprite = someIconList[idx];

            itemButtons.Add(btn);
        }

        // 默认高亮第 0 个
        if (myPrefabs.Count > 0)
            SelectIndex(0);
    }

    private void SelectIndex(int idx)
    {
        selectedIndex = idx;
        // 更新按钮高亮，例如用 Outline
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

    // 发起 Spawn 命令
    private void TrySpawnSelectedPrefab()
    {
        if (localPlayer == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 1000f, LayerMask.GetMask("Ground")))
        {
            // 调用 GetReady 中的 RequestSpawn 方法
            localPlayer.RequestSpawn(selectedIndex, hit.point);
        }
    }
}


