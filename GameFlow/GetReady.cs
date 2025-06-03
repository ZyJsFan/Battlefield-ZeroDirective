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
            var readyUI = FindObjectOfType<ReadyButtonUI>();
            if (readyUI != null)
                readyUI.gameObject.SetActive(false);

            OnLocalFactionReady?.Invoke(newValue);
        }
    }

    // ������������� UI ���ã����߷���������Ҫ����� index ����λ�� position��
    public void RequestSpawn(int prefabIndex, Vector3 spawnPos)
    {
        if (!isLocalPlayer) return;
        CmdRequestSpawn(prefabIndex, spawnPos);
    }

    [Command]
    private void CmdRequestSpawn(int prefabIndex, Vector3 spawnPos)
    {
        // ������������һ����Ӫ+����У��
        var validList = (faction == Faction.Allies) ?
                          DeploymentController.instance.allyPrefabs :
                          DeploymentController.instance.axisPrefabs;
        if (prefabIndex < 0 || prefabIndex >= validList.Count)
            return;

        // �ڷ�������ʵ������֪ͨ�ͻ���
        var go = Instantiate(validList[prefabIndex], spawnPos, Quaternion.identity);
        NetworkServer.Spawn(go, connectionToClient);
    }










    #endregion
}


//// �õ�������ҵ� GetReady
//   var localPlayer = NetworkClient.connection.identity.GetComponent<GetReady>();
// ֱ�Ӷ����� SyncVar ������
//    Debug.Log("My faction is " + localPlayer.MyFaction);