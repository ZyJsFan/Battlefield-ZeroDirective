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
        // 新增：每帧都打印距离、范围、和自上次攻击以来的间隔
        float since = Time.time - lastAttackTime;
        Debug.Log($"[Combat] Checking target: dist={dist:F2}, range={range}, sinceLast={since:F2}s");

        if (dist > range)
        {
            // 跟踪目标
            agent.isStopped = false;
            agent.SetDestination(target.transform.position);
        }
        else
        {
            agent.isStopped = true;

            // 新增：打印是否满足冷却条件
            Debug.Log($"[Combat] In range. Cooldown check: sinceLast={since:F2}s >= interval={attackInterval:F2}s ? {since >= attackInterval}");

            if (since >= attackInterval)
            {
                Debug.Log($"[Combat] Cooldown passed, attacking now");
                var health = target.GetComponent<Health>();
                if (health != null)
                {
                    Debug.Log($"[Combat] Dealing {damage} to {target.name} (hp was {health.CurrentHP})");
                    health.TakeDamage(damage);
                }
                lastAttackTime = Time.time;
            }
        }
    }
}
