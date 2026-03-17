using UnityEngine;

public class SoldierIdleState : SoldierState
{
    private float idleTimer;

    public SoldierIdleState(SoldierEnemyController soldier, SoldierStateMachine stateMachine)
        : base(soldier, stateMachine)
    {
    }

    public override void Enter()
    {
        soldier.StopMoving();
        idleTimer = Random.Range(soldier.minIdleTime, soldier.maxIdleTime);
    }

    public override void Update()
    {
        if (soldier.CanSeeTarget())
        {
            stateMachine.ChangeState(soldier.SeekCoverState);
            return;
        }

        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0f)
        {
            stateMachine.ChangeState(soldier.PatrolState);
        }
    }

    public override void Exit()
    {
    }
}
