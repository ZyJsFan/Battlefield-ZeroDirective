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

    [Header("Ranged Settings")]                // �� ����
    [Tooltip("��ѡ��ʹ��Զ�̹����߼��������߽�ս")]
    public bool isRanged = false;               // �� ����
    [Tooltip("Զ�̹�����������")]
    public float rangedRange = 10f;             // �� ����
    [Tooltip("Զ�̹�������ȴ")]
    public float rangedInterval = 1.5f;         // �� ����

    public GameObject target;
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

        float attackRange = isRanged ? rangedRange : range;       // �� ����
        float cooldown = isRanged ? rangedInterval : attackInterval; // �� ����

        float dist = Vector3.Distance(transform.position, target.transform.position);
        // ������ÿ֡����ӡ���롢��Χ�������ϴι��������ļ��
        float since = Time.time - lastAttackTime;
        Debug.Log($"[Combat:{name}] isRanged={isRanged}, dist={dist:F2}, meleeRange={range}, rangedRange={rangedRange}");

        if (dist > attackRange)
        {
            // ����Ŀ��
            agent.isStopped = false;
            agent.SetDestination(target.transform.position);
        }
        else
        {
            // ������̣���ս����
            agent.isStopped = true;
            Debug.Log($"[Combat] In attack range. cooldown check: {since:F2} >= {cooldown:F2}? {since >= cooldown}");
            if (since >= cooldown)
            {
                Debug.Log("[Combat] Cooldown passed, attacking now");
                if (isRanged)
                {
                    Debug.Log($"[Combat:{name}] ���� <Զ�̹���> ��֧����ȴ check: {since:F2}>={cooldown:F2} ?");
                    // �� Զ��ֱ�Ӵ��˺���Ҳ�ɸ�Ϊ Instantiate ������
                    var health = target.GetComponent<Health>();
                    if (health != null)
                    {
                        Debug.Log($"[Combat] Ranged dealing {damage} to {target.name}");
                        health.TakeDamage(damage);
                    }
                }
                else
                {
                    Debug.Log($"[Combat:{name}] ��ȴͨ����ʵ�ʷ��� {(isRanged ? "Զ��" : "��ս")} ����");
                    // �� ��ս
                    var health = target.GetComponent<Health>();
                    if (health != null)
                    {
                        Debug.Log($"[Combat] Melee dealing {damage} to {target.name}");
                        health.TakeDamage(damage);
                    }
                }
                lastAttackTime = Time.time;
            }
        }
    }
}
