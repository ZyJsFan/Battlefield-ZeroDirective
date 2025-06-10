using Mirror;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Selectable : NetworkBehaviour
{
    [SyncVar]
    private uint ownerNetId;         

    [Header("指示器设置")]
    public GameObject selectionIndicator;
    [Header("高亮材质")]
    public Material highlightMaterial;

    private Renderer rend;
    private Material[] originalMaterials;
    private bool isSelected = false;


    public bool IsOwnedByLocal
    {
        get
        {
            if (NetworkClient.localPlayer == null) return false;
            return ownerNetId == NetworkClient.localPlayer.netId;
        }
    }

    public bool IsSelected => isSelected;
    public event System.Action<Selectable, bool> OnSelectionChanged;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        originalMaterials = rend.sharedMaterials;
        if (selectionIndicator != null)
            selectionIndicator.SetActive(false);
        else
            Debug.LogError($"[Selectable] {name} 缺少 selectionIndicator 引用");
    }

    /// <summary>
    /// 服务器端调用，用来给这个单位设置归属客户端 ID
    /// </summary>
    [Server]
    public void InitializeOwner(uint clientId)
    {
        ownerNetId = clientId;
    }

    public void SetSelected(bool selected)
    {
        // 非自己拥有的单位，直接忽略选中
        if (!IsOwnedByLocal) return;
        if (isSelected == selected) return;

        isSelected = selected;
        UpdateVisuals();
        OnSelectionChanged?.Invoke(this, isSelected);
    }

    private void UpdateVisuals()
    {
        if (selectionIndicator != null)
            selectionIndicator.SetActive(isSelected);

        if (highlightMaterial != null)
        {
            if (isSelected)
            {
                var mats = new Material[originalMaterials.Length + 1];
                originalMaterials.CopyTo(mats, 0);
                mats[mats.Length - 1] = highlightMaterial;
                rend.materials = mats;
            }
            else
            {
                rend.materials = originalMaterials;
            }
        }
    }


    void OnDestroy()
    {
        Debug.Log($"[OnDestroy] {gameObject.name} destroyed on {(isServer ? "Server" : "Client")}, netId={(GetComponent<NetworkIdentity>()?.netId.ToString() ?? "n/a")}");
    }
}
