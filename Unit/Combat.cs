using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ս�������׷��Ŀ�ꡢ�����ֵ��������ȴ����Ŀ������˺���
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
    /// �����µĹ���Ŀ��
    /// </summary>
    public void SetTarget(GameObject t)
    {
        target = t;
    }

    /// <summary>
    /// ȡ����ǰ����
    /// </summary>
    public void CancelAttack()
    {
        target = null;
    }

    void Update()
    {
        if (target == null) return;

        float dist = Vector3.Distance(transform.position, target.transform.position);
        // ������ÿ֡����ӡ���롢��Χ�������ϴι��������ļ��
        float since = Time.time - lastAttackTime;
        Debug.Log($"[Combat] Checking target: dist={dist:F2}, range={range}, sinceLast={since:F2}s");

        if (dist > range)
        {
            // ����Ŀ��
            agent.isStopped = false;
            agent.SetDestination(target.transform.position);
        }
        else
        {
            agent.isStopped = true;

            // ��������ӡ�Ƿ�������ȴ����
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
