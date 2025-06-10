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
        if (!isLocalPlayer) return;

        uint localNetId = NetworkClient.connection.identity.netId;
        var selected = SelectionProcessor.Instance.CurrentlySelected;
        Debug.Log($"[CommandSystem] HandleCommand on netId={localNetId}, SelectedCount={selected.Count}");

        if (selected.Count == 0)
        {
            Debug.LogWarning("[CommandSystem] �� No units selected; aborting command.");
            return;
        }

        // �ռ�ѡ�е�λ�� netId
        List<uint> unitNetIds = selected
            .Select(s => s.GetComponent<NetworkIdentity>())
            .Where(ni => ni != null)
            .Select(ni => ni.netId)
            .ToList();

        // �е��ң��� Selectable.IsOwnedByLocal
        var selTarget = hit.collider.GetComponent<Selectable>();
        bool isEnemy = selTarget != null && !selTarget.IsOwnedByLocal;
        Debug.Log($"[CommandSystem] �� Hit `{hit.collider.name}`, isEnemy={isEnemy}");

        if (isEnemy)
        {
            // ����
            var targetNi = selTarget.GetComponent<NetworkIdentity>();
            if (targetNi != null)
            {
                Debug.Log($"[CommandSystem] �� CmdIssueAttack �� targetNetId={targetNi.netId}");
                CmdIssueAttack(unitNetIds, targetNi.netId);
            }
            else
            {
                Debug.LogWarning("[CommandSystem] �� Enemy hit but no NetworkIdentity");
            }
        }
        else
        {
            // �ƶ�
            Debug.Log($"[CommandSystem] �� CmdIssueMove �� dest={hit.point}");
            CmdIssueMove(unitNetIds, hit.point);
        }
    }

    [Command]
    private void CmdIssueMove(List<uint> unitNetIds, Vector3 dest)
    {
        Debug.Log($"[CommandSystem] [Server] CmdIssueMove for {unitNetIds.Count} units �� {dest}");
        foreach (uint id in unitNetIds)
        {
            if (!NetworkServer.spawned.TryGetValue(id, out var go)) continue;
            var uc = go.GetComponent<UnitController>();
            if (uc != null)
            {
                Debug.Log($"[CommandSystem] [Server] Enqueue MoveCommand on {go.name}(netId={id})");
                uc.EnqueueCommand(new MoveCommand(dest));
            }
        }
    }

    [Command]
    private void CmdIssueAttack(List<uint> unitNetIds, uint targetNetId)
    {
        Debug.Log($"[CommandSystem] [Server] CmdIssueAttack for {unitNetIds.Count} units �� targetNetId={targetNetId}");
        if (!NetworkServer.spawned.TryGetValue(targetNetId, out var targetGo)) return;
        var targetObj = targetGo.gameObject;

        foreach (uint id in unitNetIds)
        {
            if (!NetworkServer.spawned.TryGetValue(id, out var go)) continue;
            var uc = go.GetComponent<UnitController>();
            if (uc != null)
            {
                Debug.Log($"[CommandSystem] [Server] Enqueue AttackCommand on {go.name}(netId={id})");
                uc.EnqueueCommand(new AttackCommand(targetObj));
            }
        }
    }
}
