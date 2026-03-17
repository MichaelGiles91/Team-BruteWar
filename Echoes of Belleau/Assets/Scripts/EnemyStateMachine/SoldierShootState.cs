using UnityEngine;

public class SoldierShootState : SoldierState
{
    private float fireTimer;
    private float lostSightTimer;

    public SoldierShootState(SoldierEnemyController soldier, SoldierStateMachine stateMachine)
        : base(soldier, stateMachine)
    {
    }

    public override void Enter()
    {
        soldier.StopMoving();
        fireTimer = 0f;
        lostSightTimer = 0f;
    }

    public override void Update()
    {
        if (soldier.Target == null)
        {
            ReleaseCover();
            stateMachine.ChangeState(soldier.ReturnToPostState);
            return;
        }

        if (!soldier.CanSeeTarget())
        {
            lostSightTimer += Time.deltaTime;

            if (lostSightTimer >= soldier.lostTargetReturnDelay)
            {
                ReleaseCover();
                stateMachine.ChangeState(soldier.ReturnToPostState);
            }

            return;
        }

        lostSightTimer = 0f;

        if (!soldier.IsTargetInAttackRange())
        {
            ReleaseCover();
            stateMachine.ChangeState(soldier.SeekCoverState);
            return;
        }

        Vector3 lookDirection = soldier.Target.position - soldier.transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            soldier.transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f)
        {
            soldier.FireAtTarget();
            fireTimer = soldier.fireRate;
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
