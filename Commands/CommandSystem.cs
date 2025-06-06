using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class CommandSystem : NetworkBehaviour
{
    private void OnEnable() => InputManager.OnCommandIssued += HandleCommand;
    private void OnDisable() => InputManager.OnCommandIssued -= HandleCommand;

    [Client]
    private void HandleCommand(RaycastHit hit)
    {
        // 1. ���ȼ�������־��ȷ�� HandleCommand ���ڱ�������Ǳ߱����ã����鿴ѡ������
        if (NetworkClient.connection != null && NetworkClient.connection.identity != null)
        {
            uint localNetId = NetworkClient.connection.identity.netId;
            int selectedCount = SelectionProcessor.Instance?.CurrentlySelected?.Count ?? 0;
            Debug.Log($"[CommandSystem] HandleCommand called on localNetId={localNetId}, SelectedCount={selectedCount}");
        }
        else
        {
            Debug.LogWarning("[CommandSystem] HandleCommand: NetworkClient.connection or identity is null!");
        }

        // ֻ�б�����Ҳ�����ִ������
        if (!isLocalPlayer) return;

        Debug.Log($"[CommandSystem] �� HandleCommand invoked. Hit `{hit.collider.name}` at {hit.point}");

        // �õ���ǰѡ�еĵ�λ�б�
        var selected = SelectionProcessor.Instance?.CurrentlySelected;
        if (selected == null || selected.Count == 0)
        {
            Debug.LogWarning("[CommandSystem] �� No units selected; aborting command.");
            return;
        }
        Debug.Log($"[CommandSystem] �� CurrentlySelected count = {selected.Count}");

        // �ռ�����ѡ�е�λ�� netId�����ڴ���������
        var unitNetIds = selected
            .Select(s => s.GetComponent<NetworkIdentity>())
            .Where(ni => ni != null)
            .Select(ni => ni.netId)
            .ToList();

        bool isEnemy = hit.collider.CompareTag("Enemy");
        Debug.Log($"[CommandSystem] �� Command type = {(isEnemy ? "AttackCommand" : "MoveCommand")}");

        if (isEnemy)
        {
            var targetNi = hit.collider.GetComponent<NetworkIdentity>();
            if (targetNi != null)
            {
                Debug.Log($"[CommandSystem] �� Issuing attack RPC to server on target {targetNi.netId}");
                CmdIssueAttack(unitNetIds, targetNi.netId);
            }
            else
            {
                Debug.LogWarning("[CommandSystem] �� Hit an object tagged ��Enemy�� but without NetworkIdentity!");
            }
        }
        else
        {
            Debug.Log($"[CommandSystem] �� Issuing move RPC to server to {hit.point}");
            CmdIssueMove(unitNetIds, hit.point);
        }
    }

    [Command]
    private void CmdIssueMove(List<uint> unitNetIds, Vector3 dest)
    {
        Debug.Log($"[CommandSystem] [Server] CmdIssueMove for {unitNetIds.Count} units �� {dest}");
        foreach (uint id in unitNetIds)
        {
            if (NetworkServer.spawned.TryGetValue(id, out var go))
            {
                var uc = go.GetComponent<UnitController>();
                if (uc != null)
                {
                    Debug.Log($"[CommandSystem] [Server] �C Moving unit {go.name} (netId={id}) to {dest}");
                    uc.EnqueueCommand(new MoveCommand(dest));
                }
                else
                {
                    Debug.LogWarning($"[CommandSystem] [Server] Unit with netId={id} has no UnitController!");
                }
            }
            else
            {
                Debug.LogWarning($"[CommandSystem] [Server] Could not find spawned object with netId={id}");
            }
        }
    }

    [Command]
    private void CmdIssueAttack(List<uint> unitNetIds, uint targetNetId)
    {
        Debug.Log($"[CommandSystem] [Server] CmdIssueAttack for {unitNetIds.Count} units on target {targetNetId}");
        if (!NetworkServer.spawned.TryGetValue(targetNetId, out var targetGo))
        {
            Debug.LogWarning($"[CommandSystem] [Server] Could not find target object with netId={targetNetId}");
            return;
        }

        var targetObj = targetGo.gameObject;
        foreach (uint id in unitNetIds)
        {
            if (NetworkServer.spawned.TryGetValue(id, out var go))
            {
                var uc = go.GetComponent<UnitController>();
                if (uc != null)
                {
                    Debug.Log($"[CommandSystem] [Server] �C Attacking from unit {go.name} (netId={id}) to target {targetObj.name}");
                    uc.EnqueueCommand(new AttackCommand(targetObj));
                }
                else
                {
                    Debug.LogWarning($"[CommandSystem] [Server] Unit with netId={id} has no UnitController for attack!");
                }
            }
            else
            {
                Debug.LogWarning($"[CommandSystem] [Server] Could not find spawned attacker with netId={id}");
            }
        }
    }
}
