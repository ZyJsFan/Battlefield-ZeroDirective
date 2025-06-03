using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 战斗组件：追击目标、面向插值、攻击冷却、对目标造成伤害。
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Combat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float range = 2f;
    public float attackInterval = 1f;
    public float damage = 10f;

    private GameObject target;
    private float lastAttackTime;
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// 设置新的攻击目标
    /// </summary>
    public void SetTarget(GameObject t)
    {
        target = t;
    }

    /// <summary>
    /// 取消当前攻击
    /// </summary>
    public void CancelAttack()
    {
        target = null;
    }

    void Update()
    {
        if (target == null) return;

        float dist = Vector3.Distance(transform.position, target.transform.position);
        if (dist > range)
        {
            // 跟踪目标
            agent.isStopped = false;
            agent.SetDestination(target.transform.position);
        }
        else
        {
            // 停止移动并面向目标
            agent.isStopped = true;
            Vector3 dir = (target.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);

            // 攻击冷却
            if (Time.time - lastAttackTime >= attackInterval)
            {
                // 假设目标有一个 Health 组件
                //var health = target.GetComponent<Health>();
                //if (health != null)
                //    health.TakeDamage(damage);

                lastAttackTime = Time.time;
            }
        }
    }
}
