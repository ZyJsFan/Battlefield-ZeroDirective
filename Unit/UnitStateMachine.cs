using UnityEngine;
using UnityEngine.AI;
using Mirror;  // ��� Combat/Health ���������б仯��Ҳ���Լ��� SyncVar


public class UnitStateMachine : MonoBehaviour
{
    // ���� ����״̬���� ���� 
    abstract class State
    {
        protected UnitStateMachine fsm;
        public State(UnitStateMachine fsm) { this.fsm = fsm; }
        public virtual void Enter() { }
        public virtual void Exit() { }
        public abstract void Tick();
    }

    // ���� ����̬ ���� 
    class IdleState : State
    {
        public IdleState(UnitStateMachine f) : base(f) { }
        public override void Enter() => fsm.animator.Play("Idle");
        public override void Tick() { /* ���в����� */ }
    }

    // ���� �ƶ�̬ ���� 
    class MoveState : State
    {
        public MoveState(UnitStateMachine f) : base(f) { }
        public override void Enter() => fsm.animator.Play("Move");
        public override void Tick() { /* �ɸ����ٶȵ������� */ }
    }

    // ���� ����̬ ���� 
    class AttackState : State
    {
        public AttackState(UnitStateMachine f) : base(f) { }
        public override void Enter() => fsm.animator.Play("Attack");
        public override void Tick() { /* ���������������ش������� */ }
    }

 
    class DeathState : State
    {
        public DeathState(UnitStateMachine f) : base(f) { }
        public override void Enter() => fsm.animator.Play("Death");
        public override void Tick() {  }
    }

    Animator animator;
    NavMeshAgent agent;
    Combat combat;
    Health health;

    State current;
    IdleState idleState;
    MoveState moveState;
    AttackState attackState;
    DeathState deathState;

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<Combat>();
        health = GetComponent<Health>();


        idleState = new IdleState(this);
        moveState = new MoveState(this);
        attackState = new AttackState(this);
        deathState = new DeathState(this);

        current = idleState;


    }

    void OnDestroy()
    {
    }

    void OnEnable()
    {

        TransitionTo(idleState);
    }

    void Update()
    {
    
        State next = current;

        if (health.CurrentHP <= 0f)
            next = deathState;
        else if (combat.target)             
            next = attackState;
        else if (agent.velocity.magnitude > 0.1f)
            next = moveState;
        else
            next = idleState;

        if (next != current)
            TransitionTo(next);

        current.Tick();
    }

    void TransitionTo(State next)
    {
        current.Exit();
        current = next;
        current.Enter();
    }

    void OnHpChanged(float oldHp, float newHp)
    {
        if (newHp <= 0f)
            TransitionTo(deathState);
    }
}
