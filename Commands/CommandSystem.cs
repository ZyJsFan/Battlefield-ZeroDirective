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
        // only the local player should fire commands
        if (!isLocalPlayer) return;

        Debug.Log($"[CommandSystem] ¡ú HandleCommand invoked. Hit `{hit.collider.name}` at {hit.point}");

        var selected = SelectionProcessor.Instance?.CurrentlySelected;
        if (selected == null || selected.Count == 0)
        {
            Debug.LogWarning("[CommandSystem] ¡ú No units selected; aborting command.");
            return;
        }
        Debug.Log($"[CommandSystem] ¡ú CurrentlySelected count = {selected.Count}");

        // gather their netIds
        var unitNetIds = selected
            .Select(s => s.GetComponent<NetworkIdentity>())
            .Where(ni => ni != null)
            .Select(ni => ni.netId)
            .ToList();

        bool isEnemy = hit.collider.CompareTag("Enemy");
        Debug.Log($"[CommandSystem] ¡ú Command type = {(isEnemy ? "AttackCommand" : "MoveCommand")}");

        if (isEnemy)
        {
            var targetNi = hit.collider.GetComponent<NetworkIdentity>();
            if (targetNi != null)
            {
                Debug.Log($"[CommandSystem] ¡ú Issuing attack RPC to server on target {targetNi.netId}");
                CmdIssueAttack(unitNetIds, targetNi.netId);
            }
        }
        else
        {
            Debug.Log($"[CommandSystem] ¡ú Issuing move RPC to server to {hit.point}");
            CmdIssueMove(unitNetIds, hit.point);
        }
    }

    [Command]
    private void CmdIssueMove(List<uint> unitNetIds, Vector3 dest)
    {
        Debug.Log($"[CommandSystem] [Server] CmdIssueMove for {unitNetIds.Count} units ¡ú {dest}");
        foreach (uint id in unitNetIds)
        {
            if (NetworkServer.spawned.TryGetValue(id, out var go))
            {
                var uc = go.GetComponent<UnitController>();
                if (uc != null)
                    uc.EnqueueCommand(new MoveCommand(dest));
            }
        }
    }

    [Command]
    private void CmdIssueAttack(List<uint> unitNetIds, uint targetNetId)
    {
        Debug.Log($"[CommandSystem] [Server] CmdIssueAttack for {unitNetIds.Count} units on target {targetNetId}");
        if (!NetworkServer.spawned.TryGetValue(targetNetId, out var targetGo)) return;

        var targetObj = targetGo.gameObject;
        foreach (uint id in unitNetIds)
        {
            if (NetworkServer.spawned.TryGetValue(id, out var go))
            {
                var uc = go.GetComponent<UnitController>();
                if (uc != null)
                    uc.EnqueueCommand(new AttackCommand(targetObj));
            }
        }
    }
}
