using UnityEngine;
using UnityEngine.AI;
using Mirror;  // 如果 Combat/Health 在网络上有变化，也可以监听 SyncVar


public class UnitStateMachine : MonoBehaviour
{
    // —— 定义状态基类 —— 
    abstract class State
    {
        protected UnitStateMachine fsm;
        public State(UnitStateMachine fsm) { this.fsm = fsm; }
        public virtual void Enter() { }
        public virtual void Exit() { }
        public abstract void Tick();
    }

    // —— 空闲态 —— 
    class IdleState : State
    {
        public IdleState(UnitStateMachine f) : base(f) { }
        public override void Enter() => fsm.animator.Play("Idle");
        public override void Tick() { /* 空闲不做事 */ }
    }

    // —— 移动态 —— 
    class MoveState : State
    {
        public MoveState(UnitStateMachine f) : base(f) { }
        public override void Enter() => fsm.animator.Play("Move");
        public override void Tick() { /* 可根据速度调整参数 */ }
    }

    // —— 攻击态 —— 
    class AttackState : State
    {
        public AttackState(UnitStateMachine f) : base(f) { }
        public override void Enter() => fsm.animator.Play("Attack");
        public override void Tick() { /* 若持续攻击，可重触发动画 */ }
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
