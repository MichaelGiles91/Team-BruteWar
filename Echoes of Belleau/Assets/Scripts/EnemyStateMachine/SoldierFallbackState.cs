using UnityEngine;

public class SoldierFallbackState : SoldierState
{
    public SoldierFallbackState(SoldierEnemyController soldier, SoldierStateMachine stateMachine)
        : base(soldier, stateMachine)
    {
    }

    public override void Enter()
    {
        if (soldier.CurrentCover != null)
        {
            soldier.MoveTowards(soldier.CurrentCover.transform.position, soldier.combatMoveSpeed);
        }
    }

    public override void Update()
    {
        if (soldier.CanSeeTarget() && soldier.IsAtCover())
        {
            stateMachine.ChangeState(soldier.ShootState);
            return;
        }

        if (soldier.HasReachedDestination())
        {
            stateMachine.ChangeState(soldier.ShootState);
        }
    }

    public override void Exit()
    {
    }
}
