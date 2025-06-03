using Mirror;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitController : NetworkBehaviour
{
    private NavMeshAgent agent;
    private Combat combat;
    private Queue<ICommand> commandQueue = new Queue<ICommand>();

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<Combat>();
    }

    /// <summary>
    /// 只在服务器上接受并执行命令
    /// </summary>
    [Server]
    public void EnqueueCommand(ICommand cmd)
    {
        Debug.Log($"[UnitController:{name}] EnqueueCommand {cmd.GetType().Name}");
        commandQueue.Clear();
        commandQueue.Enqueue(cmd);
        cmd.Execute(this);
    }

    /// <summary>
    /// 服务器端寻路移动
    /// </summary>
    [Server]
    public void StartMove(Vector3 dest)
    {
        Debug.Log($"[UnitController:{name}] StartMove → {dest}");
        combat.CancelAttack();
        agent.isStopped = false;
        agent.SetDestination(dest);
    }

    /// <summary>
    /// 服务器端开始攻击
    /// </summary>
    [Server]
    public void StartAttack(GameObject target)
    {
        Debug.Log($"[UnitController:{name}] StartAttack → {target.name}");
        agent.isStopped = true;
        combat.SetTarget(target);
    }
}
