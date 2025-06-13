using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHpChanged))]
    private float hp;

    [SerializeField]
    public float maxHp = 100f;

    public float CurrentHP => hp;

    // �������˳�ʼ��Ѫ��
    public override void OnStartServer()
    {
        base.OnStartServer();
        hp = maxHp;
    }

    [Server]
    public void TakeDamage(float amount)
    {
        hp = Mathf.Max(0, hp - amount);
        if (hp <= 0f)
            NetworkServer.Destroy(gameObject);
    }

    public void OnHpChanged(float oldHp, float newHp)
    {
        // for state machine to call
    }
}
