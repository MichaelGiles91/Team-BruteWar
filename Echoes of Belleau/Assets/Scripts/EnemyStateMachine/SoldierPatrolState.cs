using UnityEngine;

public class SoldierPatrolState : SoldierState
{
    private Vector3 patrolTarget;

    public SoldierPatrolState(SoldierEnemyController soldier, SoldierStateMachine stateMachine)
        : base(soldier, stateMachine)
    {
    }

    public override void Enter()
    {
        patrolTarget = soldier.GetRandomPatrolPoint();
        soldier.MoveTowards(patrolTarget, soldier.patrolSpeed);
    }

    public override void Update()
    {
        if (soldier.CanSeeTarget())
        {
            stateMachine.ChangeState(soldier.SeekCoverState);
            return;
        }

        if (soldier.HasReachedDestination())
        {
            stateMachine.ChangeState(soldier.IdleState);
        }
    }

    public override void Exit()
    {
    }
}
