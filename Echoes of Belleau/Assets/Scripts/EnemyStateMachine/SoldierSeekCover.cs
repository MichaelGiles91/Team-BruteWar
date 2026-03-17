using UnityEngine;

public class SoldierSeekCoverState : SoldierState
{
    public SoldierSeekCoverState(SoldierEnemyController soldier, SoldierStateMachine stateMachine)
        : base(soldier, stateMachine)
    {
    }

    public override void Enter()
    {
        soldier.FindBestCover();
    }

    public override void Update()
    {
        if (soldier.Target == null)
        {
            stateMachine.ChangeState(soldier.IdleState);
            return;
        }

        if (!soldier.CanSeeTarget())
        {
            stateMachine.ChangeState(soldier.IdleState);
            return;
        }

        if (soldier.CurrentCover == null)
        {
            // No cover found, fall back to advancing or direct fire
            stateMachine.ChangeState(soldier.ShootState);
            return;
        }

        stateMachine.ChangeState(soldier.MoveToCoverState);
    }

    public override void Exit()
    {
    }
}