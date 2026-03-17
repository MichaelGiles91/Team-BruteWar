using UnityEngine;

public class SoldierMoveToCoverState : SoldierState
{
    public SoldierMoveToCoverState(SoldierEnemyController soldier, SoldierStateMachine stateMachine)
        : base(soldier, stateMachine)
    {
    }

    public override void Enter()
    {
        if (soldier.CurrentCover != null)
        {
            soldier.CurrentCover.isOccupied = true;
            soldier.MoveTowards(soldier.CurrentCover.transform.position, soldier.combatMoveSpeed);
        }
    }

    public override void Update()
    {
        if (soldier.Target == null)
        {
            ReleaseCover();
            stateMachine.ChangeState(soldier.ReturnToPostState);
            return;
        }

        if (soldier.IsAtCover() || soldier.HasReachedDestination())
        {
            stateMachine.ChangeState(soldier.ShootState);
        }
    }

    public override void Exit()
    {
    }

    private void ReleaseCover()
    {
        if (soldier.CurrentCover != null)
        {
            soldier.CurrentCover.isOccupied = false;
            soldier.CurrentCover = null;
        }
    }
}
