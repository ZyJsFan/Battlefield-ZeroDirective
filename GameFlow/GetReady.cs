using Mirror;
using System;
using UnityEngine;

public enum Faction { None, Allies, Axis }

public class GetReady : NetworkBehaviour
{
    // 1. 准备状态，同步时调用 HandleReadyChanged
    [SyncVar(hook = nameof(HandleReadyChanged))]
    public bool isReady = false;

    // 2. 分配阵营，同步时调用 HandleFactionAssigned
    [SyncVar(hook = nameof(HandleFactionAssigned))]
    public Faction faction = Faction.None;
    public Faction MyFaction { get; private set; } = Faction.None;

    // 外部可以通过这个静态事件拿到本地玩家脚本
    public static event Action<GetReady> OnLocalPlayerReady;

    public static event Action<Faction> OnLocalFactionReady;

    // 外部订阅准备状态变化
    public event Action<bool, bool> OnReadyChanged;

    #region 客户端本地玩家就绪通知

    // Mirror 会在本地玩家的 NetworkIdentity 完成初始化后调用
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log($"[GetReady] OnStartLocalPlayer called for netId={netId}");
        OnLocalPlayerReady?.Invoke(this);
    }

    #endregion

    #region 客户端 UI 调用：点击“准备”按钮

    public void OnClickReadyButton()
    {
        Debug.Log($"[GetReady] OnClickReadyButton called on netId={netId}, isLocalPlayer={isLocalPlayer}");
        if (!isLocalPlayer) return;
        CmdSetReady();
    }

    [Command]
    void CmdSetReady() {
        Debug.Log($"[GetReady] CmdSetReady on server for netId={netId}");
        isReady = true;
    } 

    #endregion

    #region SyncVar Hooks

    // 当 isReady 从 old → new 改变时被 Weaver 调用
    private void HandleReadyChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"[GetReady] Ready changed: {oldValue} → {newValue} (netId={netId})");
        Debug.Log($"[GetReady] HandleReadyChanged: {oldValue}→{newValue}, on netId={netId}");
        OnReadyChanged?.Invoke(oldValue, newValue);
    }

    // 当 faction 改变时被调用
    private void HandleFactionAssigned(Faction oldValue, Faction newValue)
    {
        Debug.Log($"[GetReady] Faction assigned: {newValue} (netId={netId})");
        if (isLocalPlayer)
        {
            Debug.Log($"[GetReady] Invoking OnLocalFactionReady for local player");
            MyFaction = newValue;
            Debug.Log($"Check shunxu  faction = {MyFaction}");
            var readyUI = FindObjectOfType<ReadyButtonUI>();
            if (readyUI != null)
                readyUI.gameObject.SetActive(false);

            OnLocalFactionReady?.Invoke(newValue);
        }
    }

    // 这段是新增：由 UI 调用，告诉服务器“我要部署第 index 个单位到 position”
    public void RequestSpawn(int prefabIndex, Vector3 spawnPos)
    {
        Debug.Log($"[GetReady] RequestSpawn called on netId={netId}, isLocalPlayer={isLocalPlayer}, prefabIndex={prefabIndex}, spawnPos={spawnPos}");
        if (!isLocalPlayer) {
            Debug.LogError("[GetReady] RequestSpawn called on non-local player!");
            return;
        } 
        CmdRequestSpawn(prefabIndex, spawnPos);
    }

    [Command]
    private void CmdRequestSpawn(int prefabIndex, Vector3 spawnPos)
    {
        Debug.Log($"[GetReady] CmdRequestSpawn() on server, netId={netId}, faction={faction}, prefabIndex={prefabIndex}, spawnPos={spawnPos}");
        // 服务器端再做一次阵营+索引校验
        var validList = (faction == Faction.Allies) ?
                          DeploymentController.instance.allyPrefabs :
                          DeploymentController.instance.axisPrefabs;

        Debug.Log($"[GetReady] validList count = {validList.Count}");
        if (prefabIndex < 0 || prefabIndex >= validList.Count) { 
            Debug.LogError("[GetReady] CmdRequestSpawn: prefabIndex out of range!");
        return;
    }
        // 在服务器上实例化并通知客户端
        var prefabGo = validList[prefabIndex];
        Debug.Log($"[GetReady] CmdRequestSpawn: Instantiating {prefabGo.name} at {spawnPos}");
        var go = Instantiate(prefabGo, spawnPos, Quaternion.identity);

        var sel = go.GetComponent<Selectable>();
        if (sel != null)
            sel.InitializeOwner(connectionToClient.identity.netId);

        NetworkServer.Spawn(go, connectionToClient);
        Debug.Log($"[GetReady] CmdRequestSpawn: Spawned {go.name} with netId={go.GetComponent<NetworkIdentity>().netId}");
    }










    #endregion
}


//// 拿到本地玩家的 GetReady
//   var localPlayer = NetworkClient.connection.identity.GetComponent<GetReady>();
// 直接读它的 SyncVar 或属性
//    Debug.Log("My faction is " + localPlayer.MyFaction);