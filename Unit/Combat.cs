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

    [Header("Ranged Settings")]                // ← 新增
    [Tooltip("勾选后使用远程攻击逻辑，否则走近战")]
    public bool isRanged = false;               // ← 新增
    [Tooltip("远程攻击的最大射程")]
    public float rangedRange = 10f;             // ← 新增
    [Tooltip("远程攻击的冷却")]
    public float rangedInterval = 1.5f;         // ← 新增

    public GameObject target;
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

        float attackRange = isRanged ? rangedRange : range;       // ← 新增
        float cooldown = isRanged ? rangedInterval : attackInterval; // ← 新增

        float dist = Vector3.Distance(transform.position, target.transform.position);
        // 新增：每帧都打印距离、范围、和自上次攻击以来的间隔
        float since = Time.time - lastAttackTime;
        Debug.Log($"[Combat:{name}] isRanged={isRanged}, dist={dist:F2}, meleeRange={range}, rangedRange={rangedRange}");

        if (dist > attackRange)
        {
            // 跟踪目标
            agent.isStopped = false;
            agent.SetDestination(target.transform.position);
        }
        else
        {
            // 到达射程／近战距离
            agent.isStopped = true;
            Debug.Log($"[Combat] In attack range. cooldown check: {since:F2} >= {cooldown:F2}? {since >= cooldown}");
            if (since >= cooldown)
            {
                Debug.Log("[Combat] Cooldown passed, attacking now");
                if (isRanged)
                {
                    Debug.Log($"[Combat:{name}] 进入 <远程攻击> 分支，冷却 check: {since:F2}>={cooldown:F2} ?");
                    // ← 远程直接打伤害（也可改为 Instantiate 弹道）
                    var health = target.GetComponent<Health>();
                    if (health != null)
                    {
                        Debug.Log($"[Combat] Ranged dealing {damage} to {target.name}");
                        health.TakeDamage(damage);
                    }
                }
                else
                {
                    Debug.Log($"[Combat:{name}] 冷却通过，实际发起 {(isRanged ? "远程" : "近战")} 攻击");
                    // ← 近战
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
