using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SyncVar]
    private float hp;

    [SerializeField]
    public float maxHp = 100f;


    public float CurrentHP => hp;

    void Start()
    {
        hp = maxHp;
    }

    [Server]
    public void TakeDamage(float amount)
    {
        Debug.Log($"[Health] Before damage: {gameObject.name} hp={hp}");
        hp -= amount;
        Debug.Log($"[Health] AFTER damage: {gameObject.name} hp={hp}");

        if (hp <= 0f)
        {
            Debug.Log($"[Health] {gameObject.name} died");
            NetworkServer.Destroy(gameObject);
        }
    }
}
