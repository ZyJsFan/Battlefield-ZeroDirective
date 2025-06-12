using Mirror;
using System;
using UnityEngine;

public enum Faction { None, Allies, Axis }

public class GetReady : NetworkBehaviour
{
    // 1. ׼��״̬��ͬ��ʱ���� HandleReadyChanged
    [SyncVar(hook = nameof(HandleReadyChanged))]
    public bool isReady = false;

    // 2. ������Ӫ��ͬ��ʱ���� HandleFactionAssigned
    [SyncVar(hook = nameof(HandleFactionAssigned))]
    public Faction faction = Faction.None;
    public Faction MyFaction { get; private set; } = Faction.None;

    // �ⲿ����ͨ�������̬�¼��õ�������ҽű�
    public static event Action<GetReady> OnLocalPlayerReady;

    public static event Action<Faction> OnLocalFactionReady;

    // �ⲿ����׼��״̬�仯
    public event Action<bool, bool> OnReadyChanged;

    #region �ͻ��˱�����Ҿ���֪ͨ

    // Mirror ���ڱ�����ҵ� NetworkIdentity ��ɳ�ʼ�������
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log($"[GetReady] OnStartLocalPlayer called for netId={netId}");
        OnLocalPlayerReady?.Invoke(this);
    }

    #endregion

    #region �ͻ��� UI ���ã������׼������ť

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

    // �� isReady �� old �� new �ı�ʱ�� Weaver ����
    private void HandleReadyChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"[GetReady] Ready changed: {oldValue} �� {newValue} (netId={netId})");
        Debug.Log($"[GetReady] HandleReadyChanged: {oldValue}��{newValue}, on netId={netId}");
        OnReadyChanged?.Invoke(oldValue, newValue);
    }

    // �� faction �ı�ʱ������
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

    // ������������� UI ���ã����߷���������Ҫ����� index ����λ�� position��
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
        // ������������һ����Ӫ+����У��
        var validList = (faction == Faction.Allies) ?
                          DeploymentController.instance.allyPrefabs :
                          DeploymentController.instance.axisPrefabs;

        Debug.Log($"[GetReady] validList count = {validList.Count}");
        if (prefabIndex < 0 || prefabIndex >= validList.Count) { 
            Debug.LogError("[GetReady] CmdRequestSpawn: prefabIndex out of range!");
        return;
    }
        // �ڷ�������ʵ������֪ͨ�ͻ���
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


//// �õ�������ҵ� GetReady
//   var localPlayer = NetworkClient.connection.identity.GetComponent<GetReady>();
// ֱ�Ӷ����� SyncVar ������
//    Debug.Log("My faction is " + localPlayer.MyFaction);