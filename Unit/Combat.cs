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
        if (dist > range)
        {
            // ����Ŀ��
            agent.isStopped = false;
            agent.SetDestination(target.transform.position);
        }
        else
        {
            // ֹͣ�ƶ�������Ŀ��
            agent.isStopped = true;
            Vector3 dir = (target.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);

            // ������ȴ
            if (Time.time - lastAttackTime >= attackInterval)
            {
                // ����Ŀ����һ�� Health ���
                //var health = target.GetComponent<Health>();
                //if (health != null)
                //    health.TakeDamage(damage);

                lastAttackTime = Time.time;
            }
        }
    }
}
