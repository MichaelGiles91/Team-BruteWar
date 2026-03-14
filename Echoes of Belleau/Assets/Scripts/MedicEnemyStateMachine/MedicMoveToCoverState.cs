using UnityEngine;

public class MedicMoveToCoverState : MedicState
{
    public MedicMoveToCoverState(MedicEnemyController medic, MedicStateMachine stateMachine)
        : base(medic, stateMachine)
    {
    }

    public override void Enter()
    {
        if (medic.CurrentCover != null)
        {
            medic.CurrentCover.isOccupied = true;
            medic.MoveTowards(medic.CurrentCover.transform.position, medic.moveSpeed);
        }
    }

    public override void Update()
    {
        if (medic.IsAtCover() || medic.HasReachedDestination())
        {
            stateMachine.ChangeState(medic.WaitState);
        }
    }
}
